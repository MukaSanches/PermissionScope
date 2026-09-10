const CACHE='permissionscope-shell-v2';
const CORE=['./','./index.html','./index.pt-BR.html','./style.css','./premium.css','./experience.css','./future.css','./responsive.css','./intelligence.css','./site.js','./experience.js','./future.js','./intelligence.js','./logo.svg','./manifest.webmanifest','./pwa-icon-192.svg','./pwa-icon-512.svg'];
self.addEventListener('install',event=>{event.waitUntil(caches.open(CACHE).then(cache=>cache.addAll(CORE)).then(()=>self.skipWaiting()));});
self.addEventListener('activate',event=>{event.waitUntil(caches.keys().then(keys=>Promise.all(keys.filter(k=>k!==CACHE).map(k=>caches.delete(k)))).then(()=>self.clients.claim()));});
self.addEventListener('fetch',event=>{
  const req=event.request;
  if(req.method!=='GET')return;
  const url=new URL(req.url);
  if(url.origin!==self.location.origin)return;
  if(req.mode==='navigate'){
    event.respondWith(fetch(req).then(res=>{const copy=res.clone();caches.open(CACHE).then(c=>c.put(req,copy));return res;}).catch(()=>caches.match(req).then(r=>r||caches.match('./index.html'))));
    return;
  }
  event.respondWith(caches.match(req).then(hit=>hit||fetch(req).then(res=>{if(res.ok&&url.pathname.includes('/PermissionScope/')){const copy=res.clone();caches.open(CACHE).then(c=>c.put(req,copy));}return res;})));
});
