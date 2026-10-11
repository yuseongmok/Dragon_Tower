const assert=require('node:assert/strict'),fs=require('node:fs'),vm=require('node:vm'),crypto=require('node:crypto');
const {loginEmail}=require('../Assets/WebGLTemplates/DragonTower/firebase-bridge.js');
assert.equal(loginEmail(' Bolt_01 '),'bolt_01@login.dragontower.invalid');
assert.equal(loginEmail('BOLT_01'),loginEmail('bolt_01'));
for(const id of ['', 'a','1234','a@b.com','가나다라','ab cd','abcd/xx','a'.repeat(21)])assert.throws(()=>loginEmail(id));
let docs=new Map(),sent=[],now=1000000000,userLookup=0;
const ref=path=>({path,async get(){return {data:()=>docs.get(path)};},async set(v,opt){docs.set(path,opt?.merge?{...docs.get(path),...v}:v);}});
const db={doc:ref,async runTransaction(fn){return fn({get:r=>r.get(),set:(r,v)=>docs.set(r.path,v),update:(r,v)=>docs.set(r.path,{...docs.get(r.path),...v})});}};
class HttpsError extends Error {constructor(code,message){super(message);this.code=code;}}
const modules={
 'firebase-functions/v2/https':{HttpsError,onCall:(options,fn)=>{assert.equal(options.enforceAppCheck,true);return fn;}},
 'firebase-functions/params':{defineSecret:()=>({value:()=>JSON.stringify({host:'test',from:'test@test.test',auth:{user:'x',pass:'y'}})})},
 'firebase-admin/app':{initializeApp(){}},'firebase-admin/firestore':{getFirestore:()=>db},
 'firebase-admin/auth':{getAuth:()=>({async getUserByEmail(email){userLookup++;if(email!=='bolt_01@login.dragontower.invalid')throw {code:'auth/user-not-found'};return {uid:'u1'};},async generatePasswordResetLink(){return 'https://test.invalid/reset';}})},
 'node:crypto':{...crypto,randomInt:()=>123456},'nodemailer':{createTransport:()=>({async sendMail(v){sent.push(v);},close(){}})}
};
const ctx={require:id=>modules[id],exports:{},Date:{now:()=>now},console};vm.runInNewContext(fs.readFileSync(__dirname+'/functions/index.js','utf8'),ctx);
const call=(data,uid='u1',age=0)=>ctx.exports.trainerRecovery({data,rawRequest:{ip:'local-test'},auth:uid?{uid,token:{auth_time:now/1000-age,firebase:{sign_in_provider:'password'}}}:null});
const rejects=(p,code)=>assert.rejects(p,e=>e.code===code);
(async()=>{
 await rejects(call({op:'recovery-send',email:'a@test.test'},null),'unauthenticated');
 await rejects(call({op:'recovery-send',email:'a@test.test'},'u1',301),'failed-precondition');
 await call({op:'recovery-send',email:'a@test.test'});assert.equal(sent.length,1);assert(!JSON.stringify(docs.get('accountRecovery/u1')).includes('123456'));
 await rejects(call({op:'recovery-send',email:'a@test.test'}),'resource-exhausted');
 await rejects(call({op:'recovery-confirm',code:'123456'},'u2'),'invalid-argument');
 for(let i=0;i<5;i++)await rejects(call({op:'recovery-confirm',code:'000000'}),'invalid-argument');
 await rejects(call({op:'recovery-confirm',code:'123456'}),'invalid-argument');
 now+=121000;await call({op:'recovery-send',email:'a@test.test'});await call({op:'recovery-confirm',code:'123456'});
 assert.equal(docs.get('accountRecovery/u1').verifiedEmail,'a@test.test');
 await rejects(call({op:'recovery-confirm',code:'123456'}),'invalid-argument');
 now+=121000;await call({op:'reset',loginId:'BOLT_01'},null);assert.equal(sent.at(-1).to,'a@test.test');
 now+=121000;await call({op:'reset',loginId:'unknown'},null);assert.equal(sent.length,3);
 now+=121000;await call({op:'recovery-send',email:'new@test.test'});now+=601000;await rejects(call({op:'recovery-confirm',code:'123456'}),'invalid-argument');assert.equal(docs.get('accountRecovery/u1').verifiedEmail,'a@test.test');
 console.log('USERNAME_RECOVERY_UNIT_OK normalization validation ownership recent-login rate-limit attempts expiry one-use optional-reset. Mock unit tests, not deployed Firebase/SMTP.');
})().catch(e=>{console.error(e);process.exitCode=1;});
