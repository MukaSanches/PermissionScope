const CACHE='permissionscope-shell-v10';
const PAGES=['./index.html','./index.pt-BR.html','./index.es.html','./index.fr.html','./index.de.html','./index.ar.html','./index.ja.html','./index.zh-Hans.html'];
const CORE=['./',...PAGES,'./404.html','./style.css','./premium.css','./device.css','./experience.css','./future.css','./responsive.css','./progressive.css','./intelligence.css','./quality.css','./site.js','./release-sync.js','./experience.js','./future.js','./intelligence.js','./quality.js','./platform.js','./logo.svg','./manifest.webmanifest','./pwa-icon-192.svg','./pwa-icon-512.svg','./screenshots/en-US/access-light.png'];
const ATELIER=['./atelier.css','./atelier.js'];

const putIfCacheable=async(request,response)=>{
  if(response?.ok&&response.type==='basic'){
    const cache=await caches.open(CACHE);
    await cache.put(request,response.clone());
  }
  return response;
};

self.addEventListener('install',event=>{
  event.waitUntil(caches.open(CACHE).then(cache=>cache.addAll([...CORE,...ATELIER])).then(()=>self.skipWaiting()));
});

self.addEventListener('activate',event=>{
  event.waitUntil((async()=>{
    await Promise.all((await caches.keys()).filter(key=>key!==CACHE).map(key=>caches.delete(key)));
    if(self.registration.navigationPreload)await self.registration.navigationPreload.enable();
    await self.clients.claim();

    // An updated worker may arrive after the current document already loaded old
    // cached CSS/JS. Refresh existing tabs once so the newly activated worker
    // serves the current deployment immediately instead of on a later visit.
    const windows=await self.clients.matchAll({type:'window',includeUncontrolled:true});
    await Promise.allSettled(windows.map(client=>client.navigate(client.url)));
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
        const response=(await event.preloadResponse)||await fetch(request,{cache:'no-store'});
        await putIfCacheable(request,response);
        return response;
      }catch{
        return (await caches.match(request))||(await caches.match(offlinePage(request)))||(await caches.match('./index.html'));
      }
    })());
    return;
  }

  // Online: always revalidate/fetch the current deployment first. Offline: fall
  // back to the pre-cached shell. This prevents a successful Pages deployment
  // from looking unchanged because an older CSS/JS response won the cache race.
  event.respondWith((async()=>{
    try{
      const response=await fetch(request,{cache:'no-cache'});
      await putIfCacheable(request,response);
      return response;
    }catch{
      return (await caches.match(request))||new Response('',{status:504,statusText:'Offline'});
    }
  })());
});
