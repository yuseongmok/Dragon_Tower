using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DragonTower
{
    public sealed class DragonTowerSettings : MonoBehaviour
    {
        static DragonTowerSettings instance;
        Font font;
        Canvas canvas;
        RectTransform safeAreaRoot;
        GameObject panel,confirmation;
        Slider musicSlider,sfxSlider;
        Text musicValue,sfxValue;
        float previousTimeScale=1;
        bool open;
        Rect lastSafeArea;
        Vector2 lastScreen;
        static Sprite circleSprite;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if(instance!=null)return;
            var go=new GameObject("Dragon Tower Settings");
            DontDestroyOnLoad(go);instance=go.AddComponent<DragonTowerSettings>();
        }

        void Awake()
        {
            if(instance!=null&&instance!=this){Destroy(gameObject);return;}
            instance=this;DontDestroyOnLoad(gameObject);Build();
        }

        RectTransform Rect(string name,Transform parent,Vector2 anchor,Vector2 position,Vector2 size)
        {
            var go=new GameObject(name,typeof(RectTransform));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);
            r.anchorMin=r.anchorMax=anchor;r.pivot=anchor;r.anchoredPosition=position;r.sizeDelta=size;return r;
        }
        Image Box(string name,Transform parent,Vector2 anchor,Vector2 position,Vector2 size,Color color)
        {var r=Rect(name,parent,anchor,position,size);var image=r.gameObject.AddComponent<Image>();image.color=color;return image;}
        Text Label(string value,Transform parent,Vector2 anchor,Vector2 position,Vector2 size,int fontSize,Color color,TextAnchor alignment=TextAnchor.MiddleCenter)
        {
            var r=Rect(value,parent,anchor,position,size);var text=r.gameObject.AddComponent<Text>();text.font=font;text.text=value;text.fontSize=fontSize;text.color=color;text.alignment=alignment;text.raycastTarget=false;return text;
        }
        Button MakeButton(string title,Transform parent,Vector2 anchor,Vector2 position,Vector2 size,Color color,UnityAction action)
        {
            var image=Box(title,parent,anchor,position,size,color);var button=image.gameObject.AddComponent<Button>();button.targetGraphic=image;button.onClick.AddListener(action);
            var nav=button.navigation;nav.mode=Navigation.Mode.None;button.navigation=nav;button.gameObject.AddComponent<DragonTowerUiSound>();
            DragonTowerTheme.StyleButton(button,DragonTowerTheme.Gold);Label(title,button.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(size.x-14,size.y-8),18,Color.white);return button;
        }
        static Sprite CircleSprite()
        {
            if(circleSprite!=null)return circleSprite;
            const int size=64;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);texture.name="Settings circle";texture.filterMode=FilterMode.Bilinear;texture.wrapMode=TextureWrapMode.Clamp;
            var pixels=new Color32[size*size];var center=new Vector2((size-1)*.5f,(size-1)*.5f);float radius=size*.5f-1;
            for(int y=0;y<size;y++)for(int x=0;x<size;x++){float distance=Vector2.Distance(new Vector2(x,y),center);byte alpha=(byte)Mathf.RoundToInt(Mathf.Clamp01(radius-distance)*255);pixels[y*size+x]=new Color32(255,255,255,alpha);}
            texture.SetPixels32(pixels);texture.Apply(false,true);circleSprite=Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f),100);circleSprite.name="Settings circle";return circleSprite;
        }
        Button MakeGearButton(Transform parent)
        {
            // Keep a comfortable mobile touch target while drawing a much smaller,
            // translucent icon so the battle header remains readable.
            var hitArea=Box("설정",parent,new Vector2(0,1),new Vector2(7,-7),new Vector2(48,48),Color.clear);
            var button=hitArea.gameObject.AddComponent<Button>();button.targetGraphic=hitArea;button.onClick.AddListener(Toggle);button.gameObject.AddComponent<DragonTowerUiSound>();
            var outer=Box("Gear rim",hitArea.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(40,40),new Color(1,.70f,.25f,.82f));outer.sprite=CircleSprite();outer.raycastTarget=false;
            var inner=Box("Dark center",outer.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(35,35),new Color(.025f,.04f,.07f,.76f));inner.sprite=CircleSprite();inner.raycastTarget=false;
            for(int i=0;i<8;i++)
            {
                float angle=i*45;float radians=angle*Mathf.Deg2Rad;var tooth=Box("Gear tooth",inner.transform,new Vector2(.5f,.5f),new Vector2(Mathf.Sin(radians)*9,Mathf.Cos(radians)*9),new Vector2(5,14),new Color(1,.75f,.31f,.9f));
                tooth.rectTransform.localEulerAngles=new Vector3(0,0,-angle);tooth.raycastTarget=false;
            }
            var gear=Box("Gear ring",inner.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(23,23),new Color(1,.78f,.34f,.94f));gear.sprite=CircleSprite();gear.raycastTarget=false;
            var hub=Box("Gear hub",gear.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(10,10),new Color(.025f,.04f,.07f,.9f));hub.sprite=CircleSprite();hub.raycastTarget=false;
            return button;
        }
        void Build()
        {
            var battleView=FindFirstObjectByType<BattleView>();font=battleView!=null&&battleView.font!=null?battleView.font:Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=1000;
            var scaler=gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
            gameObject.AddComponent<GraphicRaycaster>();

            safeAreaRoot=Rect("Portrait game area",transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(480,850));
            MakeGearButton(safeAreaRoot);

            panel=Box("Settings dimmer",safeAreaRoot,Vector2.zero,Vector2.zero,Vector2.zero,new Color(.01f,.015f,.025f,.86f)).gameObject;
            var dim=panel.GetComponent<RectTransform>();dim.anchorMin=Vector2.zero;dim.anchorMax=Vector2.one;dim.offsetMin=dim.offsetMax=Vector2.zero;
            var card=Box("Settings card",panel.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(420,650),new Color(.045f,.065f,.10f,.99f));DragonTowerTheme.Frame(card,DragonTowerTheme.Gold);
            Label("설정",card.transform,new Vector2(.5f,1),new Vector2(0,-40),new Vector2(350,52),30,DragonTowerTheme.Gold);
            musicSlider=MakeSlider("배경음",card.transform,-105,out musicValue);sfxSlider=MakeSlider("효과음",card.transform,-200,out sfxValue);
            MakeButton("로비로 돌아가기",card.transform,new Vector2(.5f,1),new Vector2(0,-330),new Vector2(340,60),new Color(.13f,.25f,.31f,.98f),ReturnLobby);
            MakeButton("게임 데이터 초기화",card.transform,new Vector2(.5f,1),new Vector2(0,-415),new Vector2(340,58),new Color(.42f,.15f,.14f,.98f),ShowResetConfirmation);
            MakeButton("계속하기",card.transform,new Vector2(.5f,1),new Vector2(0,-500),new Vector2(340,60),new Color(.20f,.38f,.28f,.98f),Close);
            Label("설정을 닫으면 게임이 다시 시작됩니다.",card.transform,new Vector2(.5f,1),new Vector2(0,-570),new Vector2(350,32),14,new Color(.66f,.72f,.78f));

            confirmation=Box("Reset confirmation",panel.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(380,300),new Color(.075f,.065f,.07f,1)).gameObject;
            DragonTowerTheme.Frame(confirmation.GetComponent<Image>(),new Color(1,.38f,.28f));
            Label("정말 초기화할까요?",confirmation.transform,new Vector2(.5f,1),new Vector2(0,-52),new Vector2(330,48),25,new Color(1,.72f,.48f));
            Label("보유 드래곤과 도감 기록이 삭제됩니다.\n삭제한 데이터는 복구할 수 없습니다.",confirmation.transform,new Vector2(.5f,1),new Vector2(0,-126),new Vector2(330,74),16,Color.white);
            MakeButton("취소",confirmation.transform,new Vector2(.5f,1),new Vector2(-88,-232),new Vector2(150,52),DragonTowerTheme.SlateLight,()=>confirmation.SetActive(false));
            MakeButton("초기화",confirmation.transform,new Vector2(.5f,1),new Vector2(88,-232),new Vector2(150,52),new Color(.52f,.15f,.13f),ResetGame);
            confirmation.SetActive(false);panel.SetActive(false);
            ApplySafeArea();
        }
        void Update(){if(Screen.safeArea!=lastSafeArea||lastScreen.x!=Screen.width||lastScreen.y!=Screen.height)ApplySafeArea();}
        void ApplySafeArea()
        {
            if(safeAreaRoot==null||Screen.width<=0||Screen.height<=0)return;lastSafeArea=Screen.safeArea;lastScreen=new Vector2(Screen.width,Screen.height);
            float scale=Mathf.Min(lastSafeArea.width/480f,lastSafeArea.height/850f);
            safeAreaRoot.localScale=Vector3.one*scale;
            safeAreaRoot.anchoredPosition=lastSafeArea.center-new Vector2(Screen.width,Screen.height)*.5f;
        }
        Slider MakeSlider(string title,Transform parent,float y,out Text valueLabel)
        {
            Label(title,parent,new Vector2(.5f,1),new Vector2(-125,y),new Vector2(130,38),19,Color.white,TextAnchor.MiddleLeft);
            valueLabel=Label("100%",parent,new Vector2(.5f,1),new Vector2(140,y),new Vector2(70,38),17,new Color(.72f,.81f,.9f));
            var root=Rect(title+" slider",parent,new Vector2(.5f,1),new Vector2(5,y-43),new Vector2(300,28));
            var background=Box("Background",root,Vector2.zero,Vector2.zero,Vector2.zero,new Color(.12f,.15f,.19f,1));var br=background.rectTransform;br.anchorMax=Vector2.one;br.offsetMin=br.offsetMax=Vector2.zero;
            var fillArea=Rect("Fill Area",root,Vector2.zero,new Vector2(8,0),new Vector2(-16,0));fillArea.anchorMax=Vector2.one;
            var fill=Box("Fill",fillArea,Vector2.zero,Vector2.zero,Vector2.zero,DragonTowerTheme.Gold);var fr=fill.rectTransform;fr.anchorMax=Vector2.one;fr.offsetMin=fr.offsetMax=Vector2.zero;
            var handleArea=Rect("Handle Slide Area",root,Vector2.zero,new Vector2(10,0),new Vector2(-20,0));handleArea.anchorMax=Vector2.one;
            var handle=Box("Handle",handleArea,new Vector2(.5f,.5f),Vector2.zero,new Vector2(28,40),new Color(1,.84f,.52f,1));
            var slider=root.gameObject.AddComponent<Slider>();slider.fillRect=fr;slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.direction=Slider.Direction.LeftToRight;slider.minValue=0;slider.maxValue=1;
            return slider;
        }
        void Toggle(){if(open)Close();else Open();}
        void Open()
        {
            open=true;previousTimeScale=Time.timeScale;Time.timeScale=0;DragonTowerAudio.SetPaused(true);
            DragonTowerAudio.GetVolumes(out var music,out var effects);musicSlider.SetValueWithoutNotify(music);sfxSlider.SetValueWithoutNotify(effects);UpdateValues();
            musicSlider.onValueChanged.RemoveAllListeners();sfxSlider.onValueChanged.RemoveAllListeners();musicSlider.onValueChanged.AddListener(SetMusic);sfxSlider.onValueChanged.AddListener(SetEffects);
            confirmation.SetActive(false);panel.SetActive(true);panel.transform.SetAsLastSibling();
        }
        void Close()
        {
            if(!open)return;open=false;confirmation.SetActive(false);panel.SetActive(false);Time.timeScale=previousTimeScale<=0?1:previousTimeScale;DragonTowerAudio.SetPaused(false);
        }
        void SetMusic(float value){DragonTowerAudio.GetVolumes(out _,out var effects);DragonTowerAudio.SetVolumes(value,effects);UpdateValues();}
        void SetEffects(float value){DragonTowerAudio.GetVolumes(out var music,out _);DragonTowerAudio.SetVolumes(music,value);UpdateValues();}
        void UpdateValues(){DragonTowerAudio.GetVolumes(out var music,out var effects);musicValue.text=Mathf.RoundToInt(music*100)+"%";sfxValue.text=Mathf.RoundToInt(effects*100)+"%";}
        void ReturnLobby()
        {
            Close();var controller=FindFirstObjectByType<BattleController>();if(controller!=null&&controller.Flow!=null)controller.Flow.ShowLobby();
        }
        void ShowResetConfirmation(){confirmation.SetActive(true);confirmation.transform.SetAsLastSibling();}
        void ResetGame()
        {
            PlayerPrefs.DeleteKey("DragonTower.Profile.v1.A");PlayerPrefs.DeleteKey("DragonTower.Profile.v1.B");PlayerPrefs.Save();
            open=false;Time.timeScale=1;DragonTowerAudio.SetPaused(false);panel.SetActive(false);SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        void OnDestroy(){if(instance==this&&open){Time.timeScale=previousTimeScale<=0?1:previousTimeScale;DragonTowerAudio.SetPaused(false);}}
    }
}
