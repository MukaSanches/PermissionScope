import fs from 'node:fs';
import path from 'node:path';
import http from 'node:http';
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { chromium, firefox, webkit } from '@playwright/test';

const require = createRequire(import.meta.url);
const root = path.resolve(import.meta.dirname, '..');
const site = path.join(root, 'site');
const server = http.createServer((request, response) => {
  const pathname = decodeURIComponent(new URL(request.url, 'http://localhost').pathname);
  if (pathname === '/__test/axe.js') { response.writeHead(200, { 'Content-Type':'text/javascript' }); fs.createReadStream(require.resolve('axe-core/axe.min.js')).pipe(response); return; }
  const file = path.resolve(site, '.' + (pathname === '/' ? '/index.html' : pathname));
  if (!file.startsWith(site + path.sep) || !fs.existsSync(file) || !fs.statSync(file).isFile()) { response.writeHead(404).end(); return; }
  const mime = { '.html':'text/html; charset=utf-8','.css':'text/css','.js':'text/javascript','.svg':'image/svg+xml','.png':'image/png','.json':'application/json','.webmanifest':'application/manifest+json' };
  response.writeHead(200, { 'Content-Type': mime[path.extname(file)] ?? 'application/octet-stream', 'Service-Worker-Allowed':'/' });
  fs.createReadStream(file).pipe(response);
});
await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
const origin = `http://127.0.0.1:${server.address().port}`;
const engines = [['chromium',chromium],['firefox',firefox],['webkit',webkit]];
const viewports = [
  ['phone-320',{width:320,height:568}],['phone-360',{width:360,height:800}],['phone-390',{width:390,height:844}],['phone-430',{width:430,height:932}],
  ['tablet-portrait',{width:768,height:1024}],['tablet-landscape',{width:1024,height:768}],['notebook',{width:1366,height:768}],['desktop',{width:1440,height:900}],['fullhd',{width:1920,height:1080}],['ultrawide',{width:2560,height:1440}],
  ['short-landscape',{width:844,height:390}]
];
const locales = ['en-US','pt-BR','es','fr','de','ar','ja','zh-Hans'];
const report = { passed:true, engines:{}, viewports:viewports.map(v=>v[0]), locales, atelier:true, qualityAudit:true, monitorOnly3D:true };
const errors = [];

async function waitForProgressiveStyles(page) {
  await page.waitForFunction(() => {
    const command = document.querySelector('.ps-command-launch');
    const assistant = document.querySelector('.ps-assistant-launch');
    const hashInput = document.querySelector('.ps-hash-input');
    const workstation = document.querySelector('.ps-workstation-scene');
    const device = document.querySelector('.ps-device');
    if (!command || !assistant || !hashInput || !workstation || !device || document.documentElement.dataset.qualityAudit !== 'passed') return false;
    const sheets = [...document.styleSheets];
    const expected = ['premium.css','experience.css','future.css','intelligence.css','atelier.css','quality.css'];
    if (!expected.every(name => sheets.some(sheet => sheet.href?.endsWith('/' + name)))) return false;
    return getComputedStyle(hashInput).width === '1px';
  }, null, { timeout:10000 });
  await page.evaluate(() => new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve))));
}

async function assertNoHorizontalOverflow(page, label) {
  const diagnostics = await page.evaluate(() => {
    const viewport = innerWidth;
    const scrollWidth = document.documentElement.scrollWidth;
    const offenders = [...document.querySelectorAll('body *')]
      .map((el) => {
        const rect = el.getBoundingClientRect();
        if (rect.width <= 0 || rect.height <= 0 || (rect.left >= -1 && rect.right <= viewport + 1)) return null;
        const id = el.id ? `#${el.id}` : '';
        const classes = typeof el.className === 'string' && el.className.trim() ? '.' + el.className.trim().split(/\s+/).join('.') : '';
        return { node:`${el.tagName.toLowerCase()}${id}${classes}`, left:Math.round(rect.left * 10) / 10, right:Math.round(rect.right * 10) / 10, width:Math.round(rect.width * 10) / 10 };
      })
      .filter(Boolean)
      .sort((a,b) => Math.max(-a.left, a.right - viewport) - Math.max(-b.left, b.right - viewport))
      .slice(-12);
    return { viewport, scrollWidth, offenders };
  });
  assert(diagnostics.scrollWidth <= diagnostics.viewport + 1, `${label}: horizontal overflow viewport=${diagnostics.viewport}px scrollWidth=${diagnostics.scrollWidth}px offenders=${JSON.stringify(diagnostics.offenders)}`);
}

