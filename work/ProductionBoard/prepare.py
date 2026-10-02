import json, re
from pathlib import Path
from collections import Counter

work=Path(__file__).parent
inv=json.loads((work/'inventory.json').read_text(encoding='utf-8'))
ds=inv['dragons']; ss=inv['skills']; bypath={s['_path']:s for s in ss}
order=['Ice','Fire','Earth','Lightning','Light','Wind','Water','Dark']
rank={e:i+1 for i,e in enumerate(order)}
ds.sort(key=lambda d:(rank[d['elementName']],d['speciesId']))
states=['Idle','Attack','Skill','Dodge','Hit','Death']
notes={
'luna':'사족형 중간형과 지팡이·직립 최종형의 포즈 전환 검수',
'aurora':'사족형 재사용 적합. 큰 날개·장식의 픽셀 밀도와 외곽 정리',
'ember':'중간형 이족 자세와 최종형 사족 자세 차이. 몸에 그려진 불꽃은 캐릭터 참조',
'sol':'깃털 날개와 공중 자세의 지면 정렬 검수',
'titan':'이족형의 무게 중심·큰 꼬리·발 정렬 검수',
'brandy':'이족형 등갑·발톱 실루엣. 기존 데이터 이름은 블랜디',
'volt':'이족형·최종형 무기. 작은 전투 화면에서 무기 포함 폭 검수',
'storm':'주속성 Lightning, Wind 스킬도 학습 가능. 추가 Pool 연결 시 유지 필요',
'chrono':'사족형. 시간 장식은 캐릭터 참조이며 전용기 기획은 미정',
'dante':'중간 이족/최종 사족 자세와 날개·장식 폭 검수',
'mir':'긴 몸·부유 자세·여의주. 발 하단 자동 정렬이 맞는지 별도 검수',
'zephyr':'3형태·5스킬 완료 기준. 폭풍참은 별도 기존 Common 데이터로 남아 있음',
'sharkid':'주속성 Water, Ice 스킬도 학습 가능. 추가 Pool 연결 시 유지 필요',
'okta':'촉수형. 최하단 픽셀 평균을 발로 보는 수입 규칙 검수',
'nova':'이족형. 날개·꼬리 장식 밀도 축소 여부는 캐릭터 제작 단계에서 판단',
'venom':'다두형. 프레임마다 머리 수와 실루엣 일관성 검수; 데이터 속성은 Dark'}
reasons={
'Ice':'2마리 모두 3형태 참조·기본 스킬 보유. 오로라와 루나 중간형은 검증된 사족형 절차와 가까움. 루나 최종형은 직립/지팡이 검수 필요.',
'Fire':'2마리·4개 공용 후보·참조 완비. 엠버/솔을 묶어 이족·날개 포즈 규칙 확보. 기본 전투 시스템은 그대로 사용 가능.',
'Earth':'2마리·4개 공용 후보·참조 완비. 티탄/블랜디의 이족·중량형을 Fire에서 검증한 정렬과 함께 처리.',
'Lightning':'2마리·4개 자체 후보. 볼트 이족/스톰 부유형 및 Wind 추가 학습 때문에 앞 그룹보다 연결 검수가 많음.',
'Light':'2마리·4개 공용 후보. 크로노 사족형은 재사용 용이, 단테 자세 전환·장식 단순화 확인. 신규 전투 시스템 필요는 아직 미확정.',
'Wind':'2마리 중 제피르 완료, 미르 1마리 잔여. 공용 Pool은 유일하게 존재하나 미르의 긴 몸/부유 기준점 검수가 필요. 순위는 새 속성 복제 검증을 먼저 하는 제안.',
'Water':'2마리·4개 자체 후보. 샤키드의 Ice 추가 학습과 옥타 촉수형 때문에 공용 정렬·교차 속성 검사 후 진행 권장.',
'Dark':'2마리·4개 공용 후보. 베놈의 다두 프레임 일관성과 노바의 장식 밀도 검수가 많아 표준 제작 규칙 축적 후 진행 권장.'}

