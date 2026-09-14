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
const viewport = { width:390, height:844 };
const report = { passed:true, criterion:'WCAG 2.2 2.4.11 Focus Not Obscured (Minimum)', viewport, locales, engines:{} };
const errors = [];

async function nextPaint(page) {
  await page.evaluate(() => new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve))));
}

async function waitForProgressiveStyles(page) {
  await page.waitForFunction(() => {
    const command = document.querySelector('.ps-command-launch');
    const assistant = document.querySelector('.ps-assistant-launch');
    if (!command || !assistant || document.documentElement.dataset.qualityAudit !== 'passed') return false;
    const expected = ['premium.css','experience.css','future.css','intelligence.css','atelier.css','quality.css'];
    return expected.every(name => [...document.styleSheets].some(sheet => sheet.href?.endsWith('/' + name)));
  }, null, { timeout:10000 });
  await nextPaint(page);
}

async function focusedExposure(page) {
  return page.evaluate(() => {
    const element = document.activeElement;
    if (!(element instanceof HTMLElement) || element === document.body || element === document.documentElement) return null;

    // Custom file pickers commonly keep the native input tiny while exposing a
    // larger, semantically associated label. Measure that label as the visible
    // interaction surface instead of treating the intentionally hidden input as
    // an obscured control. Do not generalize this to arbitrary hidden controls.
    let visualElement = element;
    if (element instanceof HTMLInputElement && element.type === 'file' && element.labels?.length) {
      visualElement = [...element.labels].find(labelElement => {
        const labelRect = labelElement.getBoundingClientRect();
        const style = getComputedStyle(labelElement);
        return labelRect.width > 1 && labelRect.height > 1 && style.display !== 'none' && style.visibility !== 'hidden';
      }) ?? element;
    }

    const rect = visualElement.getBoundingClientRect();
    const left = Math.max(0, rect.left);
    const top = Math.max(0, rect.top);
    const right = Math.min(innerWidth, rect.right);
    const bottom = Math.min(innerHeight, rect.bottom);
    const intersectionWidth = Math.max(0, right - left);
    const intersectionHeight = Math.max(0, bottom - top);
    const tag = element.tagName.toLowerCase();
    const id = element.id ? `#${element.id}` : '';
    const classes = typeof element.className === 'string' && element.className.trim()
      ? '.' + element.className.trim().split(/\s+/).join('.')
      : '';
    const href = element instanceof HTMLAnchorElement ? `[href=${element.getAttribute('href') ?? ''}]` : '';
    const name = element.getAttribute('name') ? `[name=${element.getAttribute('name')}]` : '';
    const type = element.getAttribute('type') ? `[type=${element.getAttribute('type')}]` : '';
    const text = (element.textContent ?? '').trim().replace(/\s+/g, ' ').slice(0, 48);
    const label = `${tag}${id}${classes}${href}${name}${type}${text ? `:${text}` : ''}`;
    if (intersectionWidth <= 0 || intersectionHeight <= 0) {
      return { label, exposed:false, reason:'outside viewport', rect:{ x:rect.x, y:rect.y, width:rect.width, height:rect.height } };
    }

    const xs = [0.1, 0.5, 0.9].map(ratio => left + Math.max(1, intersectionWidth - 2) * ratio);
    const ys = [0.1, 0.5, 0.9].map(ratio => top + Math.max(1, intersectionHeight - 2) * ratio);
    let visibleSamples = 0;
    let totalSamples = 0;
    for (const x of xs) {
      for (const y of ys) {
        if (x < 0 || y < 0 || x >= innerWidth || y >= innerHeight) continue;
        totalSamples++;
        const hit = document.elementFromPoint(x, y);
        if (hit && (hit === visualElement || visualElement.contains(hit) || hit.contains(visualElement))) visibleSamples++;
      }
    }
    return {
      label,
      exposed:visibleSamples > 0,
      visibleSamples,
      totalSamples,
      visualProxy:visualElement === element ? null : visualElement.tagName.toLowerCase(),
      rect:{ x:rect.x, y:rect.y, width:rect.width, height:rect.height }
    };
  });
}

