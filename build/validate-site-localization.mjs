import fs from 'node:fs';
import path from 'node:path';

const root=path.resolve(import.meta.dirname,'..');
const site=path.join(root,'site');
const locales=['en-US','pt-BR','es','fr','de','ar','ja','zh-Hans'];
const scripts={
  experience:fs.readFileSync(path.join(site,'experience.js'),'utf8'),
  future:fs.readFileSync(path.join(site,'future.js'),'utf8'),
  intelligence:fs.readFileSync(path.join(site,'intelligence.js'),'utf8'),
  atelier:fs.readFileSync(path.join(site,'atelier.js'),'utf8'),
  quality:fs.readFileSync(path.join(site,'quality.js'),'utf8')
};

for(const locale of locales){
  for(const [name,source] of Object.entries(scripts)){
    if(!source.includes(`'${locale}'`))throw new Error(`${name}.js is missing locale ${locale}`);
  }
  const filename=locale==='en-US'?'index.html':`index.${locale}.html`;
  const html=fs.readFileSync(path.join(site,filename),'utf8');
  if(!html.includes(`<html lang="${locale}"`))throw new Error(`${filename} has the wrong html lang`);
  if(locale==='ar'&&!html.includes('dir="rtl"'))throw new Error(`${filename} must use rtl`);
  if(locale!=='ar'&&!html.includes('dir="ltr"'))throw new Error(`${filename} must use ltr`);
  for(const required of ['id="evidence"','id="download-premium"','class="trust','class="privacy'])if(!html.includes(required))throw new Error(`${filename} is missing shared product content: ${required}`);
}

for(const token of ['t.chainKicker','t.steps.map','t.actions.map','t.verifyKicker'])if(!scripts.future.includes(token))throw new Error(`future.js is not rendering localized content through ${token}`);
for(const token of ['c.model','c.formatDesc','c.stateCodes'])if(!scripts.experience.includes(token))throw new Error(`experience.js is not rendering localized content through ${token}`);
if(!scripts.intelligence.includes('const answers={'))throw new Error('Assistant localized fallback answers are missing');
if(!scripts.intelligence.includes('Always answer in ${t.language}'))throw new Error('On-device assistant is not instructed to answer in the active locale');
for(const token of ['ps-desk-plane','ps-desk-keyboard','ps-desk-mouse','ps-desk-node','ps-desk-cable'])if(scripts.atelier.includes(token))throw new Error(`atelier.js still creates removed workstation peripheral: ${token}`);
if(!scripts.quality.includes("dataset.localeLeak"))throw new Error('Runtime locale leak audit is missing');

const sw=fs.readFileSync(path.join(site,'sw.js'),'utf8');
for(const asset of ['./quality.css','./quality.js'])if(!sw.includes(`'${asset}'`))throw new Error(`Offline shell is missing ${asset}`);
console.log(`PASS ${locales.length} locale surfaces share the same product structure; dynamic layers are localized and the 3D scene is monitor-only`);
