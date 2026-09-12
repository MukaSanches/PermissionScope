import fs from 'node:fs';
import path from 'node:path';

const root=path.resolve(import.meta.dirname,'..');
const site=path.join(root,'site');
const check=process.argv.includes('--check');
const pages=['index.html','index.pt-BR.html','index.es.html','index.fr.html','index.de.html','index.ar.html','index.ja.html','index.zh-Hans.html'];
const manifestLocales=['pt-BR','es','fr','de','ar','ja','zh-Hans'];
const skipLabels={
  'index.html':'Skip to main content',
  'index.pt-BR.html':'Pular para o conteúdo principal',
  'index.es.html':'Saltar al contenido principal',
  'index.fr.html':'Aller au contenu principal',
  'index.de.html':'Zum Hauptinhalt springen',
  'index.ar.html':'تخطي إلى المحتوى الرئيسي',
  'index.ja.html':'メインコンテンツへ移動',
  'index.zh-Hans.html':'跳到主要内容'
};
const languageLabels={
  'index.html':'Language',
  'index.pt-BR.html':'Idioma',
  'index.es.html':'Idioma',
  'index.fr.html':'Langue',
  'index.de.html':'Sprache',
  'index.ar.html':'اللغة',
  'index.ja.html':'言語',
  'index.zh-Hans.html':'语言'
};
const languageLinks=[
  ['index.html','English'],
  ['index.pt-BR.html','Português'],
  ['index.es.html','Español'],
  ['index.fr.html','Français'],
  ['index.de.html','Deutsch'],
  ['index.ar.html','العربية'],
  ['index.ja.html','日本語'],
  ['index.zh-Hans.html','简体中文']
];

const props=fs.readFileSync(path.join(root,'Directory.Build.props'),'utf8');
const versionMatch=props.match(/<Version>([^<]+)<\/Version>/);
if(!versionMatch) throw new Error('Directory.Build.props does not declare <Version>.');
const releaseVersion=versionMatch[1].trim();
if(!/^\d+\.\d+\.\d+(?:[-+][0-9A-Za-z.-]+)?$/.test(releaseVersion)) throw new Error(`Unsupported project version: ${releaseVersion}`);

const manifest='<link rel="manifest" href="manifest.webmanifest">';
const appleTouchIcon='<link rel="apple-touch-icon" href="apple-touch-icon.png">';
const platformStyle='<link rel="stylesheet" href="responsive.css">';
const progressiveStyle='<link rel="stylesheet" href="progressive.css">';
const noscriptStyle='<noscript><link rel="stylesheet" href="noscript.css"></noscript>';
const enhancementStylePreloads=['./premium.css','./device.css','./experience.css'].map(href=>`<link rel="preload" href="${href}" as="style">`).join('');
const releaseScript='<script src="release-sync.js" defer></script>';
const platformScript='<script src="platform.js" defer></script>';
const theme='<meta name="theme-color" content="#17315C">';
const referrer='<meta name="referrer" content="no-referrer">';
const contentSecurityPolicy='<meta http-equiv="Content-Security-Policy" content="default-src \'self\'; img-src \'self\'; script-src \'self\'; style-src \'self\'; connect-src \'none\'; object-src \'none\'; base-uri \'none\'; form-action \'none\'">';
const xDefault='<link rel="alternate" hreflang="x-default" href="https://mukasanches.github.io/PermissionScope/index.html">';
const ogType='<meta property="og:type" content="website">';

function languageFallback(file){
  const label=languageLabels[file];
  if(!label) throw new Error(`Missing localized language label for ${file}`);
  const links=languageLinks.map(([href,name])=>`<a href="${href}"${href===file?' aria-current="page"':''}>${name}</a>`).join('');
  return `<noscript><nav class="language-fallback" aria-label="${label}">${links}</nav></noscript>`;
}

