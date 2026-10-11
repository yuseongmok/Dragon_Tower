'use strict';
// Run only with demo-dragon-leaderboard emulator project. Never set production project IDs here.
const assert=require('node:assert/strict'),fs=require('node:fs');
const {initializeTestEnvironment,assertFails}=require('@firebase/rules-unit-testing');
const {initializeApp:clientApp}=require('firebase/app');
const {getAuth,connectAuthEmulator,createUserWithEmailAndPassword}=require('firebase/auth');
const {getFirestore:clientDb,connectFirestoreEmulator,doc,setDoc,deleteDoc,writeBatch}=require('firebase/firestore');
const {initializeApp}=require('firebase-admin/app');
const {getAuth:adminAuth}=require('firebase-admin/auth');
const {getFirestore,Timestamp}=require('firebase-admin/firestore');
const {createService,BOARD,rankKey}=require('./functions/leaderboard');
const projectId='demo-dragon-leaderboard';
if(process.env.FIRESTORE_EMULATOR_HOST!=='127.0.0.1:8088'||process.env.FIREBASE_AUTH_EMULATOR_HOST!=='127.0.0.1:9098')throw Error('Explicit local emulator endpoints required');
const run=(id,floor=10,kills=3,bond=5,dragon='ember')=>({runId:id.toString(16).padStart(32,'0'),finalized:true,score:floor*50+kills*10,maxFloor:floor,maxBond:bond,normalKills:kills,bossKills:0,dragonId:dragon,customDragonName:'내드래곤',achievements:[]});
(async()=>{
 const env=await initializeTestEnvironment({projectId,firestore:{host:'127.0.0.1',port:8088,rules:fs.readFileSync(__dirname+'/firestore.rules','utf8')}});
 await env.clearFirestore();const admin=initializeApp({projectId});const db=getFirestore(admin),service=createService(db,Timestamp);
 async function account(name){const app=clientApp({projectId,apiKey:'fake-emulator-key',appId:'emulator-'+name},name);const auth=getAuth(app);connectAuthEmulator(auth,'http://127.0.0.1:9098',{disableWarnings:true});const cred=await createUserWithEmailAndPassword(auth,name+'@example.test','test-only-password');const token=await adminAuth(admin).verifyIdToken(await cred.user.getIdToken());assert.equal(token.uid,cred.user.uid);const store=clientDb(app);connectFirestoreEmulator(store,'127.0.0.1',8088);await db.doc('users/'+token.uid).set({displayName:name});return {uid:token.uid,store};}
 const a=await account('rankalice'),b=await account('rankbob');
 await service.submit(a.uid,run(1));await service.submit(b.uid,run(2));
 let r=await service.read(a.uid);assert.equal(r.entries.length,2);assert.equal(r.myRank,1);
 await service.submit(a.uid,run(1));assert.equal((await db.collection('leaderboardSubmissions/'+a.uid+'/runs').get()).size,1);
 await assert.rejects(service.submit(a.uid,{...run(1),maxBond:6}));
 await db.doc('leaderboardLimits/'+a.uid).delete();await service.submit(a.uid,run(3,1,0,1,'okta'));assert.equal((await service.read(a.uid)).mine.dragonId,'ember');
 await db.doc('leaderboardLimits/'+a.uid).delete();await service.submit(a.uid,run(4,20,4,12,'okta'));r=await service.read(a.uid);assert.equal(r.mine.dragonId,'okta');assert.equal(r.mine.bestFloor,20);assert.equal(r.mine.bestScore,1040);
 await db.doc('users/'+a.uid).update({displayName:'변경닉네임'});await service.syncName(a.uid);assert.equal((await service.read(a.uid)).mine.playerName,'변경닉네임');
 await assertFails(setDoc(doc(a.store,BOARD,a.uid),{bestScore:999999}));await assertFails(setDoc(doc(a.store,BOARD,b.uid),{bestScore:999999}));await assertFails(deleteDoc(doc(a.store,BOARD,a.uid)));
 const batch=db.batch();for(let i=0;i<105;i++){const uid='synthetic'+i,entry={uid,bestScore:100000,bestFloor:80,bestBond:40};batch.set(db.doc(BOARD+'/'+uid),{...entry,rankKey:rankKey(entry,100,uid)});}await batch.commit();r=await service.read(b.uid);assert.equal(r.entries.length,100);assert.equal(r.myRank,107);assert.equal(r.mine.uid,b.uid);
 // The owner can remove the public record only together with a deletion tombstone.
 const deletion=writeBatch(a.store);deletion.set(doc(a.store,'accountDeletions',a.uid),{requested:true});deletion.delete(doc(a.store,BOARD,a.uid));await deletion.commit();assert.equal((await db.doc(BOARD+'/'+a.uid).get()).exists,false);await assert.rejects(service.submit(a.uid,run(5)));
 console.log('LEADERBOARD_EMULATOR_OK Firestore rules compiled; two Auth emulator accounts; verified token UIDs; transactions; top100; exact aggregate rank107; retry idempotency; lower score protected; different dragon; nickname sync; own/other score write denied; deletion gate. Production not tested.');
 await env.cleanup();await db.terminate();process.exit(0);
})().catch(e=>{console.error(e);process.exit(1);});
