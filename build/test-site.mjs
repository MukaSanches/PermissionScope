import fs from 'node:fs';
import path from 'node:path';
import http from 'node:http';
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { chromium } from '@playwright/test';

const require = createRequire(import.meta.url);
const root = path.resolve(import.meta.dirname, '..');
const site = path.join(root, 'site');
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
  await page.keyboard.press('Tab');
  assert.equal(await page.evaluate(() => document.activeElement.getAttribute('href')), '#main');
  const locales = ['en-US','pt-BR','es','fr','de','ar','ja','zh-Hans'];
  for (const locale of locales) {
    const filename = locale === 'en-US' ? 'index.html' : `index.${locale}.html`;
    await Promise.all([page.waitForURL(origin + '/' + filename), page.selectOption('#language', filename)]);
    assert.equal(await page.locator('html').getAttribute('lang'), locale);
    assert.equal(await page.locator('html').getAttribute('dir'), locale === 'ar' ? 'rtl' : 'ltr');
    assert((await page.locator('.product img').getAttribute('src')).includes(`/${locale}/`));
    for (const width of [1440, 768, 375]) {
      await page.setViewportSize({ width, height: 1000 });
      assert(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), `${locale}: horizontal overflow at ${width}px`);
    }
    await page.addScriptTag({ url: origin + '/__test/axe.js' });
    const accessibility = await page.evaluate(async () => axe.run(document, { runOnly: { type: 'tag', values: ['wcag2a', 'wcag2aa', 'wcag21aa'] } }));
    assert.deepEqual(accessibility.violations.map(item => ({ id: item.id, impact: item.impact })), [], locale);
    assert(await page.locator('.product img').evaluate(image => image.complete && image.naturalWidth > 0));
  }
  fs.mkdirSync(path.join(root, 'artifacts'), { recursive: true });
  await page.locator('h1').click();
  await page.screenshot({ path: path.join(root, 'artifacts/site-mobile.png'), fullPage: true });
  await page.setViewportSize({ width: 1440, height: 1000 });
  await page.goto(origin);
  await page.screenshot({ path: path.join(root, 'artifacts/site-desktop.png'), fullPage: true });
  await page.emulateMedia({ forcedColors: 'active', reducedMotion: 'reduce' });
  assert.equal(await page.getByRole('heading', { level: 1 }).count(), 1);
  assert.deepEqual(errors, []);
  const result = { passed: true, widths: [1440, 768, 375], locales, axeViolations: 0, keyboardSkipLink: true, forcedColors: true, externalRequestsBlocked: true };
  fs.writeFileSync(path.join(root, 'artifacts/site-tests.json'), JSON.stringify(result, null, 2));
  console.log(JSON.stringify(result));
} finally { await browser?.close(); server.close(); }
