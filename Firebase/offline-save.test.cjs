const fs=require('node:fs'),vm=require('node:vm'),assert=require('node:assert/strict');
const source=fs.readFileSync('Assets/WebGLTemplates/DragonTower/offline-save-bridge.js','utf8');
async function test({absent=false,uid='local-uid',broken=false,ready=true}={}){
 let replies=[],closed=false,readKey='',mode='',opens=0;
 const db={objectStoreNames:{contains:()=>true},transaction:(_store,m)=>{mode=m;return {objectStore:()=>({get:key=>{readKey=key;const r={};queueMicrotask(()=>{if(broken){r.error=Error('disk');r.onerror();}else{r.result=uid?{value:{uid,stsTokenManager:{accessToken:'MUST_NOT_BE_SENT'}}}:undefined;r.onsuccess();}});return r;}})};},close:()=>closed=true};
 const host={DragonTowerFirebaseConfig:{enabled:false,environment:'development',firebase:{apiKey:'public',projectId:'test'}},indexedDB:{databases:async()=>absent?[]:[{name:'firebaseLocalStorageDb'}],open:()=>{opens++;const r={};queueMicrotask(()=>{r.result=db;r.onsuccess();});return r;}},fetch:()=>{throw Error('NETWORK FORBIDDEN');}};
 const receiver={SendMessage:(_r,_m,json)=>replies.push(JSON.parse(json))};if(ready)host.dragonTower=receiver;
 vm.runInNewContext(source,{window:host});await host.DragonTowerOffline.readIdentity('flow');
 if(!ready){assert.equal(replies.length,0);host.dragonTower=receiver;host.DragonTowerOffline.ready();}
 assert.equal(replies.length,1);assert(!JSON.stringify(replies).includes('MUST_NOT_BE_SENT'));
 if(absent){assert.equal(opens,0);assert.deepEqual(replies[0],{});}else{assert.equal(mode,'readonly');assert.equal(readKey,'firebase:authUser:public:dragon-tower-development');assert(closed);if(broken)assert(replies[0].error);else assert.deepEqual(replies[0],uid?{owner:'development:test:'+uid}:{});}
}
(async()=>{await test();await test({absent:true});await test({uid:null});await test({broken:true});await test({ready:false});const html=fs.readFileSync('Assets/WebGLTemplates/DragonTower/index.html','utf8');assert(!html.includes('src="firebase-bridge.js"'));assert(!/gstatic|googleapis|firebaseapp/.test(html));assert(fs.readFileSync('Assets/WebGLTemplates/DragonTower/firebase-config.js','utf8').includes('enabled: false'));console.log('OFFLINE_BRIDGE_OK local-only identity read, no SDK/import/network, missing cache, read error, early Unity callback queued, token never sent.');})().catch(e=>{console.error(e);process.exitCode=1;});
