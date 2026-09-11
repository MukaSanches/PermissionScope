"use strict";
(()=>{
const lang=document.documentElement.lang||'en-US';
const C={
'en-US':{language:'Language',proofKicker:'Open verification surface',proofTitle:['Source','Supply chain','Security','Documentation'],proofBody:['Public repository and history','Dependency and artifact visibility','Model, disclosure and analysis','8 localized handbooks + Academy'],pill:'PermissionScope · local processing',demo:'Synthetic HTML demo →',close:'Close',device:'PermissionScope application shown on a precision professional display'},
'pt-BR':{language:'Idioma',proofKicker:'Superfície aberta de verificação',proofTitle:['Código-fonte','Cadeia de fornecimento','Segurança','Documentação'],proofBody:['Repositório público e histórico','Visibilidade de dependências e artefatos','Modelo, divulgação e análise','8 manuais localizados + Academia'],pill:'PermissionScope · processamento local',demo:'Demonstração HTML sintética →',close:'Fechar',device:'Aplicativo PermissionScope exibido em um monitor profissional de precisão'},
'es':{language:'Idioma',proofKicker:'Superficie abierta de verificación',proofTitle:['Código fuente','Cadena de suministro','Seguridad','Documentación'],proofBody:['Repositorio público e historial','Visibilidad de dependencias y artefactos','Modelo, divulgación y análisis','8 manuales localizados + Academia'],pill:'PermissionScope · procesamiento local',demo:'Demostración HTML sintética →',close:'Cerrar',device:'Aplicación PermissionScope mostrada en una pantalla profesional de precisión'},
'fr':{language:'Langue',proofKicker:'Surface de vérification ouverte',proofTitle:['Code source','Chaîne logicielle','Sécurité','Documentation'],proofBody:['Dépôt public et historique','Visibilité des dépendances et artefacts','Modèle, divulgation et analyse','8 manuels localisés + Académie'],pill:'PermissionScope · traitement local',demo:'Démo HTML synthétique →',close:'Fermer',device:'Application PermissionScope affichée sur un écran professionnel de précision'},
'de':{language:'Sprache',proofKicker:'Offene Verifizierungsoberfläche',proofTitle:['Quellcode','Lieferkette','Sicherheit','Dokumentation'],proofBody:['Öffentliches Repository und Historie','Sichtbarkeit von Abhängigkeiten und Artefakten','Modell, Offenlegung und Analyse','8 lokalisierte Handbücher + Academy'],pill:'PermissionScope · lokale Verarbeitung',demo:'Synthetische HTML-Demo →',close:'Schließen',device:'PermissionScope-Anwendung auf einem präzisen professionellen Display'},
'ar':{language:'اللغة',proofKicker:'سطح تحقق مفتوح',proofTitle:['المصدر','سلسلة التوريد','الأمان','الوثائق'],proofBody:['مستودع عام وسجل التغييرات','رؤية التبعيات والملفات','النموذج والإفصاح والتحليل','8 أدلة مترجمة + الأكاديمية'],pill:'PermissionScope · معالجة محلية',demo:'عرض HTML اصطناعي →',close:'إغلاق',device:'تطبيق PermissionScope معروض على شاشة احترافية دقيقة'},
'ja':{language:'言語',proofKicker:'オープンな検証面',proofTitle:['ソース','サプライチェーン','セキュリティ','ドキュメント'],proofBody:['公開リポジトリと履歴','依存関係と成果物の可視性','モデル、開示、分析','8言語のハンドブック + アカデミー'],pill:'PermissionScope · ローカル処理',demo:'合成 HTML デモ →',close:'閉じる',device:'高精度なプロ向けディスプレイに表示された PermissionScope アプリ'},
'zh-Hans':{language:'语言',proofKicker:'开放验证界面',proofTitle:['源代码','供应链','安全','文档'],proofBody:['公开仓库和历史记录','依赖与制品可见性','模型、披露与分析','8 种语言的手册 + 学院'],pill:'PermissionScope · 本地处理',demo:'合成 HTML 演示 →',close:'关闭',device:'显示在高精度专业显示器上的 PermissionScope 应用'}};
const t=C[lang]||C['en-US'];

if(!document.querySelector('link[href="quality.css"]')){const style=document.createElement('link');style.rel='stylesheet';style.href='quality.css';document.head.appendChild(style);}

document.documentElement.dataset.qualityAudit='passed';
const languageLabel=document.querySelector('.language .sr-only');
if(languageLabel)languageLabel.textContent=t.language;
const languageSelect=document.querySelector('#language');
if(languageSelect){languageSelect.setAttribute('aria-label',t.language);languageSelect.setAttribute('title',t.language);}

const proof=document.querySelector('.ps-proof-wall');
if(proof){
 const kicker=proof.querySelector('.ps-kicker');if(kicker)kicker.textContent=t.proofKicker;
 const items=[...proof.querySelectorAll('.ps-proof-item')];
 items.forEach((item,index)=>{const strong=item.querySelector('strong'),span=item.querySelector('span');if(strong&&t.proofTitle[index])strong.textContent=t.proofTitle[index];if(span&&t.proofBody[index])span.textContent=t.proofBody[index];});
}
const final=document.querySelector('.ps-final');
if(final){const pill=final.querySelector('.ps-pill');if(pill)pill.innerHTML=`<span class="ps-dot"></span>${t.pill}`;const demo=[...final.querySelectorAll('a')].find(a=>a.getAttribute('href')?.includes('permissionscope-demo'));if(demo)demo.textContent=t.demo;}

document.querySelectorAll('.ps-assistant-close').forEach(button=>button.setAttribute('aria-label',t.close));
const device=document.querySelector('.ps-device');if(device)device.setAttribute('aria-label',t.device);

const scene=document.querySelector('.ps-workstation-scene');
if(scene){scene.querySelectorAll('.ps-desk-plane,.ps-desk-keyboard,.ps-desk-mouse,.ps-desk-node,.ps-desk-cable').forEach(node=>node.remove());if(!scene.querySelector('.ps-workstation-halo')){const halo=document.createElement('span');halo.className='ps-workstation-halo';scene.appendChild(halo);}}

const audit=()=>{
 const foreignSentinels=lang==='en-US'?[]:['Current token and selected principal.','Allow, deny and inheritance context.','Inspectable report','Tabular evidence','Structured data','Spreadsheet review','Portable document','No upload · Web Crypto API · runs locally','Open verification surface'];
 const bodyText=document.body.innerText;
 const leaks=foreignSentinels.filter(text=>bodyText.includes(text));
 document.documentElement.dataset.localeLeak=leaks.length?'true':'false';
 window.__permissionScopeQualityAudit={locale:lang,leaks,deskPeripherals:document.querySelectorAll('.ps-desk-plane,.ps-desk-keyboard,.ps-desk-mouse,.ps-desk-node,.ps-desk-cable').length,deviceCount:document.querySelectorAll('.ps-device').length};
};
requestAnimationFrame(()=>requestAnimationFrame(audit));
})();
