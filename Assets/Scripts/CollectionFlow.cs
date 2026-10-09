using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
namespace DragonTower
{
    public static class DragonTowerTheme
    {
        public static readonly Color Night=new Color(.095f,.075f,.15f,1),Slate=new Color(.23f,.20f,.32f,.98f),SlateLight=new Color(.32f,.28f,.43f,1),Gold=new Color(.88f,.66f,.30f,1),GoldDim=new Color(.43f,.31f,.15f,1),Parchment=new Color(.82f,.74f,.59f,1);
        static Sprite pixelPanel;
        static Sprite PixelPanel()
        {
            if(pixelPanel!=null)return pixelPanel;
            const int size=32,radius=5;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);texture.name="Pixel UI panel";texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;
            var pixels=new Color32[size*size];for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {float dx=Mathf.Max(radius-Mathf.Min(x,size-1-x),0),dy=Mathf.Max(radius-Mathf.Min(y,size-1-y),0);byte alpha=(byte)(dx+dy<=radius?255:0);int border=Mathf.Min(Mathf.Min(x,size-1-x),Mathf.Min(y,size-1-y));
             byte shade=(byte)(border<2?65:border<4?(y>size/2?245:110):border<6?(y>size/2?150:220):255);
             pixels[y*size+x]=new Color32(shade,shade,shade,alpha);}
            texture.SetPixels32(pixels);texture.Apply(false,true);pixelPanel=Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(7,7,7,7));pixelPanel.name="Pixel UI panel";return pixelPanel;
        }
        public static void Frame(Image panel,Color edge,bool corners=true)
        {
            if(panel==null)return;panel.sprite=PixelPanel();panel.type=Image.Type.Sliced;
            if(edge==Gold||edge==GoldDim)edge=new Color(.58f,.49f,.70f,1);
            var fill=panel.color;float high=Mathf.Max(fill.r,Mathf.Max(fill.g,fill.b));
            if(high<.31f&&fill.a>.6f&&fill.r<fill.g*1.8f)panel.color=Color.Lerp(new Color(.16f,.13f,.24f,fill.a),new Color(.29f,.25f,.39f,fill.a),Mathf.Clamp01(high/.31f));
            var shadow=panel.gameObject.GetComponent<Shadow>()??panel.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.58f);shadow.effectDistance=new Vector2(0,-6);shadow.useGraphicAlpha=true;
            var outline=panel.gameObject.GetComponent<Outline>()??panel.gameObject.AddComponent<Outline>();outline.effectColor=new Color(edge.r,edge.g,edge.b,1);outline.effectDistance=new Vector2(2,-2);outline.useGraphicAlpha=true;
            var trim=panel.transform.Find("Pixel edge trim");PixelHudFrame pixel;
            if(trim==null){var go=new GameObject("Pixel edge trim",typeof(RectTransform),typeof(PixelHudFrame));var r=go.GetComponent<RectTransform>();r.SetParent(panel.transform,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.one*3;r.offsetMax=-Vector2.one*3;pixel=go.GetComponent<PixelHudFrame>();pixel.outlineOnly=true;pixel.raycastTarget=false;}else pixel=trim.GetComponent<PixelHudFrame>();
            pixel.edge=edge;pixel.SetVerticesDirty();
        }
        public static void StyleButton(Button button,Color accent)
        {
            if(button==null)return;var image=button.targetGraphic as Image;if(image==null)return;Frame(image,accent);var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1.12f,1.12f,1.12f,1);colors.pressedColor=new Color(.68f,.68f,.74f,1);colors.selectedColor=colors.highlightedColor;colors.disabledColor=new Color(.15f,.17f,.2f,.78f);colors.colorMultiplier=1;button.colors=colors;
        }
        public static Color Grade(ItemGrade grade){switch(grade){case ItemGrade.Rare:return new Color(.20f,.58f,.82f);case ItemGrade.Epic:return new Color(.63f,.32f,.88f);case ItemGrade.Legendary:return new Color(1,.35f,.25f);case ItemGrade.Unique:return new Color(1,.63f,.18f);default:return Parchment;}}
        public static Color Grade(AugmentGrade grade){switch(grade){case AugmentGrade.Rare:return new Color(.20f,.58f,.82f);case AugmentGrade.Epic:return new Color(.63f,.32f,.88f);case AugmentGrade.Unique:return new Color(1,.63f,.18f);case AugmentGrade.Legendary:return new Color(1,.85f,.36f);default:return Parchment;}}
    }
    public sealed partial class CollectionFlow : MonoBehaviour
    {
        BattleController controller;BattleView battleView;DragonData[] catalog;ContentDatabase database;
        RectTransform root,body,portrait,roomIcon,startPrompt;
        Text notice;
        static Sprite[] rewardIcons;
        EggGraphic egg;
        float hatchTime=-1,clock;
        int codexPage,selectPage;
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
        public Button DragonButton { get; private set; }
        public Button[] ChoiceButtons { get; private set; }
        bool AncientScreen=>ScreenName=="드래곤 로비"||ScreenName=="드래곤 상태"||ScreenName=="드래곤 도감";
        Color UiText(Color value)=>!AncientScreen?value:value==Color.white?AncientUi.Ivory:value==Gold||value==DragonTowerTheme.Gold?AncientUi.Gold:value==Muted?AncientUi.Muted:value;
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
                Session=new CollectionSession(new ProfileStore(key),catalog);progression=FloorProgressionConfig.Load();SetupAchievements();
                ShowTitleScreen();
                if(Session.RecoveredBackup)notice.text="이전 정상 저장 데이터를 복구했습니다.";
            }
            catch(Exception e){Screen("저장 확인 필요");Label(e.Message,0,340,402,180,20,Color.white);notice.text="기존 저장을 보존했습니다. 오류를 확인한 뒤 다시 실행해 주세요.";Debug.LogException(e);}
        }
        RectTransform Rect(string name,Transform parent,float x,float y,float width,float height)
        {
            var go=new GameObject(name,typeof(RectTransform));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);
            r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(width,height);return r;
        }
        Image Panel(string name,Transform parent,float x,float y,float w,float h,Color color,Color edge)
        {
            var r=Rect(name,parent,x,y,w,h);var image=r.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=false;DragonTowerTheme.Frame(image,edge);if(AncientScreen)AncientUi.Frame(image,name.Contains("Title")?AncientSurfaceKind.Title:name.Contains("portrait")?AncientSurfaceKind.Portrait:AncientSurfaceKind.Panel);return image;
        }
        Text Label(string text,float x,float y,float w,float h,int size,Color color)
        {
            var r=Rect(text,body,x,y,w,h);var t=r.gameObject.AddComponent<Text>();t.font=battleView.font;t.text=text;t.fontSize=size;t.color=UiText(color);t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;return t;
        }
        Button Button(string title,float x,float y,float w,float h,UnityAction action)
        {
            var r=Rect(title,body,x,y,w,h);var image=r.gameObject.AddComponent<Image>();image.color=DragonTowerTheme.SlateLight;
            var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;b.onClick.AddListener(action);var nav=b.navigation;nav.mode=Navigation.Mode.None;b.navigation=nav;
            b.gameObject.AddComponent<DragonTowerUiSound>();
            var text=Rect("Label",r,0,h/2,w-18,h-12).gameObject.AddComponent<Text>();text.font=battleView.font;text.text=title;text.fontSize=20;text.color=Color.white;text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;
            var textShadow=text.gameObject.AddComponent<Shadow>();textShadow.effectColor=new Color(.06f,.035f,.10f,.9f);textShadow.effectDistance=new Vector2(0,-2);
            DragonTowerTheme.StyleButton(b,DragonTowerTheme.Gold);if(AncientScreen)AncientUi.Button(b);return b;
        }
        Text CardText(Transform parent,string name,string value,float x,float y,float w,float h,int size,Color color,TextAnchor alignment)
        {
            var r=Rect(name,parent,x,y,w,h);var text=r.gameObject.AddComponent<Text>();text.font=battleView.font;text.text=value;text.fontSize=size;text.color=UiText(color);text.alignment=alignment;text.raycastTarget=false;text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Truncate;return text;
        }
        Button RewardCard(string badge,string title,string summary,Sprite icon,float y,Color fill,Color accent,UnityAction action,ElementType? itemElement=null)
        {
            if(itemElement.HasValue)badge+=" · "+ItemElementName(itemElement.Value);
            var card=Button("",0,y,410,108,action);StyleCard(card,fill,accent);
            var iconFrame=Rect("Reward icon frame",card.transform,-154,54,76,76);var frame=iconFrame.gameObject.AddComponent<Image>();frame.color=new Color(.025f,.035f,.055f,.9f);frame.raycastTarget=false;DragonTowerTheme.Frame(frame,accent,false);
            var art=Rect("Reward icon",iconFrame,0,38,64,64).gameObject.AddComponent<Image>();art.sprite=icon;art.preserveAspect=true;art.color=icon==null?accent:Color.white;art.raycastTarget=false;
            if(icon==null)CardText(iconFrame,"Fallback icon","◆",0,38,64,64,31,accent,TextAnchor.MiddleCenter);
            CardText(card.transform,"Reward badge",badge,35,17,250,22,13,accent,TextAnchor.MiddleLeft);
            CardText(card.transform,"Reward title",title,35,43,250,28,19,Color.white,TextAnchor.MiddleLeft);
            CardText(card.transform,"Reward summary",summary,35,77,250,36,14,new Color(.80f,.85f,.9f),TextAnchor.UpperLeft);
            if(itemElement.HasValue){
                var symbol=ElementBadge(card.transform,itemElement.Value,178,17,18);
                var badgeText=card.transform.Find("Reward badge").GetComponent<Text>();
                badgeText.color=ElementHudThemes.Get(itemElement.Value).primaryColor;
                badgeText.rectTransform.sizeDelta=new Vector2(228,22);badgeText.rectTransform.anchoredPosition+=new Vector2(-11,0);
            }
            return card;
        }
        static string ItemElementName(ElementType element)=>element==ElementType.Neutral?"무속성":ElementRules.DisplayName(element);
        static Sprite RewardIcon(int index)
        {
            if(rewardIcons==null)
            {
                rewardIcons=new Sprite[16];var texture=Resources.Load<Texture2D>("UI/reward-icons");
                if(texture!=null){texture.filterMode=FilterMode.Point;float cw=texture.width/4f,ch=texture.height/4f;for(int i=0;i<16;i++){int col=i%4,row=i/4;rewardIcons[i]=Sprite.Create(texture,new Rect(col*cw,(3-row)*ch,cw,ch),new Vector2(.5f,.5f),100);rewardIcons[i].name="Reward icon "+i;}}
            }
            return index>=0&&index<rewardIcons.Length?rewardIcons[index]:null;
        }
        static int EffectIcon(System.Collections.Generic.IReadOnlyList<ContentEffect> effects,string name)
        {
            if(effects!=null)foreach(var effect in effects) switch(effect.type)
            {case ContentEffectType.Heal:case ContentEffectType.MaxHP:case ContentEffectType.MaxHPPercent:return 2;case ContentEffectType.DamageReductionPercent:return 3;case ContentEffectType.SkillCooldownPercent:case ContentEffectType.AttackCooldownPercent:return 4;case ContentEffectType.DodgeCooldownPercent:case ContentEffectType.DodgeDurationPercent:return 5;case ContentEffectType.CriticalChancePercent:case ContentEffectType.CriticalDamagePercent:return 10;case ContentEffectType.AttackDamage:case ContentEffectType.AttackDamagePercent:return 0;case ContentEffectType.SkillDamagePercent:return 1;case ContentEffectType.SkillDisabled:return 15;}
            string n=(name??"").ToLowerInvariant();if(n.Contains("화염")||n.Contains("업화"))return 6;if(n.Contains("서리")||n.Contains("얼음"))return 7;if(n.Contains("전기")||n.Contains("찌릿"))return 8;if(n.Contains("독")||n.Contains("뱀파"))return 9;if(n.Contains("발톱")||n.Contains("베기")||n.Contains("분신"))return 11;if(n.Contains("보호")||n.Contains("갑옷")||n.Contains("비늘"))return 3;if(n.Contains("시간")||n.Contains("가속"))return 4;return 14;
        }
        static Sprite AugmentIcon(AugmentData value)
        {
            if(value==null)return null;if(value.icon!=null)return value.icon;int icon;
            switch(value.mechanic){case AugmentMechanic.RapidFireInstinct:case AugmentMechanic.DodgeMaster:icon=5;break;case AugmentMechanic.FlameRemnant:case AugmentMechanic.Inferno:icon=6;break;case AugmentMechanic.FrostBarrier:case AugmentMechanic.IceCream:icon=7;break;case AugmentMechanic.StaticDischarge:case AugmentMechanic.Tingly:icon=8;break;case AugmentMechanic.Vampire:icon=9;break;case AugmentMechanic.ExploitWeakness:icon=10;break;case AugmentMechanic.ConsecutiveSlash:case AugmentMechanic.Spread:case AugmentMechanic.Clone:icon=11;break;case AugmentMechanic.FirstAid:case AugmentMechanic.LastStand:icon=2;break;case AugmentMechanic.IndomitableWill:icon=3;break;case AugmentMechanic.OverloadCore:case AugmentMechanic.ManaRampage:case AugmentMechanic.Transference:case AugmentMechanic.DoubleCasting:icon=14;break;default:icon=EffectIcon(value.effects,value.displayName);break;}return RewardIcon(icon);
        }
        public static Sprite ItemIcon(ItemData value)
        {
            if(value==null)return null;if(value.icon!=null)return value.icon;if(value.autoTrigger!=null&&value.autoTrigger.enabled){switch(value.autoTrigger.art){case ItemProcArt.Fireball:return RewardIcon(6);case ItemProcArt.Meteor:var fireLibrary=Resources.Load<FireSkillLibrary>("FireSkills/Library");return fireLibrary!=null?fireLibrary.meteor:RewardIcon(6);case ItemProcArt.FrostSpear:return RewardIcon(7);case ItemProcArt.Tornado:return RewardIcon(5);case ItemProcArt.Guard:return RewardIcon(3);case ItemProcArt.Lightning:return RewardIcon(8);case ItemProcArt.Earth:return RewardIcon(3);case ItemProcArt.Light:return RewardIcon(10);case ItemProcArt.Water:return RewardIcon(7);case ItemProcArt.Dark:return RewardIcon(9);}}int icon;
            switch(value.mechanic){case ItemMechanic.HealingPotion:case ItemMechanic.GreaterHealingPotion:case ItemMechanic.BloodPotion:icon=12;break;case ItemMechanic.TimeShard:case ItemMechanic.EmergencyAccelerator:icon=4;break;case ItemMechanic.BurstCore:case ItemMechanic.EchoCrystal:icon=14;break;case ItemMechanic.GiantRoar:case ItemMechanic.StunGun:icon=8;break;case ItemMechanic.BloodTome:case ItemMechanic.InfernoBreath:icon=6;break;case ItemMechanic.FrostWitchTear:icon=7;break;case ItemMechanic.WeaknessLens:case ItemMechanic.ExecutionerMark:icon=10;break;case ItemMechanic.PoisonFang:icon=9;break;case ItemMechanic.MeteorFragment:icon=1;break;case ItemMechanic.InfightingGlove:icon=13;break;case ItemMechanic.GuardianBrooch:case ItemMechanic.BarrierStone:icon=3;break;case ItemMechanic.PhoenixFeather:icon=5;break;default:icon=EffectIcon(value.effects,value.displayName);break;}return RewardIcon(icon);
        }
        static Sprite SkillIcon(SkillData value)
        {if(value==null)return null;if(value.icon!=null)return value.icon;switch(value.elementType){case ElementType.Fire:return RewardIcon(6);case ElementType.Ice:return RewardIcon(7);case ElementType.Lightning:return RewardIcon(8);case ElementType.Wind:return RewardIcon(5);default:return RewardIcon(1);}}
        static string ElementLabel(ElementType value)
        {switch(value){case ElementType.Fire:return "불";case ElementType.Ice:return "얼음";case ElementType.Wind:return "바람";case ElementType.Earth:return "땅";case ElementType.Lightning:return "전기";case ElementType.Water:return "물";case ElementType.Dark:return "어둠";case ElementType.Light:return "빛";default:return "무속성";}}
        void StyleCard(Button button,Color fill,Color accent){if(button==null)return;button.targetGraphic.color=fill;DragonTowerTheme.StyleButton(button,accent);if(AncientScreen)AncientUi.Button(button,fill.g>fill.r*1.8f&&fill.g>.3f);}
        void Screen(string name)
        {
            CancelRewardReveal();
            DragonTowerAudio.SetMusic(MusicMood.Lobby);
            if(body!=null){body.gameObject.SetActive(false);Destroy(body.gameObject);}
            portrait=null;roomIcon=null;startPrompt=null;egg=null;HatchButton=TowerButton=ContinueButton=RoomButton=SecondRoomButton=DragonButton=null;ChoiceButtons=null;
            ScreenName=name;root.gameObject.SetActive(true);root.SetAsLastSibling();
            body=Rect(name,root,0,425,480,850);
            bool lobby=name=="드래곤 로비",title=name=="시작 화면",mapTransition=name=="탑 이동",nestScene=lobby||title;var baseLayer=body.gameObject.AddComponent<Image>();baseLayer.color=nestScene?Color.white:AncientScreen?AncientUi.Background:DragonTowerTheme.Night;baseLayer.raycastTarget=false;
            if(nestScene){baseLayer.sprite=Resources.Load<Sprite>("UI/dragon-nest-lobby");baseLayer.preserveAspect=false;}
            var skin=Resources.Load<BattleSkin>("PixelBattleSkin");if(!nestScene&&skin!=null&&skin.towerBackground!=null)
            {var scenic=Rect("Dim tower backdrop",body,0,425,480,850).gameObject.AddComponent<Image>();scenic.sprite=skin.towerBackground;scenic.preserveAspect=false;scenic.color=new Color(.23f,.29f,.38f,.28f);scenic.raycastTarget=false;}
            if(lobby)
            {
                var logo=Rect("유대의 탑 로고",body,0,70,430,124).gameObject.AddComponent<Image>();logo.sprite=Resources.Load<Sprite>("UI/bond-tower-logo");logo.preserveAspect=true;logo.raycastTarget=false;
                Panel("Lobby badge",body,0,145,170,34,new Color(.12f,.055f,.035f,.78f),new Color(1,.65f,.30f,.78f));Label("드래곤 로비",0,145,160,30,18,Color.white);
            }
            else if(!title&&!mapTransition)
            {
                Panel("Ornate content frame",body,0,454,454,674,new Color(.17f,.14f,.24f,.98f),DragonTowerTheme.GoldDim);
                Panel("Title ribbon",body,0,94,420,54,new Color(.32f,.27f,.43f,1),DragonTowerTheme.Gold);
                Label("유 대 의   탑",0,42,440,32,21,Color.white);Label(name,0,95,420,45,28,Gold);
                Label("◆",-216,94,24,30,12,DragonTowerTheme.Gold);Label("◆",216,94,24,30,12,DragonTowerTheme.Gold);
            }
            notice=Label("이 기기에 자동 저장됩니다",0,805,430,58,14,Muted);
        }
        void Portrait(DragonData dragon,float y=348,int level=1,UnityAction onClick=null,bool framed=true)
        {
            if(framed)Panel("Dragon portrait frame",body,0,y,AncientScreen?420:286,286,new Color(.025f,.04f,.065f,.72f),new Color(dragon.color.r,dragon.color.g,dragon.color.b,1));
            portrait=Rect("Selected dragon",body,0,y,framed?260:310,framed?260:310);
            var activeSprite=dragon.BattleSpriteAtLevel(level);
            Graphic target;
            if(activeSprite!=null)
            {var image=portrait.gameObject.AddComponent<Image>();image.sprite=activeSprite;image.preserveAspect=true;image.raycastTarget=onClick!=null;target=image;
             portrait.gameObject.AddComponent<DragonPortraitIdle>().Bind(dragon,DragonData.EvolutionStage(level),framed?new Vector2(164,164):new Vector2(172,160));}
            else
            {portrait.gameObject.AddComponent<CanvasRenderer>();var graphic=portrait.gameObject.AddComponent<DragonGraphic>();graphic.color=dragon.color;graphic.raycastTarget=onClick!=null;target=graphic;}
            if(!framed){var shadow=portrait.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.66f);shadow.effectDistance=new Vector2(0,-8);shadow.useGraphicAlpha=true;}
            if(onClick!=null)
            {
                DragonButton=portrait.gameObject.AddComponent<Button>();DragonButton.targetGraphic=target;DragonButton.onClick.AddListener(onClick);
                var nav=DragonButton.navigation;nav.mode=Navigation.Mode.None;DragonButton.navigation=nav;
            }
        }
        public void ShowTitleScreen()
        {
            controller.EndBattle();Screen("시작 화면");
            var logo=Rect("유대의 탑 타이틀",body,0,145,450,190).gameObject.AddComponent<Image>();logo.sprite=Resources.Load<Sprite>("UI/bond-tower-logo");logo.preserveAspect=true;logo.raycastTarget=false;
            if(Session.Selected!=null)Portrait(Session.Selected,430,1,null,false);
            else
            {
                var eggRect=Rect("타이틀 드래곤 알",body,0,430,210,240);eggRect.gameObject.AddComponent<CanvasRenderer>();egg=eggRect.gameObject.AddComponent<EggGraphic>();egg.raycastTarget=false;
            }
            Panel("Start prompt",body,0,690,350,66,new Color(.08f,.035f,.025f,.84f),new Color(1,.68f,.30f,.92f));
            var prompt=Label("화면을 터치해 모험 시작",0,690,330,54,23,Color.white);startPrompt=prompt.rectTransform;
            Label("당신의 드래곤과 함께 탑의 정상으로",0,752,410,34,16,new Color(1,.88f,.68f));
            var tapRect=Rect("화면 전체 시작 버튼",body,0,425,480,850);var tapImage=tapRect.gameObject.AddComponent<Image>();tapImage.color=new Color(0,0,0,0);tapImage.raycastTarget=true;
            var tap=tapRect.gameObject.AddComponent<Button>();tap.targetGraphic=tapImage;tap.onClick.AddListener(StartFromTitle);var nav=tap.navigation;nav.mode=Navigation.Mode.None;tap.navigation=nav;tap.gameObject.AddComponent<DragonTowerUiSound>();
            notice.text="";
        }
        void StartFromTitle(){if(Session.Selected==null)ShowEgg();else ShowLobby();}
        public void ShowEgg()
        {
            if(hatchTime>=0)return;
            if(Session.Profile.eggs<=0){ShowLobby();return;}
            controller.EndBattle();Screen("새로운 만남");
            Label("당신의 첫 모험을 기다리는 알",0,177,430,44,20,Color.white);
            HatchButton=Button("",0,350,230,250,Hatch);
            StyleCard(HatchButton,new Color(.035f,.06f,.10f,.98f),DragonTowerTheme.Gold);
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
                DragonTowerAudio.PlayEvolution();
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
            returningToLobby=true;CheckpointBattle();StopAllCoroutines();controller.EndBattle();towerRun=null;returningToLobby=false;
            if(Session.Selected==null){ShowEgg();return;}
            Screen("드래곤 로비");var d=Session.Selected;Portrait(d,389,1,ShowDragonSelect,false);
            Panel("Dragon nameplate",body,0,493,254,42,AncientUi.Stone,AncientUi.DarkGold);
            var partnerName=Label(Session.SelectedName(),0,493,230,36,28,Gold);partnerName.supportRichText=false;partnerName.resizeTextForBestFit=true;partnerName.resizeTextMinSize=16;partnerName.resizeTextMaxSize=28;
            Label(d.element+"  ·  드래곤을 눌러 교체",0,541,410,34,17,Muted);
            TowerButton=Button(Session.HasSavedRun?"이어하기 · "+Session.Profile.activeRun.Floor+"층":"타워 오르기",0,622,400,70,()=>{if(Session.HasSavedRun)ResumeRun();else EnterTower();});StyleCard(TowerButton,new Color(.12f,.36f,.23f,1),new Color(.46f,.77f,.48f));
            Button("드래곤 도감",-105,713,190,62,ShowCodex);Button("드래곤 상태",105,713,190,62,ShowStatus);
            if(Session.Profile.eggs>0)Button("보관 알 "+Session.Profile.eggs,135,159,155,40,ShowEgg);
            Button("업적 · 도전 기록",0,771,330,40,ShowProgressRecords);
            notice.text="도전 중 얻는 성장과 보상은 로비의 수집 정보와 분리됩니다.";
        }
        public void EnterTower()
        {
            EnterTowerWithRoll(UnityEngine.Random.Range(0,100),UnityEngine.Random.Range(0,100));
        }
        public void EnterTowerWithRoll(int firstRoll) { EnterTowerWithRoll(firstRoll,(firstRoll+53)%100); }
        public void EnterTowerWithRoll(int firstRoll,int secondRoll)
        {
            if(Session.HasSavedRun){ResumeRun();return;}
            if(Session.Selected==null||hatchTime>=0)return;
            towerRun=new TowerRun(Session.Selected.maxHP,firstRoll,secondRoll,Session.Selected.elementType,Session.Selected.alternateSkillElement,Session.Selected.StableId,Session.Selected);
            towerRun.AttachInstance(Session.Profile.selectedInstanceId);SaveProgress();StartCoroutine(ShowFloorTransition(0,1,true));
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
                case TowerRoomKind.Monster:return "전투 · 승리하면 유대 경험치 획득";case TowerRoomKind.Item:return "상자에서 아이템 하나 선택";
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
            Label("유대 "+towerRun.Level+"   HP "+towerRun.CurrentHP+" / "+towerRun.MaxHP+"   GOLD "+towerRun.Gold,0,202,430,35,16,Muted);
            var first=towerRun.Choices[0];
            RoomButton=Button(RoomName(first)+"\n"+RoomDetail(first),0,towerRun.Choices.Count==1?385:326,400,112,()=>SelectTowerRoom(0));
            if(towerRun.Choices.Count>1)
            {
                var second=towerRun.Choices[1];
                SecondRoomButton=Button(RoomName(second)+"\n"+RoomDetail(second),0,492,400,112,()=>SelectTowerRoom(1));
            }
            notice.text=towerRun.Floor%10==0?"보스층은 하나의 길만 열립니다.":"두 방 중 하나를 선택하세요. · "+towerRun.Progress.score+"점";
        }
        void SelectTowerRoom(int index)
        {
            towerRun.BeginFloorRoom(index,UnityEngine.Random.Range(0,int.MaxValue));SaveProgress();ShowChosenRoom();
        }
        Button IconButton(string symbol,string caption,Color color,UnityAction action)
        {
            var button=Button("",0,384,230,210,action);StyleCard(button,Color.Lerp(color,DragonTowerTheme.Night,.38f),color);roomIcon=button.GetComponent<RectTransform>();
            var mark=Label(symbol,0,340,210,105,76,Color.white);mark.transform.SetParent(button.transform,false);mark.rectTransform.anchoredPosition=new Vector2(0,52);
            var text=button.GetComponentInChildren<Text>();text.text=caption;text.fontSize=20;
            return button;
        }
        void ShowChosenRoom()
        {
            if(towerRun==null||!towerRun.RoomChosen)return;
            if(towerRun.RewardClaimed){ContinueRunPhase();return;}
            switch(towerRun.Room)
            {
                case TowerRoomKind.Augment:ShowAugmentChoices(false);return;
                case TowerRoomKind.Item:ShowItemRoom();return;case TowerRoomKind.Recovery:ShowRecoveryRoom();return;
                case TowerRoomKind.Nest:ShowNestRoom();return;case TowerRoomKind.Gold:ShowGoldRoom();return;
                case TowerRoomKind.Shop:ShowShopRoom();return;
            }
            Screen("타워 "+towerRun.Floor+"층 · "+RoomName(towerRun.Room));
            bool boss=towerRun.Room==TowerRoomKind.Boss;
            string encounter=boss?(towerRun.Floor==20?"크라켄 수호자가 심해에서 깨어납니다.":"고대 룬 골렘이 깨어납니다."):"이 층의 몬스터가 길을 막고 있습니다.";
            Label(encounter,0,300,420,80,22,Color.white);
            Label("유대 "+towerRun.Level+"   HP "+towerRun.CurrentHP+" / "+towerRun.MaxHP,0,438,420,35,17,Muted);
            RoomButton=Button(boss?"보스 전투":"전투 시작",0,585,360,72,StartCurrentBattle);
            notice.text="다음 층에서도 현재 HP가 그대로 이어집니다.";
        }
        void StartCurrentBattle()
        {
            towerRun.BeginFloorCombat(progression,UnityEngine.Random.Range(0,100));
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
            if(towerRun.BattleResume==null){enemy.floor=towerRun.Floor;towerRun.BattleResume=new BattleResumeData{initialRunJson=JsonUtility.ToJson(towerRun.Capture(false)),seed=UnityEngine.Random.Range(0,int.MaxValue),enemy=enemy,monsterId=database.monsters.FirstOrDefault(m=>m!=null&&m.displayName==enemy.displayName)?.StableId};SaveProgress();}
            else{enemy=towerRun.BattleResume.enemy;var savedMonster=database.monsters.FirstOrDefault(m=>m!=null&&m.StableId==towerRun.BattleResume.monsterId);if(savedMonster!=null)enemySprite=savedMonster.battleSprite;}
            root.gameObject.SetActive(false);ScreenName=boss?"보스 전투":"몬스터 전투";
            controller.BeginBattle(Session.Selected,NamedBattleStats(),enemy,towerRun.Floor,boss,towerRun.CurrentHP,enemySprite,towerRun.Level,towerRun.CurrentSkill(Session.Selected.SkillAtLevel(towerRun.Level)));
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
            RoomButton=IconButton("▣","상자 열기",new Color(.38f,.24f,.12f),()=>{DragonTowerAudio.PlayChest();StartCoroutine(AnimateRoomAction(ShowItemChoices));});
            Label(CurrentItemsSummary(),0,570,420,82,14,Color.white);
            SecondRoomButton=Button("상자를 열지 않고 나간다",0,680,360,56,AdvanceFloor);
            notice.text="아이템을 얻지 않고 다음 층으로 이동할 수도 있습니다.";
        }
        void ShowItemChoices()
        {
            Screen("아이템 선택");Label("하나를 선택하거나 그냥 나갈 수 있습니다",0,154,420,34,18,Muted);
            Label(CurrentItemsSummary(),0,205,420,58,13,Color.white);
            var choices=database==null?Array.Empty<ItemData>():database.PickItems(3,towerRun.RewardSeed^towerRun.Floor);
            if(choices.Length>0)
            {
                ChoiceButtons=new Button[choices.Length];for(int i=0;i<choices.Length;i++){var item=choices[i];ChoiceButtons[i]=ItemChoice(item,300+i*120);}
                SecondRoomButton=Button("아무것도 가져가지 않는다",0,690,360,54,AdvanceFloor);
            }
            else {Label("등록된 아이템이 없습니다.",0,390,420,50,22,Color.white);RoomButton=Button("다음 층",0,610,360,68,AdvanceFloor);}
        }
        string CurrentItemsSummary()
        {
            if(towerRun==null)return "현재 보유 아이템 · 없음";string text="현재 보유 아이템";
            for(int i=0;i<towerRun.ItemSlots.Count;i++){var slot=towerRun.ItemSlots[i];text+="\n"+(i+1)+". "+slot.DisplayName+(slot.Count>1?" ×"+slot.Count:"")+" — "+ItemCardSummary(slot.Item);}var consumable=towerRun.ItemAt(3);text+="\n소모품 · "+(consumable==null?"없음":consumable.displayName+" ×"+towerRun.ItemCountAt(3));return text;
        }
        Button ItemChoice(ItemData item,float y)
        {return RewardCard(ItemGradeName(item.grade)+" · "+(item.kind==ItemKind.Consumable?"소모품":"장착"),item.displayName,ItemCardSummary(item),ItemIcon(item),y,DragonTowerTheme.Slate,DragonTowerTheme.Grade(item.grade),()=>AcquireItem(item,()=>ShowRoomResult(item.displayName+"을(를) 획득했습니다.")),item.DamageElement);}
        string ItemSummary(ItemData item)
        {
            if(item==null)return "효과 없음";string kind=item.kind==ItemKind.Consumable?"소모품":"장착";
            return kind+" · "+ItemElementName(item.DamageElement)+" · "+(!string.IsNullOrWhiteSpace(item.description)?item.description:EffectSummary(item.effects));
        }
        string ItemCardSummary(ItemData item)
        {return item==null?"효과 없음":(!string.IsNullOrWhiteSpace(item.description)?item.description:EffectSummary(item.effects));}
        void AcquireItem(ItemData item,Action complete,Action decline=null)
        {
            if(towerRun.CanAddItem(item)){towerRun.AddItem(item);complete();return;}
            Screen("아이템 교체");Label("새 아이템  ["+ItemGradeName(item.grade)+"] "+item.displayName,0,185,430,54,22,Gold);
            ElementBadge(body,item.DamageElement,-194,245,22);Label(ItemSummary(item),15,245,375,58,17,ElementHudThemes.Get(item.DamageElement).primaryColor);
            bool consumable=item.kind==ItemKind.Consumable;
            bool duplicate=consumable?towerRun.ItemAt(3)?.StableId==item.StableId:towerRun.OwnsEquipment(item);
            int count=duplicate?0:consumable?1:towerRun.ItemSlots.Count;ChoiceButtons=new Button[count];
            for(int i=0;i<count;i++)
            {
                int slot=consumable?TowerRun.ConsumableSlotIndex:i;var owned=towerRun.ItemAt(slot);
                ChoiceButtons[i]=Button((consumable?"소모품 전체 교체":"장착 "+(i+1)+"번 교체")+" · "+owned.displayName+" ×"+towerRun.ItemCountAt(slot)+"\n→ "+item.displayName+" ×1",0,330+i*92,400,78,()=>{towerRun.AddItem(item,slot);complete();});
                ChoiceButtons[i].GetComponentInChildren<Text>().fontSize=16;
            }
            if(duplicate)Label(consumable?"같은 소모품을 이미 3개 보유하고 있습니다.":"이미 장착 중인 아이템입니다.\n같은 아이템은 중복 장착할 수 없습니다.",0,390,420,110,20,Gold);
            SecondRoomButton=Button("획득하지 않고 돌아가기",0,640,360,56,()=>{if(decline!=null)decline();else AdvanceFloor();});
            notice.text="장착 3종 + 소모품 1종(최대 3개) · 교체한 기존 아이템은 사라집니다.";
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
            RoomButton=IconButton("G","골드 획득",new Color(.68f,.48f,.08f),()=>StartCoroutine(AnimateRoomAction(()=>{towerRun.AddGold(50);DragonTowerAudio.PlayPurchase();ShowRoomResult("50 골드를 획득했습니다.");})));
            notice.text="골드는 이번 도전의 상점에서 사용합니다.";
        }
        void ShowShopRoom()
        {
            Screen(towerRun.Phase==FloorPhase.PostBattleShop?"떠돌이 상인 등장!":"상점방");Label("보유 골드  "+towerRun.Gold,0,175,420,42,24,Gold);
            var goods=database==null?Array.Empty<ItemData>():database.PickItems(2,towerRun.RewardSeed^(towerRun.Floor<<10));
            ChoiceButtons=new Button[goods.Length];
            for(int i=0;i<goods.Length;i++)
            {
                var item=goods[i];ChoiceButtons[i]=RewardCard(ItemGradeName(item.grade)+" · "+(item.kind==ItemKind.Consumable?"소모품":"장착")+" · "+item.price+"G",item.displayName,ItemCardSummary(item),ItemIcon(item),315+i*125,DragonTowerTheme.Slate,DragonTowerTheme.Grade(item.grade),()=>BuyItem(item),item.DamageElement);
            }
            SecondRoomButton=Button("구매하지 않고 다음 층",0,600,400,66,()=>{if(towerRun.Phase==FloorPhase.PostBattleShop)FinishPostBattleShop();else AdvanceFloor();});
            notice.text="구매한 아이템은 빈 슬롯에 넣거나 기존 아이템과 교체합니다.";
        }
        void FinishPostBattleShop(string purchase=null)
        {
            if(towerRun.Phase!=FloorPhase.PostBattleShop)return;
            towerRun.CompletePostBattleShop();SaveProgress();
            if(purchase==null){ContinueRunPhase();return;}
            Screen("구매 완료");Label(purchase,0,330,420,100,24,Gold);Label("남은 골드 "+towerRun.Gold,0,440,420,40,20,Muted);
            RoomButton=Button("다음 층으로",0,610,360,60,ContinueRunPhase);
        }
        void BuyItem(ItemData item)
        {
            if(item==null)return;if(towerRun.Gold<item.price){notice.text="골드가 부족합니다.";return;}
            DragonTowerAudio.PlayPurchase();
            bool travelling=towerRun.Phase==FloorPhase.PostBattleShop;
            AcquireItem(item,()=>{towerRun.SpendGold(item.price);if(travelling)FinishPostBattleShop(item.displayName+" 구매 · "+item.price+"G");else ShowRoomResult(item.displayName+"을(를) "+item.price+"G에 구매했습니다.");},()=>ShowShopRoom());
        }
        void ShowNestRoom()
        {
            Screen("드래곤 둥지");Label("알을 터치해 부화시키세요",0,178,420,44,23,Gold);
            RoomButton=Button("",0,375,230,245,HatchNestEgg);StyleCard(RoomButton,new Color(.035f,.06f,.10f,.98f),DragonTowerTheme.Gold);roomIcon=RoomButton.GetComponent<RectTransform>();
            var eggRect=Rect("Nest egg",RoomButton.transform,0,126,170,190);eggRect.gameObject.AddComponent<CanvasRenderer>();egg=eggRect.gameObject.AddComponent<EggGraphic>();egg.raycastTarget=false;
            notice.text="태어난 드래곤은 즉시 도감과 보유 목록에 등록됩니다.";
        }
        void HatchNestEgg()
        {
            if(RoomButton==null||!RoomButton.interactable)return;
            hatched=catalog[UnityEngine.Random.Range(0,catalog.Length)];
            try{FinishNestReward(hatched);DragonTowerAudio.PlayEvolution();if(egg!=null){egg.cracked=true;egg.SetVerticesDirty();}StartCoroutine(AnimateRoomAction(ShowNestResult));}
            catch(Exception e){notice.text="등록하지 못했습니다. 알은 유지됩니다.";Debug.LogException(e);}
        }
        void ShowNestResult()
        {
            Screen("둥지 부화 성공!");Portrait(hatched,315);Label(hatched.displayName+" · "+hatched.element,0,505,420,48,28,Gold);AutoContinueReward();
            Label("도감과 보유 목록에 등록되었습니다.",0,558,420,38,18,Color.white);
            RoomButton=Button("다음 층",0,680,360,66,AdvanceFloor);
        }
        void ShowAugmentChoices(bool levelReward)
        {
            Screen(levelReward?"유대 "+towerRun.Level+" 증강":"증강방");
            Label("이번 성장을 선택하세요",0,161,430,36,21,Gold);
            Label("카드를 눌러 자세히 읽고 선택할 수 있어요",0,194,430,30,16,Muted);
            var rewards=PickRandomRewards(towerRun.RewardSeed^towerRun.Level^(towerRun.Floor<<8),3);
            ChoiceButtons=new Button[Math.Max(3,rewards.Length)];
            if(rewards.Length>0)
            {
                for(int i=0;i<rewards.Length;i++)
                {
                    int slot=i;var reward=rewards[i];Button choice;
                    if(reward.augment!=null)
                    {
                        var augment=reward.augment;
                        choice=AugmentRewardCard(GradeName(augment.grade)+" · 증강",augment.displayName,AugmentSummary(augment),AugmentIcon(augment),301+slot*178,DragonTowerTheme.Grade(augment.grade),()=>ShowRewardDetails(augment,null,levelReward));
                    }
                    else
                    {
                        var skill=reward.skill;
                        choice=AugmentRewardCard("스킬 · "+ElementLabel(skill.elementType),skill.displayName,SkillSummary(skill),SkillIcon(skill),301+slot*178,new Color(.35f,.72f,1),()=>ShowRewardDetails(null,skill,levelReward));
                    }
                    ChoiceButtons[slot]=choice;
                }
                notice.text="증강을 불러오는 중…";
                BeginRewardReveal();
                SecondRoomButton=Button("선택하지 않고 넘어가기",0,765,400,40,()=>SkipAugmentReward(levelReward));
                SecondRoomButton.GetComponentInChildren<Text>().fontSize=17;
                SecondRoomButton.interactable=false;
            }
            else
            {
                ContinueButton=Button("다음 층으로",0,430,380,66,()=>{towerRun.ConsumeEmptyAugmentReward(levelReward);AdvanceFloor();});
                notice.text="현재 획득할 수 있는 선택지가 없습니다.";
            }
        }
        sealed class RandomReward { public AugmentData augment;public SkillData skill; }
        RandomReward[] PickRandomRewards(int seed,int count)
        {
            if(database==null)return Array.Empty<RandomReward>();
            var augments=(database.augments??Array.Empty<AugmentData>()).Where(a=>a!=null&&towerRun.CanTakeAugment(a)).ToList();
            var skills=(database.skills??Array.Empty<SkillData>()).Where(s=>s!=null&&towerRun.CanOfferSkill(s)&&s.StableId!=towerRun.CurrentSkillId(Session.Selected.SkillAtLevel(towerRun.Level))).ToList();
            var result=new System.Collections.Generic.List<RandomReward>();var random=new System.Random(seed);
            // Separate eligibility roll: ordinary rewards retain their existing random stream.
            var signatureRandom=new System.Random(seed^0x5A17);
            skills.RemoveAll(s=>s.EffectiveRewardEligibilityPercent<100&&signatureRandom.NextDouble()*100>=s.EffectiveRewardEligibilityPercent);
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
        {switch(grade){case AugmentGrade.Rare:return "레어";case AugmentGrade.Epic:return "에픽";case AugmentGrade.Unique:return "유니크";case AugmentGrade.Legendary:return "전설";default:return "일반";}}
        static string ItemGradeName(ItemGrade grade)
        {switch(grade){case ItemGrade.Rare:return "레어";case ItemGrade.Epic:return "에픽";case ItemGrade.Unique:return "유니크";case ItemGrade.Legendary:return "전설";default:return "일반";}}
        static Color GradeColor(AugmentGrade grade)
        {switch(grade){case AugmentGrade.Rare:return new Color(.18f,.38f,.58f);case AugmentGrade.Epic:return new Color(.39f,.22f,.58f);case AugmentGrade.Unique:return new Color(.68f,.40f,.12f);case AugmentGrade.Legendary:return new Color(.45f,.28f,.06f);default:return new Color(.18f,.27f,.36f);}}
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
        string StatusName(CombatStatusEffect effect)=>StatusDisplay(effect);
        void ChooseAugment(AugmentData augment,bool levelReward)
        {try{towerRun.AddAugment(augment,levelReward);DragonTowerAudio.PlayAugment();if(levelReward)FinishLevelReward(augment.displayName+"을(를) 선택했습니다.");else ShowRoomResult(augment.displayName+"을(를) 선택했습니다.");}catch(Exception e){notice.text=e.Message;}}
        void ChooseAugment(string id,bool levelReward)
        {
            towerRun.AddAugment(id,levelReward);DragonTowerAudio.PlayAugment();if(levelReward)FinishLevelReward("증강을 선택했습니다.");else ShowRoomResult("증강을 선택했습니다.");
        }
        void ChooseSkill(SkillData skill,bool levelReward)
        {try{towerRun.ReplaceSkill(skill,levelReward);DragonTowerAudio.PlayAugment();if(levelReward)FinishLevelReward(skill.displayName+" 스킬로 교체했습니다.");else ShowRoomResult(skill.displayName+" 스킬로 교체했습니다.");}catch(Exception e){notice.text=e.Message;}}
        void ShowRoomResult(string result)
        {
            FinishRoomEvent();
            Screen("방 완료");Label(result,0,310,420,100,24,Color.white);
            Label("유대 "+towerRun.Level+"   HP "+towerRun.CurrentHP+" / "+towerRun.MaxHP+"   GOLD "+towerRun.Gold,0,435,430,36,16,Muted);
            Label("방 경험치 +"+progression.RoomExperience(towerRun.Room)+" EXP",0,489,420,35,18,Gold);
            notice.text=towerRun.AdditionalBattle?"몬스터가 접근합니다…":"다음 층으로 이동합니다…";AutoContinueReward();
        }
        void AdvanceFloor()
        {
            if(towerRun==null)return;
            if(towerRun.Phase==FloorPhase.RoomEvent){FinishRoomEvent();ContinueRunPhase();return;}
            if(towerRun.Phase!=FloorPhase.Complete||towerRun.PendingEvolutionStage>0||towerRun.PendingLevelAugments>0){ContinueRunPhase();return;}
            if(towerRun.Floor>=80){Session.FinishRun(towerRun,true);towerRun.End();Screen("타워 완주");Label("80층을 돌파했습니다!\n최종 점수 "+towerRun.Progress.score+"점",0,350,420,80,28,Gold);Button("로비로",0,610,360,60,ShowLobby);return;}
            int clearedFloor=towerRun.Floor;towerRun.MoveToNextFloor(UnityEngine.Random.Range(0,100),UnityEngine.Random.Range(0,100));SaveProgress();
            if(towerRun.Floor%10==1)StartCoroutine(ShowFloorTransition(clearedFloor,towerRun.Floor,false));else ShowTowerChoices();
        }
        static int TowerAreaIndex(int floor)
        {return floor>=100?10:Mathf.Clamp((Mathf.Max(1,floor)-1)/10,0,9);}
        static string TowerAreaName(int floor)
        {
            if(floor<=10)return "지하 감옥";if(floor<=20)return "버려진 고대 수로";if(floor<=30)return "잊혀진 용광로 공방";
            if(floor<=40)return "휘몰아치는 천공 회랑";if(floor<=50)return "뇌운의 피뢰탑";if(floor<=60)return "영구동토의 수정 궁전";
            if(floor<=70)return "잊혀진 대도서관";if(floor<=80)return "영겁의 일광 성소";if(floor<=90)return "찢겨진 공허 회랑";
            if(floor<100)return "왕좌로 가는 길";return "태초의 용좌";
        }
        IEnumerator ShowFloorTransition(int clearedFloor,int targetFloor,bool firstEntry)
        {
            Screen("탑 이동");notice.gameObject.SetActive(false);
            var viewport=Rect("Tower map viewport",body,0,425,480,850);viewport.gameObject.AddComponent<RectMask2D>();
            const float mapHeight=2400,mapWidth=1600,areaCount=11;
            var mapRect=Rect("유대의 탑 확대 지도",viewport,0,425,mapWidth,mapHeight);mapRect.anchorMin=mapRect.anchorMax=mapRect.pivot=new Vector2(.5f,.5f);
            var map=mapRect.gameObject.AddComponent<Image>();map.sprite=Resources.Load<Sprite>("UI/bond-tower-world-map");map.preserveAspect=false;map.raycastTarget=false;
            float AreaOffset(int floor)
            {int area=TowerAreaIndex(floor);float localY=-mapHeight*.5f+(area+.5f)*(mapHeight/areaCount);return -localY;}
            float destination=AreaOffset(targetFloor),origin=firstEntry?destination+115:AreaOffset(clearedFloor);
            mapRect.anchoredPosition=new Vector2(0,origin);
            var shade=Rect("Map cinematic shade",viewport,0,425,480,850).gameObject.AddComponent<Image>();shade.color=new Color(.01f,.02f,.045f,.23f);shade.raycastTarget=false;
            var focus=Panel("Current area focus",viewport,0,425,310,132,new Color(.025f,.04f,.07f,.42f),new Color(1,.71f,.28f,.78f));
            var areaText=CardText(focus.transform,"Area name",TowerAreaName(firstEntry?targetFloor:clearedFloor),0,47,292,42,25,Color.white,TextAnchor.MiddleCenter);
            var floorText=CardText(focus.transform,"Floor progress",firstEntry?"1층에서 여정을 시작합니다":clearedFloor+"층 돌파",0,91,292,30,17,new Color(1,.80f,.43f),TextAnchor.MiddleCenter);
            var header=Panel("Map header",viewport,0,74,390,58,new Color(.025f,.04f,.075f,.88f),DragonTowerTheme.Gold);
            CardText(header.transform,"Transition title",firstEntry?"유대의 탑 입장":"새로운 구역으로 상승",0,29,370,44,22,Color.white,TextAnchor.MiddleCenter);
            yield return new WaitForSecondsRealtime(firstEntry?.55f:.35f);
            float elapsed=0,duration=firstEntry?1.25f:2.15f;
            while(elapsed<duration)
            {
                elapsed+=Time.unscaledDeltaTime;float t=Mathf.Clamp01(elapsed/duration);t=t*t*(3-2*t);
                mapRect.anchoredPosition=new Vector2(0,Mathf.Lerp(origin,destination,t));
                if(!firstEntry&&t>.52f){areaText.text=TowerAreaName(targetFloor);floorText.text=targetFloor+"층 도착";}
                yield return null;
            }
            DragonTowerAudio.PlayUi();float pulse=0;
            while(pulse<.55f)
            {
                pulse+=Time.unscaledDeltaTime;float scale=1+Mathf.Sin(pulse*18)*.035f;focus.rectTransform.localScale=Vector3.one*scale;yield return null;
            }
            focus.rectTransform.localScale=Vector3.one;yield return new WaitForSecondsRealtime(.3f);ShowTowerChoices();
        }
        public void ResolveBattleResult()
        {
            if(towerRun==null||controller.CurrentBattle==null||controller.CurrentBattle.Result==BattleResult.Fighting)return;
            if(controller.CurrentBattle.Result==BattleResult.Victory)
            {
                BattleSettled();controller.EndBattle();ContinueRunPhase();return;
            }
            BattleSettled();
            int floor=towerRun.Floor;towerRun.End();controller.EndBattle();
            Screen("도전 종료");Label(Session.SelectedName()+"\n"+floor+"층에서 모험을 마쳤습니다",0,312,420,70,28,Gold);
            Label("최종 점수 "+towerRun.Progress.score+"점\n최대 유대 "+towerRun.Progress.maxBond+" · 처치 "+towerRun.MonstersDefeated+" (보스 "+towerRun.BossesDefeated+")\n보유 드래곤과 업적은 유지됩니다.",0,430,420,110,19,Color.white);
            RoomButton=Button("로비로 돌아가기",0,610,360,68,ShowLobby);
            notice.text="같은 드래곤으로 다시 도전할 수 있습니다.";
        }
        void ShowExperienceReward(int previousLevel,int previousExperience)
        {
            Screen(towerRun.Phase==FloorPhase.BattleRewards?"전투 경험치":"방 보상 경험치");
            Label(Session.SelectedName(),0,170,420,36,23,Gold);
            var levelText=Label("유대 "+previousLevel,0,230,420,54,31,Color.white);
            var expText=Label("EXP "+previousExperience+" / "+towerRun.ExperienceToNext,0,294,420,34,17,Muted);
            var back=Panel("Experience gauge",body,0,350,390,24,new Color(.07f,.10f,.15f,.98f),DragonTowerTheme.GoldDim);
            var fill=Rect("Experience fill",back.transform,-195,12,390,18).gameObject.AddComponent<Image>();fill.color=new Color(.35f,.76f,1);fill.raycastTarget=false;
            fill.rectTransform.anchorMin=fill.rectTransform.anchorMax=new Vector2(0,.5f);fill.rectTransform.pivot=new Vector2(0,.5f);fill.rectTransform.anchoredPosition=Vector2.zero;
            fill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,390f*previousExperience/towerRun.ExperienceToNext);
            Label((towerRun.Phase==FloorPhase.BattleRewards?"몬스터 처치  +":"방 보상  +")+towerRun.LastExperienceGain+" EXP",0,410,420,42,21,Gold);
            if(towerRun.Phase==FloorPhase.BattleRewards&&towerRun.LastBattleGold>0)Label("처치 보상 +"+towerRun.LastBattleGold+" GOLD",0,475,420,40,21,Gold);
            notice.text="경험치가 가득 차면 유대가 상승합니다.";StartCoroutine(AnimateExperience(previousLevel,previousExperience,fill,expText,levelText));
        }
        IEnumerator AnimateExperience(int previousLevel,int previousExperience,Image fill,Text expText,Text levelText)
        {
            var owner=body;int required=towerRun.ExperienceToNext;
            int shownLevel=previousLevel,shownExperience=previousExperience;
            while(shownLevel<towerRun.Level||shownExperience<towerRun.Experience)
            {
                int target=shownLevel<towerRun.Level?required:towerRun.Experience;
                float elapsed=0,duration=Mathf.Max(.45f,1.05f*(target-shownExperience)/required);
                while(elapsed<duration)
                {
                    if(body!=owner||fill==null)yield break;
                    elapsed+=Mathf.Min(Time.deltaTime,.1f);float t=Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/duration));
                    float value=Mathf.Lerp(shownExperience,target,t);
                    fill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,390f*value/required);
                    expText.text="EXP "+Mathf.RoundToInt(value)+" / "+required;yield return null;
                }
                shownExperience=target;
                if(target==required){shownLevel++;shownExperience=0;DragonTowerAudio.PlayLevelUp();levelText.text="유대 상승!   유대 "+shownLevel;levelText.color=Gold;yield return new WaitForSeconds(.25f);}
            }
            if(body!=owner||fill==null)yield break;
            fill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,390f*towerRun.Experience/required);
            expText.text="EXP "+towerRun.Experience+" / "+required;
            ContinueButton=Button("계속",0,610,360,68,ContinueAfterExperienceReward);
            // Room rewards still flow automatically into the saved encounter decision.
            if(towerRun.Phase!=FloorPhase.BattleRewards)
            {
                yield return new WaitForSeconds(.8f);
                if(body==owner&&!returningToLobby)ContinueAfterExperienceReward();
            }
        }
        void ContinueAfterExperienceReward(){towerRun.AcknowledgeExperience();SaveProgress();ContinueRunPhase();}
        bool TryShowMonsterDrop()
        {
            if(string.IsNullOrEmpty(towerRun.PendingDropId))return false;
            var item=database.items.FirstOrDefault(x=>x.StableId==towerRun.PendingDropId);if(item==null)throw new InvalidOperationException("전리품을 찾을 수 없습니다.");
            Screen("몬스터 전리품");Label("["+ItemGradeName(item.grade)+"] "+item.displayName,0,250,420,54,26,Gold);ElementBadge(body,item.DamageElement,-194,330,22);Label(ItemSummary(item),15,330,375,82,19,ElementHudThemes.Get(item.DamageElement).primaryColor);
            Action finish=()=>{towerRun.ConsumeDrop();SaveProgress();ContinueRunPhase();};
            RoomButton=Button("획득 또는 교체",0,475,360,72,()=>AcquireItem(item,finish,finish));SecondRoomButton=Button("두고 간다",0,575,360,62,()=>finish());return true;
        }
        void ContinueAfterBattleRewards()
        {ContinueRunPhase();}
        void ContinueAfterLevelRewards()
        {
            ContinueRunPhase();
        }
        void ShowEvolutionCutscene()
        {
            int stage=towerRun.PendingEvolutionStage;var dragon=Session.Selected;
            DragonTowerAudio.PlayEvolution();
            Screen(stage==1?"중간 진화":"최종 진화");
            Label("유대 "+towerRun.Level,0,148,420,38,19,Muted);
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
            evolutionText.text=dragon.NameForStage(stage)+"(으)로 진화했습니다!\n"+
                "최대 HP +"+Mathf.RoundToInt((dragon.HealthMultiplier(stage)-1)*100)+"% · 공격력 +"+Mathf.RoundToInt((dragon.AttackMultiplier(stage)-1)*100)+"%\n"+
                dragon.passiveName+" 패시브 +"+Mathf.RoundToInt((TowerRun.PassiveEvolutionMultiplier(stage)-1)*100)+"%";
            evolutionText.color=Gold;evolutionText.fontSize=20;
            ContinueButton=Button(towerRun.PendingLevelAugments>0?"증강 선택으로":"다음 층으로",0,710,360,66,FinishEvolution);
            notice.text=stage==1?"유대 20에서 최종 진화합니다.":"최종 진화를 완료했습니다.";
        }
        void FinishEvolution()
        {
            if(towerRun==null||towerRun.PendingEvolutionStage<=0)return;
            towerRun.ConsumeEvolution();SaveProgress();ContinueAfterLevelRewards();
        }
        public void ShowCodex()
        {
            Screen("드래곤 도감");int found=0;foreach(var d in catalog)if(Session.Owns(d.StableId))found++;
            Label("발견한 드래곤  "+found+" / "+catalog.Length,0,145,420,34,19,Muted);
            int start=codexPage*6,end=Math.Min(catalog.Length,start+6);
            for(int i=start;i<end;i++)
            {
                var d=catalog[i];bool owns=Session.Owns(d.StableId);
                int local=i-start,column=local%2,row=local/2;float x=column==0?-106:106,y=237+row*143;
                var b=Button(owns?d.displayName+"\n"+ElementRules.DisplayName(d.elementType):"???\n미발견",x,y,198,136,()=>ShowCodexEntry(d));
                StyleCard(b,owns?new Color(.09f,.16f,.23f,.98f):new Color(.035f,.045f,.065f,.98f),owns?new Color(d.color.r,d.color.g,d.color.b,1):new Color(.22f,.24f,.28f));
                if(AncientScreen){AncientUi.Button(b,false,owns&&Session.Selected!=null&&Session.Selected.StableId==d.StableId,!owns);b.GetComponentInChildren<AncientStoneSurface>().kind=AncientSurfaceKind.Collection;}
                var text=b.GetComponentInChildren<Text>();text.fontSize=14;text.rectTransform.anchoredPosition=new Vector2(0,-107);text.rectTransform.sizeDelta=new Vector2(180,44);
                DragonArtwork(b.transform,d,0,0,54,86,86,!owns);
                if(owns)ElementBadge(b.transform,d.elementType,-74,105,26);
            }
            if(codexPage>0)Button("◀",-105,650,90,46,()=>{codexPage--;ShowCodex();});
            Label((codexPage+1)+" / "+Math.Max(1,(catalog.Length+5)/6),0,650,100,42,16,Muted);
            if(end<catalog.Length)Button("▶",105,650,90,46,()=>{codexPage++;ShowCodex();});
            Button("로비로 돌아가기",0,728,380,60,ShowLobby);
        }
        void ShowCodexEntry(DragonData dragon)
        {
            bool owns=Session.Owns(dragon.StableId);Screen("드래곤 도감");
            Label(owns?dragon.displayName:"미발견 드래곤",0,142,420,40,25,owns?Gold:Muted);
            if(owns)ElementBadge(body,dragon.elementType,-150,142,30);
            for(int stage=0;stage<3;stage++)
            {
                float x=(stage-1)*145;var card=Rect("Evolution form "+stage,body,x,306,136,190);
                var panel=card.gameObject.AddComponent<Image>();panel.color=new Color(.075f,.105f,.15f,.9f);panel.raycastTarget=false;DragonTowerTheme.Frame(panel,DragonTowerTheme.GoldDim);if(AncientScreen)AncientUi.Frame(panel,AncientSurfaceKind.Collection);
                DragonArtwork(card,dragon,stage,0,84,124,124,!owns);
                string formName=owns?dragon.NameForStage(stage):"???";
                var title=Label(formName,0,390,130,46,14,owns?Color.white:Muted);title.transform.SetParent(card,false);title.rectTransform.anchoredPosition=new Vector2(0,-166);
                var stageText=Label(stage==0?"기본":stage==1?"중간 진화":"최종 진화",0,426,130,24,12,Muted);stageText.transform.SetParent(card,false);stageText.rectTransform.anchoredPosition=new Vector2(0,-188);
            }
            if(owns)
            {
                string description=string.IsNullOrWhiteSpace(dragon.description)?"함께 타워를 오르는 "+dragon.element+" 속성 드래곤입니다.":dragon.description;
                DragonStatsRow(dragon,445);
                DragonInfoCard("타고난 능력",dragon.passiveName,dragon.passiveDescription,3,518,94);
                DragonInfoCard("드래곤 이야기","",description,14,626,92);
                notice.text="진화할수록 모습과 힘이 함께 성장합니다.";
            }
            else
            {
                Label("아직 발견하지 못한 드래곤입니다.\n알을 부화하거나 드래곤 둥지에서 발견할 수 있습니다.",0,555,420,110,17,Muted);
                notice.text="미발견 드래곤의 정보는 실루엣으로 표시됩니다.";
            }
            Button("도감 목록으로",0,718,380,60,ShowCodex);
        }
        static string SkillStatus(SkillData skill)
        {
            if(skill==null||skill.statusEffect==CombatStatusEffect.None||skill.statusChancePercent<=0)return "";
            string name=StatusDisplay(skill.statusEffect);
            return "  ·  "+name+" "+Mathf.RoundToInt(skill.statusChancePercent)+"%";
        }
        static string StatusDisplay(CombatStatusEffect effect)
        {switch(effect){case CombatStatusEffect.Burn:return "화상";case CombatStatusEffect.Slow:return "둔화";case CombatStatusEffect.Paralyze:return "마비";case CombatStatusEffect.Stun:return "기절";case CombatStatusEffect.Poison:return "중독";default:return "";}}
        Image DragonArtwork(Transform parent,DragonData dragon,int stage,float x,float y,float w,float h,bool silhouette)
        {
            var art=Rect("Dragon art",parent,x,y,w,h);var sprite=dragon.SpriteForStage(stage);
            if(sprite==null)
            {
                var graphic=art.gameObject.AddComponent<DragonGraphic>();graphic.color=silhouette?Color.black:dragon.color;graphic.raycastTarget=false;return null;
            }
            var image=art.gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;
            image.color=silhouette?new Color(0,0,0,1):Color.white;
            art.gameObject.AddComponent<DragonPortraitIdle>().Bind(dragon,stage,new Vector2(w,h)*(stage==0?.68f:stage==1?.84f:.98f));
            return image;
        }
        Image ElementBadge(Transform parent,ElementType element,float x,float y,float size)
        {
            var sprite=ElementIconLibrary.Get(element);if(sprite==null)return null;
            var rect=Rect("Element symbol",parent,x,y,size,size);var image=rect.gameObject.AddComponent<Image>();
            image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;return image;
        }
        public void ShowDragonSelect()
        {
            Screen("플레이 드래곤 선택");
            var owned=Session.Profile.dragons.ToArray();
            Label("함께 타워를 오를 드래곤을 선택하세요",0,145,420,34,18,Muted);
            int start=selectPage*8,end=Math.Min(owned.Length,start+8);ChoiceButtons=new Button[end-start];
            for(int i=start;i<end;i++)
            {
                var instance=owned[i];var dragon=Session.Find(instance.speciesId);bool selected=Session.Profile.selectedInstanceId==instance.instanceId;
                int local=i-start,column=local%2,row=local/2;float x=column==0?-106:106,y=215+row*100;
                var b=Button(DragonNames.Display(instance,dragon)+(selected?"\n선택 중":"\n"+dragon.element),x,y,198,86,()=>{try{Session.Select(instance.instanceId);ShowLobby();}catch(Exception){notice.text="선택을 저장하지 못했습니다.";}});
                StyleCard(b,selected?new Color(.30f,.20f,.07f,.98f):new Color(.09f,.16f,.23f,.98f),selected?DragonTowerTheme.Gold:new Color(dragon.color.r,dragon.color.g,dragon.color.b,1));
                var text=b.GetComponentInChildren<Text>();text.fontSize=14;text.rectTransform.anchoredPosition=new Vector2(35,-43);text.rectTransform.sizeDelta=new Vector2(112,76);
                DragonArtwork(b.transform,dragon,0,-57,43,68,68,false);ChoiceButtons[local]=b;
            }
            if(selectPage>0)Button("◀",-105,650,90,46,()=>{selectPage--;ShowDragonSelect();});
            Label((selectPage+1)+" / "+Math.Max(1,(owned.Length+7)/8),0,650,100,42,16,Muted);
            if(end<owned.Length)Button("▶",105,650,90,46,()=>{selectPage++;ShowDragonSelect();});
            Button("선택하지 않고 돌아가기",0,728,380,60,ShowLobby);
            notice.text="도감 정보와 플레이 드래곤 선택은 서로 분리되어 있습니다.";
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
            Screen("드래곤 상태");Portrait(d,269);
            var nameLabel=Label(Session.SelectedName()+"  /  "+d.element,0,440,420,48,27,Gold);nameLabel.resizeTextForBestFit=true;nameLabel.resizeTextMinSize=18;nameLabel.resizeTextMaxSize=27;
            DragonStatsRow(d,477);
            DragonInfoCard("타고난 능력",d.passiveName,d.passiveDescription,3,548,90);
            if(d.skill!=null)DragonInfoCard("시작 스킬",d.skill.displayName,"피해 "+d.skill.damage+"   ·   재사용 "+d.skill.cooldown+"초"+SkillStatus(d.skill),1,631,64);
            Button("이름 변경",0,684,380,44,ShowRenameDragon);
            Button("로비로 돌아가기",0,744,380,48,ShowLobby);
            notice.text="유대·진화·아이템·증강은 타워 도전 중에만 적용됩니다.";
        }
        void Update()
        {
            if(root==null||!root.gameObject.activeInHierarchy)return;
            float dt=Mathf.Min(Time.deltaTime,.1f);clock+=dt;

            if(startPrompt!=null)startPrompt.localScale=Vector3.one*(1+Mathf.Sin(clock*4)*.035f);
            if(hatchTime<0)return;
            hatchTime+=dt;if(egg!=null)egg.rectTransform.localRotation=Quaternion.Euler(0,0,Mathf.Sin(hatchTime*38)*10);
            if(hatchTime>=.85f){hatchTime=-1;ShowHatched();}
        }
    }
}
