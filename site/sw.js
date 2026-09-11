const CACHE='permissionscope-shell-v5';
const PAGES=['./index.html','./index.pt-BR.html','./index.es.html','./index.fr.html','./index.de.html','./index.ar.html','./index.ja.html','./index.zh-Hans.html'];
const CORE=['./',...PAGES,'./404.html','./style.css','./premium.css','./experience.css','./future.css','./responsive.css','./intelligence.css','./site.js','./experience.js','./future.js','./intelligence.js','./platform.js','./logo.svg','./manifest.webmanifest','./pwa-icon-192.svg','./pwa-icon-512.svg','./screenshots/en-US/access-light.png'];

self.addEventListener('install',event=>{
  event.waitUntil(caches.open(CACHE).then(cache=>cache.addAll(CORE)).then(()=>self.skipWaiting()));
});

self.addEventListener('activate',event=>{
  event.waitUntil((async()=>{
    await Promise.all((await caches.keys()).filter(key=>key!==CACHE).map(key=>caches.delete(key)));
    if(self.registration.navigationPreload)await self.registration.navigationPreload.enable();
    await self.clients.claim();
  })());
});

const offlinePage=request=>{
  const name=new URL(request.url).pathname.split('/').pop();
  return PAGES.includes(`./${name}`)?`./${name}`:'./index.html';
};

self.addEventListener('fetch',event=>{
  const request=event.request;
  if(request.method!=='GET'||request.headers.has('range'))return;
  const url=new URL(request.url);
  if(url.origin!==self.location.origin)return;

  if(request.mode==='navigate'){
    event.respondWith((async()=>{
      try{
        const response=(await event.preloadResponse)||await fetch(request);
        if(response?.ok){
          const copy=response.clone();
          event.waitUntil(caches.open(CACHE).then(cache=>cache.put(request,copy)));
        }
        return response;
      }catch{
        return (await caches.match(request))||(await caches.match(offlinePage(request)))||(await caches.match('./index.html'));
      }
    })());
    return;
  }

  event.respondWith((async()=>{
    const cached=await caches.match(request);
    const network=fetch(request).then(async response=>{
      if(response.ok&&response.type==='basic'){
        const cache=await caches.open(CACHE);
        await cache.put(request,response.clone());
      }
      return response;
    });
    if(cached){
      event.waitUntil(network.catch(()=>undefined));
      return cached;
    }
    try{return await network;}catch{return new Response('',{status:504,statusText:'Offline'});}
  })());
});
