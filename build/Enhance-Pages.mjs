import fs from 'node:fs';
import path from 'node:path';

const root=path.resolve(import.meta.dirname,'..');
const site=path.join(root,'site');
const check=process.argv.includes('--check');
const pages=['index.html','index.pt-BR.html','index.es.html','index.fr.html','index.de.html','index.ar.html','index.ja.html','index.zh-Hans.html'];

const manifest='<link rel="manifest" href="manifest.webmanifest">';
const platformStyle='<link rel="stylesheet" href="responsive.css">';
const platformScript='<script src="platform.js" defer></script>';
const theme='<meta name="theme-color" content="#17315C">';
const xDefault='<link rel="alternate" hreflang="x-default" href="https://mukasanches.github.io/PermissionScope/index.html">';

function enhanced(source){
  let html=source;
  if(!html.includes(theme)) html=html.replace('<meta name="description"',`${theme}\n<meta name="description"`);
  if(!html.includes(xDefault)) html=html.replace(/(<link rel="alternate" hreflang="zh-Hans"[^>]+>)/,`$1\n${xDefault}`);
  if(!html.includes(manifest)) html=html.replace('<link rel="icon" href="logo.svg" type="image/svg+xml">',`<link rel="icon" href="logo.svg" type="image/svg+xml">${manifest}`);
  if(!html.includes(platformStyle)) html=html.replace('<link rel="stylesheet" href="style.css">',`<link rel="stylesheet" href="style.css">${platformStyle}`);
  if(!html.includes(platformScript)) html=html.replace('<script src="site.js" defer></script>',`<script src="site.js" defer></script>${platformScript}`);
  return html;
}

for(const file of pages){
  const target=path.join(site,file);
  const source=fs.readFileSync(target,'utf8');
  const result=enhanced(source);
  if(check){
    if(result!==source) throw new Error(`Stale enhanced Page: ${file}`);
  } else {
    fs.writeFileSync(target,result);
  }
}

console.log(`PASS ${check?'verified':'enhanced'} PWA/platform integration across ${pages.length} localized Pages`);
