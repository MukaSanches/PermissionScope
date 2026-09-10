import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
const root=path.resolve(import.meta.dirname,'..');
const locales=JSON.parse(fs.readFileSync(path.join(root,'docs/content/locales.json'),'utf8'));
const manifest=JSON.parse(fs.readFileSync(path.join(root,'docs/screenshots/manifest.json'),'utf8').replace(/^\uFEFF/,''));
const scenes=['home','analyze','access','access-path','compare','simulation','technical','unknown'];
for(const locale of Object.keys(locales)) {
  for(const scene of scenes) if(!manifest.captures.some(c=>c.locale===locale&&c.scene===scene&&c.theme==='Light'))throw Error(`Missing capture ${locale}/${scene}`);
  if(!manifest.captures.some(c=>c.locale===locale&&c.scene==='access'&&c.theme==='Dark'))throw Error(`Missing dark capture ${locale}`);
  const readme=fs.readFileSync(path.join(root,locale==='en-US'?'README.md':`README.${locale}.md`),'utf8');
  if(!readme.includes(`docs/screenshots/${locale}/access-light.png`))throw Error(`README screenshot locale mismatch: ${locale}`);
  if((readme.match(/^## /gm)||[]).length!==9)throw Error(`README section parity: ${locale}`);
}
for(const capture of manifest.captures) {
  const file=path.resolve(root,'docs/screenshots',capture.path);
  if(!file.startsWith(path.join(root,'docs/screenshots')+path.sep))throw Error('Invalid manifest path');
  const bytes=fs.readFileSync(file);
  if(crypto.createHash('sha256').update(bytes).digest('hex').toUpperCase()!==capture.sha256)throw Error('Image hash mismatch: '+capture.path);
  if(bytes.readUInt32BE(16)!==capture.width||bytes.readUInt32BE(20)!==capture.height)throw Error('Image dimensions mismatch');
  if(capture.fixture!=='permissionscope-demo-v1')throw Error('Fixture version mismatch');
}
function files(dir){return fs.readdirSync(dir,{withFileTypes:true}).flatMap(d=>d.isDirectory()?files(path.join(dir,d.name)):[path.join(dir,d.name)]);}
const documents=[...fs.readdirSync(root).filter(f=>f.endsWith('.md')).map(f=>path.join(root,f)),...files(path.join(root,'docs')).filter(f=>f.endsWith('.md'))];
for(const file of documents){
 const text=fs.readFileSync(file,'utf8');
 for(const match of text.matchAll(/!?\[[^\]]*\]\(([^)]+)\)/g)){
   const target=match[1].split('#')[0];
   if(!target||/^[a-z]+:/i.test(target))continue;
   if(!fs.existsSync(path.resolve(path.dirname(file),target)))throw Error(`Broken local link: ${path.relative(root,file)} -> ${target}`);
 }
 if(/C:\\Users\\|gh[pousr]_[A-Za-z0-9]{20,}|github_pat_[A-Za-z0-9_]+/.test(text))throw Error('Private path or credential pattern: '+file);
}
const fixture=JSON.parse(fs.readFileSync(path.join(root,'samples/reports/permissionscope-demo.json'),'utf8').replace(/^\uFEFF/,''));
const serialized=JSON.stringify(fixture);
if(!serialized.includes('permissionscope-demo-v1')||/C:\\\\Users\\\\/.test(serialized))throw Error('Unsafe sample fixture');
for(const sid of serialized.match(/S-1-5-21-[0-9-]+/g)||[])if(!sid.startsWith('S-1-5-21-111111111-222222222-333333333-'))throw Error('Non-fixture account SID');
console.log(`PASS ${documents.length} documents, ${manifest.captures.length} image hashes, eight locale mappings and synthetic SID allowlist`);
