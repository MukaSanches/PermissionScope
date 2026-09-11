import fs from 'node:fs';
import path from 'node:path';

const root = path.resolve(import.meta.dirname, '..');
const directory = path.join(root, 'src/PermissionScope.App/Locales');
const baseline = JSON.parse(fs.readFileSync(path.join(directory, 'en-US.json'), 'utf8'));
const required = ['Home', 'Analyze', 'Cancel', 'Settings', 'Unknown', 'Granted', 'Denied', 'ChangeWarning', 'ConfirmPath', 'ErrorTitle'];
const placeholders = value => [...value.matchAll(/\{\d+(?:[^{}]*)\}/g)].map(match => match[0]).sort().join('|');
const coverage = [];
for (const name of fs.readdirSync(directory).filter(name => name.endsWith('.json'))) {
  const source = fs.readFileSync(path.join(directory, name), 'utf8');
  const entries = [...source.matchAll(/"([^"\\]+)"\s*:/g)].map(match => match[1]);
  if (new Set(entries).size !== entries.length) throw new Error(`Duplicate resource key: ${name}`);
  const catalog = JSON.parse(source);
  for (const [key, value] of Object.entries(catalog)) {
    if (!(key in baseline)) throw new Error(`Unknown key: ${name}/${key}`);
    if (typeof value !== 'string' || !value.trim()) throw new Error(`Empty resource: ${name}/${key}`);
    if (placeholders(value) !== placeholders(baseline[key])) throw new Error(`Placeholder mismatch: ${name}/${key}`);
  }
  for (const key of required) if (!catalog[key]) throw new Error(`Missing critical UI text: ${name}/${key}`);
  coverage.push({ locale: name.replace('.json', ''), translatedKeys: entries.length, totalKeys: Object.keys(baseline).length, fallbackKeys: Object.keys(baseline).filter(key => !(key in catalog)) });
}
fs.mkdirSync(path.join(root, 'artifacts'), { recursive: true });
fs.writeFileSync(path.join(root, 'artifacts/localization-coverage.json'), JSON.stringify(coverage, null, 2));
console.log(`PASS ${coverage.length} resource catalogs: keys, required safety text and placeholders`);

const siteDir = path.join(root, 'site');
const html = fs.readFileSync(path.join(siteDir, 'index.html'), 'utf8');
for (const match of html.matchAll(/(?:href|src)="([^"#?:]+)"/g)) {
  if (!fs.existsSync(path.join(siteDir, match[1]))) throw new Error(`Missing site asset: ${match[1]}`);
}
if (!html.includes('id="main"') || !html.includes('class="skip"')) throw new Error('Missing accessible page navigation');

const webLocales = Object.keys(JSON.parse(fs.readFileSync(path.join(root, 'docs/content/locales.json'), 'utf8')));
const pageFor = locale => locale === 'en-US' ? 'index.html' : `index.${locale}.html`;
const sw = fs.readFileSync(path.join(siteDir, 'sw.js'), 'utf8');
for (const locale of webLocales) {
  const page = pageFor(locale);
  const pagePath = path.join(siteDir, page);
  if (!fs.existsSync(pagePath)) throw new Error(`Missing localized Pages document: ${page}`);
  const localized = fs.readFileSync(pagePath, 'utf8');
  if (!localized.includes(`<html lang="${locale}"`)) throw new Error(`Wrong lang metadata: ${page}`);
  if (!sw.includes(`./${page}`)) throw new Error(`Localized page missing from offline shell: ${page}`);
}
for (const requiredSiteFile of ['404.html','manifest.webmanifest','robots.txt','sitemap.xml','.well-known/security.txt']) {
  if (!fs.existsSync(path.join(siteDir, requiredSiteFile))) throw new Error(`Missing Pages platform file: ${requiredSiteFile}`);
}

const manifest = JSON.parse(fs.readFileSync(path.join(siteDir, 'manifest.webmanifest'), 'utf8'));
if (manifest.id !== '/PermissionScope/') throw new Error('PWA manifest must keep a stable explicit app id');
if (manifest.start_url !== './' || manifest.scope !== './') throw new Error('Unexpected PWA start URL or scope');
if (!Array.isArray(manifest.screenshots) || manifest.screenshots.length < 1) throw new Error('PWA manifest has no install preview screenshot');
for (const screenshot of manifest.screenshots) {
  if (!screenshot.src || !screenshot.sizes || !screenshot.type || !screenshot.label) throw new Error('Incomplete PWA screenshot metadata');
  if (!fs.existsSync(path.join(siteDir, screenshot.src))) throw new Error(`Missing PWA screenshot asset: ${screenshot.src}`);
  if (!sw.includes(`./${screenshot.src}`)) throw new Error(`PWA screenshot missing from offline shell: ${screenshot.src}`);
}
if (!sw.includes('navigationPreload.enable()')) throw new Error('Service worker navigation preload is not enabled');
if (!sw.includes("CACHE='permissionscope-shell-v4'")) throw new Error('Unexpected service worker cache generation');
console.log(`PASS local website assets, ${webLocales.length} localized Pages, stable PWA identity, install metadata, offline parity and platform metadata`);
