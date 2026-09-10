import fs from 'node:fs';
import path from 'node:path';
const root=path.resolve(import.meta.dirname,'..');
const languages=JSON.parse(fs.readFileSync(path.join(root,'docs/content/locales.json'),'utf8'));
const check=process.argv.includes('--check');
const repo='https://github.com/MukaSanches/PermissionScope';
const web='https://mukasanches.github.io/PermissionScope/';
const readme=l=>l==='en-US'?'README.md':`README.${l}.md`;
const page=l=>l==='en-US'?'index.html':`index.${l}.html`;
const esc=s=>s.replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('"','&quot;');
function write(file,text){const target=path.join(root,file);if(check){if(!fs.existsSync(target)||fs.readFileSync(target,'utf8').replace(/\r\n/g,'\n')!==text)throw Error('Stale generated content: '+file);}else{fs.mkdirSync(path.dirname(target),{recursive:true});fs.writeFileSync(target,text);}}
function copy(source,target){const a=path.join(root,source),b=path.join(root,target);if(check){if(!fs.existsSync(b)||!fs.readFileSync(a).equals(fs.readFileSync(b)))throw Error('Stale copy: '+target);}else{fs.mkdirSync(path.dirname(b),{recursive:true});fs.copyFileSync(a,b);}}
const cli=['./permissionscope-cli.exe demo --output ./demo-reports','./permissionscope-cli.exe explain "C:\\Finance"','./permissionscope-cli.exe scan "C:\\Finance" --save','./permissionscope-cli.exe export <snapshot-id> --format html --output report.html'].join('\n');
for(const [locale,t] of Object.entries(languages)){
 const links=Object.entries(languages).map(([l,v])=>`[${v.name}](${readme(l)})`).join(' · ');
 const image=(scene)=>`![${t.caption}](docs/screenshots/${locale}/${scene}-light.png)`;
 const blocks=[[t.learn,t.meaning],[t.why,t.technical+'\n\n'+image('access-path')],[t.features,t.featureText+'\n\n'+String.fromCharCode(96).repeat(3)+'powershell\n'+cli+'\n'+String.fromCharCode(96).repeat(3)],[t.safety,t.scope+'\n\n'+t.apply],[t.install,t.installText]];
 let md=`<!-- Generated from docs/content/locales.json by build/Build-Documentation.mjs. -->\n# PermissionScope\n\n${links}\n\n${t.tagline}\n\n[${t.download}](${repo}/releases) · [${t.guide}](docs/guides/${locale}.md) · [${t.sample}](${web}reports/permissionscope-demo.html)\n\n${image('access')}\n\n## ${t.start}\n\n${t.steps.map((s,i)=>`${i+1}. ${s}`).join('\n')}\n`;
 for(const [h,p] of blocks)md+=`\n## ${h}\n\n${p}\n`;
 md+=`\n## ${t.docs}\n\n- [${t.guide}](docs/guides/${locale}.md)\n- [${t.reference}](docs/access-model.md)\n- [${t.faq}](docs/faq.md)\n- [${t.glossary}](docs/glossary.md)\n- [${t.privacy}](docs/privacy.md)\n- [${t.status}](docs/release-status.md)\n\n## ${t.build}\n\n${t.buildText}\n\n[Development](docs/development.md)\n\n## ${t.help}\n\n${t.helpText}\n\n${t.created} · ssanches011@gmail.com · [Apache-2.0](LICENSE) · [Security](SECURITY.md)\n`;
 write(readme(locale),md);
 const scenes={home:t.steps[2],analyze:t.steps[3],access:t.meaning,'access-path':t.technical,compare:t.featureText,simulation:t.apply,technical:t.helpText,unknown:t.scope};
 let guide=`# PermissionScope · ${t.guide}\n\n[${t.docs}](../../${readme(locale)})\n\n${t.caption}\n`;
 for(const [scene,text] of Object.entries(scenes))guide+=`\n## ${scene}\n\n${text}\n\n![${text}](../screenshots/${locale}/${scene}-light.png)\n`;
 guide+=`\n[Development](../development.md) · [${t.faq}](../faq.md) · [${t.status}](../release-status.md)\n`;
 write(`docs/guides/${locale}.md`,guide);
 const options=Object.entries(languages).map(([l,v])=>`<option value="${page(l)}"${l===locale?' selected':''}>${v.name}</option>`).join('');
 const sections=[[t.learn,t.meaning],[t.why,t.technical],[t.features,t.featureText],[t.safety,t.scope],[t.help,t.helpText]];
 const html=`<!doctype html>
<html lang="${locale}" dir="${locale==='ar'?'rtl':'ltr'}"><head>
<meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<meta name="description" content="${esc(t.tagline)}"><meta name="color-scheme" content="light dark">
<meta http-equiv="Content-Security-Policy" content="default-src 'self'; img-src 'self'; script-src 'self'; style-src 'self'; connect-src 'none'; base-uri 'none'; form-action 'none'">
<title>PermissionScope — ${esc(t.tagline)}</title><link rel="canonical" href="${web}${page(locale)}">
${Object.keys(languages).map(l=>`<link rel="alternate" hreflang="${l}" href="${web}${page(l)}">`).join('\n')}
<meta property="og:title" content="PermissionScope"><meta property="og:description" content="${esc(t.tagline)}"><meta property="og:image" content="${web}screenshots/${locale}/access-light.png">
<link rel="icon" href="logo.svg" type="image/svg+xml"><link rel="stylesheet" href="style.css"><script src="site.js" defer></script></head>
<body><a class="skip" href="#main">${esc(t.start)}</a>
<header class="shell navigation"><a class="wordmark" href="${page(locale)}"><img src="logo.svg" width="30" height="34" alt="">PermissionScope</a><nav aria-label="${esc(t.docs)}"><a href="#evidence">${esc(t.why)}</a><a href="#download">${esc(t.download)}</a><a href="${repo}">GitHub</a></nav><label class="language"><span class="sr-only">Language</span><select id="language">${options}</select></label></header>
<main id="main"><section class="shell hero"><p class="eyebrow">PermissionScope · Windows · Apache-2.0</p><h1>${esc(t.tagline)}</h1><p class="lede">${esc(t.featureText)}</p><div class="actions"><a class="primary" href="#download">${esc(t.download)}</a><a href="reports/permissionscope-demo.html">${esc(t.sample)}</a></div><figure class="product"><img src="screenshots/${locale}/access-light.png" width="3744" height="2064" alt="${esc(t.caption)}" fetchpriority="high"><figcaption>${esc(t.caption)}</figcaption></figure></section>
<section class="shell section" id="evidence"><h2>${esc(t.start)}</h2><ol>${t.steps.map(s=>`<li>${esc(s)}</li>`).join('')}</ol><div class="features">${sections.map(([h,p])=>`<article><h3>${esc(h)}</h3><p>${esc(p)}</p></article>`).join('')}</div><p>${esc(t.apply)}</p><a href="${repo}/blob/main/docs/guides/${locale}.md">${esc(t.guide)} →</a></section>
<section class="shell section download-grid" id="download"><div><h2>${esc(t.install)}</h2><p>${esc(t.installText)}</p></div><div class="downloads"><a class="download-row" href="${repo}/releases">${esc(t.download)} →</a><a href="${repo}/blob/main/docs/release-status.md">${esc(t.status)}</a><p><a href="${repo}/blob/main/${readme(locale)}">${esc(t.reference)} / CLI</a></p></div></section>
<section class="shell section"><h2>${esc(t.docs)}</h2><div class="actions"><a href="${repo}/blob/main/docs/faq.md">${esc(t.faq)}</a><a href="${repo}/blob/main/docs/privacy.md">${esc(t.privacy)}</a>${['html','csv','json','xlsx','pdf'].map(f=>`<a href="reports/permissionscope-demo.${f}">${f.toUpperCase()}</a>`).join('')}</div></section></main>
<footer class="shell"><span>${esc(t.created)}</span><span>ssanches011@gmail.com · Apache-2.0</span><a href="${repo}/blob/main/SECURITY.md">Security</a></footer></body></html>
`;
 write('site/'+page(locale),html);
 copy(`docs/screenshots/${locale}/access-light.png`,`site/screenshots/${locale}/access-light.png`);
}
for(const ext of ['html','csv','json','xlsx','pdf'])copy(`samples/reports/permissionscope-demo.${ext}`,`site/reports/permissionscope-demo.${ext}`);
console.log('PASS '+(check?'verified':'generated')+' eight equivalent READMEs, visual guides and static localized pages');

