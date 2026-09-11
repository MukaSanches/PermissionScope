"use strict";
(()=>{
  if('serviceWorker' in navigator){
    addEventListener('load',async()=>{
      try{
        const registration=await navigator.serviceWorker.register('./sw.js',{scope:'./',updateViaCache:'none'});
        await registration.update();
      }catch{}
    },{once:true});
  }

  let installPrompt=null;
  addEventListener('beforeinstallprompt',event=>{
    event.preventDefault();
    installPrompt=event;
    document.documentElement.dataset.pwaInstallable='true';
  });
  addEventListener('appinstalled',()=>{
    installPrompt=null;
    delete document.documentElement.dataset.pwaInstallable;
  });

  const install=document.querySelector('[data-pwa-install]');
  if(install){
    install.addEventListener('click',async()=>{
      if(!installPrompt)return;
      await installPrompt.prompt();
      installPrompt=null;
      delete document.documentElement.dataset.pwaInstallable;
    });
  }
})();
