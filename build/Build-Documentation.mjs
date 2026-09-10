import fs from 'node:fs';
import path from 'node:path';

const root=path.resolve(import.meta.dirname,'..');
const languages=JSON.parse(fs.readFileSync(path.join(root,'docs/content/locales.json'),'utf8'));
const check=process.argv.includes('--check');
const repo='https://github.com/MukaSanches/PermissionScope';
const raw='https://raw.githubusercontent.com/MukaSanches/PermissionScope/main';
const web='https://mukasanches.github.io/PermissionScope/';
const fence=String.fromCharCode(96).repeat(3);
const readme=l=>l==='en-US'?'README.md':`README.${l}.md`;
const page=l=>l==='en-US'?'index.html':`index.${l}.html`;
const esc=s=>s.replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('"','&quot;');
function write(file,text){const target=path.join(root,file);if(check){if(!fs.existsSync(target)||fs.readFileSync(target,'utf8').replace(/\r\n/g,'\n')!==text)throw Error('Stale generated content: '+file);}else{fs.mkdirSync(path.dirname(target),{recursive:true});fs.writeFileSync(target,text);}}
function copy(source,target){const a=path.join(root,source),b=path.join(root,target);if(check){if(!fs.existsSync(b)||!fs.readFileSync(a).equals(fs.readFileSync(b)))throw Error('Stale copy: '+target);}else{fs.mkdirSync(path.dirname(b),{recursive:true});fs.copyFileSync(a,b);}}

