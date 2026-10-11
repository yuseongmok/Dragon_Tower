mergeInto(LibraryManager.library, {
  DT_ReadOfflineIdentity: function(receiver) { window.DragonTowerOffline.readIdentity(UTF8ToString(receiver)); },
  DT_AccountRequest: function (receiver, payload) {
    var target = UTF8ToString(receiver), data = UTF8ToString(payload);
    if (window.DragonTowerFirebase) window.DragonTowerFirebase.request(target, data);
    else {
      var r = JSON.parse(data);
      SendMessage(target, 'OnAccountResponse', JSON.stringify({ id: r.id, ok: false, code: 'bridge-unavailable' }));
    }
  }
});
