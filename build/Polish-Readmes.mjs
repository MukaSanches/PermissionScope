import fs from 'node:fs';
import path from 'node:path';

// Post-process the single README generator so every locale receives the same
// GitHub-safe brand system, navigation, verification links and localized flow.
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

const labels=`const readmeUi={
'en-US':{website:'Website',trust:'Trust Center',academy:'Academy',roadmap:'Roadmap',security:'Security',handbooks:'Handbooks PDF · 8 languages',governance:'Governance',support:'Support',citation:'Citation',development:'Development',benchmarks:'Benchmarks',checksums:'SHA-256 checksums',flow:['Windows identity','      ↓','SID + recorded membership context','      ↓','ACL / discretionary permission entries','      ↓','Windows Authz evaluation','      ↓','Granted · Partial · Denied · Unknown','      ↓','Access Path → contributing evidence']},
'pt-BR':{website:'Site',trust:'Central de confiança',academy:'Academia',roadmap:'Roteiro',security:'Segurança',handbooks:'Manuais em PDF · 8 idiomas',governance:'Governança',support:'Suporte',citation:'Citação',development:'Desenvolvimento',benchmarks:'Desempenho',checksums:'Checksums SHA-256',flow:['Identidade do Windows','      ↓','SID + contexto de associação registrado','      ↓','ACL / entradas de permissão discricionária','      ↓','Avaliação pelo Windows Authz','      ↓','Permitido · Parcial · Negado · Desconhecido','      ↓','Access Path → evidências contribuintes']},
'es':{website:'Sitio web',trust:'Centro de confianza',academy:'Academia',roadmap:'Hoja de ruta',security:'Seguridad',handbooks:'Manuales PDF · 8 idiomas',governance:'Gobernanza',support:'Soporte',citation:'Citación',development:'Desarrollo',benchmarks:'Rendimiento',checksums:'Checksums SHA-256',flow:['Identidad de Windows','      ↓','SID + contexto de pertenencia registrado','      ↓','ACL / entradas de permisos discrecionales','      ↓','Evaluación con Windows Authz','      ↓','Permitido · Parcial · Denegado · Desconocido','      ↓','Access Path → evidencia contribuyente']},
'fr':{website:'Site web',trust:'Centre de confiance',academy:'Académie',roadmap:'Feuille de route',security:'Sécurité',handbooks:'Manuels PDF · 8 langues',governance:'Gouvernance',support:'Assistance',citation:'Citation',development:'Développement',benchmarks:'Performances',checksums:'Sommes SHA-256',flow:['Identité Windows','      ↓','SID + contexte d’appartenance enregistré','      ↓','ACL / entrées d’autorisation discrétionnaire','      ↓','Évaluation Windows Authz','      ↓','Autorisé · Partiel · Refusé · Inconnu','      ↓','Access Path → preuves contributrices']},
'de':{website:'Website',trust:'Vertrauenszentrum',academy:'Akademie',roadmap:'Roadmap',security:'Sicherheit',handbooks:'PDF-Handbücher · 8 Sprachen',governance:'Governance',support:'Support',citation:'Zitierung',development:'Entwicklung',benchmarks:'Benchmarks',checksums:'SHA-256-Prüfsummen',flow:['Windows-Identität','      ↓','SID + erfasster Mitgliedschaftskontext','      ↓','ACL / diskretionäre Berechtigungseinträge','      ↓','Windows-Authz-Auswertung','      ↓','Erlaubt · Teilweise · Verweigert · Unbekannt','      ↓','Access Path → beitragende Evidenz']},
'ar':{website:'الموقع',trust:'مركز الثقة',academy:'الأكاديمية',roadmap:'خارطة الطريق',security:'الأمان',handbooks:'أدلة PDF · 8 لغات',governance:'الحوكمة',support:'الدعم',citation:'الاستشهاد',development:'التطوير',benchmarks:'اختبارات الأداء',checksums:'بصمات SHA-256',flow:['هوية Windows','      ↓','SID + سياق العضوية المسجل','      ↓','ACL / إدخالات الأذونات التقديرية','      ↓','تقييم Windows Authz','      ↓','مسموح · جزئي · مرفوض · غير معروف','      ↓','Access Path → الأدلة المساهمة']},
'ja':{website:'ウェブサイト',trust:'トラストセンター',academy:'アカデミー',roadmap:'ロードマップ',security:'セキュリティ',handbooks:'PDF ハンドブック · 8言語',governance:'ガバナンス',support:'サポート',citation:'引用',development:'開発',benchmarks:'ベンチマーク',checksums:'SHA-256 チェックサム',flow:['Windows ID','      ↓','SID + 記録された所属コンテキスト','      ↓','ACL / 随意アクセス許可エントリ','      ↓','Windows Authz 評価','      ↓','許可 · 一部許可 · 拒否 · 不明','      ↓','Access Path → 寄与証拠']},
'zh-Hans':{website:'网站',trust:'信任中心',academy:'学院',roadmap:'路线图',security:'安全',handbooks:'PDF 手册 · 8 种语言',governance:'治理',support:'支持',citation:'引用',development:'开发',benchmarks:'基准测试',checksums:'SHA-256 校验和',flow:['Windows 身份','      ↓','SID + 已记录的成员关系上下文','      ↓','ACL / 自主权限条目','      ↓','Windows Authz 评估','      ↓','允许 · 部分允许 · 拒绝 · 未知','      ↓','Access Path → 贡献证据']}
};
`;
if(!src.includes('const readmeUi={')) src=src.replace('function makeReadme(locale,t){',labels+'\nfunction makeReadme(locale,t){');
if(!src.includes("const n=readmeUi[locale]")) src=src.replace("function makeReadme(locale,t){\n const u=ui[locale]||ui['en-US'];","function makeReadme(locale,t){\n const u=ui[locale]||ui['en-US'];\n const n=readmeUi[locale]||readmeUi['en-US'];");