function enhanced(source,file){
  let html=source;
  const skipLabel=skipLabels[file];
  if(!skipLabel) throw new Error(`Missing localized skip-navigation label for ${file}`);
  const fallback=languageFallback(file);
  html=html.replace(/<html([^>]*)\sdata-release-version="[^"]*"([^>]*)>/,`<html$1 data-release-version="${releaseVersion}"$2>`);
  if(!html.includes('data-release-version=')) html=html.replace(/<html([^>]*)>/,`<html$1 data-release-version="${releaseVersion}">`);
  if(!html.includes(referrer)) html=html.replace('<meta name="viewport" content="width=device-width,initial-scale=1">',`<meta name="viewport" content="width=device-width,initial-scale=1">\n${referrer}`);
  if(!html.includes(theme)) html=html.replace('<meta name="description"',`${theme}\n<meta name="description"`);
  if(/<meta http-equiv="Content-Security-Policy" content="[^"]*">/.test(html)){
    html=html.replace(/<meta http-equiv="Content-Security-Policy" content="[^"]*">/,contentSecurityPolicy);
  }else{
    html=html.replace('<meta name="description"',`${contentSecurityPolicy}\n<meta name="description"`);
  }
  if(!html.includes(xDefault)) html=html.replace(/(<link rel="alternate" hreflang="zh-Hans"[^>]+>)/,`$1\n${xDefault}`);
  const canonicalMatch=html.match(/<link rel="canonical" href="([^"]+)">/);
  if(!canonicalMatch) throw new Error(`Missing canonical URL in ${file}`);
  const canonicalUrl=canonicalMatch[1];
  const imageAltMatch=html.match(/<figure class="product"><img[^>]*\salt="([^"]*)"/);
  if(!imageAltMatch || !imageAltMatch[1].trim()) throw new Error(`Missing localized product image alternative text in ${file}`);
  const ogUrl=`<meta property="og:url" content="${canonicalUrl}">`;
  const ogImageAlt=`<meta property="og:image:alt" content="${imageAltMatch[1]}">`;
  html=html.replace(/<meta property="og:type" content="[^"]*">/g,'');
  html=html.replace(/<meta property="og:url" content="[^"]*">/g,'');
  html=html.replace(/<meta property="og:image:alt" content="[^"]*">/g,'');
  html=html.replace(/(<meta property="og:title" content="[^"]*">)/,`$1${ogType}${ogUrl}`);
  html=html.replace(/(<meta property="og:image" content="[^"]*">)/,`$1${ogImageAlt}`);
  if(!html.includes(appleTouchIcon)) html=html.replace('<link rel="icon" href="logo.svg" type="image/svg+xml">',`<link rel="icon" href="logo.svg" type="image/svg+xml">${appleTouchIcon}`);
  if(!html.includes(manifest)) html=html.replace(appleTouchIcon,`${appleTouchIcon}${manifest}`);
  if(!html.includes(platformStyle)) html=html.replace('<link rel="stylesheet" href="style.css">',`<link rel="stylesheet" href="style.css">${platformStyle}`);
  if(!html.includes(progressiveStyle)) html=html.replace(platformStyle,`${platformStyle}${progressiveStyle}`);
  if(!html.includes(noscriptStyle)) html=html.replace(progressiveStyle,`${progressiveStyle}${noscriptStyle}`);
  html=html.replace(/<link rel="preload" href="(?:premium|device|experience)\.css" as="style">/g,'');
  if(!html.includes(enhancementStylePreloads)) html=html.replace(noscriptStyle,`${noscriptStyle}${enhancementStylePreloads}`);
  if(!html.includes(releaseScript)) html=html.replace('<script src="site.js" defer></script>',`<script src="site.js" defer></script>${releaseScript}`);
  if(!html.includes(platformScript)) html=html.replace(releaseScript,`${releaseScript}${platformScript}`);
  html=html.replaceAll('href="#download-premium"','href="#download"');
  html=html.replace(/<a class="skip" href="#main">[^<]*<\/a>/,`<a class="skip" href="#main">${skipLabel}</a>`);
  html=html.replace(/<noscript><nav class="language-fallback"[\s\S]*?<\/nav><\/noscript>/g,'');
  html=html.replace('</label></header>',`</label>${fallback}</header>`);
  return html;
}

function pageIds(html){
  return new Set([...html.matchAll(/\sid="([^"]+)"/g)].map(match=>match[1]));
}

function validateInternalAnchors(html,file){
  const ids=pageIds(html);
  for(const match of html.matchAll(/href="#([^"]+)"/g)){
    const fragment=match[1];
    if(!ids.has(fragment)) throw new Error(`Broken internal anchor in ${file}: #${fragment}`);
  }
}

function validateOpenGraph(html,file){
  const canonical=html.match(/<link rel="canonical" href="([^"]+)">/)?.[1];
  const ogUrl=html.match(/<meta property="og:url" content="([^"]+)">/)?.[1];
  const ogImageAlt=html.match(/<meta property="og:image:alt" content="([^"]+)">/)?.[1];
  if(!canonical) throw new Error(`Missing canonical URL in ${file}`);
  if(!html.includes(ogType)) throw new Error(`Open Graph type must be website in ${file}`);
  if(ogUrl!==canonical) throw new Error(`Open Graph URL must match the canonical URL in ${file}`);
  if(!ogImageAlt?.trim()) throw new Error(`Open Graph image must include localized alternative text in ${file}`);
}

