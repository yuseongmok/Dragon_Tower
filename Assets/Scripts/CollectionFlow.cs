using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
namespace DragonTower
{
    public sealed class CollectionFlow : MonoBehaviour
    {
        BattleController controller;BattleView battleView;DragonData[] catalog;ContentDatabase database;
        RectTransform root,body,portrait,roomIcon;
        Text notice;
        EggGraphic egg;
        float hatchTime=-1,clock;
        DragonData hatched;
        TowerRun towerRun;
        public CollectionSession Session { get; private set; }
        public TowerRun CurrentRun => towerRun;
        public string ScreenName { get; private set; }
        public Button HatchButton { get; private set; }
        public Button TowerButton { get; private set; }
        public Button ContinueButton { get; private set; }
        public Button RoomButton { get; private set; }
        public Button SecondRoomButton { get; private set; }
        public Button[] ChoiceButtons { get; private set; }
        static Color Background=>new Color(.035f,.055f,.09f);
        static Color Gold=>new Color(1,.76f,.42f);
        static Color Muted=>new Color(.65f,.74f,.83f);
        public void Initialize(BattleController owner,BattleView view,DragonData[] dragons)
        {
            controller=owner;battleView=view;database=ContentDatabase.Load();
            catalog=database!=null&&database.dragons!=null&&database.dragons.Length>0?database.dragons:dragons;
            root=Rect("Collection screens",view.frame,0,425,480,850);
            var back=root.gameObject.AddComponent<Image>();back.color=Background;
            root.SetAsLastSibling();
            try
            {
                string key="DragonTower.Profile.v1";
#if UNITY_EDITOR
                // A test process can use its own keys; production saves are never cleared by tests.
                string isolated=Environment.GetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE");
                if(!string.IsNullOrEmpty(isolated))key=isolated;
#endif
                Session=new CollectionSession(new ProfileStore(key),catalog);
                if(Session.Selected==null)ShowEgg();else ShowLobby();
                if(Session.RecoveredBackup)notice.text="이전 정상 저장 데이터를 복구했습니다.";
            }
            catch(Exception e){Screen("저장 확인 필요");Label(e.Message,0,340,402,180,20,Color.white);notice.text="기존 저장을 보존했습니다. 오류를 확인한 뒤 다시 실행해 주세요.";Debug.LogException(e);}
        }
        RectTransform Rect(string name,Transform parent,float x,float y,float width,float height)
        {
            var go=new GameObject(name,typeof(RectTransform));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);
            r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(width,height);return r;
        }
        Text Label(string text,float x,float y,float w,float h,int size,Color color)
        {
            var r=Rect(text,body,x,y,w,h);var t=r.gameObject.AddComponent<Text>();t.font=battleView.font;t.text=text;t.fontSize=size;t.color=color;t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;return t;
        }
        Button Button(string title,float x,float y,float w,float h,UnityAction action)
        {
            var r=Rect(title,body,x,y,w,h);var image=r.gameObject.AddComponent<Image>();image.color=new Color(.18f,.27f,.36f);
            var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;b.onClick.AddListener(action);var nav=b.navigation;nav.mode=Navigation.Mode.None;b.navigation=nav;
            var text=Rect("Label",r,0,h/2,w-12,h-8).gameObject.AddComponent<Text>();text.font=battleView.font;text.text=title;text.fontSize=20;text.color=Color.white;text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;return b;
        }
        void Screen(string name)
        {
            if(body!=null){body.gameObject.SetActive(false);Destroy(body.gameObject);}
            portrait=null;roomIcon=null;egg=null;HatchButton=TowerButton=ContinueButton=RoomButton=SecondRoomButton=null;ChoiceButtons=null;
            ScreenName=name;root.gameObject.SetActive(true);root.SetAsLastSibling();
            body=Rect(name,root,0,425,480,850);
            Label("D R A G O N   T O W E R",0,42,440,32,21,Color.white);
            Label(name,0,95,420,45,28,Gold);
            notice=Label("이 기기에 자동 저장됩니다",0,805,430,58,14,Muted);
        }
        void Portrait(DragonData dragon,float y=348,int level=1)
        {
            portrait=Rect("Selected dragon",body,0,y,260,260);
            var activeSprite=dragon.BattleSpriteAtLevel(level);
            if(activeSprite!=null)
            {var image=portrait.gameObject.AddComponent<Image>();image.sprite=activeSprite;image.preserveAspect=true;image.raycastTarget=false;}
            else
            {portrait.gameObject.AddComponent<CanvasRenderer>();var graphic=portrait.gameObject.AddComponent<DragonGraphic>();graphic.color=dragon.color;graphic.raycastTarget=false;}
        }
        public void ShowEgg()
        {
            if(hatchTime>=0)return;
            if(Session.Profile.eggs<=0){ShowLobby();return;}
            controller.EndBattle();Screen("새로운 만남");
            Label("당신의 첫 모험을 기다리는 알",0,177,430,44,20,Color.white);
            HatchButton=Button("",0,350,230,250,Hatch);
            HatchButton.targetGraphic.color=new Color(.07f,.105f,.155f);
            var eggRect=Rect("Dragon egg",HatchButton.transform,0,125,180,200);
            eggRect.gameObject.AddComponent<CanvasRenderer>();egg=eggRect.gameObject.AddComponent<EggGraphic>();egg.raycastTarget=false;
            Label("알을 터치해 부화시키세요",0,535,430,45,23,Gold);
            Label("8가지 속성 중 한 드래곤과 만납니다",0,584,430,38,16,Muted);
            Label("보관 중인 알  "+Session.Profile.eggs,0,653,400,35,18,Color.white);
            if(Session.Selected!=null)Button("로비로 돌아가기",0,729,320,48,ShowLobby);
        }
        public void Hatch()
        {
            if(hatchTime>=0||Session==null||Session.Profile.eggs<=0)return;
            try
            {
                // Commit the outcome before animation so closing/reloading cannot reroll it.
                hatched=Session.Hatch(UnityEngine.Random.Range(0,catalog.Length));
                if(hatched==null)return;
                hatchTime=0;if(HatchButton!=null)HatchButton.interactable=false;
                if(egg!=null){egg.cracked=true;egg.SetVerticesDirty();}
                notice.text="새 친구가 깨어나고 있어요…";
            }
            catch(Exception e){notice.text="저장하지 못했습니다. 알은 소비되지 않았습니다.";Debug.LogException(e);}
        }
        void ShowHatched()
        {
            Screen("부화 성공!");Portrait(hatched);
            Label(hatched.displayName,0,528,430,52,34,Gold);
            Label(hatched.element+" 속성 · 도감에 등록되었습니다",0,587,430,42,18,Color.white);
            ContinueButton=Button("로비로 가기",0,702,380,68,ShowLobby);
            notice.text="부화 결과가 저장되었습니다";
        }
        public void ShowLobby()
        {
            if(hatchTime>=0)return;
            controller.EndBattle();towerRun=null;
            if(Session.Selected==null){ShowEgg();return;}
            Screen("드래곤 로비");var d=Session.Selected;Portrait(d,324);
            Label(d.displayName,0,493,410,52,32,Gold);
            Label(d.element+"  ·  다음 모험을 준비 중",0,541,410,34,17,Muted);
            TowerButton=Button("타워 오르기",0,622,400,70,EnterTower);
            Button("드래곤 도감",-105,713,190,62,ShowCodex);Button("드래곤 상태",105,713,190,62,ShowStatus);
            if(Session.Profile.eggs>0)Button("보관 알 "+Session.Profile.eggs,135,159,155,40,ShowEgg);
            notice.text="도전 중 얻는 성장과 보상은 로비의 수집 정보와 분리됩니다.";
        }
        public void EnterTower()
        {
            EnterTowerWithRoll(UnityEngine.Random.Range(0,100),UnityEngine.Random.Range(0,100));
        }
        public void EnterTowerWithRoll(int firstRoll) { EnterTowerWithRoll(firstRoll,(firstRoll+53)%100); }
        public void EnterTowerWithRoll(int firstRoll,int secondRoll)
        {
            if(Session.Selected==null||hatchTime>=0)return;
            towerRun=new TowerRun(Session.Selected.maxHP,firstRoll,secondRoll,Session.Selected.elementType);ShowTowerChoices();
        }
        string RoomName(TowerRoomKind room)
        {
            switch(room)
            {
                case TowerRoomKind.Monster:return "몬스터방";case TowerRoomKind.Item:return "아이템방";
                case TowerRoomKind.Augment:return "증강방";case TowerRoomKind.Recovery:return "회복방";
                case TowerRoomKind.Nest:return "드래곤 둥지";case TowerRoomKind.Gold:return "골드방";
                case TowerRoomKind.Shop:return "상점방";default:return "보스방";
            }
        }
        string RoomDetail(TowerRoomKind room)
        {
            switch(room)
            {
                case TowerRoomKind.Monster:return "전투 · 승리하면 레벨 상승";case TowerRoomKind.Item:return "상자에서 아이템 하나 선택";
                case TowerRoomKind.Augment:return "세 가지 증강 중 하나 선택";case TowerRoomKind.Recovery:return "초록 십자를 눌러 완전 회복";
                case TowerRoomKind.Nest:return "알을 부화시켜 도감에 등록";case TowerRoomKind.Gold:return "골드를 획득";
                case TowerRoomKind.Shop:return "골드로 회복 구매";default:return "강력한 수호자와 전투";
            }
        }
        void ShowTowerChoices()
        {
            if(towerRun==null||!towerRun.Active)return;
            controller.EndBattle();Screen("다음 길 선택");
            Label("타워 "+towerRun.Floor+"층",0,154,420,45,23,Gold);
            Label("LV "+towerRun.Level+"   HP "+towerRun.CurrentHP+" / "+towerRun.MaxHP+"   GOLD "+towerRun.Gold,0,202,430,35,16,Muted);
            var first=towerRun.Choices[0];
            RoomButton=Button(RoomName(first)+"\n"+RoomDetail(first),0,towerRun.Choices.Count==1?385:326,400,112,()=>SelectTowerRoom(0));
            if(towerRun.Choices.Count>1)
            {
                var second=towerRun.Choices[1];
                SecondRoomButton=Button(RoomName(second)+"\n"+RoomDetail(second),0,492,400,112,()=>SelectTowerRoom(1));
            }
            notice.text=towerRun.Floor%10==0?"보스층은 하나의 길만 열립니다.":"두 방 중 하나를 선택하세요.";
        }
        void SelectTowerRoom(int index)
        {
            towerRun.ChooseRoom(index);ShowChosenRoom();
        }
        Button IconButton(string symbol,string caption,Color color,UnityAction action)
        {
            var button=Button("",0,384,230,210,action);button.targetGraphic.color=color;roomIcon=button.GetComponent<RectTransform>();
            var mark=Label(symbol,0,340,210,105,76,Color.white);mark.transform.SetParent(button.transform,false);mark.rectTransform.anchoredPosition=new Vector2(0,52);
            var text=button.GetComponentInChildren<Text>();text.text=caption;text.fontSize=20;
            return button;
        }
        void ShowChosenRoom()
        {
            if(towerRun==null||!towerRun.RoomChosen)return;
            switch(towerRun.Room)
            {
                case TowerRoomKind.Augment:ShowAugmentChoices(false);return;
                case TowerRoomKind.Item:ShowItemRoom();return;case TowerRoomKind.Recovery:ShowRecoveryRoom();return;
                case TowerRoomKind.Nest:ShowNestRoom();return;case TowerRoomKind.Gold:ShowGoldRoom();return;
                case TowerRoomKind.Shop:ShowShopRoom();return;
            }
            Screen("타워 "+towerRun.Floor+"층 · "+RoomName(towerRun.Room));
            bool boss=towerRun.Room==TowerRoomKind.Boss;
            Label(boss?"고대 룬 골렘이 깨어납니다.":"이 층의 몬스터가 길을 막고 있습니다.",0,300,420,80,22,Color.white);
            Label("LV "+towerRun.Level+"   HP "+towerRun.CurrentHP+" / "+towerRun.MaxHP,0,438,420,35,17,Muted);
            RoomButton=Button(boss?"보스 전투":"전투 시작",0,585,360,72,StartCurrentBattle);
            notice.text="다음 층에서도 현재 HP가 그대로 이어집니다.";
        }
        void StartCurrentBattle()
        {
            bool boss=towerRun.Room==TowerRoomKind.Boss;
            var skin=Resources.Load<BattleSkin>("PixelBattleSkin");Sprite enemySprite=null;BattleEnemyStats enemy;
            if(boss)
            {
                var data=database==null?null:database.PickMonster(towerRun.Floor,true,UnityEngine.Random.Range(0,int.MaxValue));
                enemy=data==null?BattleEnemyStats.AncientGolem(towerRun.Floor):data.CreateBattleStats(towerRun.Floor);
                enemySprite=data==null?(skin==null?null:skin.ancientGolemBoss):data.battleSprite;
            }
            else
            {
                var data=database==null?null:database.PickMonster(towerRun.Floor,false,UnityEngine.Random.Range(0,int.MaxValue));
                if(data!=null){enemy=data.CreateBattleStats(towerRun.Floor);enemySprite=data.battleSprite;}
                else
                {
                    int index=UnityEngine.Random.Range(0,BattleEnemyStats.FirstAreaCount);enemy=BattleEnemyStats.FirstArea(index);
                    if(skin!=null&&skin.floorMonsters!=null&&index<skin.floorMonsters.Length)enemySprite=skin.floorMonsters[index];
                    if(enemySprite==null&&skin!=null)enemySprite=skin.rockSlime;
                }
            }
            root.gameObject.SetActive(false);ScreenName=boss?"보스 전투":"몬스터 전투";
            controller.BeginBattle(Session.Selected,towerRun.BuildBattleStats(Session.Selected.Snapshot(towerRun.Level)),enemy,towerRun.Floor,boss,towerRun.CurrentHP,enemySprite,towerRun.Level,towerRun.CurrentSkill(Session.Selected.skill));
        }
        IEnumerator AnimateRoomAction(Action done)
        {
            if(RoomButton!=null)RoomButton.interactable=false;float time=0;
            while(time<.55f){time+=Time.deltaTime;float pulse=1+Mathf.Sin(time*18)*.09f;if(roomIcon!=null)roomIcon.localScale=Vector3.one*pulse;yield return null;}
            if(roomIcon!=null)roomIcon.localScale=Vector3.one;done?.Invoke();
        }
        void ShowItemRoom()
        {
            Screen("아이템방");Label("상자를 터치하세요",0,190,420,46,25,Gold);
            RoomButton=IconButton("▣","상자 열기",new Color(.38f,.24f,.12f),()=>StartCoroutine(AnimateRoomAction(ShowItemChoices)));
            notice.text="세 아이템 중 하나만 가져갈 수 있습니다.";
        }
        void ShowItemChoices()
        {
            Screen("아이템 선택");Label("하나를 선택하세요",0,168,420,40,21,Muted);
            var choices=database==null?Array.Empty<ItemData>():database.PickItems(3,Environment.TickCount^towerRun.Floor);
            if(choices.Length>0)
            {
                ChoiceButtons=new Button[choices.Length];for(int i=0;i<choices.Length;i++){var item=choices[i];ChoiceButtons[i]=ItemChoice(item,285+i*125);}
            }
            else {Label("등록된 아이템이 없습니다.",0,390,420,50,22,Color.white);RoomButton=Button("다음 층",0,610,360,68,AdvanceFloor);}
        }
        Button ItemChoice(ItemData item,float y)
        {return Button("["+ItemGradeName(item.grade)+"] "+item.displayName+"\n"+ItemSummary(item),0,y,400,96,()=>AcquireItem(item,()=>ShowRoomResult(item.displayName+"을(를) 획득했습니다.")));}
        string ItemSummary(ItemData item)
        {
            if(item==null)return "효과 없음";string kind=item.kind==ItemKind.Consumable?"소모품":"장착";
            return kind+" · "+(!string.IsNullOrWhiteSpace(item.description)?item.description:EffectSummary(item.effects));
        }
        void AcquireItem(ItemData item,Action complete)
        {
            if(towerRun.CanAddItem(item)){towerRun.AddItem(item);complete();return;}
            Screen("아이템 교체");Label("새 아이템  ["+ItemGradeName(item.grade)+"] "+item.displayName,0,185,430,54,22,Gold);
            Label(ItemSummary(item),0,245,420,58,17,Color.white);
            ChoiceButtons=new Button[towerRun.ItemSlots.Count];
            for(int i=0;i<towerRun.ItemSlots.Count;i++)
            {
                int slot=i;var owned=towerRun.ItemSlots[i];
                ChoiceButtons[i]=Button((i+1)+"번 교체 · "+owned.DisplayName+(owned.Count>1?" ×"+owned.Count:"")+"\n→ "+item.displayName,0,355+i*120,400,92,()=>{towerRun.AddItem(item,slot);complete();});
            }
            notice.text="아이템은 두 종류만 보유할 수 있습니다. 같은 아이템은 한 슬롯에 중첩됩니다.";
        }
        string EffectSummary(System.Collections.Generic.IReadOnlyList<ContentEffect> effects)
        {
            if(effects==null||effects.Count==0)return "효과 없음";string result="";
            for(int i=0;i<effects.Count;i++){if(i>0)result+=" · ";result+=EffectName(effects[i].type)+" "+effects[i].value.ToString("+0.##;-0.##;0");}
            return result;
        }
        string EffectName(ContentEffectType type)
        {
            switch(type)
            {
                case ContentEffectType.Heal:return "HP 회복";case ContentEffectType.MaxHP:return "최대 HP";case ContentEffectType.AttackDamage:return "공격력";
                case ContentEffectType.SkillDamagePercent:return "스킬 피해 %";case ContentEffectType.SkillCooldownPercent:return "스킬 쿨타임 감소 %";
                case ContentEffectType.AttackCooldownPercent:return "공격 쿨타임 감소 %";case ContentEffectType.DodgeCooldownPercent:return "회피 쿨타임 감소 %";
                case ContentEffectType.DodgeDurationPercent:return "회피 시간 %";case ContentEffectType.MaxHPPercent:return "최대 HP %";
                case ContentEffectType.AttackDamagePercent:return "일반 공격 피해 %";case ContentEffectType.CriticalChancePercent:return "치명타 확률 %";
                case ContentEffectType.CriticalDamagePercent:return "치명타 피해 %";case ContentEffectType.SkillDisabled:return "스킬 봉인";
                case ContentEffectType.SkillCooldownSetZero:return "스킬 쿨타임 삭제";case ContentEffectType.DamageReductionPercent:return "받는 피해 감소 %";default:return "골드";
            }
        }
        void ShowRecoveryRoom()
        {
            Screen("회복방");Label("초록 십자를 터치하세요",0,185,420,42,24,Gold);
            RoomButton=IconButton("+","HP 완전 회복",new Color(.12f,.52f,.31f),()=>StartCoroutine(AnimateRoomAction(()=>{int healed=towerRun.HealFull();ShowRoomResult("HP를 "+healed+" 회복했습니다.");})));
            notice.text="회복방은 전투 사이의 귀중한 회복 수단입니다.";
        }
        void ShowGoldRoom()
        {
            Screen("골드방");Label("골드 더미를 터치하세요",0,185,420,42,24,Gold);
            RoomButton=IconButton("G","골드 획득",new Color(.68f,.48f,.08f),()=>StartCoroutine(AnimateRoomAction(()=>{towerRun.AddGold(50);ShowRoomResult("50 골드를 획득했습니다.");})));
            notice.text="골드는 이번 도전의 상점에서 사용합니다.";
        }
        void ShowShopRoom()
        {
            Screen("상점방");Label("보유 골드  "+towerRun.Gold,0,175,420,42,24,Gold);
            var goods=database==null?Array.Empty<ItemData>():database.PickItems(2,Environment.TickCount^(towerRun.Floor<<10));
            ChoiceButtons=new Button[goods.Length];
            for(int i=0;i<goods.Length;i++)
            {
                var item=goods[i];ChoiceButtons[i]=Button("["+ItemGradeName(item.grade)+"] "+item.displayName+" · "+item.price+"G\n"+ItemSummary(item),0,315+i*125,400,100,()=>BuyItem(item));
            }
            SecondRoomButton=Button("구매하지 않고 다음 층",0,600,400,66,AdvanceFloor);
            notice.text="구매한 아이템은 빈 슬롯에 넣거나 기존 아이템과 교체합니다.";
        }
        void BuyItem(ItemData item)
        {
            if(item==null)return;if(!towerRun.SpendGold(item.price)){notice.text="골드가 부족합니다.";return;}
            AcquireItem(item,()=>ShowRoomResult(item.displayName+"을(를) "+item.price+"G에 구매했습니다."));
        }
        void ShowNestRoom()
        {
            Screen("드래곤 둥지");Label("알을 터치해 부화시키세요",0,178,420,44,23,Gold);
            RoomButton=Button("",0,375,230,245,HatchNestEgg);RoomButton.targetGraphic.color=new Color(.07f,.105f,.155f);roomIcon=RoomButton.GetComponent<RectTransform>();
            var eggRect=Rect("Nest egg",RoomButton.transform,0,126,170,190);eggRect.gameObject.AddComponent<CanvasRenderer>();egg=eggRect.gameObject.AddComponent<EggGraphic>();egg.raycastTarget=false;
            notice.text="태어난 드래곤은 즉시 도감과 보유 목록에 등록됩니다.";
        }
        void HatchNestEgg()
        {
            if(RoomButton==null||!RoomButton.interactable)return;
            hatched=catalog[UnityEngine.Random.Range(0,catalog.Length)];
            try{Session.RegisterHatchedDragon(hatched);if(egg!=null){egg.cracked=true;egg.SetVerticesDirty();}StartCoroutine(AnimateRoomAction(ShowNestResult));}
            catch(Exception e){notice.text="등록하지 못했습니다. 알은 유지됩니다.";Debug.LogException(e);}
        }
        void ShowNestResult()
        {
            Screen("둥지 부화 성공!");Portrait(hatched,315);Label(hatched.displayName+" · "+hatched.element,0,505,420,48,28,Gold);
            Label("도감과 보유 목록에 등록되었습니다.",0,558,420,38,18,Color.white);
            RoomButton=Button("다음 층",0,680,360,66,AdvanceFloor);
        }
        void ShowAugmentChoices(bool levelReward)
        {
            Screen(levelReward?"레벨 "+towerRun.Level+" 증강":"증강방");
            Label("세 선택지는 모두 무작위로 등장합니다",0,165,430,44,20,Gold);
            Label("스킬은 현재 드래곤과 같은 속성만 등장",0,208,420,32,16,Muted);
            var rewards=PickRandomRewards(Environment.TickCount^towerRun.Level^(towerRun.Floor<<8),3);
            ChoiceButtons=new Button[Math.Max(3,rewards.Length)];
            if(rewards.Length>0)
            {
                for(int i=0;i<rewards.Length;i++)
                {
                    int slot=i;var reward=rewards[i];Button choice;
                    if(reward.augment!=null)
                    {
                        var augment=reward.augment;
                        choice=Button("["+GradeName(augment.grade)+"] "+augment.displayName+"\n"+AugmentSummary(augment),0,300+slot*120,400,100,()=>ChooseAugment(augment,levelReward));
                        choice.targetGraphic.color=GradeColor(augment.grade);
                    }
                    else
                    {
                        var skill=reward.skill;
                        choice=Button("[스킬] "+skill.displayName+"\n"+SkillSummary(skill),0,300+slot*120,400,100,()=>ChooseSkill(skill,levelReward));
                        choice.targetGraphic.color=new Color(.22f,.34f,.52f);
                    }
                    choice.GetComponentInChildren<Text>().fontSize=15;ChoiceButtons[slot]=choice;
                }
                notice.text="일반 60% · 레어 28% · 에픽 10% · 유니크 2%";
            }
            else
            {
                for(int i=0;i<3;i++){int index=i;ChoiceButtons[i]=Button("증강 슬롯 "+(i+1)+"\n효과 내용은 추후 추가",0,300+i*120,400,92,()=>ChooseAugment("augment-slot-"+index,levelReward));}
                notice.text="콘텐츠 관리 창에서 증강 데이터를 추가할 수 있습니다.";
            }
        }
        sealed class RandomReward { public AugmentData augment;public SkillData skill; }
        RandomReward[] PickRandomRewards(int seed,int count)
        {
            if(database==null)return Array.Empty<RandomReward>();
            var augments=(database.augments??Array.Empty<AugmentData>()).Where(a=>a!=null&&towerRun.CanTakeAugment(a)).ToList();
            var skills=(database.skills??Array.Empty<SkillData>()).Where(s=>s!=null&&s.elementType==Session.Selected.elementType&&s.StableId!=towerRun.CurrentSkillId(Session.Selected.skill)).ToList();
            var result=new System.Collections.Generic.List<RandomReward>();var random=new System.Random(seed);
            while(result.Count<count&&(augments.Count>0||skills.Count>0))
            {
                // A skill is possible in every slot but never guaranteed. With both pools present,
                // each slot independently has a 25% skill chance.
                bool pickSkill=skills.Count>0&&(augments.Count==0||random.Next(100)<25);
                if(pickSkill)
                {
                    int index=random.Next(skills.Count);result.Add(new RandomReward{skill=skills[index]});skills.RemoveAt(index);
                }
                else
                {
                    var augment=ContentDatabase.PickWeightedAugment(augments,random);if(augment==null)break;
                    result.Add(new RandomReward{augment=augment});augments.Remove(augment);
                }
            }
            return result.ToArray();
        }
        static string GradeName(AugmentGrade grade)
        {switch(grade){case AugmentGrade.Rare:return "레어";case AugmentGrade.Epic:return "에픽";case AugmentGrade.Unique:return "유니크";default:return "일반";}}
        static string ItemGradeName(ItemGrade grade)
        {switch(grade){case ItemGrade.Rare:return "레어";case ItemGrade.Epic:return "에픽";case ItemGrade.Unique:return "유니크";default:return "일반";}}
        static Color GradeColor(AugmentGrade grade)
        {switch(grade){case AugmentGrade.Rare:return new Color(.18f,.38f,.58f);case AugmentGrade.Epic:return new Color(.39f,.22f,.58f);case AugmentGrade.Unique:return new Color(.68f,.40f,.12f);default:return new Color(.18f,.27f,.36f);}}
        string AugmentSummary(AugmentData augment)
        {
            if(augment==null)return "효과 없음";
            if(!string.IsNullOrWhiteSpace(augment.description))return augment.description;
            return EffectSummary(augment.effects);
        }
        string SkillSummary(SkillData skill)
        {
            string hits=skill.hitCount>1?skill.damage+" × "+skill.hitCount:skill.damage.ToString();
            string status=skill.statusEffect==CombatStatusEffect.None?"":(" · "+StatusName(skill.statusEffect)+" "+skill.statusChancePercent.ToString("0")+"%");
            return hits+" 피해 · "+skill.cooldown.ToString("0.#")+"초"+status;
        }
        string StatusName(CombatStatusEffect effect)
        {switch(effect){case CombatStatusEffect.Burn:return "화상";case CombatStatusEffect.Paralyze:return "마비";case CombatStatusEffect.Slow:return "둔화";default:return "";}}
        void ChooseAugment(AugmentData augment,bool levelReward)
        {try{towerRun.AddAugment(augment,levelReward);ShowRoomResult(augment.displayName+"을(를) 선택했습니다.");}catch(Exception e){notice.text=e.Message;}}
        void ChooseAugment(string id,bool levelReward)
        {
            towerRun.AddAugment(id,levelReward);ShowRoomResult("증강을 선택했습니다.");
        }
        void ChooseSkill(SkillData skill,bool levelReward)
        {try{towerRun.ReplaceSkill(skill,levelReward);ShowRoomResult(skill.displayName+" 스킬로 교체했습니다.");}catch(Exception e){notice.text=e.Message;}}
        void ShowRoomResult(string result)
        {
            Screen("방 완료");Label(result,0,310,420,100,24,Color.white);
            Label("LV "+towerRun.Level+"   HP "+towerRun.CurrentHP+" / "+towerRun.MaxHP+"   GOLD "+towerRun.Gold,0,435,430,36,16,Muted);
            RoomButton=Button("다음 층",0,610,360,68,AdvanceFloor);
        }
        void AdvanceFloor()
        {
            towerRun.AdvanceFloor(UnityEngine.Random.Range(0,100),UnityEngine.Random.Range(0,100));ShowTowerChoices();
        }
        public void ResolveBattleResult()
        {
            if(towerRun==null||controller.CurrentBattle==null||controller.CurrentBattle.Result==BattleResult.Fighting)return;
            if(controller.CurrentBattle.Result==BattleResult.Victory)
            {
                int hp=controller.CurrentBattle.PlayerHP;towerRun.RecordBattleVictory(hp);controller.EndBattle();
                if(TryShowMonsterDrop())return;ContinueAfterBattleRewards();return;
            }
            int floor=towerRun.Floor;towerRun.End();controller.EndBattle();
            Screen("도전 종료");Label(floor+"층에서 모험을 마쳤습니다",0,312,420,70,28,Gold);
            Label("보유 드래곤과 도감은 그대로 유지됩니다.\n레벨·골드·아이템·증강은 초기화됩니다.",0,430,420,110,19,Color.white);
            RoomButton=Button("로비로 돌아가기",0,610,360,68,ShowLobby);
            notice.text="같은 드래곤으로 다시 도전할 수 있습니다.";
        }
        bool TryShowMonsterDrop()
        {
            int chance=towerRun.Room==TowerRoomKind.Boss?25:towerRun.Room==TowerRoomKind.Monster?12:0;
            if(database==null||database.items==null||database.items.Length==0||UnityEngine.Random.Range(0,100)>=chance)return false;
            var drops=database.PickItems(1,Environment.TickCount^(towerRun.Floor<<12));if(drops.Length==0)return false;var item=drops[0];
            Screen("몬스터 전리품");Label("["+ItemGradeName(item.grade)+"] "+item.displayName,0,250,420,54,26,Gold);
            Label(ItemSummary(item),0,330,420,82,19,Color.white);
            RoomButton=Button("획득 또는 교체",0,475,360,72,()=>AcquireItem(item,ContinueAfterBattleRewards));
            SecondRoomButton=Button("두고 간다",0,575,360,62,ContinueAfterBattleRewards);
            notice.text="몬스터가 아이템을 떨어뜨렸습니다.";return true;
        }
        void ContinueAfterBattleRewards()
        {if(towerRun.PendingEvolutionStage>0)ShowEvolutionCutscene();else ContinueAfterLevelRewards();}
        void ContinueAfterLevelRewards()
        {
            if(towerRun.PendingLevelAugments>0)ShowAugmentChoices(true);else AdvanceFloor();
        }
        void ShowEvolutionCutscene()
        {
            int stage=towerRun.PendingEvolutionStage;var dragon=Session.Selected;
            Screen(stage==1?"중간 진화":"최종 진화");
            Label("LEVEL "+towerRun.Level,0,148,420,38,19,Muted);
            Label(dragon.NameForStage(stage-1)+"  →  "+dragon.NameForStage(stage),0,202,440,44,23,Gold);
            var auraRect=Rect("Evolution aura",body,0,416,350,350);var aura=auraRect.gameObject.AddComponent<Image>();
            aura.color=new Color(dragon.color.r,dragon.color.g,dragon.color.b,.18f);aura.raycastTarget=false;
            var artRect=Rect("Evolution dragon",body,0,416,330,330);var art=artRect.gameObject.AddComponent<Image>();
            art.sprite=dragon.SpriteForStage(stage-1);art.preserveAspect=true;art.raycastTarget=false;
            var evolutionText=Label("빛이 드래곤을 감쌉니다…",0,624,440,48,20,Color.white);
            notice.text="진화 연출이 끝나면 다음 보상으로 이어집니다.";
            StartCoroutine(AnimateEvolution(dragon,stage,art,aura,evolutionText));
        }
        IEnumerator AnimateEvolution(DragonData dragon,int stage,Image art,Image aura,Text evolutionText)
        {
            float time=0;
            while(time<1.05f)
            {
                time+=Mathf.Min(Time.deltaTime,.1f);float t=Mathf.Clamp01(time/1.05f);
                art.rectTransform.localScale=Vector3.one*(1+Mathf.Sin(time*24)*.07f+t*.18f);
                art.color=Color.Lerp(Color.white,new Color(1,1,1,.12f),t);
                aura.rectTransform.localScale=Vector3.one*(.65f+t*.55f);
                aura.rectTransform.localRotation=Quaternion.Euler(0,0,time*75);yield return null;
            }
            art.sprite=dragon.SpriteForStage(stage);art.color=Color.white;time=0;
            while(time<.9f)
            {
                time+=Mathf.Min(Time.deltaTime,.1f);float t=Mathf.Clamp01(time/.9f);
                float overshoot=1+Mathf.Sin(t*Mathf.PI)*.22f;
                art.rectTransform.localScale=Vector3.one*overshoot;
                aura.color=new Color(dragon.color.r,dragon.color.g,dragon.color.b,(1-t)*.65f);
                aura.rectTransform.localScale=Vector3.one*(1.2f+t*.55f);yield return null;
            }
            art.rectTransform.localScale=Vector3.one;aura.color=new Color(dragon.color.r,dragon.color.g,dragon.color.b,.12f);
            evolutionText.text=dragon.NameForStage(stage)+"(으)로 진화했습니다!";evolutionText.color=Gold;evolutionText.fontSize=24;
            ContinueButton=Button(towerRun.PendingLevelAugments>0?"증강 선택으로":"다음 층으로",0,710,360,66,FinishEvolution);
            notice.text=stage==1?"레벨 40에서 최종 진화합니다.":"최종 진화를 완료했습니다.";
        }
        void FinishEvolution()
        {
            if(towerRun==null||towerRun.PendingEvolutionStage<=0)return;
            towerRun.ConsumeEvolution();ContinueAfterLevelRewards();
        }
        public void ShowCodex()
        {
            Screen("드래곤 도감");int found=0;foreach(var d in catalog)if(Session.Owns(d.StableId))found++;
            Label("발견한 드래곤  "+found+" / "+catalog.Length,0,166,420,38,20,Muted);
            for(int i=0;i<catalog.Length;i++)
            {
                var d=catalog[i];bool owns=Session.Owns(d.StableId);
                int column=i%2,row=i/2;float x=column==0?-106:106,y=250+row*106;
                var b=Button(owns?d.displayName+"\n"+d.element:"???\n미발견",x,y,198,82,()=>SelectSpecies(d));
                b.GetComponentInChildren<Text>().fontSize=16;
                b.interactable=owns;
            }
            Label("만난 드래곤을 눌러 함께할 친구를 선택하세요",0,649,430,50,16,Muted);
            Button("로비로 돌아가기",0,728,380,60,ShowLobby);
        }
        void SelectSpecies(DragonData data)
        {
            foreach(var d in Session.Profile.dragons)if(d.speciesId==data.StableId)
            {
                try {Session.Select(d.instanceId);ShowLobby();}
                catch(Exception e){notice.text="선택을 저장하지 못했습니다.";Debug.LogException(e);}return;
            }
        }
        public void ShowStatus()
        {
            var d=Session.Selected;if(d==null)return;
            Screen("드래곤 상태");Portrait(d,271);
            Label(d.displayName+"  /  "+d.element,0,440,420,48,27,Gold);
            Label("기본 체력  "+d.maxHP+"\n기본 공격력  "+d.attackDamage+"\n공격 간격  0.3초\n"+d.skill.displayName+"  ·  피해 "+d.skill.damage+"\n스킬 쿨타임  "+d.skill.cooldown+"초",0,572,430,184,21,Color.white);
            Button("로비로 돌아가기",0,728,380,60,ShowLobby);
            notice.text="레벨·진화·아이템·증강은 타워 도전 중에만 적용됩니다.";
        }
        void Update()
        {
            if(root==null||!root.gameObject.activeInHierarchy)return;
            float dt=Mathf.Min(Time.deltaTime,.1f);clock+=dt;
            if(portrait!=null)portrait.localScale=new Vector3(1-Mathf.Sin(clock*3)*.015f,1+Mathf.Sin(clock*3)*.025f,1);
            if(hatchTime<0)return;
            hatchTime+=dt;if(egg!=null)egg.rectTransform.localRotation=Quaternion.Euler(0,0,Mathf.Sin(hatchTime*38)*10);
            if(hatchTime>=.85f){hatchTime=-1;ShowHatched();}
        }
    }
}
