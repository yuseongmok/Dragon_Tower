// Unity can synchronously issue a load inside an authentication completion callback.
// Neither success nor failure callbacks may leave the bridge locked during dispatch.
const assert=require('node:assert/strict');
require('../Assets/WebGLTemplates/DragonTower/firebase-bridge.js');
let nested,replies=[];
global.dragonTower={SendMessage(_receiver,_method,json){const r=JSON.parse(json);replies.push(r);if(r.id==='outer')nested=global.DragonTowerFirebase.request('test',JSON.stringify({id:'inner',op:'init'}));}};
(async()=>{await global.DragonTowerFirebase.request('test',JSON.stringify({id:'outer',op:'init'}));await nested;assert.equal(replies.length,2);assert.equal(replies[1].code,'not-configured');assert.notEqual(replies[1].code,'busy');console.log('BRIDGE_CALLBACK_REENTRY_OK');})().catch(e=>{console.error(e);process.exitCode=1;});
