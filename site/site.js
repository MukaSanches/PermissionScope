"use strict";
const selector = document.getElementById('language');
selector.addEventListener('change', () => {
  const selected = selector.selectedOptions[0];
  if (selected) location.assign(new URL(selected.value, location.href));
});

// Keep the public download action stable across releases. The release workflow
// publishes these asset names under the repository's latest GitHub Release.
const download = document.querySelector('.download-row');
if (download) {
  const repo = 'https://github.com/MukaSanches/PermissionScope';
  download.href = `${repo}/releases/latest/download/PermissionScope-1.0.0-x64-Setup.exe`;
  const labels = {
    'pt-BR': 'Baixar instalador x64 →',
    'es': 'Descargar instalador x64 →',
    'fr': 'Télécharger l’installation x64 →',
    'de': 'x64-Installer herunterladen →',
    'ar': 'تنزيل مثبّت x64 ←',
    'ja': 'x64 インストーラーをダウンロード →',
    'zh-Hans': '下载 x64 安装程序 →',
    'en-US': 'Download x64 installer →'
  };
  download.textContent = labels[document.documentElement.lang] || labels['en-US'];

  const arm64 = document.createElement('a');
  arm64.href = `${repo}/releases/latest/download/PermissionScope-1.0.0-arm64-Setup.exe`;
  arm64.textContent = 'ARM64';
  arm64.setAttribute('aria-label', 'Download PermissionScope ARM64 installer');
  download.insertAdjacentElement('afterend', arm64);

  const portable = document.createElement('a');
  portable.href = `${repo}/releases/latest/download/PermissionScope-1.0.0-win-x64.zip`;
  portable.textContent = 'Portable ZIP x64';
  portable.setAttribute('aria-label', 'Download PermissionScope portable ZIP for x64');
  arm64.insertAdjacentElement('afterend', portable);
}
