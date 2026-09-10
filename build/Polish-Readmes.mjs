import fs from 'node:fs';
import path from 'node:path';

// This post-processes the single README generator so every locale receives the same visual system.
const root=path.resolve(import.meta.dirname,'..');
const file=path.join(root,'build/Build-Documentation.mjs');
let src=fs.readFileSync(file,'utf8');

const oldLinks=" const links=Object.entries(languages).map(([l,v])=>`[${v.name}](${repo}/blob/main/${readme(l)})`).join(' · ');";
const newLinks=" const languageEntries=Object.entries(languages);\n const languageCells=languageEntries.map(([l,v])=>`<td align=\\\"center\\\" width=\\\"25%\\\"><a href=\\\"${repo}/blob/main/${readme(l)}\\\"><strong>${v.name}</strong></a><br><sub>${l}</sub></td>`);\n const links=`<table><tr>${languageCells.slice(0,4).join('')}</tr><tr>${languageCells.slice(4,8).join('')}</tr></table>`;";
if(src.includes(oldLinks)) src=src.replace(oldLinks,newLinks);

src=src.replace(
  '<p align="center"><img src="${raw}/docs/brand/repository-banner.svg" alt="PermissionScope" width="100%"></p>',
  '<p align="center"><img src="${raw}/docs/brand/repository-banner.png" alt="PermissionScope — See who has access. Understand why." width="100%"></p>\\n\\n<p align="center"><img src="${raw}/docs/brand/project-avatar.png" alt="PermissionScope mark" width="72" height="72"></p>'
);
src=src.replace('<p align="center">${links}</p>','<p align="center"><sub>${u.language}</sub></p>\\n${links}');

const oldNav='<p align="center"><a href="${web}${page(locale)}">Website</a> · <a href="${repo}/releases/latest">${t.download}</a> · <a href="${repo}/blob/main/docs/TRUST-CENTER.md">Trust Center</a> · <a href="${repo}/blob/main/docs/courses/README.md">Academy</a> · <a href="${repo}/blob/main/ROADMAP.md">Roadmap</a> · <a href="${repo}/blob/main/SECURITY.md">Security</a></p>';
const newNav='<table><tr><td align="center"><a href="${web}${page(locale)}"><strong>Website</strong></a></td><td align="center"><a href="${repo}/releases/latest"><strong>${t.download}</strong></a></td><td align="center"><a href="${repo}/blob/main/docs/TRUST-CENTER.md"><strong>Trust Center</strong></a></td></tr><tr><td align="center"><a href="${repo}/blob/main/docs/courses/README.md">Academy</a></td><td align="center"><a href="${repo}/blob/main/ROADMAP.md">Roadmap</a></td><td align="center"><a href="${repo}/blob/main/SECURITY.md">Security</a></td></tr></table>';
if(src.includes(oldNav)) src=src.replace(oldNav,newNav);

fs.writeFileSync(file,src);
console.log('README generator polished: raster brand assets + structured language switcher + product navigation');
