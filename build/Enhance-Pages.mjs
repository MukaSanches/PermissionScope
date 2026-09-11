import fs from 'node:fs';
import path from 'node:path';

const root=path.resolve(import.meta.dirname,'..');
const site=path.join(root,'site');
const check=process.argv.includes('--check');
const pages=['index.html','index.pt-BR.html','index.es.html','index.fr.html','index.de.html','index.ar.html','index.ja.html','index.zh-Hans.html'];

const props=fs.readFileSync(path.join(root,'Directory.Build.props'),'utf8');
const versionMatch=props.match(/<Version>([^<]+)<\/Version>/);
if(!versionMatch) throw new Error('Directory.Build.props does not declare <Version>.');
const releaseVersion=versionMatch[1].trim();
if(!/^\d+\.\d+\.\d+(?:[-+][0-9A-Za-z.-]+)?$/.test(releaseVersion)) throw new Error(`Unsupported project version: ${releaseVersion}`);

const manifest='<link rel="manifest" href="manifest.webmanifest">';
const platformStyle='<link rel="stylesheet" href="responsive.css">';
const progressiveStyle='<link rel="stylesheet" href="progressive.css">';
const releaseScript='<script src="release-sync.js" defer></script>';
const platformScript='<script src="platform.js" defer></script>';
const theme='<meta name="theme-color" content="#17315C">';
const referrer='<meta name="referrer" content="no-referrer">';
const xDefault='<link rel="alternate" hreflang="x-default" href="https://mukasanches.github.io/PermissionScope/index.html">';

function enhanced(source){
  let html=source;
  html=html.replace(/<html([^>]*)\sdata-release-version="[^"]*"([^>]*)>/,`<html$1 data-release-version="${releaseVersion}"$2>`);
  if(!html.includes('data-release-version=')) html=html.replace(/<html([^>]*)>/,`<html$1 data-release-version="${releaseVersion}">`);
  if(!html.includes(referrer)) html=html.replace('<meta name="viewport" content="width=device-width,initial-scale=1">',`<meta name="viewport" content="width=device-width,initial-scale=1">\n${referrer}`);
  if(!html.includes(theme)) html=html.replace('<meta name="description"',`${theme}\n<meta name="description"`);
  if(!html.includes(xDefault)) html=html.replace(/(<link rel="alternate" hreflang="zh-Hans"[^>]+>)/,`$1\n${xDefault}`);
  if(!html.includes(manifest)) html=html.replace('<link rel="icon" href="logo.svg" type="image/svg+xml">',`<link rel="icon" href="logo.svg" type="image/svg+xml">${manifest}`);
  if(!html.includes(platformStyle)) html=html.replace('<link rel="stylesheet" href="style.css">',`<link rel="stylesheet" href="style.css">${platformStyle}`);
  if(!html.includes(progressiveStyle)) html=html.replace(platformStyle,`${platformStyle}${progressiveStyle}`);
  if(!html.includes(releaseScript)) html=html.replace('<script src="site.js" defer></script>',`<script src="site.js" defer></script>${releaseScript}`);
  if(!html.includes(platformScript)) html=html.replace(releaseScript,`${releaseScript}${platformScript}`);
  html=html.replaceAll('href="#download-premium"','href="#download"');
  return html;
}

function validateInternalAnchors(html,file){
  const ids=new Set([...html.matchAll(/\sid="([^"]+)"/g)].map(match=>match[1]));
  for(const match of html.matchAll(/href="#([^"]+)"/g)){
    const fragment=match[1];
    if(!ids.has(fragment)) throw new Error(`Broken internal anchor in ${file}: #${fragment}`);
  }
}

const releaseSyncPath=path.join(site,'release-sync.js');
if(!fs.existsSync(releaseSyncPath)) throw new Error('Missing site/release-sync.js');

const securityTxtPath=path.join(site,'.well-known','security.txt');
if(!fs.existsSync(securityTxtPath)) throw new Error('Missing project-scoped site/.well-known/security.txt');
const securityTxt=fs.readFileSync(securityTxtPath,'utf8');
if(!/^Contact:\s+\S+/m.test(securityTxt)) throw new Error('security.txt must declare a Contact field');
const expiresMatch=securityTxt.match(/^Expires:\s+(\S+)\s*$/m);
if(!expiresMatch || Number.isNaN(Date.parse(expiresMatch[1]))) throw new Error('security.txt must declare a valid Expires field');
if(Date.parse(expiresMatch[1]) <= Date.now()) throw new Error('security.txt has expired and must be refreshed or removed');
if(!securityTxt.includes('Policy: https://github.com/MukaSanches/PermissionScope/blob/main/SECURITY.md')) throw new Error('security.txt must point to the maintained repository security policy');
if(/^Canonical:/m.test(securityTxt)) throw new Error('Project-scoped GitHub Pages security.txt must not claim RFC 9116 origin-root canonical placement');

const serviceWorkerPath=path.join(site,'sw.js');
if(!fs.existsSync(serviceWorkerPath)) throw new Error('Missing site/sw.js');
const serviceWorker=fs.readFileSync(serviceWorkerPath,'utf8');
if(!serviceWorker.includes("const SHELL_PREFIX='permissionscope-shell-';")) throw new Error('Service worker cache ownership prefix is missing');
if(!serviceWorker.includes('keys.filter(key=>key.startsWith(SHELL_PREFIX)&&key!==CACHE)')) throw new Error('Service worker must delete only obsolete PermissionScope-owned caches');
if(serviceWorker.includes('keys.filter(key=>key!==CACHE)')) throw new Error('Service worker must not delete unrelated origin-wide caches');

for(const file of pages){
  const target=path.join(site,file);
  const source=fs.readFileSync(target,'utf8');
  const result=enhanced(source);
  validateInternalAnchors(result,file);
  if(check){
    if(result!==source) throw new Error(`Stale enhanced Page: ${file}`);
    if(!source.includes(`data-release-version="${releaseVersion}"`)) throw new Error(`Release version drift in ${file}`);
    if(!source.includes(releaseScript)) throw new Error(`Missing release sync script in ${file}`);
  } else {
    fs.writeFileSync(target,result);
  }
}

console.log(`PASS ${check?'verified':'enhanced'} privacy/PWA/platform integration, internal anchors, scoped offline-cache ownership, disclosure metadata and release ${releaseVersion} parity across ${pages.length} localized Pages`);