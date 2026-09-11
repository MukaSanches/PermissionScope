import fs from 'node:fs';
import path from 'node:path';

const root=path.resolve(import.meta.dirname,'..');
const props=fs.readFileSync(path.join(root,'Directory.Build.props'),'utf8');
const version=props.match(/<Version>([^<]+)<\/Version>/)?.[1]?.trim();
if(!version)throw new Error('Could not resolve project Version from Directory.Build.props');

const sources=['site.js','future.js'].map(name=>({name,content:fs.readFileSync(path.join(root,'site',name),'utf8')}));
const primary=sources.find(x=>x.name==='site.js').content;
const future=sources.find(x=>x.name==='future.js').content;
const all=sources.map(x=>x.content).join('\n');
const expected=[
  `PermissionScope-${version}-x64-Setup.exe`,
  `PermissionScope-${version}-arm64-Setup.exe`,
  `PermissionScope-${version}-win-x64.zip`,
  `PermissionScope-${version}-win-arm64.zip`,
  'SHA256SUMS.txt'
];

if(!primary.includes(`const releaseVersion='${version}'`))throw new Error(`Primary website download version is stale. Expected ${version}.`);
if(!future.includes(`const releaseVersion='${version}'`))throw new Error(`Command Center download version is stale. Expected ${version}.`);
for(const asset of expected){
  if(!all.includes(asset.replace(version,'${releaseVersion}'))&&!all.includes(asset))throw new Error(`Website does not reference required release asset: ${asset}`);
}
const obsolete=[...all.matchAll(/PermissionScope-(\d+\.\d+\.\d+)-(?:x64|arm64|win-)/g)].map(match=>match[1]).filter(v=>v!==version);
if(obsolete.length)throw new Error(`Website contains obsolete direct-download versions: ${[...new Set(obsolete)].join(', ')}`);
if(!primary.includes("device.className='ps-device'"))throw new Error('3D product device wrapper is missing from the website.');

for(const required of ['site/atelier.css','site/atelier.js','site/device.css']){
  if(!fs.existsSync(path.join(root,required)))throw new Error(`Missing premium website asset: ${required}`);
}
const sw=fs.readFileSync(path.join(root,'site/sw.js'),'utf8');
for(const asset of ['./device.css','./atelier.css','./atelier.js'])if(!sw.includes(`'${asset}'`))throw new Error(`Premium website asset missing from offline shell: ${asset}`);
if(!future.includes("atelier.href='atelier.css'")||!future.includes("atelierScript.src='atelier.js'"))throw new Error('Final art-direction layer is not chained into the website.');
console.log(`PASS all website download surfaces match PermissionScope ${version}; premium workstation layer and offline cache are wired`);