function validateManifestShortcuts(html,file,manifestData){
  const ids=pageIds(html);
  const shortcuts=Array.isArray(manifestData.shortcuts)?manifestData.shortcuts:[];
  for(const shortcut of shortcuts){
    if(!shortcut || typeof shortcut.url!=='string') throw new Error('Every manifest shortcut must declare a string url');
    if(typeof shortcut.description!=='string' || !shortcut.description.trim()) throw new Error('Every manifest shortcut must declare a useful description for assistive technology');
    if(!shortcut.description_localized || typeof shortcut.description_localized!=='object' || Array.isArray(shortcut.description_localized)){
      throw new Error('Every manifest shortcut must localize its description across supported languages');
    }
    for(const locale of manifestLocales){
      const localized=shortcut.description_localized[locale];
      if(typeof localized!=='string' || !localized.trim()) throw new Error(`Manifest shortcut is missing a localized description for ${locale}`);
    }
    const resolved=new URL(shortcut.url,'https://mukasanches.github.io/PermissionScope/manifest.webmanifest');
    if(resolved.origin!=='https://mukasanches.github.io' || !resolved.pathname.startsWith('/PermissionScope/')){
      throw new Error(`Manifest shortcut escapes PermissionScope scope: ${shortcut.url}`);
    }
    const fragment=decodeURIComponent(resolved.hash.slice(1));
    if(fragment && !ids.has(fragment)) throw new Error(`Broken manifest shortcut in ${file}: #${fragment}`);
  }
}

const manifestPath=path.join(site,'manifest.webmanifest');
if(!fs.existsSync(manifestPath)) throw new Error('Missing site/manifest.webmanifest');
let manifestData;
try{
  manifestData=JSON.parse(fs.readFileSync(manifestPath,'utf8'));
}catch(error){
  throw new Error(`Invalid site/manifest.webmanifest: ${error.message}`);
}
const displayOverrides=Array.isArray(manifestData.display_override)?manifestData.display_override:[];
if(displayOverrides.includes('window-controls-overlay')){
  throw new Error('Do not opt into window-controls-overlay until the site implements and tests titlebar-area-* safe layout handling.');
}

const appleTouchIconPath=path.join(site,'apple-touch-icon.png');
const officialMarkPath=path.join(root,'docs','brand','project-avatar.png');
if(!fs.existsSync(appleTouchIconPath)) throw new Error('Missing site/apple-touch-icon.png for Safari/iOS home-screen identity');
if(!fs.existsSync(officialMarkPath)) throw new Error('Missing official docs/brand/project-avatar.png');
if(!fs.readFileSync(appleTouchIconPath).equals(fs.readFileSync(officialMarkPath))) throw new Error('Safari/iOS touch icon must reuse the single official PermissionScope mark');

const noscriptCssPath=path.join(site,'noscript.css');
if(!fs.existsSync(noscriptCssPath)) throw new Error('Missing site/noscript.css for script-disabled language navigation');

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
  const result=enhanced(source,file);
  validateInternalAnchors(result,file);
  validateOpenGraph(result,file);
  validateManifestShortcuts(result,file,manifestData);
  if(check){
    if(result!==source) throw new Error(`Stale enhanced Page: ${file}`);
    if(!source.includes(`data-release-version="${releaseVersion}"`)) throw new Error(`Release version drift in ${file}`);
    if(!source.includes(appleTouchIcon)) throw new Error(`Missing Safari/iOS touch icon metadata in ${file}`);
    if(!source.includes(noscriptStyle)) throw new Error(`Missing no-script stylesheet gate in ${file}`);
    if(!source.includes(languageFallback(file))) throw new Error(`Missing localized no-script language navigation in ${file}`);
    if(!source.includes(enhancementStylePreloads)) throw new Error(`Missing early enhancement stylesheet discovery in ${file}`);
    if(!source.includes(releaseScript)) throw new Error(`Missing release sync script in ${file}`);
    if(!source.includes(contentSecurityPolicy)) throw new Error(`Content Security Policy must disable legacy object/embed content in ${file}`);
    if(!source.includes(`<a class="skip" href="#main">${skipLabels[file]}</a>`)) throw new Error(`Incorrect localized skip-navigation label in ${file}`);
  } else {
    fs.writeFileSync(target,result);
  }
}

console.log(`PASS ${check?'verified':'enhanced'} privacy/PWA/platform integration, hardened object/embed CSP, complete localized Open Graph metadata, no-script language navigation, early enhancement stylesheet discovery, conservative installed-window behavior, localized shortcut and skip-navigation accessibility, Safari/iOS touch identity, page and manifest shortcuts, scoped offline-cache ownership, disclosure metadata and release ${releaseVersion} parity across ${pages.length} localized Pages`);