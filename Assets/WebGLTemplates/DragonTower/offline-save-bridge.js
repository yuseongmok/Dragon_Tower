// Local-only compatibility reader. Never loads Firebase SDKs or contacts a server.
(function(host){'use strict';const pending=[];host.DragonTowerOffline={ready(){while(pending.length){const [receiver,data]=pending.shift();host.dragonTower.SendMessage(receiver,'OnOfflineIdentity',JSON.stringify(data));}},async readIdentity(receiver){
 const send=data=>{if(host.dragonTower)host.dragonTower.SendMessage(receiver,'OnOfflineIdentity',JSON.stringify(data));else pending.push([receiver,data]);};let db;
 try{
  if(!host.indexedDB){send({error:'indexeddb-unavailable'});return;}
  if(host.indexedDB.databases){const list=await host.indexedDB.databases();if(!list.some(d=>d.name==='firebaseLocalStorageDb')){send({});return;}}
  db=await new Promise((resolve,reject)=>{const r=host.indexedDB.open('firebaseLocalStorageDb');r.onupgradeneeded=()=>{r.transaction.abort();reject(new Error('absent'));};r.onerror=()=>reject(r.error);r.onsuccess=()=>resolve(r.result);});
  if(!db.objectStoreNames.contains('firebaseLocalStorage')){send({});return;}
  const cfg=host.DragonTowerFirebaseConfig,key='firebase:authUser:'+cfg.firebase.apiKey+':dragon-tower-'+cfg.environment;
  const row=await new Promise((resolve,reject)=>{const r=db.transaction('firebaseLocalStorage','readonly').objectStore('firebaseLocalStorage').get(key);r.onsuccess=()=>resolve(r.result);r.onerror=()=>reject(r.error);});
  const uid=row?.value?.uid;if(typeof uid==='string'&&uid.length>0&&uid.length<=128)send({owner:cfg.environment+':'+cfg.firebase.projectId+':'+uid});else send({});
 }catch(e){send(e?.message==='absent'?{}:{error:'local-identity-unreadable'});}finally{db?.close();}
}};})(window);
