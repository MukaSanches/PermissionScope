import fs from 'node:fs';
import path from 'node:path';
import { chromium } from '@playwright/test';

const root=path.resolve(import.meta.dirname,'..');
const locales=JSON.parse(fs.readFileSync(path.join(root,'docs/content/locales.json'),'utf8'));
const out=path.join(root,'docs/handbooks');
fs.mkdirSync(out,{recursive:true});

const esc=s=>String(s).replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('>','&gt;');
const browser=await chromium.launch({headless:true});
const index=[];
for(const [locale,t] of Object.entries(locales)){
  const dir=locale==='ar'?'rtl':'ltr';
  const title=`PermissionScope ${locale} Handbook`;
  const sections=[
    [t.start,`<ol>${t.steps.map(x=>`<li>${esc(x)}</li>`).join('')}</ol>`],
    [t.learn,`<p>${esc(t.meaning)}</p>`],
    [t.why,`<p>${esc(t.technical)}</p>`],
    [t.features,`<p>${esc(t.featureText)}</p><pre>permissionscope-cli.exe demo --output ./demo-reports\npermissionscope-cli.exe explain "C:\\Finance"\npermissionscope-cli.exe scan "C:\\Finance" --save</pre>`],
    [t.safety,`<p>${esc(t.scope)}</p><p>${esc(t.apply)}</p>`],
    [t.install,`<p>${esc(t.installText)}</p>`],
    [t.build,`<p>${esc(t.buildText)}</p>`],
    [t.help,`<p>${esc(t.helpText)}</p>`]
  ];
  const html=`<!doctype html><html lang="${locale}" dir="${dir}"><head><meta charset="utf-8"><style>
  @page{size:A4;margin:18mm 16mm 18mm}*{box-sizing:border-box}body{font-family:"Segoe UI",Arial,sans-serif;color:#17191d;line-height:1.55;margin:0}.cover{height:250mm;display:flex;flex-direction:column;justify-content:space-between;background:linear-gradient(145deg,#17315c,#2764e7);color:white;padding:24mm 18mm;border-radius:8mm}.mark{font-size:18px;font-weight:700;letter-spacing:.02em}.kicker{font-size:11px;letter-spacing:.16em;text-transform:uppercase;opacity:.8}.cover h1{font-size:42px;line-height:1.05;letter-spacing:-.04em;margin:10mm 0 5mm;max-width:150mm}.cover p{font-size:17px;max-width:145mm;opacity:.92}.meta{font-size:11px;opacity:.8}.pagebreak{page-break-after:always}h2{font-size:25px;line-height:1.12;color:#17315c;margin:0 0 6mm;padding-top:2mm}section{page-break-inside:avoid;margin-bottom:13mm}p,li{font-size:11.5px}ol{padding-inline-start:6mm}pre{white-space:pre-wrap;background:#f2f5fa;border:1px solid #d7dce3;border-radius:3mm;padding:5mm;font-size:9.5px;color:#17315c}.trust{display:grid;grid-template-columns:1fr 1fr;gap:4mm;margin:8mm 0}.trust div{border:1px solid #d7dce3;border-radius:3mm;padding:5mm}.trust b{display:block;color:#2764e7}.footer{font-size:9px;color:#565e6a;border-top:1px solid #d7dce3;padding-top:4mm;margin-top:10mm}a{color:#2764e7}
  </style></head><body>
  <div class="cover"><div><div class="mark">PermissionScope</div><div class="kicker">Official handbook · 1.0.0 · ${locale}</div><h1>${esc(t.tagline)}</h1><p>${esc(t.featureText)}</p></div><div class="meta">Created by Samuel Sanches · Apache-2.0 · Local-first · No telemetry</div></div>
  <div class="pagebreak"></div>
  <section><div class="kicker">Trust Center</div><h2>PermissionScope</h2><div class="trust"><div><b>Local-first</b>No application backend or account.</div><div><b>Windows Authz</b>Permission evaluation uses Windows authorization APIs.</div><div><b>Open source</b>Source, security model and release process are inspectable.</div><div><b>Integrity</b>Releases include SHA-256 checksums and SBOM data.</div></div></section>
  ${sections.map(([h,b])=>`<section><h2>${esc(h)}</h2>${b}</section>`).join('')}
  <div class="footer">PermissionScope 1.0.0 · github.com/MukaSanches/PermissionScope · mukasanches.github.io/PermissionScope/</div>
  </body></html>`;
  const page=await browser.newPage();
  await page.setContent(html,{waitUntil:'load'});
  const file=`PermissionScope-Handbook-${locale}.pdf`;
  await page.pdf({path:path.join(out,file),format:'A4',printBackground:true,preferCSSPageSize:true});
  await page.close();
  index.push(`- [${t.name}](${file})`);
}
await browser.close();
fs.writeFileSync(path.join(out,'README.md'),`# PermissionScope official handbooks\n\nProfessional, reproducibly generated PDF handbooks from the project's maintained localization source.\n\n${index.join('\n')}\n\nThese PDFs complement — and do not replace — the canonical technical documentation in the repository.\n`);
console.log(`PASS generated ${index.length} multilingual handbooks`);
