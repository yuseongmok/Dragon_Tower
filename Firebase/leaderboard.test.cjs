'use strict';
const assert=require('node:assert/strict');
const {createService,validate,rankKey,BOARD}=require('./functions/leaderboard');
// In-memory transaction double: no production accounts, credentials or documents are touched.
let now=1800000000000;const data=new Map();const copy=x=>x===undefined?undefined:structuredClone(x);
function snap(path){const value=copy(data.get(path));return {exists:value!==undefined,data:()=>value};}
function query(path,filter,limit=Infinity){return {orderBy:()=>query(path,filter,limit),limit:n=>query(path,filter,n),where:(field,op,value)=>query(path,d=>d[field]<value,limit),count:()=>({aggregate:true,query:query(path,filter,limit)}),path,filter,limitValue:limit};}
const db={doc:path=>({path,get:async()=>snap(path)}),collection:path=>query(path),async runTransaction(fn){const pending=[];const result=await fn({async get(ref){if(ref.aggregate){const r=await this.get(ref.query);return {data:()=>({count:r.docs.length})};}if(ref.limitValue!==undefined){return {docs:[...data].filter(([k,v])=>k.startsWith(ref.path+'/')&&!k.slice(ref.path.length+1).includes('/')&&(!ref.filter||ref.filter(v))).sort((a,b)=>a[1].rankKey<b[1].rankKey?-1:1).slice(0,ref.limitValue).map(([k])=>snap(k))};}return snap(ref.path);},set:(ref,value)=>pending.push(()=>data.set(ref.path,copy(value))),update:(ref,value)=>pending.push(()=>data.set(ref.path,{...data.get(ref.path),...copy(value)}))});pending.forEach(f=>f());return result;}};
const service=createService(db,{now:()=>({toMillis:()=>now})});
// Keep Timestamp out of stored mock objects (the production SDK accepts Timestamp instances).
const oldClone=global.structuredClone;global.structuredClone=x=>JSON.parse(JSON.stringify(x));
const run=(id,floor=10,kills=3,bond=5,dragon='ember')=>({runId:id.toString(16).padStart(32,'0'),finalized:true,score:floor*50+kills*10,maxFloor:floor,maxBond:bond,normalKills:kills,bossKills:0,dragonId:dragon,customDragonName:'불꽃친구',achievements:[]});
(async()=>{
 data.set('users/alice',{displayName:'첫플레이어'});data.set('users/bob',{displayName:'둘째플레이어'});
 await service.submit('alice',run(1));await service.submit('bob',run(2));
 let r=await service.read('alice');assert.equal(r.entries.length,2);assert.equal(r.myRank,1);assert.equal(r.entries[0].uid,'alice');
 now+=3000;assert.equal((await service.submit('alice',run(1))).improved,true);assert.equal(data.get(BOARD+'/alice').runId,run(1).runId);
 await assert.rejects(service.submit('alice',{...run(1),maxBond:9}),e=>e.code==='already-exists');
 assert.equal((await service.submit('alice',run(3,1,0,1,'okta'))).improved,false);assert.equal(data.get(BOARD+'/alice').dragonId,'ember');
 now+=3000;assert.equal((await service.submit('alice',run(4,20,4,12,'okta'))).improved,true);assert.equal(data.get(BOARD+'/alice').dragonId,'okta');assert.equal(data.get(BOARD+'/alice').bestFloor,20);
 data.set('users/alice',{displayName:'바뀐닉네임'});await service.syncName('alice');assert.equal((await service.read('alice')).mine.playerName,'바뀐닉네임');
 assert.throws(()=>validate({...run(5),finalized:false}));assert.throws(()=>validate({...run(5),score:999}));assert.throws(()=>validate({...run(5),dragonId:'unknown'}));
 const tie={bestScore:100,bestFloor:5,bestBond:2};assert.ok(rankKey({...tie,bestScore:101},2,'z')<rankKey(tie,1,'a'));assert.ok(rankKey({...tie,bestFloor:6},2,'z')<rankKey(tie,1,'a'));assert.ok(rankKey({...tie,bestBond:3},2,'z')<rankKey(tie,1,'a'));assert.ok(rankKey(tie,1,'z')<rankKey(tie,2,'a'));assert.ok(rankKey(tie,1,'a')<rankKey(tie,1,'b'));
 for(let i=0;i<105;i++){const uid='synthetic'+i;const record={...tie,uid,bestScore:100000,rankKey:rankKey({...tie,bestScore:100000},now,uid)};data.set(BOARD+'/'+uid,record);}
 r=await service.read('bob');assert.equal(r.entries.length,100);assert.equal(r.myRank,107);assert.equal(r.mine.uid,'bob');
 data.set('accountDeletions/alice',{requested:true});now+=3000;await assert.rejects(service.submit('alice',run(6)),e=>e.code==='failed-precondition');await assert.rejects(service.read('alice'));
 const faulty=createService({runTransaction:async()=>{throw Error('network');}},{});await assert.rejects(faulty.submit('bob',run(7)));assert.equal(data.get(BOARD+'/bob').runId,run(2).runId);
 console.log('LEADERBOARD_SERVER_MOCK_OK two UIDs, best update, lower retained, idempotency, conflict, atomic dragon snapshot, nickname, all tie keys, top100 + rank107, invalid records, deletion, network failure. NOT live Firebase.');
})().catch(e=>{console.error(e);process.exitCode=1;});
