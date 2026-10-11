/* Firebase is loaded only when requested. The game and its local save work without it. */
(function (host) {
  'use strict';
  let context, initializing, busy = false;
  const fail = code => Object.assign(new Error(code), { code });
  const nameValid = s => typeof s === 'string' && s === s.trim() && /^[A-Za-z0-9가-힣 _-]{1,12}$/.test(s) && /[A-Za-z0-9가-힣]/.test(s);
  function loginEmail(value) {
    const id=typeof value==='string'?value.trim().toLowerCase():'';
    if(!/^[a-z][a-z0-9_]{3,19}$/.test(id))throw fail('invalid-login-id');
    return id+'@login.dragontower.invalid';
  }
  const fields = ['displayName','bodyType','skinTone','hairStyle','equippedOutfit','equippedHeadAccessory','equippedFaceAccessory','selectedDragonId'];
  function clean(p) {
    if (!p || !nameValid(p.displayName) || ![0,1].includes(p.bodyType) || ![0,1,2].includes(p.skinTone) || ![0,1,2].includes(p.hairStyle) || p.equippedOutfit !== 'starter' || p.equippedHeadAccessory !== '' || p.equippedFaceAccessory !== '' || typeof p.selectedDragonId !== 'string' || !/^[A-Za-z0-9_-]{0,80}$/.test(p.selectedDragonId)) throw fail('invalid-profile');
    return Object.fromEntries(fields.map(k => [k,p[k]]));
  }
  async function init() {
    if (context) return context;
    if (initializing) return initializing;
    initializing = (async () => {
      const cfg = host.DragonTowerFirebaseConfig;
      if (!cfg?.enabled || !cfg.firebase?.apiKey || !cfg.firebase?.projectId || !cfg.firebase?.appId) throw fail('not-configured');
      if (cfg.environment !== 'development' && cfg.environment !== 'production') throw fail('invalid-environment');
      const base = 'https://www.gstatic.com/firebasejs/13.0.0/';
      const [app,auth,fire] = await Promise.all([import(base+'firebase-app.js'),import(base+'firebase-auth.js'),import(base+'firebase-firestore.js')]);
      const application = app.initializeApp(cfg.firebase,'dragon-tower-'+cfg.environment);
      const a = auth.getAuth(application);
      await auth.setPersistence(a,auth.browserLocalPersistence);
      await a.authStateReady();
      context = { auth, fire, a, db:fire.getFirestore(application) };
      return context;
    })();
    try { return await initializing; } finally { initializing = null; }
  }
  function user(c) { const u=c.a.currentUser;return {scope:host.DragonTowerFirebaseConfig.environment+':'+host.DragonTowerFirebaseConfig.firebase.projectId,uid:u?.uid||'',anonymous:!!u?.isAnonymous,emailVerified:!!u?.emailVerified}; }
  function assertOwner(c,uid) { if (!uid || c.a.currentUser?.uid !== uid) throw fail('account-changed'); }
  async function callable(c) {
    const cfg=host.DragonTowerFirebaseConfig;
    if(!cfg.appCheckSiteKey)throw fail('leaderboard-not-configured');
    if(!c.functions){
      const base='https://www.gstatic.com/firebasejs/13.0.0/';
      const [fn,check]=await Promise.all([import(base+'firebase-functions.js'),import(base+'firebase-app-check.js')]);
      check.initializeAppCheck(c.a.app,{provider:new check.ReCaptchaV3Provider(cfg.appCheckSiteKey),isTokenAutoRefreshEnabled:true});
      c.functions=fn;c.fx=fn.getFunctions(c.a.app,cfg.functionsRegion||'asia-northeast3');
    }
    return c.functions.httpsCallable(c.fx,'onlineLeaderboard');
  }
  async function execute(r) {
    const c = await init(), A=c.auth,F=c.fire;
    if (r.op==='init') return user(c);
    if (r.op==='guest') { if(!c.a.currentUser) await A.signInAnonymously(c.a); return user(c); }
    if (r.op==='login') { await A.signInWithEmailAndPassword(c.a,loginEmail(r.loginId),r.password);return user(c); }
    if (r.op==='link') {
      const address=loginEmail(r.loginId);
      if(typeof r.password!=='string'||r.password.length<8)throw fail('auth/weak-password');
      if(!c.a.currentUser)await A.signInAnonymously(c.a);
      if(r.uid)assertOwner(c,r.uid);
      if(!c.a.currentUser.isAnonymous) throw fail('already-linked');
      // Never sign in automatically on credential collision: preserve the anonymous UID and data.
      await A.linkWithCredential(c.a.currentUser,A.EmailAuthProvider.credential(address,r.password));
      return user(c);
    }
    if(['recovery-send','recovery-confirm','reset'].includes(r.op)) {
      const cfg=host.DragonTowerFirebaseConfig;
      if(!cfg.recoveryEnabled||!cfg.appCheckSiteKey)throw fail('recovery-not-configured');
      if(r.op!=='reset')assertOwner(c,r.uid);
      if(!c.functions){
        const base='https://www.gstatic.com/firebasejs/13.0.0/';
        const [fn,check]=await Promise.all([import(base+'firebase-functions.js'),import(base+'firebase-app-check.js')]);
        check.initializeAppCheck(c.a.app,{provider:new check.ReCaptchaV3Provider(cfg.appCheckSiteKey),isTokenAutoRefreshEnabled:true});
        c.functions=fn;c.fx=fn.getFunctions(c.a.app,cfg.functionsRegion||'asia-northeast3');
      }
      const payload=r.op==='reset'?{op:'reset',loginId:r.loginId}:{op:r.op,email:r.email,code:r.code};
      await c.functions.httpsCallable(c.fx,'trainerRecovery')(payload);return user(c);
    }
    if(r.op==='logout') {await A.signOut(c.a);return user(c);}
    assertOwner(c,r.uid);
    if(r.op==='leaderboard-read'||r.op==='leaderboard-submit') {
      if(!host.DragonTowerFirebaseConfig.leaderboardEnabled)throw fail('leaderboard-not-configured');
      const invoke=await callable(c);
      const result=await invoke({op:r.op==='leaderboard-read'?'read':'submit',record:r.record});
      assertOwner(c,r.uid);return result.data;
    }
    const ref=F.doc(c.db,'users',r.uid);
    const deletion=F.doc(c.db,'accountDeletions',r.uid);
    if(r.op==='delete-account') {
      const target=c.a.currentUser;
      if(r.code!=='삭제')throw fail('delete-confirmation-required');
      if(!target.isAnonymous) {
        if(!r.password)throw fail('auth/invalid-credential');
        await A.reauthenticateWithCredential(target,A.EmailAuthProvider.credential(target.email,r.password));
      }
      assertOwner(c,r.uid);
      const batch=F.writeBatch(c.db);
      // The tombstone prevents another tab or a delayed request from restoring the profile.
      batch.set(deletion,{requested:true});batch.delete(ref);batch.delete(F.doc(c.db,'accountRecovery',r.uid));
      if(host.DragonTowerFirebaseConfig.leaderboardEnabled)batch.delete(F.doc(c.db,'leaderboardPlaytest',r.uid));
      await batch.commit();
      assertOwner(c,r.uid);
      try { await A.deleteUser(target); } catch(e) {throw fail('delete-auth-incomplete');}
      return {deleted:true,...user(c)};
    }
    try {if((await F.getDocFromServer(deletion)).exists())throw fail('account-deletion-pending');}
    catch(e){if(e.code!=='permission-denied')throw e;} // Old deployed rules have no tombstone access yet; normal login remains available.
    if(r.op==='load') {const snap=await F.getDocFromServer(ref);assertOwner(c,r.uid);return {...user(c),exists:snap.exists(),profile:snap.exists()?snap.data():null};}
    if(r.op==='save') {
      const p=clean(r.profile);
      const result=await F.runTransaction(c.db,async tx=>{
        assertOwner(c,r.uid);const snap=await tx.get(ref),old=snap.exists()?snap.data():null;
        if((old?.revision||0)!==r.revision)throw fail('profile-conflict');
        const data={...p,schemaVersion:1,revision:r.revision+1,createdAt:old?.createdAt||F.serverTimestamp(),updatedAt:F.serverTimestamp()};
        if(old) tx.update(ref,data);else tx.set(ref,data);
        return {...p,schemaVersion:1,revision:r.revision+1};
      });
      assertOwner(c,r.uid);return {...user(c),exists:true,profile:result};
    }
    throw fail('unsupported-operation');
  }
  async function request(receiver,raw) {
    let r;try{r=JSON.parse(raw);}catch{return;}
    const send=data=>host.dragonTower?.SendMessage(receiver,'OnAccountResponse',JSON.stringify({id:r.id,...data}));
    if(busy){send({ok:false,code:'busy'});return;}
    busy=true;
    try { const data=await execute(r);busy=false;send({ok:true,...data}); }
    catch(e) { busy=false;send({ok:false,code:typeof e.code==='string'?e.code:'network-or-init-failed'}); }
    finally {r.password='';}
  }
  host.DragonTowerFirebase={request};
  // Pure validation export for the local contract tests; no Firebase mocks enter a build.
  if(typeof module!=='undefined')module.exports={clean,nameValid,loginEmail};
})(typeof window!=='undefined'?window:globalThis);