const ui={
'en-US':{hero:'Windows permissions, made inspectable.',localTitle:'Local-first',localBody:'No PermissionScope account, application telemetry or required cloud service.',nativeTitle:'Windows-native decision',nativeBody:'Effective access is evaluated with Windows Authz instead of a hand-written approximation.',unknownTitle:'Unknown stays Unknown',unknownBody:'Missing context is never silently converted into Granted or Denied.',flow:'From result to evidence',product:'Product surface',trust:'Built for verification',academy:'Learn the model, not just the buttons',boundaries:'Engineering boundaries',analyze:'Analyze',analyzeD:'Inspect effective access for a folder and identity.',explain:'Explain',explainD:'Follow Access Path and contributing evidence.',snapshot:'Snapshot',snapshotD:'Save local observations for later review.',compare:'Compare',compareD:'Compare observations without assuming missing resources were deleted.',simulate:'Simulate',simulateD:'Model removal of a rule in memory before considering a real change.',export:'Export',exportD:'HTML, CSV, JSON, XLSX and PDF outputs.',source:'Source',releases:'Releases',supply:'Supply chain',security:'Security',platform:'Platform',documentation:'Documentation',privacy:'Privacy',sourceD:'Public repository and commit history',releasesD:'Versioned artifacts and SHA-256 checksums',supplyD:'CycloneDX SBOM and pinned automation where documented',securityD:'Security model, disclosure guidance and CodeQL workflow',platformD:'x64 and ARM64 build paths',documentationD:'Eight localized handbooks, visual guides and Academy courses',privacyD:'Local-first design and explicit export sensitivity guidance',course1:'Permission fundamentals',course2:'Reading Access Path',course3:'Safe troubleshooting',limits:'PermissionScope does not pretend discretionary ACL evaluation explains every possible file-open outcome. Integrity policy, encryption, locks, some remote/S4U contexts, conditional rules, administrative privilege effects and unverified reparse targets can be outside the available decision context. Those limits are documented instead of hidden.',cta:'Open the Trust Center',language:'Languages'},
'pt-BR':{hero:'Permissões do Windows, finalmente inspecionáveis.',localTitle:'Local-first',localBody:'Sem conta PermissionScope, telemetria do aplicativo ou serviço de nuvem obrigatório.',nativeTitle:'Decisão nativa do Windows',nativeBody:'O acesso efetivo é avaliado com Windows Authz, não por uma aproximação criada à mão.',unknownTitle:'Desconhecido continua Desconhecido',unknownBody:'Contexto ausente nunca é transformado silenciosamente em Permitido ou Negado.',flow:'Do resultado à evidência',product:'Superfície do produto',trust:'Construído para ser verificado',academy:'Aprenda o modelo, não só os botões',boundaries:'Limites de engenharia',analyze:'Analisar',analyzeD:'Inspecione o acesso efetivo de uma pasta e identidade.',explain:'Explicar',explainD:'Siga o Access Path e as evidências que contribuíram.',snapshot:'Snapshot',snapshotD:'Salve observações locais para revisar depois.',compare:'Comparar',compareD:'Compare observações sem presumir que recursos ausentes foram excluídos.',simulate:'Simular',simulateD:'Modele a remoção de uma regra em memória antes de considerar uma alteração real.',export:'Exportar',exportD:'Saídas HTML, CSV, JSON, XLSX e PDF.',source:'Código-fonte',releases:'Releases',supply:'Cadeia de fornecimento',security:'Segurança',platform:'Plataforma',documentation:'Documentação',privacy:'Privacidade',sourceD:'Repositório público e histórico de commits',releasesD:'Artefatos versionados e checksums SHA-256',supplyD:'SBOM CycloneDX e automações fixadas quando documentado',securityD:'Modelo de segurança, divulgação responsável e CodeQL',platformD:'Builds x64 e ARM64',documentationD:'Oito handbooks localizados, guias visuais e cursos da Academy',privacyD:'Arquitetura local-first e orientação explícita sobre dados sensíveis em exports',course1:'Fundamentos de permissões',course2:'Lendo o Access Path',course3:'Diagnóstico seguro',limits:'O PermissionScope não finge que a ACL discricionária explica todo resultado possível ao abrir arquivos. Integridade, criptografia, bloqueios, alguns contextos remotos/S4U, regras condicionais, privilégios administrativos e destinos de reparse não verificados podem ficar fora do contexto disponível. Esses limites são documentados, não escondidos.',cta:'Abrir o Trust Center',language:'Idiomas'},
'es':{hero:'Permisos de Windows, ahora inspeccionables.',localTitle:'Local-first',localBody:'Sin cuenta PermissionScope, telemetría de la aplicación ni servicio cloud obligatorio.',nativeTitle:'Decisión nativa de Windows',nativeBody:'El acceso efectivo se evalúa con Windows Authz y no con una aproximación manual.',unknownTitle:'Desconocido sigue siendo Desconocido',unknownBody:'El contexto que falta nunca se convierte silenciosamente en Permitido o Denegado.',flow:'Del resultado a la evidencia',product:'Superficie del producto',trust:'Diseñado para verificarse',academy:'Aprenda el modelo, no solo los botones',boundaries:'Límites de ingeniería',analyze:'Analizar',analyzeD:'Inspeccione el acceso efectivo para una carpeta e identidad.',explain:'Explicar',explainD:'Siga Access Path y la evidencia que contribuye.',snapshot:'Instantánea',snapshotD:'Guarde observaciones locales para revisarlas después.',compare:'Comparar',compareD:'Compare observaciones sin asumir que los recursos ausentes fueron eliminados.',simulate:'Simular',simulateD:'Modele la eliminación de una regla en memoria antes de considerar un cambio real.',export:'Exportar',exportD:'Salidas HTML, CSV, JSON, XLSX y PDF.',source:'Código fuente',releases:'Releases',supply:'Cadena de suministro',security:'Seguridad',platform:'Plataforma',documentation:'Documentación',privacy:'Privacidad',sourceD:'Repositorio público e historial de commits',releasesD:'Artefactos versionados y checksums SHA-256',supplyD:'SBOM CycloneDX y automatización fijada cuando se documenta',securityD:'Modelo de seguridad, divulgación responsable y CodeQL',platformD:'Rutas de compilación x64 y ARM64',documentationD:'Ocho manuales localizados, guías visuales y cursos de Academy',privacyD:'Diseño local-first y guía explícita sobre sensibilidad de las exportaciones',course1:'Fundamentos de permisos',course2:'Leer Access Path',course3:'Diagnóstico seguro',limits:'PermissionScope no afirma que una ACL discrecional explique todos los resultados posibles al abrir archivos. Integridad, cifrado, bloqueos, algunos contextos remotos/S4U, reglas condicionales, privilegios administrativos y destinos reparse no verificados pueden quedar fuera del contexto disponible. Esos límites se documentan en lugar de ocultarse.',cta:'Abrir Trust Center',language:'Idiomas'},
'fr':{hero:'Les autorisations Windows, enfin inspectables.',localTitle:'Local-first',localBody:'Aucun compte PermissionScope, aucune télémétrie applicative ni service cloud obligatoire.',nativeTitle:'Décision native Windows',nativeBody:'L’accès effectif est évalué avec Windows Authz, pas avec une approximation artisanale.',unknownTitle:'Inconnu reste Inconnu',unknownBody:'Un contexte manquant n’est jamais converti silencieusement en Autorisé ou Refusé.',flow:'Du résultat à la preuve',product:'Surface du produit',trust:'Conçu pour être vérifié',academy:'Apprenez le modèle, pas seulement les boutons',boundaries:'Limites d’ingénierie',analyze:'Analyser',analyzeD:'Inspectez l’accès effectif pour un dossier et une identité.',explain:'Expliquer',explainD:'Suivez Access Path et les preuves contributrices.',snapshot:'Instantané',snapshotD:'Enregistrez des observations locales pour les revoir plus tard.',compare:'Comparer',compareD:'Comparez des observations sans supposer que les ressources absentes ont été supprimées.',simulate:'Simuler',simulateD:'Modélisez le retrait d’une règle en mémoire avant un changement réel.',export:'Exporter',exportD:'Sorties HTML, CSV, JSON, XLSX et PDF.',source:'Source',releases:'Releases',supply:'Chaîne logicielle',security:'Sécurité',platform:'Plateforme',documentation:'Documentation',privacy:'Confidentialité',sourceD:'Dépôt public et historique des commits',releasesD:'Artefacts versionnés et sommes SHA-256',supplyD:'SBOM CycloneDX et automatisations épinglées lorsque documentées',securityD:'Modèle de sécurité, divulgation responsable et CodeQL',platformD:'Chemins de build x64 et ARM64',documentationD:'Huit manuels localisés, guides visuels et cours Academy',privacyD:'Conception local-first et consignes explicites sur la sensibilité des exports',course1:'Bases des autorisations',course2:'Lire Access Path',course3:'Diagnostic sûr',limits:'PermissionScope ne prétend pas qu’une ACL discrétionnaire explique tous les résultats possibles d’ouverture de fichier. Intégrité, chiffrement, verrous, certains contextes distants/S4U, règles conditionnelles, privilèges administratifs et cibles reparse non vérifiées peuvent être hors contexte. Ces limites sont documentées au lieu d’être masquées.',cta:'Ouvrir le Trust Center',language:'Langues'},
'de':{hero:'Windows-Berechtigungen, nachvollziehbar gemacht.',localTitle:'Local-first',localBody:'Kein PermissionScope-Konto, keine App-Telemetrie und kein erforderlicher Cloud-Dienst.',nativeTitle:'Windows-native Entscheidung',nativeBody:'Effektiver Zugriff wird mit Windows Authz ausgewertet, nicht mit einer handgeschriebenen Näherung.',unknownTitle:'Unbekannt bleibt Unbekannt',unknownBody:'Fehlender Kontext wird nie stillschweigend in Erlaubt oder Verweigert umgewandelt.',flow:'Vom Ergebnis zur Evidenz',product:'Produktoberfläche',trust:'Für Überprüfbarkeit gebaut',academy:'Das Modell lernen, nicht nur die Schaltflächen',boundaries:'Technische Grenzen',analyze:'Analysieren',analyzeD:'Effektiven Zugriff für Ordner und Identität prüfen.',explain:'Erklären',explainD:'Access Path und beitragende Evidenz verfolgen.',snapshot:'Snapshot',snapshotD:'Lokale Beobachtungen zur späteren Prüfung speichern.',compare:'Vergleichen',compareD:'Beobachtungen vergleichen, ohne fehlende Ressourcen als gelöscht anzunehmen.',simulate:'Simulieren',simulateD:'Entfernung einer Regel im Speicher modellieren, bevor eine echte Änderung erwogen wird.',export:'Exportieren',exportD:'HTML-, CSV-, JSON-, XLSX- und PDF-Ausgaben.',source:'Quellcode',releases:'Releases',supply:'Lieferkette',security:'Sicherheit',platform:'Plattform',documentation:'Dokumentation',privacy:'Datenschutz',sourceD:'Öffentliches Repository und Commit-Historie',releasesD:'Versionierte Artefakte und SHA-256-Prüfsummen',supplyD:'CycloneDX-SBOM und dokumentierte, gepinnte Automatisierung',securityD:'Sicherheitsmodell, Responsible Disclosure und CodeQL',platformD:'x64- und ARM64-Buildpfade',documentationD:'Acht lokalisierte Handbücher, visuelle Guides und Academy-Kurse',privacyD:'Local-first-Design und klare Hinweise zu sensiblen Exportdaten',course1:'Berechtigungsgrundlagen',course2:'Access Path lesen',course3:'Sichere Diagnose',limits:'PermissionScope behauptet nicht, dass die diskretionäre ACL jedes mögliche Ergebnis beim Öffnen einer Datei erklärt. Integritätsrichtlinien, Verschlüsselung, Sperren, einige Remote-/S4U-Kontexte, bedingte Regeln, Administratorprivilegien und ungeprüfte Reparse-Ziele können außerhalb des verfügbaren Kontexts liegen. Diese Grenzen werden dokumentiert statt versteckt.',cta:'Trust Center öffnen',language:'Sprachen'},
'ar':{hero:'أذونات Windows بشكل يمكن فحصه.',localTitle:'محلي أولاً',localBody:'لا حساب PermissionScope ولا قياس عن بُعد للتطبيق ولا خدمة سحابية إلزامية.',nativeTitle:'قرار أصلي من Windows',nativeBody:'يتم تقييم الوصول الفعلي بواسطة Windows Authz بدلاً من تقريب مكتوب يدوياً.',unknownTitle:'غير معروف يبقى غير معروف',unknownBody:'السياق المفقود لا يتحول بصمت إلى مسموح أو مرفوض.',flow:'من النتيجة إلى الدليل',product:'سطح المنتج',trust:'مصمم للتحقق',academy:'تعلم النموذج وليس الأزرار فقط',boundaries:'حدود هندسية',analyze:'تحليل',analyzeD:'افحص الوصول الفعلي لمجلد وهوية.',explain:'شرح',explainD:'اتبع Access Path والأدلة المساهمة.',snapshot:'لقطة',snapshotD:'احفظ الملاحظات المحلية للمراجعة لاحقاً.',compare:'مقارنة',compareD:'قارن الملاحظات دون افتراض حذف الموارد المفقودة.',simulate:'محاكاة',simulateD:'حاكي إزالة قاعدة في الذاكرة قبل التفكير في تغيير فعلي.',export:'تصدير',exportD:'مخرجات HTML وCSV وJSON وXLSX وPDF.',source:'المصدر',releases:'الإصدارات',supply:'سلسلة التوريد',security:'الأمان',platform:'المنصة',documentation:'الوثائق',privacy:'الخصوصية',sourceD:'مستودع عام وسجل الالتزامات',releasesD:'ملفات إصدار ببصمات SHA-256',supplyD:'SBOM بصيغة CycloneDX وأتمتة مثبتة حيث تم توثيقها',securityD:'نموذج أمان وإرشادات إفصاح وCodeQL',platformD:'مسارات بناء x64 وARM64',documentationD:'ثمانية أدلة مترجمة وأدلة مرئية ودورات Academy',privacyD:'تصميم محلي أولاً وإرشادات واضحة لحساسية بيانات التصدير',course1:'أساسيات الأذونات',course2:'قراءة Access Path',course3:'تشخيص آمن',limits:'لا يدّعي PermissionScope أن تقييم ACL التقديري يفسر كل نتيجة ممكنة لفتح الملفات. قد تبقى سياسة التكامل والتشفير والأقفال وبعض سياقات Remote/S4U والقواعد الشرطية وامتيازات الإدارة وأهداف reparse غير الموثقة خارج السياق المتاح. يتم توثيق هذه الحدود بدلاً من إخفائها.',cta:'فتح Trust Center',language:'اللغات'},
'ja':{hero:'Windows 権限を、検証できる形に。',localTitle:'ローカル優先',localBody:'PermissionScope アカウント、アプリのテレメトリ、必須クラウドサービスはありません。',nativeTitle:'Windows ネイティブの判定',nativeBody:'有効アクセスは手書きの近似ではなく Windows Authz で評価します。',unknownTitle:'Unknown は Unknown のまま',unknownBody:'不足している文脈を勝手に Granted や Denied へ変換しません。',flow:'結果から証拠へ',product:'製品機能',trust:'検証可能性を重視',academy:'ボタンではなくモデルを学ぶ',boundaries:'エンジニアリング上の境界',analyze:'分析',analyzeD:'フォルダーと ID の有効アクセスを確認します。',explain:'説明',explainD:'Access Path と寄与した証拠を追跡します。',snapshot:'スナップショット',snapshotD:'ローカル観測を保存し、後で確認します。',compare:'比較',compareD:'見つからないリソースを削除済みと決めつけずに比較します。',simulate:'シミュレーション',simulateD:'実変更の前にルール削除をメモリ上でモデル化します。',export:'エクスポート',exportD:'HTML、CSV、JSON、XLSX、PDF 出力。',source:'ソース',releases:'リリース',supply:'サプライチェーン',security:'セキュリティ',platform:'プラットフォーム',documentation:'ドキュメント',privacy:'プライバシー',sourceD:'公開リポジトリとコミット履歴',releasesD:'バージョン付き成果物と SHA-256',supplyD:'CycloneDX SBOM と文書化された固定済み自動化',securityD:'セキュリティモデル、開示ガイダンス、CodeQL',platformD:'x64 と ARM64 のビルド経路',documentationD:'8 言語のハンドブック、ビジュアルガイド、Academy コース',privacyD:'ローカル優先設計とエクスポートの機微情報に関する明示的な説明',course1:'権限の基礎',course2:'Access Path の読み方',course3:'安全な診断',limits:'PermissionScope は、随意 ACL の評価だけですべてのファイルオープン結果を説明できるとは主張しません。整合性ポリシー、暗号化、ロック、一部の Remote/S4U コンテキスト、条件付きルール、管理者権限の影響、未検証の reparse ターゲットは利用可能な文脈の外にある場合があります。これらの境界は隠さず文書化します。',cta:'Trust Center を開く',language:'言語'},
'zh-Hans':{hero:'让 Windows 权限真正可检查。',localTitle:'本地优先',localBody:'无需 PermissionScope 账户、应用遥测或强制云服务。',nativeTitle:'Windows 原生判定',nativeBody:'有效访问由 Windows Authz 评估，而不是手写近似算法。',unknownTitle:'未知保持未知',unknownBody:'缺少上下文时绝不会静默变成允许或拒绝。',flow:'从结果到证据',product:'产品能力',trust:'为可验证性而构建',academy:'学习模型，而不只是按钮',boundaries:'工程边界',analyze:'分析',analyzeD:'检查文件夹与身份的有效访问。',explain:'解释',explainD:'沿 Access Path 查看贡献证据。',snapshot:'快照',snapshotD:'保存本地观察，供之后复核。',compare:'比较',compareD:'比较观察结果，不把缺失资源自动视为已删除。',simulate:'模拟',simulateD:'真正修改前，先在内存中模拟移除规则。',export:'导出',exportD:'HTML、CSV、JSON、XLSX 和 PDF 输出。',source:'源代码',releases:'发行版',supply:'供应链',security:'安全',platform:'平台',documentation:'文档',privacy:'隐私',sourceD:'公开仓库与提交历史',releasesD:'版本化产物与 SHA-256 校验和',supplyD:'CycloneDX SBOM 与文档中固定版本的自动化',securityD:'安全模型、披露指南与 CodeQL',platformD:'x64 与 ARM64 构建路径',documentationD:'八种语言的手册、视觉指南与 Academy 课程',privacyD:'本地优先设计，并明确说明导出内容可能包含敏感信息',course1:'权限基础',course2:'阅读 Access Path',course3:'安全诊断',limits:'PermissionScope 不声称随意 ACL 评估可以解释所有文件打开结果。完整性策略、加密、锁定、部分 Remote/S4U 上下文、条件规则、管理员权限影响以及未经验证的 reparse 目标可能超出可用判定上下文。这些边界会被明确记录，而不是隐藏。',cta:'打开 Trust Center',language:'语言'}
};

