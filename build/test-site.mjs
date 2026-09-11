import fs from 'node:fs';
import path from 'node:path';
import http from 'node:http';
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { chromium } from '@playwright/test';

const require = createRequire(import.meta.url);
const root = path.resolve(import.meta.dirname, '..');
const site = path.join(root, 'site');
const props = fs.readFileSync(path.join(root, 'Directory.Build.props'), 'utf8');
const version = props.match(/<Version>([^<]+)<\/Version>/)?.[1]?.trim();
if (!version) throw new Error('Could not resolve project Version from Directory.Build.props');
const releasePrefix = `/releases/download/v${version}/`;
const errors = [];
const server = http.createServer((request, response) => {
  const pathname = decodeURIComponent(new URL(request.url, 'http://localhost').pathname);
  if (pathname === '/__test/axe.js') { response.writeHead(200, { 'Content-Type': 'text/javascript' }); fs.createReadStream(require.resolve('axe-core/axe.min.js')).pipe(response); return; }
  const file = path.resolve(site, '.' + (pathname === '/' ? '/index.html' : pathname));
  if (!file.startsWith(site + path.sep) || !fs.existsSync(file) || !fs.statSync(file).isFile()) { response.writeHead(404).end(); return; }
  const mime = { '.html': 'text/html; charset=utf-8', '.css': 'text/css', '.js': 'text/javascript', '.svg': 'image/svg+xml', '.png': 'image/png' };
  response.writeHead(200, { 'Content-Type': mime[path.extname(file)] ?? 'application/octet-stream' });
  fs.createReadStream(file).pipe(response);
});
await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
const origin = `http://127.0.0.1:${server.address().port}`;
let browser;
try {
  browser = await chromium.launch({ channel: 'msedge', headless: true });
  const context = await browser.newContext({ viewport: { width: 1440, height: 1000 }, locale: 'en-US' });
  await context.route('**/*', route => route.request().url().startsWith(origin) ? route.continue() : route.abort());
  const page = await context.newPage();
  page.on('pageerror', error => errors.push(error.message));
  await page.goto(origin);
  await page.getByRole('heading', { level: 1 }).waitFor();
  await page.locator('.ps-workstation-scene').waitFor();
  await page.locator('.ps-command-launch').waitFor();
  await page.locator('.ps-assistant-launch').waitFor();
  await page.keyboard.press('Tab');
  assert.equal(await page.evaluate(() => document.activeElement.getAttribute('href')), '#main');
  assert.equal(await page.locator('.ps-device').count(), 1, 'premium device should render exactly once');
  assert.equal(await page.locator('.ps-device-port-rail i').count(), 4, 'premium device port rail should expose four physical port details');
  assert.equal(await page.locator('.ps-release-rail').count(), 1, 'hero release rail should render exactly once');
  await page.locator('.ps-command-launch').click();
  await page.locator('.ps-command').waitFor({ state: 'visible' });
  const commandHrefs = await page.locator('.ps-command-item').evaluateAll(nodes => nodes.map(node => node.getAttribute('href')));
  assert(commandHrefs[0]?.includes(`${releasePrefix}PermissionScope-${version}-x64-Setup.exe`), 'command center x64 download must target current release');
  assert(commandHrefs[1]?.includes(`${releasePrefix}PermissionScope-${version}-arm64-Setup.exe`), 'command center ARM64 download must target current release');
  assert(commandHrefs.slice(0, 2).every(href => href?.includes(releasePrefix)), 'command center must not expose stale release links');
  await page.locator('.ps-command-close').click();

  await page.locator('.ps-assistant-launch').click();
  await page.locator('.ps-assistant').waitFor({ state: 'visible' });
  assert.equal(await page.locator('.ps-assistant-compose textarea').count(), 1, 'assistant composer is missing');
  assert((await page.locator('.ps-assistant-suggestions button').count()) >= 3, 'assistant should expose starter questions');
  await page.locator('.ps-assistant-suggestions button').first().click();
  await page.locator('.ps-msg.user').last().waitFor();
  await page.locator('.ps-msg.assistant').last().waitFor();
  assert((await page.locator('.ps-msg.assistant').last().textContent())?.trim().length > 1, 'assistant should return a local or on-device answer');
  await page.locator('.ps-assistant-close').click();

  const locales = ['en-US','pt-BR','es','fr','de','ar','ja','zh-Hans'];
  for (const locale of locales) {
    const filename = locale === 'en-US' ? 'index.html' : `index.${locale}.html`;
    await Promise.all([page.waitForURL(origin + '/' + filename), page.selectOption('#language', filename)]);
    await page.locator('.ps-workstation-scene').waitFor();
    assert.equal(await page.locator('html').getAttribute('lang'), locale);
    assert.equal(await page.locator('html').getAttribute('dir'), locale === 'ar' ? 'rtl' : 'ltr');
    assert((await page.locator('.product img').getAttribute('src')).includes(`/${locale}/`));
    assert.equal(await page.locator('.ps-device').count(), 1, `${locale}: premium device duplicated`);
    assert.equal(await page.locator('.ps-release-rail').count(), 1, `${locale}: release rail duplicated`);
    const downloadHrefs = await page.locator('.ps-download-card').evaluateAll(nodes => nodes.map(node => node.getAttribute('href')));
    assert.equal(downloadHrefs.length, 4, `${locale}: expected four primary download choices`);
    assert(downloadHrefs.every(href => href?.includes(releasePrefix)), `${locale}: stale primary download URL`);
    for (const width of [1440, 768, 375]) {
      await page.setViewportSize({ width, height: 1000 });
      assert(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), `${locale}: horizontal overflow at ${width}px`);
    }
    await page.addScriptTag({ url: origin + '/__test/axe.js' });
    const accessibility = await page.evaluate(async () => axe.run(document, { runOnly: { type: 'tag', values: ['wcag2a', 'wcag2aa', 'wcag21aa'] } }));
    const violations = accessibility.violations.map(item => ({
      id: item.id,
      impact: item.impact,
      nodes: item.nodes.slice(0, 8).map(node => ({ target: node.target, html: node.html, summary: node.failureSummary }))
    }));
    assert.deepEqual(violations, [], `${locale}: ${JSON.stringify(violations)}`);
    assert(await page.locator('.product img').evaluate(image => image.complete && image.naturalWidth > 0));
  }
  fs.mkdirSync(path.join(root, 'artifacts'), { recursive: true });
  await page.locator('h1').click();
  await page.screenshot({ path: path.join(root, 'artifacts/site-mobile.png'), fullPage: true });
  await page.setViewportSize({ width: 1440, height: 1000 });
  await page.goto(origin);
  await page.locator('.ps-workstation-scene').waitFor();
  await page.screenshot({ path: path.join(root, 'artifacts/site-desktop.png'), fullPage: true });
  await page.emulateMedia({ forcedColors: 'active', reducedMotion: 'reduce' });
  assert.equal(await page.getByRole('heading', { level: 1 }).count(), 1);
  assert.deepEqual(errors, []);
  const result = { passed: true, version, widths: [1440, 768, 375], locales, axeViolations: 0, keyboardSkipLink: true, premiumWorkstation: true, currentReleaseLinks: true, assistantSmokeTest: true, forcedColors: true, externalRequestsBlocked: true };
  fs.writeFileSync(path.join(root, 'artifacts/site-tests.json'), JSON.stringify(result, null, 2));
  console.log(JSON.stringify(result));
} finally { await browser?.close(); server.close(); }
