"use strict";

(()=>{
  const version=(document.documentElement.dataset.releaseVersion||'').trim();
  if(!/^\d+\.\d+\.\d+(?:[-+][0-9A-Za-z.-]+)?$/.test(version))return;

  const repo='https://github.com/MukaSanches/PermissionScope';
  const base=`${repo}/releases/download/v${version}`;
  const urls=[
    `${base}/PermissionScope-${version}-x64-Setup.exe`,
    `${base}/PermissionScope-${version}-arm64-Setup.exe`,
    `${base}/PermissionScope-${version}-win-x64.zip`,
    `${base}/PermissionScope-${version}-win-arm64.zip`
  ];

  const download=document.getElementById('download-premium');
  if(!download)return;

  const cards=[...download.querySelectorAll('.ps-download-card')];
  cards.slice(0,urls.length).forEach((card,index)=>card.href=urls[index]);

  const chip=download.querySelector('.ps-release-chip');
  const chipLabel=chip?.querySelector('span');
  if(chipLabel){
    const prefix=chipLabel.textContent?.replace(/\s+\d+\.\d+\.\d+(?:[-+][0-9A-Za-z.-]+)?\s*$/,'').trim();
    chipLabel.textContent=`${prefix||'Release'} ${version}`;
  }
  const checksum=chip?.querySelector('a');
  if(checksum)checksum.href=`${base}/SHA256SUMS.txt`;

  download.dataset.releaseVersion=version;
})();
