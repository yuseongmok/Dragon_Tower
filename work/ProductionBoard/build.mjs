import fs from 'node:fs/promises';
import {Workbook,SpreadsheetFile} from '@oai/artifact-tool';
const dir=new URL('.',import.meta.url);
const tables=JSON.parse(await fs.readFile(new URL('board-data.json',dir),'utf8'));
const wb=Workbook.create();
function col(n){let s='';for(n++;n;n=Math.floor((n-1)/26))s=String.fromCharCode(65+(n-1)%26)+s;return s;}
for(const t of tables)wb.worksheets.add(t.name);
for(const t of tables){
 const sh=wb.worksheets.getItem(t.name);const last=col(t.headers.length-1),end=t.rows.length+4;
 sh.showGridLines=false;
 sh.getRange(`A1:${last}${end}`).format.font={name:'Arial',size:11,color:'#253247'};
 sh.getRange('A1').values=[[t.title]];sh.getRange('A1').format.font={name:'Arial',size:18,bold:true,color:'#172339'};
 sh.getRange('A2').values=[[t.subtitle]];sh.getRange('A2').format.font={name:'Arial',size:10,color:'#64748B'};
 sh.getRange('A1').format.rowHeight=30;sh.getRange('A2').format.rowHeight=23;
 sh.getRange(`A4:${last}${end}`).values=[t.headers,...t.rows];
 sh.getRange(`A4:${last}${end}`).format.wrapText=true;
 sh.getRange(`A4:${last}${end}`).format.verticalAlignment='center';
 sh.getRange(`A5:${last}${end}`).format.rowHeightPx=t.height;
 sh.getRange(`A4:${last}4`).format={fill:'#263D59',font:{name:'Arial',size:11,bold:true,color:'#FFFFFF'},rowHeightPx:42,wrapText:true};
 t.widths.forEach((w,i)=>sh.getRange(`${col(i)}1:${col(i)}${end}`).format.columnWidthPx=w);
 sh.tables.add(`A4:${last}${end}`,true,t.name.replaceAll(' ','')+'Table');
 sh.freezePanes.freezeRows(4);sh.freezePanes.freezeColumns(t.name==='Elements'?1:2);
 const status=sh.getRange(`A5:${last}${end}`);
 for(const [text,fill,color] of [['DONE','#E3F1E7','#23603D'],['NEEDS DESIGN','#FFF1D5','#805B1C'],['NEEDS REMAKE','#FCE9DF','#874C2E'],['NOT STARTED','#EDF0F5','#526078']])status.conditionalFormats.add('cellIs',{operator:'equal',formula:'"'+text+'"',format:{fill,font:{color}}});
}
const el=wb.worksheets.getItem('Elements');
for(let r=5;r<=12;r++){
 el.getRange(`B${r}`).formulas=[[`=COUNTIF('Board'!$B$5:$B$20,A${r})`]];
 el.getRange(`D${r}`).formulas=[[`=COUNTIFS('Board'!$B$5:$B$20,A${r},'Board'!$F$5:$F$20,"DONE")`]];
}
el.getRange('A15:C20').values=[['작업량','수량','의미'],['전체 드래곤',null,'제피르 포함'],['완료 드래곤',null,'3형태 캐릭터 세트 기준'],['남은 형태',null,'드래곤당 3형태'],['남은 상태 클립',null,'형태당 6상태'],['프레임 산정 예시',null,'상태당 4프레임 가정. 확정 견적 아님']];
el.getRange('B16:B20').formulas=[['=SUM(B5:B12)'],['=SUM(D5:D12)'],['=(B16-B17)*3'],['=B18*6'],['=B19*4']];
el.getRange('A15:C20').format.rowHeightPx=36;el.getRange('C15:C20').format.wrapText=true;el.getRange('C16:C20').format.rowHeightPx=55;
el.getRange('A15:C15').format={fill:'#E6ECF3',font:{bold:true,color:'#253247'}};
const sk=wb.worksheets.getItem('Skill Data');sk.getRange('E5:E38').setNumberFormat('0.0');sk.getRange('F5:F38').setNumberFormat('0');sk.getRange('G5:H38').setNumberFormat('0.00');sk.getRange('J5:J38').setNumberFormat('0');
const check=await wb.inspect({kind:'table',range:'Elements!A5:E12',include:'values,formulas',tableMaxRows:8,tableMaxCols:5});console.log(check.ndjson);
console.log((await wb.inspect({kind:'match',searchTerm:'#REF!|#DIV/0!|#VALUE!|#NAME\\?|#N/A|#NUM!|#NULL!|#SPILL!|#CALC!',options:{useRegex:true,maxResults:20},summary:'Formula scan'})).ndjson);
await fs.mkdir(new URL('previews/',dir),{recursive:true});
for(const t of tables){
 const png=await wb.render({sheetName:t.name,range:`A1:${col(t.headers.length-1)}${t.name==='Elements'?12:8}`,scale:1,format:'png'});
 await fs.writeFile(new URL('previews/'+t.name.replaceAll(' ','_')+'.png',dir),new Uint8Array(await png.arrayBuffer()));
}
const out=await SpreadsheetFile.exportXlsx(wb);await out.save(new URL('ProductionBoard.xlsx',dir).pathname.replace(/^\/([A-Za-z]:)/,'$1'));
console.log('Saved ProductionBoard.xlsx');