const cli=['./permissionscope-cli.exe demo --output ./demo-reports','./permissionscope-cli.exe explain "C:\\Finance"','./permissionscope-cli.exe scan "C:\\Finance" --save','./permissionscope-cli.exe export <snapshot-id> --format html --output report.html'].join('\n');

function makeReadme(locale,t){
 const u=ui[locale]||ui['en-US'];
 const languageEntries=Object.entries(languages);
 const languageCells=languageEntries.map(([l,v])=>`<td align=\"center\" width=\"25%\"><a href=\"${repo}/blob/main/${readme(l)}\"><strong>${v.name}</strong></a><br><sub>${l}</sub></td>`);
 const links=`<table><tr>${languageCells.slice(0,4).join('')}</tr><tr>${languageCells.slice(4,8).join('')}</tr></table>`;
 const screen=`${raw}/docs/screenshots/${locale}/access-light.png`;
 return `<!-- Generated from docs/content/locales.json by build/Build-Documentation.mjs. Do not edit generated localized READMEs by hand. -->
<p align="center"><img src="${raw}/docs/brand/repository-banner.png" alt="PermissionScope — See who has access. Understand why." width="100%"></p>\n\n<p align="center"><img src="${raw}/docs/brand/project-avatar.png" alt="PermissionScope mark" width="72" height="72"></p>

<p align="center">
  <a href="${repo}/actions/workflows/build.yml"><img alt="Windows build" src="${repo}/actions/workflows/build.yml/badge.svg"></a>
  <a href="${repo}/actions/workflows/codeql.yml"><img alt="CodeQL" src="${repo}/actions/workflows/codeql.yml/badge.svg"></a>
  <a href="${repo}/blob/main/LICENSE"><img alt="Apache-2.0" src="https://img.shields.io/badge/license-Apache--2.0-2764E7"></a>
  <a href="${repo}/releases"><img alt="Release" src="https://img.shields.io/github/v/release/MukaSanches/PermissionScope?display_name=tag&color=2764E7"></a>
</p>

<p align="center"><strong>${u.hero}</strong><br>${t.tagline}</p>
<table><tr><td align="center"><a href="${web}${page(locale)}"><strong>Website</strong></a></td><td align="center"><a href="${repo}/releases/latest"><strong>${t.download}</strong></a></td><td align="center"><a href="${repo}/blob/main/docs/TRUST-CENTER.md"><strong>Trust Center</strong></a></td></tr><tr><td align="center"><a href="${repo}/blob/main/docs/courses/README.md">Academy</a></td><td align="center"><a href="${repo}/blob/main/ROADMAP.md">Roadmap</a></td><td align="center"><a href="${repo}/blob/main/SECURITY.md">Security</a></td></tr></table>

<p align="center"><sub>${u.language}</sub></p>\n${links}

---

<table><tr><td width="33%"><strong>${u.localTitle}</strong><br><sub>${u.localBody}</sub></td><td width="33%"><strong>${u.nativeTitle}</strong><br><sub>${u.nativeBody}</sub></td><td width="33%"><strong>${u.unknownTitle}</strong><br><sub>${u.unknownBody}</sub></td></tr></table>

<p align="center"><img src="${screen}" alt="${t.caption}" width="94%"></p>

## ${t.start}

${t.steps.map((s,i)=>`${i+1}. ${s}`).join('\n')}

## ${u.flow}

${t.technical}

${fence}text
Windows identity
      ↓
SID + recorded membership context
      ↓
ACL / discretionary permission entries
      ↓
Windows Authz evaluation
      ↓
Granted · Partial · Denied · Unknown
      ↓
Access Path → contributing evidence
${fence}

## ${t.learn}

${t.meaning}

## ${t.why}

${t.technical}

<p align="center"><img src="${raw}/docs/screenshots/${locale}/access-path-light.png" alt="Access Path" width="94%"></p>

## ${u.product}

<table><tr><td><strong>${u.analyze}</strong><br><sub>${u.analyzeD}</sub></td><td><strong>${u.explain}</strong><br><sub>${u.explainD}</sub></td><td><strong>${u.snapshot}</strong><br><sub>${u.snapshotD}</sub></td></tr><tr><td><strong>${u.compare}</strong><br><sub>${u.compareD}</sub></td><td><strong>${u.simulate}</strong><br><sub>${u.simulateD}</sub></td><td><strong>${u.export}</strong><br><sub>${u.exportD}</sub></td></tr></table>

## ${t.features}

${t.featureText}

${fence}powershell
${cli}
${fence}

## ${t.safety}

${t.scope}

> ${t.apply}

## ${t.install}

${t.installText}

[${t.download}](${repo}/releases/latest) · [SHA-256](${repo}/releases/latest) · [${t.status}](${repo}/blob/main/docs/release-status.md)

## ${u.trust}

| ${u.trust} | |
|---|---|
| ${u.source} | ${u.sourceD} |
| ${u.releases} | ${u.releasesD} |
| ${u.supply} | ${u.supplyD} |
| ${u.security} | ${u.securityD} |
| ${u.platform} | ${u.platformD} |
| ${u.documentation} | ${u.documentationD} |
| ${u.privacy} | ${u.privacyD} |

[${u.cta}](${repo}/blob/main/docs/TRUST-CENTER.md)

## ${u.academy}

- [${u.course1}](${repo}/blob/main/docs/courses/fundamentals.md)
- [${u.course2}](${repo}/blob/main/docs/courses/access-path.md)
- [${u.course3}](${repo}/blob/main/docs/courses/troubleshooting.md)
- [Handbooks PDF · 8 languages](${repo}/tree/main/docs/handbooks)

## ${t.docs}

- [${t.guide}](${repo}/blob/main/docs/guides/${locale}.md)
- [${t.reference}](${repo}/blob/main/docs/access-model.md)
- [${t.faq}](${repo}/blob/main/docs/faq.md)
- [${t.glossary}](${repo}/blob/main/docs/glossary.md)
- [${t.privacy}](${repo}/blob/main/docs/privacy.md)
- [${t.status}](${repo}/blob/main/docs/release-status.md)
- [Governance](${repo}/blob/main/GOVERNANCE.md)
- [Support](${repo}/blob/main/SUPPORT.md)
- [Citation](${repo}/blob/main/CITATION.cff)

## ${u.boundaries}

${u.limits}

## ${t.build}

${t.buildText}

[Development](${repo}/blob/main/docs/development.md) · [Benchmarks](${repo}/blob/main/docs/benchmarks.md) · [Roadmap](${repo}/blob/main/ROADMAP.md)

## ${t.help}

${t.helpText}

---

<p align="center"><sub>${t.created} · ssanches011@gmail.com · <a href="${repo}/blob/main/LICENSE">Apache-2.0</a> · <a href="${repo}/blob/main/SECURITY.md">Security</a> · <a href="${repo}/blob/main/SUPPORT.md">Support</a></sub></p>
`;
}