board=[]; character=[]; refs=[]
for d in ds:
 done=d['speciesId']=='zephyr';e=d['elementName'];name=d['displayName'];p='DONE' if done else f'P{rank[e]}'
 eligible=[s for s in ss if s['registered'] and s['elementName'] in [e,d['altName']] and s['rarityName']!='Legendary']
 pool=('WindShared 연결: 5개' if done else ('Wind 후보 5개 / Pool 미연결' if e=='Wind' else e+' 후보 4개 / Pool 미생성'))
 if d['altName']!='Neutral':pool+='; '+d['altName']+' 추가 '+str(sum(s['elementName']==d['altName'] for s in eligible))+'개'
 board.append([name,e,*(['DONE']*3 if done else ['NEEDS REMAKE']*3), 'DONE' if done else 'NOT STARTED',pool,'NEEDS DESIGN','DONE: 단천' if done else 'NEEDS DESIGN','DONE' if done else 'EXISTING',p,bypath[d['skillPath']]['displayName'],'EXISTING',notes[d['speciesId']]])
 for i,form in enumerate(['Base','Middle','Final']):
  character.append([name,e,form,'EXISTING','DONE' if done else 'NEEDS REMAKE',*(['DONE']*6 if done else ['NOT STARTED']*6),('DragonAnimations/Zephyr'+form if done else '미연결'),('기존 결과 보호' if done else '외형 정리 → Idle 검증 → 나머지 5상태 → 데이터 연결')])
 refs.append([name,d['speciesId'],d['_path'],d['battleSpritePath'],d['intermediateEvolutionSpritePath'],d['finalEvolutionSpritePath'],d['evolutionSheetPath'],d['intermediateName'],d['finalName'],d['passiveName'],d['description'],'새 6상태 세트' if done else '3형태 정적 이미지 + 공용 Transform/피격 연출'])

skills=[]
statusnames={'0':'없음','1':'Burn','2':'Paralyze','3':'Slow','4':'Stun','5':'Poison'}
for s in sorted(ss,key=lambda s:(rank[s['elementName']],s['rarityName'],s['skillId'])):
 hits=int(s.get('hitCount','1'));status=statusnames[s.get('statusEffect','0')];chance=float(s.get('statusChancePercent','0'))
 role=('단일 타격' if hits==1 else f'{hits}회 연속 타격')+(' + '+status if status!='없음' and chance else '')
 if s['skillId']=='skill_dantian':role+=' / 시전 보호 '+s['protectedCastDuration']+'초'
 use='Zephyr 전용 Signature' if s['rarityName']=='Legendary' else ('WindShared' if s['elementName']=='Wind' else '속성 필터 후보 / Pool 에셋 없음')
 skills.append([s['elementName'],s['displayName'],s['rarityName'],role,float(s['cooldown']),hits,float(s.get('hitInterval','.14')),float(s.get('initialHitDelay','0')),status, chance,use,'EXISTING' if s['battleVfxPath'] else '별도 코드 VFX 존재',s['skillId'],s['_path']])