try {
  for (const [engineName, type] of engines) {
    const browser = await type.launch({ headless:true });
    report.engines[engineName] = { passed:true, checks:0 };
    try {
      for (const [name, viewport] of viewports) {
        const context = await browser.newContext({ viewport, locale:'en-US', reducedMotion:'reduce' });
        await context.route('**/*', route => route.request().url().startsWith(origin) ? route.continue() : route.abort());
        const page = await context.newPage();
        const pageErrors = [];
        page.on('pageerror', e => pageErrors.push(e.message));
        await page.goto(origin + '/index.html', { waitUntil:'domcontentloaded' });
        await page.getByRole('heading', { level:1 }).waitFor();
        await waitForProgressiveStyles(page);
        await assertNoHorizontalOverflow(page, `${engineName}/${name}`);
        assert(await page.locator('.wordmark').isVisible(), `${engineName}/${name}: wordmark hidden`);
        assert.equal(await page.locator('.ps-device').count(), 1, `${engineName}/${name}: premium device missing or duplicated`);
        assert.equal(await page.locator('.ps-release-rail').count(), 1, `${engineName}/${name}: release rail missing or duplicated`);
        assert.equal(await page.locator('.ps-desk-plane,.ps-desk-keyboard,.ps-desk-mouse,.ps-desk-node,.ps-desk-cable').count(), 0, `${engineName}/${name}: desk peripherals rendered`);
        assert.equal(await page.evaluate(() => document.documentElement.dataset.localeLeak), 'false', `${engineName}/${name}: locale leak audit failed`);
        assert(await page.locator('.ps-command-launch').isVisible(), `${engineName}/${name}: command launch hidden`);
        assert(await page.locator('.ps-assistant-launch').isVisible(), `${engineName}/${name}: assistant launch hidden`);
        const cmdBox = await page.locator('.ps-command-launch').boundingBox();
        const aiBox = await page.locator('.ps-assistant-launch').boundingBox();
        if (cmdBox && aiBox) {
          const overlap = !(cmdBox.x + cmdBox.width <= aiBox.x || aiBox.x + aiBox.width <= cmdBox.x || cmdBox.y + cmdBox.height <= aiBox.y || aiBox.y + aiBox.height <= cmdBox.y);
          assert(!overlap, `${engineName}/${name}: floating controls overlap`);
        }
        await page.locator('.ps-command-launch').click();
        assert(await page.locator('.ps-command').evaluate(el => el.open), `${engineName}/${name}: command dialog did not open`);
        await page.keyboard.press('Escape');
        await page.locator('.ps-assistant-launch').click();
        assert(await page.locator('.ps-assistant').evaluate(el => el.open), `${engineName}/${name}: assistant dialog did not open`);
        await page.keyboard.press('Escape');
        assert.equal(pageErrors.length, 0, `${engineName}/${name}: ${pageErrors.join('; ')}`);
        report.engines[engineName].checks++;
        await context.close();
      }
      const context = await browser.newContext({ viewport:{width:390,height:844}, locale:'en-US', reducedMotion:'reduce' });
      await context.route('**/*', route => route.request().url().startsWith(origin) ? route.continue() : route.abort());
      const page = await context.newPage();
      for (const locale of locales) {
        const filename = locale === 'en-US' ? 'index.html' : `index.${locale}.html`;
        await page.goto(origin + '/' + filename, { waitUntil:'domcontentloaded' });
        await waitForProgressiveStyles(page);
        assert.equal(await page.locator('html').getAttribute('lang'), locale);
        assert.equal(await page.locator('html').getAttribute('dir'), locale === 'ar' ? 'rtl' : 'ltr');
        await assertNoHorizontalOverflow(page, `${engineName}/${locale}`);
        assert.equal(await page.locator('#download').count(), 1, `${engineName}/${locale}: canonical download section missing or duplicated`);
        assert.equal(await page.locator('#download-premium').count(), 0, `${engineName}/${locale}: legacy download fragment leaked after enhancement`);
        assert(await page.locator('#download .ps-download-card').count() >= 1, `${engineName}/${locale}: enhanced download cards missing`);
        assert(await page.locator('.ps-hash-tool').count() === 1, `${engineName}/${locale}: SHA tool missing`);
        assert(await page.locator('.ps-workstation-scene').count() === 1, `${engineName}/${locale}: monitor scene missing`);
        assert.equal(await page.locator('.ps-desk-plane,.ps-desk-keyboard,.ps-desk-mouse,.ps-desk-node,.ps-desk-cable').count(), 0, `${engineName}/${locale}: desk peripherals rendered`);
        assert.equal(await page.evaluate(() => document.documentElement.dataset.localeLeak), 'false', `${engineName}/${locale}: locale leak audit failed`);
      }
      await context.close();
    } finally { await browser.close(); }
  }
  fs.mkdirSync(path.join(root,'artifacts'), { recursive:true });
  fs.writeFileSync(path.join(root,'artifacts','site-compatibility.json'), JSON.stringify(report,null,2));
  console.log(JSON.stringify(report));
} catch (error) {
  report.passed = false; errors.push(error.stack || error.message); fs.mkdirSync(path.join(root,'artifacts'), { recursive:true }); fs.writeFileSync(path.join(root,'artifacts','site-compatibility.json'), JSON.stringify({...report,errors},null,2)); throw error;
} finally { server.close(); }
