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
  if (pathname === '/__test/axe.js') { response.writeHead(200, { 'Content-Type': 'text/javascript' }); fs.createReadStream(require.resolve('axe-core/axe.min.js')).pipe(response); return; }
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
const report = { passed:true, engines:{}, viewports:viewports.map(v=>v[0]), locales };
const errors = [];
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
        assert(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth + 1), `${engineName}/${name}: horizontal overflow`);
        assert(await page.locator('.wordmark').isVisible(), `${engineName}/${name}: wordmark hidden`);
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
        assert.equal(await page.locator('html').getAttribute('lang'), locale);
        assert.equal(await page.locator('html').getAttribute('dir'), locale === 'ar' ? 'rtl' : 'ltr');
        assert(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth + 1), `${engineName}/${locale}: localized mobile overflow`);
        assert(await page.locator('#download-premium').count() === 1, `${engineName}/${locale}: premium downloads missing`);
        assert(await page.locator('.ps-hash-tool').count() === 1, `${engineName}/${locale}: SHA tool missing`);
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