phases=[
['0','GLOBAL 준비','제작 시작 전 1회','현재 파일/ID 기준 목록 고정. 기본형 수입·단계별 검사 도구 보완 범위 결정','기존 런타임을 복사하지 않는 최소 도구 계획','아래 Pipeline 탭의 제약 먼저 확인; 이번에는 구현 안 함'],
['A0','Character 모델 기준','속성 그룹당 2마리 묶음','각 드래곤 Base/Middle/Final 키 포즈를 먼저 비교','얼굴·팔다리·날개 수·픽셀 밀도·모바일 실루엣 승인','45형태를 한 번에 생성하지 않음. 그룹 단위 승인'],
['A1','Idle Batch','승인된 형태만','3형태 Idle → 같은 전투 프레임에서 크기/발/Origin 확인','캐릭터와 HUD/VFX 공간이 확보된 상태','수정은 해당 형태만. 기본형 세트를 먼저 연결'],
['A2','Animation Batch','같은 체형/상태끼리','Attack/Skill → Dodge/Hit/Death 순서. 기본 4프레임 규칙 출발','각 형태 6상태·기존 이벤트 시간·취소/사망 검사','첫 체형 샘플 통과 후 나머지에 적용. 프레임 증가는 근거 있을 때만'],
['B','Element Art Direction','속성당 1회','색/명도/형태/운동/물성/Trail/Impact/규모 규칙 신규 결정','속성 스타일 문서와 대표 최소 샘플 승인','현재는 NEEDS DESIGN. Wind의 색/모양을 다른 속성에 복제하지 않음'],
['C0','Skill Concept','속성 공용 후보 전체','기존 이름·역할·CD 구조 조사 → Common/Rare/Unique 위계 검토','스킬별 Concept → Visual Concept → VFX Structure 승인','현재 rarity를 임의로 재분류하지 않음. 수치 밸런싱 별도'],
['C1','Element Skill Batch','속성당 공용 제작','승인 구조로 공용 모듈/전용 실루엣 제작 및 기존 이벤트 연결','실제 피해 시점·모바일 화면·성능·다중 사용자 호환 검사','같은 속성 2마리용 VFX를 중복 제작하지 않음'],
['D','Dragon Signature Batch','드래곤별 1개씩','최종형과 정체성 확정 후 Legendary 개별 기획·구현','전용 소유권·차별적 Signature Fantasy·종료 복원 검사','제피르 제외 15개 NEEDS DESIGN. 현재 이름/효과/카메라 방식 미정'],
['E','통합·회귀 Batch','속성 그룹 완료 시','2마리 × 3형태 × 공용 스킬, 전용기는 소유자만 검사','입력/쿨타임/HP/HUD/사망/취소/WebGL/기준 기기 통과','공용 모듈 변경 시 영향받는 속성만 재검사; 마지막 전체 회귀'],
['F','완료 판정','검수 증거별','보드·경로·미해결 항목 갱신','DONE은 해당 범위 검수 완료일 때만','새 그룹은 이전 그룹의 공용 문제를 정리한 후 시작']]

pipeline=[
['GLOBAL','DragonData / Evolution Forms','EXISTING','ID, 0/1/2 단계, Visual/Animation Set/수치 override','재사용. ID·전투 시스템 복사 금지','Assets/Scripts/DragonData.cs'],
['GLOBAL','6상태 프레임 재생 / 입력·전투','EXISTING','공용 BattleAnimation, BattleController, BattleModel','Sprite Set 데이터 교체. 체형마다 전투 코드 분기 추가 금지','Assets/Scripts/BattleAnimation.cs'],
['GLOBAL','기본형 수입 준비','NOT STARTED','EvolutionSpriteSetup은 stage 1~2만 받고 기존 Base 세트를 요구','대량 제작 전 Base 수입/시간 원본 선택을 1회 공용화 검토','Assets/Editor/EvolutionSpriteSetup.cs'],
['GLOBAL','Foot/Anchor 규칙','EXISTING','하단 불투명 픽셀 평균으로 발 정렬; 진화별 VFX Anchor 필드 없음','부유·촉수·긴 몸 샘플 검증 후 필요한 경우에만 데이터 보정 확장','Assets/Editor/EvolutionSpriteSetup.cs'],
['GLOBAL','체형 분류','EXISTING','Small/LargeQuadruped, Biped, Flying, Serpentine enum은 메타데이터','자동 리깅/자동 모션 생성 기능으로 오해하지 않음','Assets/Scripts/DragonAnimationSet.cs'],
['GLOBAL','단계별 제작 검사','NEEDS DESIGN','현재 Inspector는 Signature 없음도 완성 전 연결 필요로 표시','A/C 단계 검수와 D 완료 검수를 분리하는 최소 도구 보완 계획. 미기획 Legendary는 오류 아님','Assets/Editor/DragonProductionInspector.cs'],
['GLOBAL','재사용 회귀 Harness','EXISTING','중간/최종 검사는 stage 인자 지원하나 Zephyr와 Wind 스킬 전제','드래곤/Pool/형태를 입력받는 공용 검사로 1회 보완 검토','Assets/Editor/ZephyrMiddleValidation.cs'],
['GLOBAL','VFX 연결/풀/피격·숫자','EXISTING','SkillCast/Skill/SkillHit 경로, 다단히트, 풀, 취소 처리','현재 Wind 컴포넌트 연결은 명시적. 모든 속성 자동 라우팅이 완성된 것은 아님','Assets/Scripts/BattleAnimation.cs'],
['PER ELEMENT','Element HUD Theme','EXISTING','8속성 Theme + Neutral fallback 존재','현재 HUD/정보색 유지. VFX 아트 방향과 별개','Assets/Resources/HudThemes'],
['PER ELEMENT','Element Skill Pool','EXISTING','WindShared 1개. 15마리는 Pool 없이 속성 필터 경로','7속성 Pool 생성·연결은 후속 제작. Storm Wind / Sharkid Ice 추가 Pool 보존','Assets/Scripts/DragonData.cs; Assets/Data/SkillPools/WindShared.asset'],
['PER ELEMENT','VFX Art Direction/공용 Skill','NEEDS DESIGN','기존 VFX는 Legacy. 이름·등급·역할·CD 데이터만 기획 입력','팔레트/물성/형태/운동을 속성별 설계. Common/Rare/Unique는 공용 제작','Assets/Scripts/SkillData.cs'],
['PER DRAGON','3형태 Visual + Animation','NOT STARTED','제피르 DONE, 나머지 15마리는 참조만 EXISTING','각 3형태 × 6상태 제작. 다른 드래곤 Sprite 확대/색상 교체로 대체하지 않음','Assets/Data/Dragon*.asset'],
['PER DRAGON','Legendary Signature','NEEDS DESIGN','단천만 구현·소유자 zephyr. 나머지 15개는 의도적으로 미기획','최종 외형/속성/정체성 기반 별도 기획. 공용 Pool에 넣지 않음','Assets/Data/Skills/skill_dantian.asset'],
['PER DRAGON','Signature 권한 검수','EXISTING','exclusiveDragonId + signatureSkill + CanEquip/CanOfferSkill 경로','동일 속성의 다른 드래곤에게 지급/장착되지 않는지 검사','Assets/Scripts/SkillData.cs; Assets/Scripts/DragonData.cs']]