const oldNav='<table><tr><td align="center"><a href="${web}${page(locale)}"><strong>Website</strong></a></td><td align="center"><a href="${repo}/releases/latest"><strong>${t.download}</strong></a></td><td align="center"><a href="${repo}/blob/main/docs/TRUST-CENTER.md"><strong>Trust Center</strong></a></td></tr><tr><td align="center"><a href="${repo}/blob/main/docs/courses/README.md">Academy</a></td><td align="center"><a href="${repo}/blob/main/ROADMAP.md">Roadmap</a></td><td align="center"><a href="${repo}/blob/main/SECURITY.md">Security</a></td></tr></table>';
const newNav='<table><tr><td align="center"><a href="${web}${page(locale)}"><strong>${n.website}</strong></a></td><td align="center"><a href="${repo}/releases/latest"><strong>${t.download}</strong></a></td><td align="center"><a href="${repo}/blob/main/docs/TRUST-CENTER.md"><strong>${n.trust}</strong></a></td></tr><tr><td align="center"><a href="${repo}/blob/main/docs/courses/README.md">${n.academy}</a></td><td align="center"><a href="${repo}/blob/main/ROADMAP.md">${n.roadmap}</a></td><td align="center"><a href="${repo}/blob/main/SECURITY.md">${n.security}</a></td></tr></table>';
if(src.includes(oldNav)) src=src.replace(oldNav,newNav);

const flow=`Windows identity
      ↓
SID + recorded membership context
      ↓
ACL / discretionary permission entries
      ↓
Windows Authz evaluation
      ↓
Granted · Partial · Denied · Unknown
      ↓
Access Path → contributing evidence`;
if(src.includes(flow)) src=src.replace(flow,"${n.flow.join('\\n')}");

src=src.replace('[SHA-256](${repo}/releases/latest)', '[${n.checksums}](${repo}/releases/latest/download/SHA256SUMS.txt)');
src=src.replace('[Handbooks PDF · 8 languages](${repo}/tree/main/docs/handbooks)', '[${n.handbooks}](${repo}/tree/main/docs/handbooks)');
src=src.replace('[Governance](${repo}/blob/main/GOVERNANCE.md)', '[${n.governance}](${repo}/blob/main/GOVERNANCE.md)');
src=src.replace('[Support](${repo}/blob/main/SUPPORT.md)', '[${n.support}](${repo}/blob/main/SUPPORT.md)');
src=src.replace('[Citation](${repo}/blob/main/CITATION.cff)', '[${n.citation}](${repo}/blob/main/CITATION.cff)');
src=src.replace('[Development](${repo}/blob/main/docs/development.md) · [Benchmarks](${repo}/blob/main/docs/benchmarks.md) · [Roadmap](${repo}/blob/main/ROADMAP.md)', '[${n.development}](${repo}/blob/main/docs/development.md) · [${n.benchmarks}](${repo}/blob/main/docs/benchmarks.md) · [${n.roadmap}](${repo}/blob/main/ROADMAP.md)');

fs.writeFileSync(file,src);
console.log('README generator polished: localized navigation/flow, direct checksum asset, raster brand assets and structured language switcher');
