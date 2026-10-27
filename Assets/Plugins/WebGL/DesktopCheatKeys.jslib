mergeInto(LibraryManager.library, {
 DT_DesktopCheatKeys: function() {
  var mobile = /Android|iPhone|iPad|iPod|Mobile/i.test(navigator.userAgent) || (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1);
  if (mobile) return 0;
  if (!window.dtCheatKeysInstalled) {
   window.dtCheatKeysInstalled = true;
   document.addEventListener('keydown', function(e) {
    var tag = e.target && e.target.tagName;
    if (tag === 'INPUT' || tag === 'TEXTAREA' || (e.target && e.target.isContentEditable)) return;
    if (e.key === 'F1' || e.key === 'F2' || e.key === 'F3' || e.key === 'F4') e.preventDefault();
   }, true);
  }
  return 1;
 }
});