elements=[]
for e in order:
 members=[d for d in ds if d['elementName']==e]; pool=[s for s in ss if s['elementName']==e and s['rarityName']!='Legendary']
 elements.append([e,None,' / '.join(d['displayName'] for d in members),None,len(pool),'WindShared EXISTING' if e=='Wind' else 'NOT STARTED','NEEDS DESIGN','EXISTING',f'P{rank[e]}',reasons[e]])

tables=[
{'name':'Elements','title':'Dragon Production Board','subtitle':'2026-10-02 · 현재 프로젝트 조사 / 제작 우선순위는 제안','headers':['Element','드래곤 수','Dragon','캐릭터 완료','공용 후보 수','명시적 Pool','New VFX Art Direction','UI Theme','권장 순서','근거 / 검수 부담'],'rows':elements,'widths':[100,90,190,100,110,190,190,110,100,640],'height':82},
{'name':'Board','title':'전체 드래곤 제작 현황','subtitle':'DONE 완료 / EXISTING 기존 자료·기능 있음 / NEEDS REMAKE 재제작 / NEEDS DESIGN 기획 필요 / NOT STARTED 미착수','headers':['Dragon','Element','Base','Middle','Final','Animation Set','Element Skill Pool','New VFX Art Direction','Legendary Signature','Pipeline Status','Priority','현재 기본 스킬','Legacy VFX','Notes'],'rows':board,'widths':[105,100,140,140,140,145,300,180,170,150,85,130,135,430],'height':78},
{'name':'Character Tasks','title':'형태별 캐릭터 작업','subtitle':'EXISTING 레퍼런스와 DONE 게임용 Animation Set은 다름. 각 상태는 기존 4프레임 규칙으로 시작하는 계획.','headers':['Dragon','Element','Form','Reference','Visual','Idle','Attack','Skill','Dodge','Hit','Death','Animation Set 경로','필요 작업'],'rows':character,'widths':[100,90,95,115,145,135,135,135,135,135,135,270,340],'height':46},
{'name':'Skill Data','title':'현재 Skill Data 목록','subtitle':'역할은 현재 hitCount/status 데이터 요약. 새 VFX 콘셉트 아님. CD 단위 초 / 피해량 밸런싱 제외 / 등급 원문 유지.','headers':['Element','Skill','현재 등급','현재 역할','Cooldown (s)','Hit Count','Hit Interval (s)','Initial Delay (s)','Status','Status Chance (%)','현재 Pool / 소유권','Legacy VFX','Stable ID','Source'],'rows':skills,'widths':[100,135,120,250,125,100,130,130,100,150,280,180,230,380],'height':60},
{'name':'References','title':'프로젝트 레퍼런스와 현재 연결','subtitle':'출처 경로는 C:/Users/PC/Dragon_Tower 기준. 16개 진화 시트 시각 확인 및 연결된 64개 이미지 파일 읽기 확인.','headers':['Dragon','Stable ID','Dragon Data','Base reference','Middle reference','Final reference','Evolution reference','Middle name','Final name','기존 Passive','기존 소개','현재 구현'],'rows':refs,'widths':[100,100,250,410,410,410,400,140,140,160,420,290],'height':90},
{'name':'Batch Plan','title':'Batch 제작 단계','subtitle':'속성별 소규모 묶음으로 A → B → C → D → E. A0 승인 전에 전체 프레임을 생성하지 않음.','headers':['Phase','Batch','단위','작업','완료 조건','주의 / 의존성'],'rows':phases,'widths':[80,210,210,470,430,450],'height':95},
{'name':'Pipeline','title':'재사용 범위와 공용 준비 작업','subtitle':'현 상태와 후속 계획을 분리했다. 이번 조사에서 코드는 변경하지 않았다.','headers':['Scope','항목','Status','실제 현재 상태','후속 제작에서 할 일','Source'],'rows':pipeline,'widths':[150,230,155,440,520,450],'height':95}]

