import fs from 'node:fs/promises';
import path from 'node:path';
import { chromium } from '@playwright/test';

const root = path.resolve(import.meta.dirname, '..');
const media = path.join(root, 'distribution', 'media');
const browser = await chromium.launch({ channel: 'msedge', headless: true });
try {
  const page = await browser.newPage({ viewport: { width: 1440, height: 900 }, deviceScaleFactor: 1 });
  await page.route('**/*', route => route.abort());
  const svg = await fs.readFile(path.join(media, '02-access-path.svg'), 'utf8');
  await page.setContent(`<html><head><style>html,body{margin:0;width:1440px;height:900px}</style></head><body>${svg}</body></html>`);
  await page.evaluate(() => document.fonts.ready);
  await page.screenshot({ path: path.join(media, '02-access-path.png') });
  console.log('Rendered synthetic explanatory diagram; no application screenshot was altered.');
} finally { await browser.close(); }
