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
  const mime = {
    '.html':'text/html; charset=utf-8', '.css':'text/css', '.js':'text/javascript',
    '.svg':'image/svg+xml', '.png':'image/png', '.json':'application/json',
    '.webmanifest':'application/manifest+json'
  };
  response.writeHead(200, {
    'Content-Type': mime[path.extname(file)] ?? 'application/octet-stream',
    'Service-Worker-Allowed':'/'
  });
  fs.createReadStream(file).pipe(response);
});

await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
const origin = `http://127.0.0.1:${server.address().port}`;
const engines = [['chromium', chromium], ['firefox', firefox], ['webkit', webkit]];
const locales = ['en-US', 'pt-BR', 'es', 'fr', 'de', 'ar', 'ja', 'zh-Hans'];
const fullSpacingLocales = new Set(['en-US', 'pt-BR', 'es', 'fr', 'de']);
const report = {
  passed:true,
  viewport:{ width:1280, height:800 },
  locales,
  spacing:{ lineHeight:1.5, paragraphAfter:'2em', letterSpacing:'0.12em', wordSpacing:'0.16em' },
  engines:{}
};
const errors = [];

async function waitForProgressiveStyles(page) {
  await page.waitForFunction(() => {
    const command = document.querySelector('.ps-command-launch');
    const assistant = document.querySelector('.ps-assistant-launch');
    const hashInput = document.querySelector('.ps-hash-input');
    if (!command || !assistant || !hashInput || document.documentElement.dataset.qualityAudit !== 'passed') return false;
    const expected = ['premium.css','experience.css','future.css','intelligence.css','atelier.css','quality.css'];
    return expected.every(name => [...document.styleSheets].some(sheet => sheet.href?.endsWith('/' + name)));
  }, null, { timeout:10000 });
  await page.evaluate(() => new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve))));
}

async function assertNoHorizontalOverflow(page, label) {
  const metrics = await page.evaluate(() => ({ viewport:innerWidth, scrollWidth:document.documentElement.scrollWidth }));
  assert(metrics.scrollWidth <= metrics.viewport + 1, `${label}: horizontal overflow viewport=${metrics.viewport}px scrollWidth=${metrics.scrollWidth}px`);
}

try {
  for (const [engineName, type] of engines) {
    const browser = await type.launch({ headless:true });
    report.engines[engineName] = { passed:true, checks:0 };
    try {
      for (const locale of locales) {
        const context = await browser.newContext({ viewport:{ width:1280, height:800 }, locale, reducedMotion:'reduce' });
        await context.route('**/*', route => route.request().url().startsWith(origin) ? route.continue() : route.abort());
        const page = await context.newPage();
        const pageErrors = [];
        page.on('pageerror', error => pageErrors.push(error.message));

        const filename = locale === 'en-US' ? 'index.html' : `index.${locale}.html`;
        await page.goto(`${origin}/${filename}`, { waitUntil:'domcontentloaded' });
        await page.getByRole('heading', { level:1 }).waitFor();
        await waitForProgressiveStyles(page);

        const fullSpacing = fullSpacingLocales.has(locale);
        await page.evaluate(({ fullSpacing }) => {
          for (const element of document.body.querySelectorAll('*')) {
            element.style.setProperty('line-height', '1.5', 'important');
            if (fullSpacing) {
              element.style.setProperty('letter-spacing', '0.12em', 'important');
              element.style.setProperty('word-spacing', '0.16em', 'important');
            }
          }
          for (const paragraph of document.querySelectorAll('p')) {
            paragraph.style.setProperty('margin-bottom', '2em', 'important');
          }
        }, { fullSpacing });
        await page.evaluate(() => new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve))));

        assert.equal(await page.locator('html').getAttribute('lang'), locale, `${engineName}/${locale}: language metadata drifted`);
        assert.equal(await page.locator('html').getAttribute('dir'), locale === 'ar' ? 'rtl' : 'ltr', `${engineName}/${locale}: direction metadata drifted`);
        await assertNoHorizontalOverflow(page, `${engineName}/${locale}/text-spacing`);

        const h1 = page.getByRole('heading', { level:1 });
        const command = page.locator('.ps-command-launch');
        const assistant = page.locator('.ps-assistant-launch');
        assert(await h1.isVisible(), `${engineName}/${locale}: primary heading hidden with text spacing overrides`);
        assert(await command.isVisible(), `${engineName}/${locale}: command control hidden with text spacing overrides`);
        assert(await assistant.isVisible(), `${engineName}/${locale}: assistant control hidden with text spacing overrides`);
        assert(await page.locator('#download').isVisible(), `${engineName}/${locale}: download section hidden with text spacing overrides`);

        await command.click();
        assert(await page.locator('.ps-command').evaluate(el => el.open), `${engineName}/${locale}: command dialog failed with text spacing overrides`);
        await page.keyboard.press('Escape');
        await assistant.click();
        assert(await page.locator('.ps-assistant').evaluate(el => el.open), `${engineName}/${locale}: assistant dialog failed with text spacing overrides`);
        await page.keyboard.press('Escape');

        assert.equal(pageErrors.length, 0, `${engineName}/${locale}: ${pageErrors.join('; ')}`);
        report.engines[engineName].checks++;
        await context.close();
      }
    } finally {
      await browser.close();
    }
  }

  fs.mkdirSync(path.join(root, 'artifacts'), { recursive:true });
  fs.writeFileSync(path.join(root, 'artifacts', 'site-text-spacing-compatibility.json'), JSON.stringify(report, null, 2));
  console.log(JSON.stringify(report));
} catch (error) {
  report.passed = false;
  errors.push(error.stack || error.message);
  fs.mkdirSync(path.join(root, 'artifacts'), { recursive:true });
  fs.writeFileSync(path.join(root, 'artifacts', 'site-text-spacing-compatibility.json'), JSON.stringify({ ...report, errors }, null, 2));
  throw error;
} finally {
  server.close();
}
