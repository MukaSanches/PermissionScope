"use strict";
(()=>{
  const lang=document.documentElement.lang||'en-US';
  const copy={
    'en-US':{release:'Current release',local:'Local-first',native:'Windows-native',open:'Open source',status:'PermissionScope product status',device:'PermissionScope application shown on a precision professional display'},
    'pt-BR':{release:'Versão atual',local:'Processamento local',native:'Nativo do Windows',open:'Código aberto',status:'Status do produto PermissionScope',device:'Aplicativo PermissionScope exibido em um monitor profissional de precisão'},
    'es':{release:'Versión actual',local:'Procesamiento local',native:'Nativo de Windows',open:'Código abierto',status:'Estado del producto PermissionScope',device:'Aplicación PermissionScope mostrada en una pantalla profesional de precisión'},
    'fr':{release:'Version actuelle',local:'Traitement local',native:'Natif Windows',open:'Code source ouvert',status:'État du produit PermissionScope',device:'Application PermissionScope affichée sur un écran professionnel de précision'},
    'de':{release:'Aktuelle Version',local:'Lokale Verarbeitung',native:'Windows-nativ',open:'Open Source',status:'PermissionScope-Produktstatus',device:'PermissionScope-Anwendung auf einem präzisen professionellen Display'},
    'ar':{release:'الإصدار الحالي',local:'معالجة محلية',native:'أصلي لويندوز',open:'مفتوح المصدر',status:'حالة منتج PermissionScope',device:'تطبيق PermissionScope معروض على شاشة احترافية دقيقة'},
    'ja':{release:'現在のリリース',local:'ローカル処理',native:'Windows ネイティブ',open:'オープンソース',status:'PermissionScope 製品ステータス',device:'高精度なプロ向けディスプレイに表示された PermissionScope アプリ'},
    'zh-Hans':{release:'当前版本',local:'本地处理',native:'Windows 原生',open:'开源',status:'PermissionScope 产品状态',device:'显示在高精度专业显示器上的 PermissionScope 应用'}
  };
  const t=copy[lang]||copy['en-US'];
  const currentRelease=(()=>{
    const href=document.querySelector('.ps-download-card[href*="/releases/download/v"]')?.getAttribute('href')||'';
    return href.match(/\/releases\/download\/v([^/]+)\//)?.[1]||document.documentElement.dataset.releaseVersion||'current';
  })();

  const hero=document.querySelector('.hero');
  if(hero&&!hero.querySelector('.ps-release-rail')){
    const rail=document.createElement('div');
    rail.className='ps-release-rail';
    rail.setAttribute('aria-label',t.status);
    rail.innerHTML=`<span><i></i><strong>${t.release} ${currentRelease}</strong></span><span>${t.local}</span><span>${t.native}</span><span>${t.open} · Apache-2.0</span>`;
    hero.querySelector('.actions')?.insertAdjacentElement('afterend',rail);
  }

  const stage=document.querySelector('.ps-device-stage');
  if(stage&&!stage.querySelector('.ps-workstation-scene')){
    const scene=document.createElement('div');
    scene.className='ps-workstation-scene';
    scene.setAttribute('aria-hidden','true');
    scene.innerHTML='<span class="ps-workstation-halo"></span>';
    stage.prepend(scene);
  }

  const display=document.querySelector('.ps-device-display');
  if(display&&!display.querySelector('.ps-device-speaker')){
    const speaker=document.createElement('span');speaker.className='ps-device-speaker';speaker.setAttribute('aria-hidden','true');display.appendChild(speaker);
    const ports=document.createElement('span');ports.className='ps-device-port-rail';ports.setAttribute('aria-hidden','true');ports.innerHTML='<i></i><i></i><i></i><i></i>';display.appendChild(ports);
    const hinge=document.createElement('span');hinge.className='ps-device-hinge';hinge.setAttribute('aria-hidden','true');display.appendChild(hinge);
    const side=document.createElement('span');side.className='ps-device-side';side.setAttribute('aria-hidden','true');display.appendChild(side);
    const back=document.createElement('span');back.className='ps-device-backplane';back.setAttribute('aria-hidden','true');display.appendChild(back);
  }

  const navLinks=[...document.querySelectorAll('.navigation nav a[href^="#"]')];
  const targets=navLinks.map(a=>({a,id:a.getAttribute('href')?.slice(1)})).filter(x=>x.id&&document.getElementById(x.id));
  if(targets.length&&'IntersectionObserver'in window){
    const observer=new IntersectionObserver(entries=>{
      const visible=entries.filter(e=>e.isIntersecting).sort((a,b)=>b.intersectionRatio-a.intersectionRatio)[0];
      if(!visible)return;
      for(const item of targets)item.a.removeAttribute('aria-current');
      targets.find(item=>item.id===visible.target.id)?.a.setAttribute('aria-current','true');
    },{rootMargin:'-22% 0px -62% 0px',threshold:[0,.2,.5]});
    for(const item of targets)observer.observe(document.getElementById(item.id));
  }

  document.querySelectorAll('a[href^="https://github.com/MukaSanches/PermissionScope/releases/download/"]').forEach(a=>{
    a.setAttribute('rel','noopener');
    a.dataset.releaseAsset='true';
  });

  const device=document.querySelector('.ps-device');
  if(device){
    device.setAttribute('role','img');
    device.setAttribute('aria-label',t.device);
  }
})();
