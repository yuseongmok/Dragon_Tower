using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DragonTower
{
    public sealed partial class CollectionFlow
    {
        Transform regionEntranceOwner;
        bool TryShowRegionEntrance()
        {
            if(towerRun==null||towerRun.Phase!=FloorPhase.Choosing)return false;
            var skin=Resources.Load<BattleSkin>("PixelBattleSkin");
            var region=skin!=null?(towerRun.Floor==1?DungeonEntrance():skin.EntranceForFloor(towerRun.Floor)):null;
            if(region==null)return false;
            if(Session.HasDiscoveredRegion(region.regionId)&&!region.replayDiscovered)return false;
            if(Session.HasPresentedRegion(region.regionId,towerRun.Progress.runId))return false;
            if(regionEntranceOwner!=null&&regionEntranceOwner==body)return true;
            StartCoroutine(ShowRegionEntrance(region,skin));return true;
        }
        static EnvironmentBackgroundSet DungeonEntrance()=>new EnvironmentBackgroundSet{
            firstFloor=1,lastFloor=10,regionId="dungeon-entry",regionNumber=1,regionName="지하 감옥",
            entranceMessage="지하 감옥의 문이 열립니다",entranceSeconds=2.6f,replayDiscovered=true,
            entranceTint=new Color(.78f,.57f,.29f,1),particleTint=new Color(.91f,.73f,.43f,1),particleCount=0};
        IEnumerator ShowRegionEntrance(EnvironmentBackgroundSet region,BattleSkin skin)
        {
            // The next floor was already committed by AdvanceFloor. This presentation never awards or advances.
            bool dungeon=region.regionId=="dungeon-entry";
            bool canSkip=region.replayDiscovered&&Session.HasDiscoveredRegion(region.regionId),skip=false;
            Screen("구역 진입");var owner=body;regionEntranceOwner=owner;notice.text=string.IsNullOrEmpty(region.entranceMessage)?"새로운 물길이 열립니다":region.entranceMessage;
            if(canSkip)notice.text+=" · 터치하여 건너뛰기";
            var scenery=RoomScenery("UI/boss-stairway",EnvironmentScreen.BossEntrance);
            var world=Rect("Region entrance world",owner,0,425,480,850);world.SetSiblingIndex(scenery.transform.GetSiblingIndex());
            scenery.transform.SetParent(world,false);
            var doorRoot=Rect("Region gate",world,0,235,300,350);
            var door=doorRoot.gameObject.AddComponent<TowerDoorVisual>();
            door.Build(DoorSprite(true,towerRun.Floor),RewardIcon(14),region.entranceTint,true,TowerRoomKind.Boss,dungeon?1:0,0);
            RectTransform gear=null;
            if(region.forgeEffects){gear=Rect("Region rotating gear",doorRoot,0,150,64,64);var graphic=gear.gameObject.AddComponent<RegionGearGraphic>();graphic.raycastTarget=false;}
            var water=Rect("Region water particles",owner,0,425,480,850).gameObject.AddComponent<RegionWaterParticles>();
            water.raycastTarget=false;water.tint=region.particleTint;water.count=region.particleCount;water.embers=region.forgeEffects;water.wind=region.skyEffects;water.thunder=region.thunderEffects;water.ice=region.iceEffects;water.library=region.libraryEffects;water.sunlight=region.sunlightEffects;water.voidRifts=region.voidEffects;water.throne=region.throneEffects;
            var veil=Rect("Region teal light",owner,0,425,480,850).gameObject.AddComponent<Image>();veil.raycastTarget=false;
            var title=Panel("Region arrival title",owner,0,585,430,112,new Color(.025f,.07f,.09f,.90f),region.entranceTint);
            var group=title.gameObject.AddComponent<CanvasGroup>();group.alpha=0;
            bool twoLines=region.forgeEffects||region.skyEffects||region.thunderEffects||region.iceEffects||region.libraryEffects||region.sunlightEffects||region.voidEffects||region.throneEffects;
            CardText(title.transform,"Region number","제"+region.regionNumber+"구역"+(twoLines?"\n":" — ")+region.regionName,0,39,410,twoLines?65:45,23,AncientUi.Ivory,TextAnchor.MiddleCenter);
            CardText(title.transform,"Region floor",region.firstFloor+"층 도착",0,82,400,30,18,region.particleTint,TextAnchor.MiddleCenter);
            if(canSkip){var target=Rect("Region skip",owner,0,425,480,850);var hit=target.gameObject.AddComponent<Image>();hit.color=Color.clear;var button=target.gameObject.AddComponent<Button>();button.targetGraphic=hit;button.onClick.AddListener(()=>skip=true);}
            float elapsed=0,duration=Mathf.Clamp(region.entranceSeconds,2,8);var origin=world.anchoredPosition;bool revealed=false;
            while(elapsed<duration&&!skip&&body==owner&&!returningToLobby)
            {
                elapsed+=Time.unscaledDeltaTime;float t=elapsed/duration;
                door.SetOpening(Mathf.SmoothStep(0,1,Mathf.InverseLerp(.10f,.40f,t)));
                if(gear!=null)gear.localRotation=Quaternion.Euler(0,0,-120*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.10f,.40f,t)));
                float rise=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.32f,.65f,t));
                world.localScale=Vector3.one*(1+rise*.40f);world.anchoredPosition=origin+new Vector2(0,-rise*100);
                float flash=Mathf.Clamp01(1-Mathf.Abs(t-.64f)/.15f);
                // Thunder darkens gently. Its cyan light stays inside the existing gate, never a screen flash.
                if(dungeon){veil.color=new Color(.015f,.02f,.025f,.6f*(1-Mathf.SmoothStep(0,1,t*3)));}
                else if(region.throneEffects)
                {
                    veil.color=new Color(.18f,.13f,.27f,.18f*(1-Mathf.SmoothStep(0,1,t*3)));
                }
                else if(region.voidEffects)
                {
                    // Fade the old warm light; no full-screen flash or distortion post-process.
                    float warm=1-Mathf.SmoothStep(0,1,t*3);
                    veil.color=new Color(.45f,.36f,.22f,.12f*warm);
                }
                else if(region.sunlightEffects)
                {
                    // The old archive tint fades; warm light stays local to the existing gate.
                    veil.color=new Color(.25f,.21f,.31f,.24f*(1-Mathf.SmoothStep(0,1,t*2.5f)));
                }
                else if(region.libraryEffects)
                {
                    veil.color=new Color(.15f,.23f,.35f,.16f*(1-Mathf.SmoothStep(0,1,t*3)));
                }
                else if(region.iceEffects)
                {
                    // Cold tint fades gently; the gate supplies local light without a full-screen flash.
                    veil.color=new Color(.15f,.23f,.35f,.12f*(1-Mathf.SmoothStep(0,1,t*2)));
                }
                else if(region.thunderEffects)
                {
                    float dark=Mathf.SmoothStep(0,.22f,Mathf.InverseLerp(0,.25f,t))*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.45f,.70f,t)));
                    veil.color=new Color(.08f,.11f,.19f,dark);
                }
                else veil.color=new Color(region.entranceTint.r,region.entranceTint.g,region.entranceTint.b,flash*(region.skyEffects?.45f:.85f));
                water.age=elapsed;water.intensity=Mathf.Clamp01((t-.15f)*5)*(1-Mathf.Clamp01((t-.8f)*5));
                if(!revealed&&t>=.65f){revealed=true;doorRoot.gameObject.SetActive(false);scenery.sprite=skin.BackgroundForScreen(towerRun.Floor,EnvironmentScreen.Battle);world.localScale=Vector3.one;world.anchoredPosition=origin;notice.text="";}
                if(revealed){world.localScale=Vector3.one;world.anchoredPosition=origin;group.alpha=Mathf.Clamp01((t-.67f)*10);}
                yield return null;
            }
            if(owner!=body||returningToLobby)yield break;
            // Mark only after the complete reveal. Interrupted presentations replay on resume, with no rewards repeated.
            Session.DiscoverRegion(region.regionId,towerRun.Progress.runId);regionEntranceOwner=null;ShowTowerChoices();
        }
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class RegionWaterParticles:MaskableGraphic
    {
        public float age,intensity;public int count=28;public Color tint=Color.cyan;public bool embers,wind,thunder,ice,library,sunlight,voidRifts,throne;
        void Update(){SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper v)
        {
            v.Clear();var r=rectTransform.rect;
            if(throne)
            {
                for(int i=0;i<Mathf.Clamp(count,0,10);i++)
                {
                    float side=i%2==0?-1:1,x=side*(r.width*.25f+(i%3)*10),y=40+i/2*35;
                    if(i<4){var c=new Color(.40f,.33f,.61f,intensity*Mathf.Clamp01(1-age*1.3f)*.4f);Quad(v,x,y,2,17,c);Quad(v,x-3,y+15,5,2,c);}
                    else {var c=i%2==0?new Color(.56f,.73f,.91f,intensity*.35f):new Color(.78f,.66f,.42f,intensity*.35f);Quad(v,x,y+Mathf.Round(age*7/2)*2,2,4,c);}
                }
                return;
            }
            if(voidRifts)
            {
                // Transition-only stepped fissures and floating chips in one bounded mesh.
                // Keep the centre clear; never distort the camera, floor, or combat UI.
                for(int i=0;i<Mathf.Clamp(count,0,10);i++)
                {
                    float side=i%2==0?-1:1,x=side*(r.width*.25f+(i%3)*12);
                    float y=45+(i/2)*34+Mathf.Round(Mathf.Sin(age+i)*2)*2;
                    var c=new Color(tint.r,tint.g,tint.b,intensity*.38f);
                    if(i<6){Quad(v,x,y,2,12,c);Quad(v,x-4,y+10,6,2,c);Quad(v,x-4,y+12,2,8,c);}
                    else Quad(v,x,y,5,4,new Color(.33f,.32f,.42f,intensity*.65f));
                }
                return;
            }
            if(sunlight)
            {
                // Twelve small transition-only motes in one mesh; no full-screen flash or combat emitter.
                for(int i=0;i<Mathf.Clamp(count,0,12);i++)
                {
                    float side=i%2==0?-1:1;
                    float x=side*(r.width*.23f+(i%3)*13)+Mathf.Sin(age+i)*3;
                    float y=35+Mathf.Repeat(i*23+age*18,190);
                    x=Mathf.Round(x/2)*2;y=Mathf.Round(y/2)*2;
                    var c=new Color(tint.r,tint.g,tint.b,intensity*(.25f+.15f*Mathf.Sin(age+i)));
                    Quad(v,x,y,2,4,c);
                    if(i<4)Quad(v,x-1,y+1,4,2,c);
                }
                return;
            }
            if(library)
            {
                // Transition-only bounded mesh. Candles ignite in sequence; small glyphs stay at the edges.
                for(int i=0;i<Mathf.Clamp(count,0,10);i++)
                {
                    float side=i%2==0?-1:1,x=side*r.width*.32f;
                    if(i<4)
                    {
                        float lit=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.12f+i*.12f,.32f+i*.12f,age));
                        float y=30+(i/2)*75;var c=new Color(.90f,.64f,.37f,lit*intensity*.8f);
                        Quad(v,x,y,4,10,c);Quad(v,x+1,y+10,2,5,c);
                    }
                    else if(i<8)
                    {
                        float y=90+(i-4)/2*58;var c=new Color(.60f,.44f,.69f,intensity*.4f);
                        Quad(v,x-4,y,10,2,c);Quad(v,x,y-4,2,10,c);
                        Quad(v,x-3,y-3,2,2,c);Quad(v,x+3,y+3,2,2,c);
                    }
                    else
                    {
                        float y=180+Mathf.Round(Mathf.Sin(age*1.5f+i)*3)*2;
                        var c=new Color(.34f,.25f,.23f,intensity*.7f);
                        Quad(v,x-6,y,12,8,c);Quad(v,x-4,y+2,8,4,new Color(.72f,.60f,.40f,intensity*.5f));
                    }
                }
                return;
            }
            if(ice)
            {
                // One transition-only mesh: eight snow specks, six frost clusters and two fading sparks.
                for(int i=0;i<Mathf.Clamp(count,0,16);i++)
                {
                    float side=i%2==0?-1:1;
                    var c=new Color(tint.r,tint.g,tint.b,intensity*.42f);
                    if(i<8)
                    {
                        float x=side*(r.width*.30f+i%4*12)+Mathf.Sin(age+i)*4;
                        float y=r.height*.30f-Mathf.Repeat(i*29+age*22,220);
                        Quad(v,Mathf.Round(x/2)*2,Mathf.Round(y/2)*2,2,2,c);
                    }
                    else if(i<14)
                    {
                        float grow=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.2f,.9f,age));
                        float x=side*r.width*.29f,y=-10+(i-8)/2*46;
                        Quad(v,x,y,2,16*grow,c);Quad(v,x-4,y+4,10*grow,2,c);
                        Quad(v,x-2,y+10,6*grow,2,c);
                    }
                    else
                    {
                        c.a=Mathf.Clamp01(1-age/.65f)*.38f;
                        for(int j=0;j<4;j++)Quad(v,side*r.width*.38f+j%2*3,r.height*.23f-j*4,5,2,c);
                    }
                }
                return;
            }
            if(thunder)
            {
                // Transition-only bounded mesh: four clouds, eight motes, two short edge arcs. No emitters.
                for(int i=0;i<Mathf.Clamp(count,0,14);i++)
                {
                    float side=i%2==0?-1:1;
                    if(i<4)
                    {
                        float x=Mathf.Round((side*r.width*.32f+Mathf.Sin(age*.35f+i)*12)/2)*2;
                        float y=r.height*(.24f+i*.035f);var c=new Color(.17f,.21f,.32f,intensity*.32f);
                        Quad(v,x,y,44+i*6,10,c);Quad(v,x+8,y+10,24+i*4,6,c);
                    }
                    else if(i<12)
                    {
                        float x=side*(r.width*.34f+i%3*6),y=r.height*.08f+Mathf.Repeat(i*29+age*18,160);
                        Quad(v,Mathf.Round(x/2)*2,Mathf.Round(y/2)*2,2,4,new Color(tint.r,tint.g,tint.b,intensity*.30f));
                    }
                    else
                    {
                        float pulse=Mathf.Clamp01(1-Mathf.Abs(age-(i==12?.85f:1.35f))/.22f);
                        float x=side*r.width*.39f,y=r.height*.22f;var c=new Color(tint.r,tint.g,tint.b,intensity*pulse*.55f);
                        for(int step=0;step<5;step++)Quad(v,x+(step%2)*4,y-step*4,6,3,c);
                    }
                }
                return;
            }
            for(int i=0;i<Mathf.Clamp(count,0,64);i++)
            {
                float x=Mathf.Sin(i*17.13f)*r.width*.44f,y=Mathf.Repeat(i*37+age*(22+i%7),r.height)-r.height*.5f;
                float w=2+i%3,h=embers?w:4+i%4;Color c=tint;c.a=intensity*(.20f+.35f*Mathf.Abs(Mathf.Sin(i+age)));
                if(embers&&i<4){w=16+i*3;h=5+i;x=Mathf.Sin(i*17.13f+age*.2f)*r.width*.35f;c=new Color(.65f,.64f,.61f,intensity*.10f);}
                if(wind)
                {
                    // One small UI mesh, no emitters or per-frame allocations. Used only during the transition.
                    x=Mathf.Repeat(i*73+age*(35+i%5*4),r.width+60)-r.width*.5f-30;
                    y=Mathf.Sin(i*7.13f)*r.height*.36f+Mathf.Sin(age+i)*3;
                    w=i<4?36+i*6:14+i%5*4;h=i<4?10+i*2:2;
                    x=Mathf.Round(x/2)*2;y=Mathf.Round(y/2)*2;
                    c.a=intensity*(i<4?.12f:.30f);
                }
                int n=v.currentVertCount;
                v.AddVert(new Vector3(x,y),c,Vector2.zero);v.AddVert(new Vector3(x,y+h),c,Vector2.zero);
                v.AddVert(new Vector3(x+w,y+h),c,Vector2.zero);v.AddVert(new Vector3(x+w,y),c,Vector2.zero);
                v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);
                if(wind&&i<4)
                {
                    // Stepped cloud top keeps the mist in the same pixel vocabulary as the environment.
                    n=v.currentVertCount;
                    v.AddVert(new Vector3(x+w*.2f,y+h),c,Vector2.zero);v.AddVert(new Vector3(x+w*.2f,y+h+6),c,Vector2.zero);
                    v.AddVert(new Vector3(x+w*.7f,y+h+6),c,Vector2.zero);v.AddVert(new Vector3(x+w*.7f,y+h),c,Vector2.zero);
                    v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);
                }
            }
        }
        static void Quad(VertexHelper v,float x,float y,float w,float h,Color c)
        {
            int n=v.currentVertCount;
            v.AddVert(new Vector3(x,y),c,Vector2.zero);v.AddVert(new Vector3(x,y+h),c,Vector2.zero);
            v.AddVert(new Vector3(x+w,y+h),c,Vector2.zero);v.AddVert(new Vector3(x+w,y),c,Vector2.zero);
            v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);
        }
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class RegionGearGraphic:MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper v)
        {
            v.Clear();float radius=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.5f;
            for(int i=0;i<40;i++)
            {
                float a=i*Mathf.PI/20,b=(i+1)*Mathf.PI/20;
                float outer=radius*(i%5<3?1:.79f);int n=v.currentVertCount;Color c=new Color(.65f,.40f,.20f);
                v.AddVert(new Vector3(Mathf.Cos(a),Mathf.Sin(a))*outer,c,Vector2.zero);
                v.AddVert(new Vector3(Mathf.Cos(b),Mathf.Sin(b))*outer,c,Vector2.zero);
                v.AddVert(new Vector3(Mathf.Cos(b),Mathf.Sin(b))*radius*.48f,c,Vector2.zero);
                v.AddVert(new Vector3(Mathf.Cos(a),Mathf.Sin(a))*radius*.48f,c,Vector2.zero);
                v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);
            }
        }
    }
}
