import fs from 'node:fs';
import path from 'node:path';

const root=path.resolve(import.meta.dirname,'..');
const props=fs.readFileSync(path.join(root,'Directory.Build.props'),'utf8');
const version=props.match(/<Version>([^<]+)<\/Version>/)?.[1]?.trim();
if(!version)throw new Error('Could not resolve project Version from Directory.Build.props');

const site=fs.readFileSync(path.join(root,'site/site.js'),'utf8');
const expected=[
  `PermissionScope-${version}-x64-Setup.exe`,
  `PermissionScope-${version}-arm64-Setup.exe`,
  `PermissionScope-${version}-win-x64.zip`,
  `PermissionScope-${version}-win-arm64.zip`,
  'SHA256SUMS.txt'
];

if(!site.includes(`const releaseVersion='${version}'`)){
  throw new Error(`Website download version is stale. Expected ${version}.`);
}
for(const asset of expected){
  if(!site.includes(asset.replace(version,'${releaseVersion}')) && !site.includes(asset)){
    throw new Error(`Website does not reference required release asset: ${asset}`);
  }
}
if(/PermissionScope-1\.0\.0-(?:x64|arm64|win-)/.test(site)){
  throw new Error('Website still contains obsolete 1.0.0 direct-download assets.');
}
if(!site.includes("device.className='ps-device'")){
  throw new Error('3D product device wrapper is missing from the website.');
}
const sw=fs.readFileSync(path.join(root,'site/sw.js'),'utf8');
if(!sw.includes("'./device.css'")) throw new Error('3D device stylesheet is missing from the offline shell.');
console.log(`PASS website direct downloads match PermissionScope ${version} and 3D product presentation is wired`);
