import fs from 'node:fs';
import path from 'node:path';
import http from 'node:http';
import assert from 'node:assert/strict';
import { chromium, firefox, webkit } from '@playwright/test';

const root = path.resolve(import.meta.dirname, '..');
const site = path.join(root, 'site');
const server = http.createServer((request, response) => {
  const pathname = decodeURIComponent(new URL(request.url, 'http://localhost').pathname);
  const file = path.resolve(site, '.' + (pathname === '/' ? '/index.html' : pathname));
  if (!file.startsWith(site + path.sep) || !fs.existsSync(file) || !fs.statSync(file).isFile()) {
    response.writeHead(404).end();
    return;
  }
  const mime = { '.html':'text/html; charset=utf-8', '.css':'text/css', '.js':'text/javascript', '.svg':'image/svg+xml', '.png':'image/png', '.json':'application/json', '.webmanifest':'application/manifest+json' };
  response.writeHead(200, { 'Content-Type':mime[path.extname(file)] ?? 'application/octet-stream', 'Service-Worker-Allowed':'/' });
  fs.createReadStream(file).pipe(response);
});

await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
const origin = `http://127.0.0.1:${server.address().port}`;
const engines = [['chromium', chromium], ['firefox', firefox], ['webkit', webkit]];
const report = { passed:true, touch:true, minimumTargetSize:24, engines:{} };

try {
  for (const [engineName, type] of engines) {
    const browser = await type.launch({ headless:true });
    try {
      const context = await browser.newContext({ viewport:{ width:390, height:844 }, locale:'en-US', reducedMotion:'reduce', hasTouch:true });
      await context.route('**/*', route => route.request().url().startsWith(origin) ? route.continue() : route.abort());
      const page = await context.newPage();
      const pageErrors = [];
      page.on('pageerror', error => pageErrors.push(error.message));
      await page.goto(origin + '/index.html', { waitUntil:'domcontentloaded' });
      await page.getByRole('heading', { level:1 }).waitFor();

      const command = page.locator('.ps-command-launch');
      const assistant = page.locator('.ps-assistant-launch');
      await command.waitFor({ state:'visible' });
      await assistant.waitFor({ state:'visible' });

      assert((await page.evaluate(() => navigator.maxTouchPoints)) > 0, `${engineName}: touch capability was not exposed`);
      for (const [label, control] of [['command', command], ['assistant', assistant]]) {
        const box = await control.boundingBox();
        assert(box && box.width >= 24 && box.height >= 24, `${engineName}: ${label} touch target is smaller than 24x24 CSS px`);
      }

      await command.tap();
      assert(await page.locator('.ps-command').evaluate(el => el.open), `${engineName}: command dialog did not open from touch`);
      await page.locator('.ps-command').evaluate(el => el.close());
      await assistant.tap();
      assert(await page.locator('.ps-assistant').evaluate(el => el.open), `${engineName}: assistant dialog did not open from touch`);

      const overflow = await page.evaluate(() => document.documentElement.scrollWidth - innerWidth);
      assert(overflow <= 1, `${engineName}: touch viewport has horizontal overflow of ${overflow}px`);
      assert.equal(pageErrors.length, 0, `${engineName}: ${pageErrors.join('; ')}`);
      report.engines[engineName] = { passed:true };
      await context.close();
    } finally {
      await browser.close();
    }
  }
  fs.mkdirSync(path.join(root, 'artifacts'), { recursive:true });
  fs.writeFileSync(path.join(root, 'artifacts', 'site-touch-compatibility.json'), JSON.stringify(report, null, 2));
  console.log(JSON.stringify(report));
} catch (error) {
  report.passed = false;
  fs.mkdirSync(path.join(root, 'artifacts'), { recursive:true });
  fs.writeFileSync(path.join(root, 'artifacts', 'site-touch-compatibility.json'), JSON.stringify({ ...report, error:error.stack || error.message }, null, 2));
  throw error;
} finally {
  server.close();
}