guide='''드래곤 리뉴얼 Production Guide
조사 기준: 2026-10-02, C:\\Users\\PC\\Dragon_Tower

목적과 범위
기존 프로젝트 데이터/참조를 조사한 제작 계획이다. Sprite/Animation/VFX/스킬/전투/HUD를 변경하거나 생성하지 않았다.
ProductionBoard.xlsx의 Elements → Board → Character Tasks → Skill Data → References → Batch Plan → Pipeline 순으로 읽는다.
DONE은 해당 제작 범위의 완료 상태, EXISTING은 기존 자료/기능의 존재만 의미한다. 참조 이미지가 있어도 새 게임용 프레임이 완성된 것은 아니다.
NEEDS REMAKE는 기존 외형의 게임용 재제작, NEEDS DESIGN은 기획 필요, NOT STARTED는 제작 미착수다.
제피르 DONE은 현재 연결 에셋 및 직전 제작 검수 기록 기준이다. 이번 읽기 전용 조사에서 게임을 다시 실행하지 않았다.

조사 결과
등록 드래곤 16마리 / 실제 주속성 8개 / 각각 2마리. Neutral은 enum/Theme fallback으로만 있으며 Neutral 드래곤은 없다.
모든 드래곤에 Base/Middle/Final 분리 이미지와 Evolution 시트가 연결돼 있다. 64개 연결 이미지 모두 읽기 가능하다.
진화 시트 16장을 직접 확인했다. 15마리는 정적 3형태 이미지와 기존 공용 연출을 쓰고, 새 표준 Sprite Animation은 없다.
제피르는 Base/Middle/Final 모두 6상태 세트가 연결됐다. 나머지는 45형태, 45 Animation Set, 270 상태 클립이 필요하다.
상태당 4프레임을 유지한다면 1,080프레임 규모라는 산정 예시다. 확정 제작량/일정/비용 견적이 아니며 체형별 승인 후 정한다.
완성된 Base 세트, 시트 여백·발 정렬 검토는 후속 Evolution 수입의 선행 조건이다.

공용 Skill과 Legendary
현재 등록 Skill Data는 34개: 공용 후보 33개 + 제피르 전용 단천 1개.
Wind 후보는 질풍 강타(Common), 폭풍참(Common), 바람칼날(Rare), 질풍참(Rare), 귀참(Unique) 5개.
바람칼날의 Stable ID는 기존 skill_falling_flower를 유지한다. 표시명으로 중복 파일을 만들지 않는다.
폭풍참은 현재 실제로 남아 있는 별도 Common 데이터다. 삭제/제외/승격을 이번에 결정하지 않는다.
다른 7속성은 각 4개, 총 28개 스킬이 현재 모두 Common으로 저장돼 있다.
스킬 이름이 화려하거나 쿨타임이 길어도 Rare/Unique/Legendary로 임의 해석하지 않는다.
향후 Common/Rare/Unique 역할 위계는 Skill Concept 단계에서 검토한다. 이번에는 현재 등급과 CD/타격 구조만 기록했다.
명시적 ElementSkillPool 에셋은 WindShared 하나이며 제피르만 연결돼 있다.
나머지 15마리는 ContentDatabase.skills를 속성/소유권으로 거르는 기존 경로다. 미르는 Wind 후보 5개를 사용 가능하다.
스톰은 Lightning + Wind(4+5개), 샤키드는 Water + Ice(4+4개) 학습 경로를 갖는다. 주속성 그룹에는 각각 한 번만 집계한다.
Pool 연결 시 additionalSkillPools를 이용해 이 기존 권한을 보존해야 한다. 단천은 다른 Wind 사용자에게도 허용되지 않는다.
Legendary는 각 드래곤만의 Signature다. 제피르를 제외한 15마리는 모두 '신규 기획 필요'이며 누락 오류가 아니다.

Legacy VFX와 신규 아트 방향
기존 외부 Prefab과 현재 VFX 구현은 존재 여부를 기록하는 Legacy 자료다. 새 디자인의 제약이나 형태 참고로 사용하지 않는다.
현재 32개 Skill Data에 외부 battleVfx 참조가 남아 있지만 이 참조가 실제 활성 표시를 뜻하지는 않는다.
Wind 스킬에는 코드 기반 전용 표시 경로가 있어 오래된 Prefab 참조를 우회한다. 따라서 Prefab 경로로 화면 형태를 추론하지 않았다.
모든 속성의 향후 공용 VFX Art Direction은 NEEDS DESIGN으로 둔다. Wind는 제피르에서 검증한 제작 철학이 있는 상태다.
제피르의 승인된 스킬을 다시 만들겠다는 뜻은 아니다. 다른 사용자/속성으로 확장할 범위는 후속 기획에서 결정한다.
순서: Skill Concept → Visual Concept → VFX Structure → 실제 구현.
재사용할 철학: 명도/색상 Layer, Bright Core, Energy Body, Trail/Afterimage, Impact, Pixel Fragment, 등급별 상대 규모.
색상·형태·움직임·물성은 속성마다 새로 설계한다. Fire/Ice 등을 Wind의 색상 교체로 만들지 않는다.
Common은 소수 레이어와 짧은 수명, Rare는 명확한 Core/짧은 잔상, Unique는 다층 구조/강한 마무리를 출발 기준으로 한다.
이는 정확한 레이어 수/파티클 수의 강제가 아니라 상대적 예산이다. 실제 모바일 프레임 시간·드로우콜·할당량을 기준으로 검수한다.

Legendary 제작 원칙
각 드래곤의 최종 외형, 속성, 기존 소개/Passive, 정체성을 먼저 확인한 뒤 고유 Signature Fantasy를 기획한다.
단순 고데미지 Unique가 아니다. 필요 시 Fullscreen VFX, 암전, 특수 Camera, Overlay, 짧은 Cinematic을 사용할 수 있다.
모든 Legendary를 단천의 화면 베기로 복제하지 않는다. 연출 기법과 스킬 이름·효과는 이번 단계에서 정하지 않는다.
단천의 무적 시간을 모든 Signature에 자동 복제하지 않는다. 새 기술의 게임플레이 규칙은 개별 기획에서 명시한다.
exclusiveDragonId / signatureSkill / CanEquip 경로를 재사용하고 공용 Pool에는 넣지 않는다.
시전 중 사망·타깃 사망·취소·장면 종료 때 시간/위치/색/Overlay/입력이 복원되는지 검사한다.

추천 제작 순서
모든 속성의 드래곤 수와 참조 준비 상태가 같으므로 수로 우열을 만들지 않았다.
아래 순위는 체형 재사용과 교차 속성 검수 부담을 기준으로 한 제안이다. 소요 시간/새 시스템 필요성을 확정한 점수는 아니다.
P1 Ice: 오로라와 루나의 사족형부터 기존 프레임 재생/정렬 규칙 검증. 루나 최종형의 직립/지팡이는 별도 검수.
P2 Fire: 엠버/솔의 이족·큰 날개 제작 규칙 확보. 최종 자세 차이 확인.
P3 Earth: 티탄/블랜디의 무거운 이족형에 앞선 포즈·정렬 경험 재사용.
P4 Lightning: 볼트/스톰. 이족/부유형 및 스톰의 Wind 추가 학습 검수.
P5 Light: 크로노/단테. 사족형 재사용과 장식·자세 전환 검수.
P6 Wind: 미르만 잔여. 긴 몸과 부유 정렬 검증이 필요하며 제피르 작업은 보호한다.
P7 Water: 샤키드/옥타. Ice 교차 스킬과 촉수형 정렬을 묶어 확인.
P8 Dark: 노바/베놈. 다두형 프레임 일관성과 복잡한 장식의 모바일 단순화를 마지막 체형 Batch로 검수.
Light와 Wind 등 인접 그룹의 세부 순위는 첫 샘플 검증 결과에 따라 조정할 수 있다.

Batch 운영과 작업량 절약
전체 15마리의 모든 프레임을 먼저 생성하지 않는다. 속성당 최대 2마리의 작은 Batch로 A~E를 통과시키며 규칙을 축적한다.
A0: 세 진화 단계의 승인 키 포즈를 먼저 나란히 비교. 얼굴/다리/날개/꼬리 계보와 해부 구조를 확정한다.
A1: 승인된 형태의 Idle만 수입해서 전투 크기·발 정렬·Origin·HUD 간섭을 확인한다.
A2: 같은 체형/같은 상태의 포즈를 묶어 Attack/Skill, Dodge/Hit/Death를 만든다. 첫 샘플 실패를 여러 캐릭터에 복제하지 않는다.
B: 속성별 VFX 언어를 한 번 확정한다. 캐릭터 A0 이후 별도 작업 가능하며 A 완료 전체를 기다릴 필요는 없다.
C: 해당 속성 공용 스킬을 한 번 제작하고 같은 속성 두 드래곤에서 검사한다. 공유 가능 모듈은 승인 콘셉트에서 도출한다.
D: 최종형/정체성 승인 후 각 드래곤 Signature를 독립 기획한다. 공통 자원/검사만 Batch로 묶고 연출은 복제하지 않는다.
E: 2마리 × 3형태 × 스킬 풀을 검사한 뒤 완료 처리. 스톰/샤키드는 추가 속성도 포함한다.
매번 긴 채팅을 다시 읽는 대신 드래곤별 한 장 브리프를 유지한다: ID, 참조 경로, 세 형태 키 포즈, 6상태 타이밍, 기준점, 미정 사항, 승인 이력.
생성 프롬프트는 공용 규격 + 체형 규칙 + 드래곤 특징만 바꾸고, 실패한 시트/상태만 보정한다.
변경 파일 목록과 승인된 기준 캡처를 저장한다. 공용 모듈이 바뀌지 않았다면 관련 없는 속성 전체 검사를 반복하지 않는다.

GLOBAL / PER ELEMENT / PER DRAGON
GLOBAL: DragonData/Evolution, 입력, BattleModel/HP/Damage, 공용 6상태 프레임 재생기, HUD, Pool 수명 관리, 제작 수입/검사 도구.
PER ELEMENT: 신규 VFX Art Direction, Common/Rare/Unique Pool 및 공용 VFX 모듈/아틀라스, 기존 Element UI Theme 데이터.
PER DRAGON: Base/Middle/Final Visual, 3 Animation Set, 포즈별 정렬/필요한 기준점, Legendary Signature. 기본/진화 스탯은 기존 구조 유지.
스킬별 고유 실루엣은 PER SKILL 작업이다. 속성 안에서도 모든 기술을 동일 Sprite 확대/색 변경으로 해결하지 않는다.

대량 제작 전 공용 준비 작업 — 이번에는 기록만
1) EvolutionSpriteSetup.Install은 stage 1~2만 받으며 dragon.LoadAnimationSet(0)을 즉시 사용한다. 새 Base 수입과 시간 원본 선택 경로를 최소 공용화할 필요가 있다.
2) Foot 검사는 읽을 수 있는 텍스처의 최하단 픽셀 평균이다. 공중·촉수·긴 몸은 자동 정렬을 검증하고 필요하면 명시 기준점 데이터를 최소 확장한다.
3) archetype enum은 분류용이며 자동 리깅/모션 기능이 아니다. 체형별 Controller를 복제할 근거가 되지 않는다.
4) DragonProductionChecks는 Signature가 없으면 완성 전 연결 필요 메시지를 낸다. 캐릭터 제작 단계와 최종 Signature 단계의 검수를 분리할 계획을 둔다. 현재 15개 미정 Signature를 결함으로 집계하지 않는다.
5) 현재 회귀 Harness에는 Zephyr/Wind 전제가 있다. 드래곤/단계/Pool을 인자로 받는 범위만 보완하고 전투 런타임은 유지한다.
6) VFX는 Skill 이벤트와 기존 Pool/피격 기능을 재사용할 수 있으나 모든 속성용 범용 라우터가 이미 완성된 것은 아니다. 새로운 콘셉트 확정 후 필요한 연결만 설계한다.
7) SkillData의 외부 VFX 사용 Tooltip은 기존 제작 제약이다. 향후 아트 방향을 제한하는 규칙으로 채택하지 않는다.

검수 원칙
캐릭터: 3형태 계보, 해부 구조, 픽셀 밀도, 6상태, Origin/지면, 모바일 실루엣.
전투: 탭/스와이프/스킬, 실제 피해·다단히트·쿨타임, HP/사망, HUD/Theme, 취소/장면 종료.
공용 스킬: 같은 속성 두 드래곤과 허용된 추가 속성 사용자에서 동일 스킬로 확인.
Legendary: 소유자만 사용, 고유 Fantasy, 시전/종료 복원. 다른 드래곤에게 단천을 배정하지 않는다.
성능: 승인된 최소 프레임, Point 필터, 짧은 Lifetime, Pool, 불필요한 투명 중첩 제한. 실물 WebGL 기준 기기 검사를 별도로 둔다.
일정과 정확한 프레임 비용은 첫 새 체형 Batch 후 산정한다. 이번 보드의 수량은 작업 분해용이다.

자료 출처와 보존
ContentDatabase.dragons/skills와 실제 DragonData/SkillData GUID 참조를 대조했다. 16마리와 34개 스킬이 모두 등록돼 있다.
경로 출처는 References/Skill Data/Pipeline 탭에 수록했다. 진화 레퍼런스의 장식은 외형 관찰이며 새 스킬/VFX 콘셉트가 아니다.
기존 Assets, Packages, ProjectSettings 파일 해시를 조사 전후 비교한다. 이 작업 결과는 별도 문서이며 게임 에셋/코드에 반영하지 않는다.
다음 제작은 시작하지 않는다.
'''
(work/'board-data.json').write_text(json.dumps(tables,ensure_ascii=False,indent=2),encoding='utf-8')
(work/'ProductionGuide.txt').write_text(guide,encoding='utf-8')
assert len(ds)==16 and len(ss)==34 and len(character)==48
assert sum(s['rarityName']=='Legendary' for s in ss)==1
assert all(d['registered'] for d in ds) and all(s['registered'] for s in ss)
print('Prepared 16 dragons, 48 forms, 34 skills, 8 element groups. No project assets changed.')