try {
  for (const [engineName, type] of engines) {
    const browser = await type.launch({ headless:true });
    report.engines[engineName] = { passed:true, checks:0, focusStops:0 };
    try {
      for (const locale of locales) {
        const context = await browser.newContext({ viewport, locale, reducedMotion:'reduce' });
        await context.route('**/*', route => route.request().url().startsWith(origin) ? route.continue() : route.abort());
        const page = await context.newPage();
        const pageErrors = [];
        page.on('pageerror', error => pageErrors.push(error.message));

        const filename = locale === 'en-US' ? 'index.html' : `index.${locale}.html`;
        await page.goto(`${origin}/${filename}`, { waitUntil:'domcontentloaded' });
        await page.getByRole('heading', { level:1 }).waitFor();
        await waitForProgressiveStyles(page);
        assert.equal(await page.locator('html').getAttribute('lang'), locale, `${engineName}/${locale}: language metadata drifted`);
        assert.equal(await page.locator('html').getAttribute('dir'), locale === 'ar' ? 'rtl' : 'ltr', `${engineName}/${locale}: direction metadata drifted`);

        // Browser engines can choose different sequential-focus starting points
        // after a synthetic pointer click. Anchor the traversal on the skip link
        // itself, verify it is a normal tabbable link, then continue with real Tab
        // presses through the rest of the document.
        const skip = page.locator('a.skip');
        assert.notEqual(await skip.getAttribute('tabindex'), '-1', `${engineName}/${locale}: skip link was removed from sequential focus order`);
        await skip.focus();
        await nextPaint(page);
        const initialExposure = await focusedExposure(page);
        assert(initialExposure?.label.startsWith('a.skip'), `${engineName}/${locale}: skip link could not receive focus`);
        assert(initialExposure.exposed, `${engineName}/${locale}: skip link is entirely obscured or outside the viewport: ${JSON.stringify(initialExposure)}`);

        const seen = new Set([initialExposure.label]);
        let focusStops = 1;
        for (let step = 0; step < 80; step++) {
          await page.keyboard.press('Tab');
          // Let the engine finish any focus-triggered scroll/layout work before
          // measuring. This observes native behavior; it does not scroll for it.
          await nextPaint(page);
          const exposure = await focusedExposure(page);
          if (!exposure) continue;
          if (seen.has(exposure.label)) break;
          seen.add(exposure.label);
          focusStops++;
          assert(exposure.exposed, `${engineName}/${locale}: ${exposure.label} is entirely obscured or outside the viewport: ${JSON.stringify(exposure)}`);
        }

        assert(focusStops >= 8, `${engineName}/${locale}: keyboard traversal covered only ${focusStops} focus stops`);
        assert([...seen].some(label => label.startsWith('a.skip')), `${engineName}/${locale}: skip link was not keyboard reachable`);
        assert([...seen].some(label => label.includes('ps-command-launch')), `${engineName}/${locale}: command launcher was not keyboard reachable`);
        assert([...seen].some(label => label.includes('ps-assistant-launch')), `${engineName}/${locale}: assistant launcher was not keyboard reachable`);
        assert.equal(pageErrors.length, 0, `${engineName}/${locale}: ${pageErrors.join('; ')}`);

        report.engines[engineName].checks++;
        report.engines[engineName].focusStops += focusStops;
        await context.close();
      }
    } finally {
      await browser.close();
    }
  }

  fs.mkdirSync(path.join(root, 'artifacts'), { recursive:true });
  fs.writeFileSync(path.join(root, 'artifacts', 'site-focus-visibility.json'), JSON.stringify(report, null, 2));
  console.log(JSON.stringify(report));
} catch (error) {
  report.passed = false;
  errors.push(error.stack || error.message);
  fs.mkdirSync(path.join(root, 'artifacts'), { recursive:true });
  fs.writeFileSync(path.join(root, 'artifacts', 'site-focus-visibility.json'), JSON.stringify({ ...report, errors }, null, 2));
  throw error;
} finally {
  server.close();
}
