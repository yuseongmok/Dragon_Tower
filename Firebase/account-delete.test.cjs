const assert=require('node:assert/strict'),vm=require('node:vm'),fs=require('node:fs');
let current={uid:'owner',email:'owner@login.dragontower.invalid',isAnonymous:false},calls=[],reply,dbFails=false,authFails=false;
const state={get currentUser(){return current;}};
const auth={getAuth:()=>state,browserLocalPersistence:{},async setPersistence(){},EmailAuthProvider:{credential:(email,password)=>({email,password})},async reauthenticateWithCredential(user,credential){calls.push('reauth');if(credential.password!=='correct')throw {code:'auth/invalid-credential'};},async deleteUser(user){calls.push('auth-delete');if(authFails)throw Error('network');assert.equal(user.uid,'owner');current=null;}};
state.authStateReady=async()=>{};
const fire={getFirestore:()=>({}),doc:(_db,collection,uid)=>collection+'/'+uid,writeBatch:()=>{const staged=[];return {set:(ref,data)=>staged.push(['set',ref,data]),delete:ref=>staged.push(['delete',ref]),async commit(){calls.push('commit');if(dbFails)throw {code:'permission-denied'};calls.push(...staged);}}}};
const ctx={globalThis:null,DragonTowerFirebaseConfig:{enabled:true,environment:'development',firebase:{apiKey:'test',projectId:'test',appId:'test'}},dragonTower:{SendMessage(_r,_m,json){reply=JSON.parse(json);}},sdk:{'firebase-app.js':{initializeApp:()=>({})},'firebase-auth.js':auth,'firebase-firestore.js':fire},module:{exports:{}}};ctx.globalThis=ctx;
let source=fs.readFileSync(__dirname+'/../Assets/WebGLTemplates/DragonTower/firebase-bridge.js','utf8').replace(/import\(base\+'([^']+)'\)/g,"Promise.resolve(sdk['$1'])");vm.runInNewContext(source,ctx);
async function request(overrides={}){calls=[];await ctx.DragonTowerFirebase.request('test',JSON.stringify({id:'test',op:'delete-account',uid:'owner',code:'삭제',password:'correct',...overrides}));return reply;}
(async()=>{
 assert.equal((await request({uid:'other'})).code,'account-changed');assert.equal(calls.length,0);
 assert.equal((await request({code:''})).code,'delete-confirmation-required');assert.equal(calls.length,0);
 assert.equal((await request({password:'wrong'})).code,'auth/invalid-credential');assert.deepEqual(calls,['reauth']);
 dbFails=true;assert.equal((await request()).code,'permission-denied');assert(!calls.includes('auth-delete'));dbFails=false;
 authFails=true;assert.equal((await request()).code,'delete-auth-incomplete');assert(current);authFails=false;
 assert.equal((await request()).deleted,true);assert.equal(current,null);assert(calls.indexOf('commit')<calls.indexOf('auth-delete'));assert(calls.some(c=>Array.isArray(c)&&c[1]==='users/owner'));assert(calls.some(c=>Array.isArray(c)&&c[1]==='accountRecovery/owner'));
 current={uid:'owner',isAnonymous:true};assert.equal((await request({password:''})).deleted,true);assert(!calls.includes('reauth'));
 console.log('ACCOUNT_DELETE_UNIT_OK owner confirmation password failure ordering partial-failure retry guest. No live accounts deleted.');
})().catch(e=>{console.error(e);process.exitCode=1;});
