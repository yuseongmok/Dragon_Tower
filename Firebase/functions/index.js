'use strict';
const {onCall,HttpsError}=require('firebase-functions/v2/https');
const {defineSecret}=require('firebase-functions/params');
const {initializeApp}=require('firebase-admin/app');
const {getAuth}=require('firebase-admin/auth');
const {getFirestore}=require('firebase-admin/firestore');
const {randomInt,createHash}=require('node:crypto');
const nodemailer=require('nodemailer');
initializeApp();
const {Timestamp}=require('firebase-admin/firestore');
const {onDocumentWritten}=require('firebase-functions/v2/firestore');
const leaderboard=require('./leaderboard');
const boardService=leaderboard.createService(getFirestore(),Timestamp);
exports.onlineLeaderboard=onCall({region:'asia-northeast3',enforceAppCheck:true,timeoutSeconds:30,maxInstances:5},async req=>{
 if(!req.auth)throw new HttpsError('unauthenticated','Sign in required');
 const d=req.data||{};
 // UID is always taken from the verified token, never from submitted data.
 if(d.uid&&d.uid!==req.auth.uid)throw new HttpsError('permission-denied','Wrong owner');
 try {
  if(d.op==='submit')return await boardService.submit(req.auth.uid,d.record);
  if(d.op==='read'){await throttle('leaderboard-read:'+req.auth.uid,5);return await boardService.read(req.auth.uid);}
  throw new HttpsError('invalid-argument','Unknown operation');
 }catch(e){if(e instanceof HttpsError)throw e;const allowed=['invalid-argument','already-exists','failed-precondition','resource-exhausted'];throw new HttpsError(allowed.includes(e.code)?e.code:'internal',allowed.includes(e.code)?e.message:'Leaderboard unavailable');}
});
exports.leaderboardNameSync=onDocumentWritten({document:'users/{uid}',region:'asia-northeast3'},async event=>{if(event.data?.after.exists)await boardService.syncName(event.params.uid);});
exports.leaderboardAccountCleanup=onDocumentWritten({document:'accountDeletions/{uid}',region:'asia-northeast3'},async event=>{
 if(!event.data?.after.exists)return;const db=getFirestore(),uid=event.params.uid;
 await db.doc(leaderboard.BOARD+'/'+uid).delete();
 await db.recursiveDelete(db.doc('leaderboardSubmissions/'+uid));
 await db.doc('leaderboardLimits/'+uid).delete();
});
const smtp=defineSecret('TRAINER_SMTP');
const hash=s=>createHash('sha256').update(s).digest('hex');
function alias(value){const id=typeof value==='string'?value.trim().toLowerCase():'';if(!/^[a-z][a-z0-9_]{3,19}$/.test(id))throw new HttpsError('invalid-argument','Invalid ID');return id+'@login.dragontower.invalid';}
// These documents are server-only under the existing default-deny Firestore rules.
async function throttle(key,seconds){const db=getFirestore(),ref=db.doc('accountRateLimits/'+hash(key));await db.runTransaction(async tx=>{const old=await tx.get(ref),now=Date.now();if((old.data()?.nextAt||0)>now)throw new HttpsError('resource-exhausted','Try later');tx.set(ref,{nextAt:now+seconds*1000});});}
async function mail(to,subject,text){const cfg=JSON.parse(smtp.value());if(!cfg.host||!cfg.from||!cfg.auth?.user||!cfg.auth?.pass)throw new HttpsError('failed-precondition','Mail not configured');const transport=nodemailer.createTransport({host:cfg.host,port:cfg.port||465,secure:cfg.secure!==false,auth:cfg.auth,requireTLS:true,connectionTimeout:10000,socketTimeout:15000});try{await transport.sendMail({from:cfg.from,to,subject,text});}finally{transport.close();}}
exports.trainerRecovery=onCall({region:'asia-northeast3',enforceAppCheck:true,secrets:[smtp],timeoutSeconds:30,maxInstances:5},async req=>{
 const d=req.data||{},db=getFirestore(),auth=getAuth();
 if(d.op==='reset'){
  const address=alias(d.loginId);
  await throttle('reset-ip:'+req.rawRequest.ip,60);
  await throttle('reset-account:'+address,120);
  // Never expose whether the ID exists or has a recovery email.
  try{const user=await auth.getUserByEmail(address);if((await db.doc('accountDeletions/'+user.uid).get()).exists)return {ok:true};const recovery=(await db.doc('accountRecovery/'+user.uid).get()).data();if(recovery?.verifiedEmail){const link=await auth.generatePasswordResetLink(address);await mail(recovery.verifiedEmail,'유대의 탑 · 비밀번호 재설정','요청한 계정의 비밀번호를 변경하려면 아래 링크를 사용하세요. 요청하지 않았다면 무시하세요.\n'+link);}}catch(e){if(e.code!=='auth/user-not-found')console.warn('Recovery delivery unavailable');}
  return {ok:true};
 }
 if(!['recovery-send','recovery-confirm'].includes(d.op))throw new HttpsError('invalid-argument','Unknown operation');
 if(!req.auth||req.auth.token.firebase?.sign_in_provider==='anonymous')throw new HttpsError('unauthenticated','Linked account required');
 // A stolen old session must not be enough to change password recovery ownership.
 if(Date.now()/1000-(req.auth.token.auth_time||0)>300)throw new HttpsError('failed-precondition','Sign in again before changing recovery email');
 const uid=req.auth.uid,ref=db.doc('accountRecovery/'+uid);
 if(d.op==='recovery-send'){
  const email=typeof d.email==='string'?d.email.trim():'';
  if(email.length>254||! /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)||email.toLowerCase().endsWith('.invalid'))throw new HttpsError('invalid-argument','Invalid email');
  await throttle('verify-ip:'+req.rawRequest.ip,30);await throttle('verify-user:'+uid,60);await throttle('verify-email:'+email.toLowerCase(),60);
  const code=String(randomInt(0,1000000)).padStart(6,'0');
  await db.runTransaction(async tx=>{if((await tx.get(db.doc('accountDeletions/'+uid))).exists)throw new HttpsError('failed-precondition','Account deletion pending');tx.set(ref,{...(await tx.get(ref)).data(),pendingEmail:email,codeHash:hash(uid+':'+code),expiresAt:Date.now()+600000,attempts:0});});
  await mail(email,'유대의 탑 · 복구 이메일 인증','인증번호: '+code+'\n10분 안에 게임에서 입력하세요. 요청하지 않았다면 무시하세요.');return {ok:true};
 }
 if(typeof d.code!=='string'||!/^\d{6}$/.test(d.code))throw new HttpsError('invalid-argument','Invalid code');
 const accepted=await db.runTransaction(async tx=>{if((await tx.get(db.doc('accountDeletions/'+uid))).exists)return false;const data=(await tx.get(ref)).data();if(!data||data.expiresAt<Date.now()||data.attempts>=5||!data.pendingEmail)return false;
  if(data.codeHash!==hash(uid+':'+d.code)){tx.update(ref,{attempts:data.attempts+1});return false;}
  tx.set(ref,{verifiedEmail:data.pendingEmail,verifiedAt:Date.now()});return true;
 });
 if(!accepted)throw new HttpsError('invalid-argument','Invalid or expired code');return {ok:true};
});