for(const [locale,t] of Object.entries(languages)){
 const md=makeReadme(locale,t);
 write(readme(locale),md);
 if(locale==='en-US')write('.github/README.md',md);

 const scenes={home:t.steps[2],analyze:t.steps[3],access:t.meaning,'access-path':t.technical,compare:t.featureText,simulation:t.apply,technical:t.helpText,unknown:t.scope};
 let guide=`# PermissionScope · ${t.guide}\n\n[${t.docs}](${repo}/blob/main/${readme(locale)})\n\n${t.caption}\n`;
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
<header class="shell navigation"><a class="wordmark" href="${page(locale)}"><img src="logo.svg" width="30" height="34" alt="">PermissionScope</a><nav aria-label="${esc(t.docs)}"><a href="#evidence">${esc(t.why)}</a><a href="#download-premium">${esc(t.download)}</a><a href="${repo}">GitHub</a></nav><label class="language"><span class="sr-only">Language</span><select id="language">${options}</select></label></header>
<main id="main"><section class="shell hero"><p class="eyebrow">PermissionScope · Windows · Apache-2.0</p><h1>${esc(t.tagline)}</h1><p class="lede">${esc(t.featureText)}</p><div class="actions"><a class="primary" href="#download-premium">${esc(t.download)}</a><a href="reports/permissionscope-demo.html">${esc(t.sample)}</a></div><figure class="product"><img src="screenshots/${locale}/access-light.png" width="3744" height="2064" alt="${esc(t.caption)}" fetchpriority="high"><figcaption>${esc(t.caption)}</figcaption></figure></section>
<section class="shell section" id="evidence"><h2>${esc(t.start)}</h2><ol>${t.steps.map(s=>`<li>${esc(s)}</li>`).join('')}</ol><div class="features">${sections.map(([h,p])=>`<article><h3>${esc(h)}</h3><p>${esc(p)}</p></article>`).join('')}</div><p>${esc(t.apply)}</p><a href="${repo}/blob/main/docs/guides/${locale}.md">${esc(t.guide)} →</a></section>
<section class="shell section download-grid" id="download"><div><h2>${esc(t.install)}</h2><p>${esc(t.installText)}</p></div><div class="downloads"><a class="download-row" href="${repo}/releases">${esc(t.download)} →</a><a href="${repo}/blob/main/docs/release-status.md">${esc(t.status)}</a><p><a href="${repo}/blob/main/${readme(locale)}">${esc(t.reference)} / CLI</a></p></div></section>
<section class="shell section"><h2>${esc(t.docs)}</h2><div class="actions"><a href="${repo}/blob/main/docs/faq.md">${esc(t.faq)}</a><a href="${repo}/blob/main/docs/privacy.md">${esc(t.privacy)}</a>${['html','csv','json','xlsx','pdf'].map(f=>`<a href="reports/permissionscope-demo.${f}">${f.toUpperCase()}</a>`).join('')}</div></section></main>
<footer class="shell"><span>${esc(t.created)}</span><span>ssanches011@gmail.com · Apache-2.0</span><a href="${repo}/blob/main/SECURITY.md">Security</a></footer></body></html>
`;
 write('site/'+page(locale),html);
 copy(`docs/screenshots/${locale}/access-light.png`,`site/screenshots/${locale}/access-light.png`);
}
for(const ext of ['html','csv','json','xlsx','pdf'])copy(`samples/reports/permissionscope-demo.${ext}`,`site/reports/permissionscope-demo.${ext}`);
console.log('PASS '+(check?'verified':'generated')+' eight equivalent premium READMEs, visual guides and static localized pages');
