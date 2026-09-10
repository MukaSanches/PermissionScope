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

const html = fs.readFileSync(path.join(root, 'site/index.html'), 'utf8');
for (const match of html.matchAll(/(?:href|src)="([^"#?:]+)"/g)) {
  if (!fs.existsSync(path.join(root, 'site', match[1]))) throw new Error(`Missing site asset: ${match[1]}`);
}
if (!html.includes('id="main"') || !html.includes('class="skip"')) throw new Error('Missing accessible page navigation');
console.log('PASS local website assets and primary accessibility landmarks');
