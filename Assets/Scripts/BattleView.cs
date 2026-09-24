using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
namespace DragonTower
{
    public class BattleView : MonoBehaviour
    {
        public Font font;
        public RectTransform frame;
        public Text playerName, playerHP, enemyHP, enemyName, floorLabel, modeLabel, warning, message, skillLabel, attackLabel, dodgeLabel, itemLabel1, itemLabel2, resultTitle, resultDetail;
        public Image playerFill, enemyFill, windupFill;
        public DragonGraphic playerArt, enemyArt;
        public Button attackButton, skillButton, dodgeButton, itemButton1, itemButton2, restartButton;
        public GameObject resultPanel;
        public Text dragonInfo;
        BattleSkin skin;
        Image pixelPlayer, pixelEnemy;
        int lastPlayerHP = -1, lastEnemyHP = -1,lastShieldHP=-1;
        BattleStats lastDragon;
        double nextTextRefresh;
        Rect lastSafeArea;
        Vector2 lastScreen;
        BattleAnimation motion;
        public Sprite CurrentEnemySprite => pixelEnemy==null ? null : pixelEnemy.sprite;
        static readonly Color Ink = new Color(.055f,.075f,.12f), Muted = new Color(.57f,.66f,.76f);
        static Color C(float r,float g,float b) => new Color(r,g,b);
        RectTransform Rect(string label,Transform parent,float x,float y,float w,float h)
        {
            var go = new GameObject(label,typeof(RectTransform));
            var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);
            r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.pivot=new Vector2(.5f,.5f);
            r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
        }
        Image Panel(string name,Transform parent,float x,float y,float w,float h,Color color)
        {var r=Rect(name,parent,x,y,w,h);var im=r.gameObject.AddComponent<Image>();im.color=color;im.raycastTarget=false;return im;}
        Text Label(string text,Transform parent,float x,float y,float w,float h,int size,Color color,TextAnchor align=TextAnchor.MiddleCenter)
        {
            var r=Rect(text,parent,x,y,w,h);var t=r.gameObject.AddComponent<Text>();
            t.font=font;t.text=text;t.fontSize=size;t.color=color;t.alignment=align;t.raycastTarget=false;
            return t;
        }
        Button MakeButton(string text,Transform parent,float x,float y,float w,float h,Color color,out Text label)
        {
            var im=Panel(text,parent,x,y,w,h,color);im.raycastTarget=true;
            var b=im.gameObject.AddComponent<Button>();b.targetGraphic=im;
            var colors=b.colors;colors.disabledColor=C(.38f,.42f,.48f);colors.pressedColor=C(.7f,.76f,.85f);b.colors=colors;
            var nav=b.navigation;nav.mode=Navigation.Mode.None;b.navigation=nav;
            label=Label(text,im.transform,0,h/2,w-8,h-8,18,Color.white);return b;
        }
        Image Bar(string name,float y,Color tint)
        {
            var back=Panel(name,frame,0,y,400,8,C(.12f,.17f,.23f));
            var fill=Panel("Fill",back.transform,0,4,400,8,tint);
            fill.rectTransform.anchorMin=new Vector2(0,.5f);fill.rectTransform.anchorMax=new Vector2(0,.5f);
            fill.rectTransform.pivot=new Vector2(0,.5f);fill.rectTransform.anchoredPosition=Vector2.zero;
            return fill;
        }
        public void Build()
        {
            if(font==null) font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
            gameObject.AddComponent<GraphicRaycaster>();
            var bg=Panel("Backdrop",transform,0,0,10,10,Ink);
            bg.rectTransform.anchorMin=Vector2.zero;bg.rectTransform.anchorMax=Vector2.one;bg.rectTransform.sizeDelta=Vector2.zero;bg.rectTransform.anchoredPosition=Vector2.zero;
            frame=Rect("Portrait Battle · 480 x 850",transform,0,0,480,850);frame.anchorMin=frame.anchorMax=new Vector2(.5f,.5f);frame.pivot=new Vector2(.5f,.5f);frame.anchoredPosition=Vector2.zero;
            Panel("Arena",frame,0,425,480,850,C(.065f,.095f,.15f));
            Label("D R A G O N   T O W E R",frame,-30,34,390,28,21,Color.white);
            floorLabel=Label("01",frame,204,33,45,32,23,C(.96f,.75f,.40f));
            modeLabel=Label("타워 1층  /  MONSTER ROOM",frame,0,66,400,24,12,Muted);
            Panel("Enemy card",frame,0,129,440,80,C(.09f,.13f,.20f));
            enemyName=Label("바위 슬라임",frame,-60,111,280,28,20,Color.white,TextAnchor.MiddleLeft);
            enemyHP=Label("240 / 240",frame,140,111,120,26,16,Muted,TextAnchor.MiddleRight);
            enemyFill=Bar("Enemy HP",146,C(.89f,.42f,.47f));
            for(int i=0;i<7;i++) Panel("Tower step",frame,0,208+i*28,330-i*22,1,C(.12f,.17f,.24f));
            var e=Rect("Enemy Dragon",frame,30,264,235,235);enemyArt=e.gameObject.AddComponent<DragonGraphic>();enemyArt.color=C(.65f,.53f,.85f);enemyArt.enemy=true;enemyArt.raycastTarget=false;
            warning=Label("",frame,0,385,440,36,18,C(1,.74f,.38f));
            windupFill=Bar("Attack timing",414,C(1,.65f,.30f));
            var p=Rect("Player Dragon",frame,-26,532,210,210);playerArt=p.gameObject.AddComponent<DragonGraphic>();playerArt.raycastTarget=false;
            message=Label("",frame,0,634,440,30,16,C(.7f,.82f,.9f));
            playerName=Label("",frame,-60,674,280,28,19,Color.white,TextAnchor.MiddleLeft);
            playerHP=Label("",frame,140,674,120,28,16,Muted,TextAnchor.MiddleRight);
            playerFill=Bar("Player HP",700,C(.36f,.83f,.69f));
            itemButton1=MakeButton("아이템 1 · 빈 슬롯",frame,-108,727,204,38,C(.20f,.25f,.31f),out itemLabel1);
            itemButton2=MakeButton("아이템 2 · 빈 슬롯",frame,108,727,204,38,C(.20f,.25f,.31f),out itemLabel2);
            itemLabel1.fontSize=itemLabel2.fontSize=13;
            skillButton=MakeButton("스킬",frame,-154,790,132,60,C(.22f,.30f,.48f),out skillLabel);
            attackButton=MakeButton("공격",frame,0,790,148,68,C(.76f,.34f,.20f),out attackLabel);
            dodgeButton=MakeButton("회피",frame,154,790,132,60,C(.17f,.37f,.38f),out dodgeLabel);
            dragonInfo=Label("",frame,0,835,440,20,11,Muted);
            var modal=Panel("Result overlay",frame,0,425,480,850,new Color(.025f,.04f,.08f,.96f));modal.raycastTarget=true;resultPanel=modal.gameObject;
            Label("D R A G O N   T O W E R",modal.transform,0,244,430,30,16,Muted);
            resultTitle=Label("",modal.transform,0,324,430,65,42,C(1,.77f,.41f));
            resultDetail=Label("",modal.transform,0,409,390,90,19,Color.white);
            restartButton=MakeButton("다시 도전 · 랜덤 드래곤",modal.transform,0,525,360,64,C(.7f,.31f,.20f),out _);
            Label("이번 단계: 전투 연습\n알 부화와 성장·층 이동은 다음 단계입니다",modal.transform,0,614,400,66,15,Muted);
            resultPanel.SetActive(false);
            Fit();
        }
        void Awake() { EnsureItemControls();ApplyPixelSkin(); }
        void EnsureItemControls()
        {
            if(frame==null||itemButton1!=null)return;
            itemButton1=MakeButton("아이템 1 · 빈 슬롯",frame,-108,727,204,38,C(.20f,.25f,.31f),out itemLabel1);
            itemButton2=MakeButton("아이템 2 · 빈 슬롯",frame,108,727,204,38,C(.20f,.25f,.31f),out itemLabel2);
            itemLabel1.fontSize=itemLabel2.fontSize=13;
            MoveControl(skillButton,-154,790,132,60);MoveControl(attackButton,0,790,148,68);MoveControl(dodgeButton,154,790,132,60);
            if(dragonInfo!=null){dragonInfo.rectTransform.anchoredPosition=new Vector2(0,-835);dragonInfo.rectTransform.sizeDelta=new Vector2(440,20);dragonInfo.fontSize=11;}
        }
        static void MoveControl(Button button,float x,float y,float w,float h)
        {
            if(button==null)return;var rect=button.GetComponent<RectTransform>();rect.anchoredPosition=new Vector2(x,-y);rect.sizeDelta=new Vector2(w,h);
        }
        public void ApplyPixelSkin()
        {
            if (pixelPlayer != null) return;
            skin = Resources.Load<BattleSkin>("PixelBattleSkin");
            if (skin == null || frame == null) return;
            var arena = frame.Find("Arena").GetComponent<Image>();
            arena.sprite = skin.towerBackground;
            arena.color = Color.white;
            foreach (Transform child in frame)
                if (child.name == "Tower step") child.gameObject.SetActive(false);
            enemyArt.enabled = false;
            pixelEnemy = SpriteChild(enemyArt.transform, skin.rockSlime);
            enemyArt.rectTransform.sizeDelta = new Vector2(256,256);
            pixelPlayer = SpriteChild(playerArt.transform, skin.babyDragon);
            playerArt.rectTransform.sizeDelta = new Vector2(256,256);
            // Small opaque panels preserve text contrast without expensive effects or masks.
            var header = Panel("Pixel header",frame,0,44,480,88,new Color(.025f,.04f,.07f,.92f));
            header.transform.SetSiblingIndex(arena.transform.GetSiblingIndex()+1);
            var footer = Panel("Pixel controls backing",frame,0,738,480,224,new Color(.025f,.04f,.07f,.96f));
            footer.transform.SetSiblingIndex(header.transform.GetSiblingIndex()+1);
            DecorateButton(attackButton,new Color(1,.67f,.32f));
            DecorateButton(skillButton,new Color(.55f,.69f,.9f));
            DecorateButton(dodgeButton,new Color(.45f,.78f,.70f));
            DecorateButton(itemButton1,new Color(.64f,.68f,.74f));
            DecorateButton(itemButton2,new Color(.64f,.68f,.74f));
            restartButton.GetComponentInChildren<Text>().text="다시 도전";
        }
        Image SpriteChild(Transform parent, Sprite sprite)
        {
            var child=new GameObject("Pixel sprite",typeof(RectTransform),typeof(Image));
            var rect=child.GetComponent<RectTransform>();rect.SetParent(parent,false);
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            var image=child.GetComponent<Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;
            return image;
        }
        void DecorateButton(Button button,Color edge)
        {
            var r=button.GetComponent<RectTransform>().rect;
            Panel("Top pixel edge",button.transform,0,2,r.width,4,edge);
            Panel("Bottom pixel edge",button.transform,0,r.height-2,r.width,4,new Color(.04f,.055f,.08f));
            Panel("Left pixel edge",button.transform,-r.width/2+2,r.height/2,4,r.height,edge);
            Panel("Right pixel edge",button.transform,r.width/2-2,r.height/2,4,r.height,new Color(.04f,.055f,.08f));
        }
        public void SetDragonArt(DragonData dragon,int level=1)
        {
            ApplyPixelSkin();
            var activeSprite=dragon.BattleSpriteAtLevel(level);
            if(pixelPlayer==null && activeSprite!=null)
                pixelPlayer=SpriteChild(playerArt.transform,activeSprite);
            bool usePixel=activeSprite!=null && pixelPlayer!=null;
            playerArt.enabled=!usePixel;
            playerArt.color=dragon.color;
            if(pixelPlayer!=null)
            {
                pixelPlayer.sprite=activeSprite;
                pixelPlayer.color=Color.white;
                pixelPlayer.gameObject.SetActive(usePixel);
            }
            if(usePixel)playerArt.rectTransform.sizeDelta=new Vector2(256,256);
            else if(playerArt.GetComponent<CanvasRenderer>()==null)
                playerArt.gameObject.AddComponent<CanvasRenderer>();
            lastDragon=null;lastPlayerHP=lastEnemyHP=lastShieldHP=-1;nextTextRefresh=0;
            if(motion==null)
            {
                motion=gameObject.AddComponent<BattleAnimation>();
                motion.Initialize(this);
            }
            if(dragon.skill!=null)motion.ResetBattle(dragon.skill);
            else motion.ResetBattle(dragon.skillEffect);
        }
        public void SetSkillPresentation(SkillData skill)
        {
            if(skill==null)return;
            if(motion==null){motion=gameObject.AddComponent<BattleAnimation>();motion.Initialize(this);}
            motion.ResetBattle(skill);
        }
        public void PlayCue(CombatCue cue,int damage) { if(motion!=null) motion.Play(cue,damage); }
        public void SetEncounter(BattleEnemyStats enemy,int floor,bool boss,Sprite enemySprite=null)
        {
            if(floorLabel==null||modeLabel==null||enemyName==null)
            {
                foreach(var label in frame.GetComponentsInChildren<Text>(true))
                {
                    if(floorLabel==null&&label.text=="01")floorLabel=label;
                    if(modeLabel==null&&label.text.Contains("BATTLE"))modeLabel=label;
                    if(enemyName==null&&(label.text=="타워 수호룡"||label.text=="바위 슬라임"))enemyName=label;
                }
            }
            if(enemyName==null||floorLabel==null||modeLabel==null)
                throw new System.InvalidOperationException("Battle encounter labels are missing.");
            enemyName.text=enemy.displayName+"  /  "+ElementRules.DisplayName(enemy.elementType);floorLabel.text=floor.ToString("00");
            modeLabel.text="타워 "+floor+"층  /  "+(boss?"BOSS ROOM":"MONSTER ROOM");
            if(pixelEnemy!=null&&enemySprite!=null)pixelEnemy.sprite=enemySprite;
            enemyArt.rectTransform.sizeDelta=boss?new Vector2(310,310):new Vector2(256,256);
        }
        public void StepAnimation(BattleModel battle,float delta) { if(motion!=null) motion.Step(battle,delta); }
        void LateUpdate() { Fit(); }
        void Fit()
        {
            if(frame==null || Screen.width==0 || Screen.height==0) return;
            Rect safe=Screen.safeArea;
            var screen=new Vector2(Screen.width,Screen.height);
            if(safe==lastSafeArea && screen==lastScreen) return;
            lastSafeArea=safe;lastScreen=screen;
            float scale=Mathf.Min(safe.width/480f,safe.height/850f);
            frame.localScale=Vector3.one*scale;
            frame.anchoredPosition=safe.center-new Vector2(Screen.width,Screen.height)*.5f;
        }
        public void Bind(UnityAction attack,UnityAction skill,UnityAction dodge,UnityAction restart)
        {attackButton.onClick.AddListener(attack);skillButton.onClick.AddListener(skill);dodgeButton.onClick.AddListener(dodge);restartButton.onClick.AddListener(restart);}
        public void BindItems(UnityAction first,UnityAction second)
        {itemButton1.onClick.AddListener(first);itemButton2.onClick.AddListener(second);}
        public void SetItems(TowerRun run)
        {
            SetItemButton(itemButton1,itemLabel1,run,0);SetItemButton(itemButton2,itemLabel2,run,1);
        }
        void SetItemButton(Button button,Text label,TowerRun run,int index)
        {
            var item=run==null?null:run.ItemAt(index);int count=run==null?0:run.ItemCountAt(index);
            if(item==null){label.text="아이템 "+(index+1)+" · 빈 슬롯";button.interactable=false;return;}
            string countText=count>1?" ×"+count:"";
            label.text=(item.kind==ItemKind.Consumable?"사용 · ":"장착 · ")+item.displayName+countText;
            button.interactable=item.kind==ItemKind.Consumable;
        }
        public void Show(BattleModel b)
        {
            if(lastDragon!=b.Dragon)
            {
                lastDragon=b.Dragon;
                playerName.text=b.Dragon.displayName+"  /  "+b.Dragon.element;
                string hits=b.Dragon.skill.hitCount>1?" × "+b.Dragon.skill.hitCount:"";
                dragonInfo.text="공격 0.3초  ·  "+b.Dragon.skill.displayName+" "+b.Dragon.skill.damage+hits+" 피해 / "+b.Dragon.skill.cooldown+"초";
            }
            if(lastPlayerHP!=b.PlayerHP||lastShieldHP!=b.ShieldHP)
            {
                lastPlayerHP=b.PlayerHP;lastShieldHP=b.ShieldHP;playerHP.text=b.PlayerHP+" / "+b.Dragon.maxHP+(b.ShieldHP>0?"  + 보호막 "+b.ShieldHP:"");
                playerFill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,400f*b.PlayerHP/b.Dragon.maxHP);
            }
            if(lastEnemyHP!=b.EnemyHP)
            {
                lastEnemyHP=b.EnemyHP;enemyHP.text=b.EnemyHP+" / "+b.CurrentEnemyMaxHP;
                enemyFill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,400f*b.EnemyHP/b.CurrentEnemyMaxHP);
            }
            float until=(float)(b.NextEnemyStrike-b.Time);
            float progress=1-Mathf.Clamp01(until/BattleModel.WindupDuration);
            windupFill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,400*progress);
            bool refreshText=b.Time>=nextTextRefresh;
            if(refreshText) { warning.text=until<=BattleModel.WindupDuration ? "공격 임박!  "+until.ToString("0.0")+"초  ·  회피 준비" : "적의 움직임을 살피세요";nextTextRefresh=b.Time+.1; }
            warning.color=until<.42f ? new Color(1,.4f,.3f) : new Color(1,.75f,.43f);
            SetButton(attackButton,attackLabel,"공격",b.AttackReady-b.Time,b,refreshText);
            if(b.Dragon.skillDisabled){skillButton.interactable=false;skillLabel.text="스킬\n봉인됨";}
            else SetButton(skillButton,skillLabel,b.Dragon.skill.displayName,b.SkillReady-b.Time,b,refreshText);
            SetButton(dodgeButton,dodgeLabel,"회피",b.DodgeReady-b.Time,b,refreshText);
            if(b.Result!=BattleResult.Fighting){itemButton1.interactable=false;itemButton2.interactable=false;}
            resultPanel.SetActive(b.Result!=BattleResult.Fighting && (motion==null || motion.ResultReady));
            if(b.Result!=BattleResult.Fighting)
            {
                resultTitle.text=b.Result==BattleResult.Victory?"승리!":"다시 도전!";
                resultDetail.text=b.Result==BattleResult.Victory?"적을 물리쳤습니다.\n남은 HP  "+b.PlayerHP+" / "+b.Dragon.maxHP:"드래곤이 쓰러졌습니다.\n주황색 게이지가 차기 직전에 회피하세요.";
                restartButton.GetComponentInChildren<Text>().text=b.Result==BattleResult.Victory?"다음 층":"도전 결과";
            }
        }
        void SetButton(Button button,Text label,string title,double remaining,BattleModel model,bool refreshText)
        {
            bool ready=model.Result==BattleResult.Fighting && remaining<=0;
            bool changed=button.interactable!=ready;
            button.interactable=ready;
            if(refreshText || changed) label.text=title+"\n"+(remaining>0?remaining.ToString("0.0")+"초":"TAP");
        }
    }
}

