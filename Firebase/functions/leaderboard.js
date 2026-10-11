'use strict';
// This is a PLAYTEST board, not proof of a legitimate playthrough.
// Input arithmetic is checked against exported game data; combat still runs on the client.
const rules=require('./leaderboard-data.json');
const {createHash}=require('node:crypto');
const BOARD='leaderboardPlaytest';
const error=(code,message)=>Object.assign(new Error(message||code),{code});
const integer=(n,min,max)=>Number.isSafeInteger(n)&&n>=min&&n<=max;
function validate(r){
 if(!r||r.finalized!==true||!/^([a-f0-9]{32})$/.test(r.runId||''))throw error('invalid-argument','Finalized run required');
 if(!Object.hasOwn(rules.dragons,r.dragonId)||!integer(r.score,0,999999999)||!integer(r.maxFloor,1,80)||!integer(r.maxBond,1,999999))throw error('invalid-argument','Invalid record');
 if(!integer(r.normalKills,0,80)||!integer(r.bossKills,0,8)||r.normalKills+r.bossKills>r.maxFloor||r.bossKills>Math.floor(r.maxFloor/10))throw error('invalid-argument','Invalid kills');
 const a=r.achievements||[];
 if(!Array.isArray(a)||new Set(a).size!==a.length||a.some(x=>!Object.hasOwn(rules.achievements,x)))throw error('invalid-argument','Invalid achievements');
 if(r.score!==r.normalKills*rules.normalKill+r.bossKills*rules.bossKill+r.maxFloor*rules.newFloor+a.reduce((s,x)=>s+rules.achievements[x],0))throw error('invalid-argument','Score does not match counters');
 const name=r.customDragonName||'';
 if(typeof name!=='string'||name!==name.trim()||[...name].length>12||name&&!/^[\p{L}\p{N} _-]+$/u.test(name))throw error('invalid-argument','Invalid dragon name');
 return {runId:r.runId,dragonId:r.dragonId,dragonName:name,originalDragonName:rules.dragons[r.dragonId],bestScore:r.score,bestFloor:r.maxFloor,bestBond:r.maxBond};
}
function better(a,b){return !b||a.bestScore>b.bestScore||a.bestScore===b.bestScore&&(a.bestFloor>b.bestFloor||a.bestFloor===b.bestFloor&&a.bestBond>b.bestBond);}
// One order key drives both top-100 ordering and rank aggregation. Earliest server acceptance wins ties.
function rankKey(r,time,uid){return [String(999999999-r.bestScore).padStart(9,'0'),String(80-r.bestFloor).padStart(2,'0'),String(999999-r.bestBond).padStart(6,'0'),String(time).padStart(15,'0'),uid].join(':');}
function publicRecord(d){if(!d)return null;return Object.fromEntries(['uid','runId','playerName','dragonId','dragonName','originalDragonName','bestScore','bestFloor','bestBond','verification','achievedAtMs'].map(k=>[k,d[k]]));}
function createService(db,Timestamp){
 async function submit(uid,raw){
  const record=validate(raw),digest=createHash('sha256').update(JSON.stringify(record)).digest('hex');
  const best=db.doc(BOARD+'/'+uid),receipt=db.doc('leaderboardSubmissions/'+uid+'/runs/'+record.runId);
  return db.runTransaction(async tx=>{
   const [deleted,profile,seen,previous,rate]=await Promise.all([tx.get(db.doc('accountDeletions/'+uid)),tx.get(db.doc('users/'+uid)),tx.get(receipt),tx.get(best),tx.get(db.doc('leaderboardLimits/'+uid))]);
   if(deleted.exists)throw error('failed-precondition','Account deleted');
   if(!profile.exists)throw error('failed-precondition','Nickname required');
   if(seen.exists){if(seen.data().digest!==digest)throw error('already-exists','Run ID reused with different data');return {accepted:true,improved:seen.data().improved};}
   const now=Timestamp.now(),ms=now.toMillis();
   if((rate.data()?.nextAt||0)>ms)throw error('resource-exhausted','Try later');
   const improved=better(record,previous.data());
   if(improved)tx.set(best,{...record,uid,playerName:profile.data().displayName,recordVersion:1,verification:'client-reported',achievedAt:now,achievedAtMs:ms,rankKey:rankKey(record,ms,uid)});
   tx.set(receipt,{digest,improved,receivedAt:now});
   tx.set(db.doc('leaderboardLimits/'+uid),{nextAt:ms+2000});
   return {accepted:true,improved};
  });
 }
 async function read(uid){
  if((await db.doc('accountDeletions/'+uid).get()).exists)throw error('failed-precondition','Account deleted');
  // A single read-only transaction provides a consistent snapshot for rows, self and count.
  return db.runTransaction(async tx=>{
   const top=await tx.get(db.collection(BOARD).orderBy('rankKey','asc').limit(100));
   const me=await tx.get(db.doc(BOARD+'/'+uid));
   let myRank=0;
   if(me.exists){const count=await tx.get(db.collection(BOARD).where('rankKey','<',me.data().rankKey).count());myRank=count.data().count+1;}
   return {entries:top.docs.map(d=>publicRecord(d.data())),mine:publicRecord(me.data()),myRank,board:'playtest',readAtMs:Date.now()};
  },{readOnly:true});
 }
 async function syncName(uid){
  await db.runTransaction(async tx=>{const ref=db.doc(BOARD+'/'+uid);const [p,b,d]=await Promise.all([tx.get(db.doc('users/'+uid)),tx.get(ref),tx.get(db.doc('accountDeletions/'+uid))]);if(b.exists&&p.exists&&!d.exists)tx.update(ref,{playerName:p.data().displayName});});
 }
 return {submit,read,syncName};
}
module.exports={BOARD,validate,better,rankKey,createService};
