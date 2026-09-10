"use strict";
const selector = document.getElementById('language');
selector.addEventListener('change', () => {
  const selected = selector.selectedOptions[0];
  if (selected) location.assign(new URL(selected.value, location.href));
});
