"use strict";
(()=>{
  const lang=document.documentElement.lang||'en-US';
  const copy={
    'en-US':{release:'Current release',local:'Local-first',native:'Windows-native',open:'Open source'},
    'pt-BR':{release:'Versão atual',local:'Local-first',native:'Nativo do Windows',open:'Código aberto'},
    'es':{release:'Versión actual',local:'Local-first',native:'Nativo de Windows',open:'Código abierto'},
    'fr':{release:'Version actuelle',local:'Local-first',native:'Natif Windows',open:'Open source'},
    'de':{release:'Aktuelle Version',local:'Local-first',native:'Windows-nativ',open:'Open Source'},
    'ar':{release:'الإصدار الحالي',local:'محلي أولاً',native:'أصلي لويندوز',open:'مفتوح المصدر'},
    'ja':{release:'現在のリリース',local:'ローカル優先',native:'Windows ネイティブ',open:'オープンソース'},
    'zh-Hans':{release:'当前版本',local:'本地优先',native:'Windows 原生',open:'开源'}
  };
  const t=copy[lang]||copy['en-US'];

  const hero=document.querySelector('.hero');
  if(hero&&!hero.querySelector('.ps-release-rail')){
    const rail=document.createElement('div');
    rail.className='ps-release-rail';
    rail.setAttribute('aria-label','PermissionScope product status');
    rail.innerHTML=`<span><i></i><strong>${t.release} 1.0.1</strong></span><span>${t.local}</span><span>${t.native}</span><span>${t.open} · Apache-2.0</span>`;
    hero.querySelector('.actions')?.insertAdjacentElement('afterend',rail);
  }

  const stage=document.querySelector('.ps-device-stage');
  if(stage&&!stage.querySelector('.ps-workstation-scene')){
    const scene=document.createElement('div');
    scene.className='ps-workstation-scene';
    scene.setAttribute('aria-hidden','true');
    scene.innerHTML='<span class="ps-workstation-halo"></span><span class="ps-desk-plane"></span><span class="ps-desk-keyboard"></span><span class="ps-desk-mouse"></span><span class="ps-desk-node"></span><span class="ps-desk-cable"></span>';
    stage.prepend(scene);
  }

  const display=document.querySelector('.ps-device-display');
  if(display&&!display.querySelector('.ps-device-speaker')){
    const speaker=document.createElement('span');speaker.className='ps-device-speaker';speaker.setAttribute('aria-hidden','true');display.appendChild(speaker);
    const ports=document.createElement('span');ports.className='ps-device-port-rail';ports.setAttribute('aria-hidden','true');ports.innerHTML='<i></i><i></i><i></i><i></i>';display.appendChild(ports);
    const hinge=document.createElement('span');hinge.className='ps-device-hinge';hinge.setAttribute('aria-hidden','true');display.appendChild(hinge);
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
    device.setAttribute('aria-label','PermissionScope application shown on a premium desktop display');
  }
})();
