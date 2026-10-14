using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    // Presentation only. All health, timing, inventory and readiness values come from the existing model.
    public partial class BattleView
    {
        Text regionSubtitle,carriedGold,enemyFloor,bondBalance;
        Image nameElement,timingNeedle;
        BattleController hudController;
        int displayedFloor=-1,displayedGold=-1;
        string displayedDragonName;
        string casualHeader;
        static readonly string[] HudRegions={"지하 감옥","고대 수로","용광로 공방","천공 회랑","뇌운의 피뢰탑","수정 궁전","대도서관","일광 성소","공허 회랑","태초의 용좌"};
        void EnsureCasualHud()
        {
            if(regionSubtitle!=null)return;
            hudController=FindFirstObjectByType<BattleController>();
            regionSubtitle=Label("",frame,0,55,310,18,11,new Color(.82f,.87f,.91f));
            regionSubtitle.name="Battle region subtitle";
            carriedGold=Label("",frame,174,33,87,26,17,Color.white,TextAnchor.MiddleRight);carriedGold.name="Battle carried gold";
            carriedGold.resizeTextForBestFit=true;carriedGold.resizeTextMinSize=11;carriedGold.resizeTextMaxSize=17;
            var coin=Rect("Battle gold symbol",frame,117,33,22,22).gameObject.AddComponent<CasualGoldSymbol>();coin.raycastTarget=false;
            BondPointSymbol.Add(frame,new Vector2(117,-56),18).transform.SetSiblingIndex(resultPanel.transform.GetSiblingIndex());
            bondBalance=Label("",frame,174,56,87,20,13,new Color(.68f,.96f,.87f),TextAnchor.MiddleRight);bondBalance.name="Permanent bond points";bondBalance.resizeTextForBestFit=true;bondBalance.resizeTextMinSize=10;bondBalance.resizeTextMaxSize=13;
            bondBalance.transform.SetSiblingIndex(resultPanel.transform.GetSiblingIndex());
            enemyFloor=Label("",frame,167,74,65,17,12,Color.white,TextAnchor.MiddleRight);enemyFloor.name="Enemy floor level";
            nameElement=ElementIcon(frame,30,684,26);nameElement.name="Dragon name element";
            timingNeedle=Panel("Attack timing needle",windupFill.transform.parent,0,0,2,20,new Color(1,.88f,.85f));
            timingNeedle.rectTransform.anchorMin=timingNeedle.rectTransform.anchorMax=new Vector2(0,.5f);
            timingNeedle.gameObject.AddComponent<Outline>().effectColor=new Color(.92f,.13f,.28f,.5f);
            foreach(var fill in new[]{enemyFill,playerFill,windupFill})
            {
                var back=fill.transform.parent.GetComponent<Image>();back.color=new Color(.025f,.055f,.085f,.88f);
                var trimRect=Rect("Fine HP outline",back.transform,0,5,400,10);trimRect.gameObject.AddComponent<CanvasRenderer>();
                var trim=trimRect.gameObject.AddComponent<PixelHudFrame>();
                trim.minimal=true;trim.outlineOnly=true;trim.edge=new Color(.22f,.37f,.45f);trim.raycastTarget=false;trim.transform.SetAsFirstSibling();
            }
            // Keep the existing 400-unit fill width and original gauge center; change only its rendering.
            windupFill.enabled=false;
            var gauge=new GameObject("Timing color strip",typeof(RectTransform),typeof(CanvasRenderer),typeof(CasualTimingStrip));
            var gaugeRect=gauge.GetComponent<RectTransform>();gaugeRect.SetParent(windupFill.transform,false);
            gaugeRect.anchorMin=Vector2.zero;gaugeRect.anchorMax=Vector2.one;gaugeRect.offsetMin=gaugeRect.offsetMax=Vector2.zero;
            gauge.GetComponent<CasualTimingStrip>().raycastTarget=false;
            screenRim.gameObject.SetActive(false);
            foreach(var jewel in slotJewels)jewel.gameObject.SetActive(false);
            foreach(var label in new[]{regionSubtitle,carriedGold,enemyFloor})
            {var outline=label.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.015f,.025f,.045f,.95f);outline.effectDistance=new Vector2(1,-1);label.transform.SetSiblingIndex(resultPanel.transform.GetSiblingIndex());}
            coin.transform.SetSiblingIndex(resultPanel.transform.GetSiblingIndex());nameElement.transform.SetSiblingIndex(resultPanel.transform.GetSiblingIndex());
        }
        void UpdateCasualHud(BattleModel battle)
        {
            EnsureCasualHud();
            int floor=Mathf.Max(1,battle.Enemy.floor);
            if(displayedFloor!=floor)
            {
                displayedFloor=floor;int region=Mathf.Clamp((floor-1)/10,0,HudRegions.Length-1);
                casualHeader=floor+"F · "+HudRegions[region];enemyFloor.text=floor+"층";
                regionSubtitle.text="";
                var battleSkin=Resources.Load<BattleSkin>("PixelBattleSkin");
                if(battleSkin!=null&&battleSkin.environmentBackgrounds!=null)foreach(var area in battleSkin.environmentBackgrounds)
                    if(area!=null&&floor>=area.firstFloor&&floor<=area.lastFloor){regionSubtitle.text=area.regionName;break;}
            }
            if(modeLabel.text!=casualHeader)modeLabel.text=casualHeader;
            modeLabel.rectTransform.anchoredPosition=new Vector2(-25,-32);modeLabel.rectTransform.sizeDelta=new Vector2(238,28);modeLabel.fontSize=18;
            modeLabel.resizeTextForBestFit=true;modeLabel.resizeTextMinSize=13;modeLabel.resizeTextMaxSize=18;modeLabel.color=Color.white;
            int gold=hudController?.Flow?.CurrentRun?.Gold??0;
            string points=(hudController?.Flow?.Session?.Profile.bondPoints??0).ToString("N0");if(bondBalance.text!=points)bondBalance.text=points;
            if(displayedGold!=gold){displayedGold=gold;carriedGold.text=gold.ToString("N0");}
            enemyName.text=battle.Enemy.displayName;
            enemyHP.color=playerHP.color=Color.white;
            playerName.text=battle.Dragon.displayName;
            playerName.rectTransform.anchoredPosition=new Vector2(-35,-684);playerName.rectTransform.sizeDelta=new Vector2(156,28);
            playerName.fontSize=16;playerName.resizeTextMinSize=11;playerName.resizeTextMaxSize=16;
            if(displayedDragonName!=playerName.text)
            {displayedDragonName=playerName.text;nameElement.rectTransform.anchoredPosition=new Vector2(Mathf.Min(58,-113+playerName.preferredWidth+18),-684);}
            nameElement.sprite=ElementIconLibrary.Get(battle.Dragon.elementType);nameElement.gameObject.SetActive(nameElement.sprite!=null);
            // The cursor reads EnemyWindupProgress; no attack/dodge thresholds are modified.
            timingNeedle.rectTransform.anchoredPosition=new Vector2(400*battle.EnemyWindupProgress,0);
            warning.rectTransform.sizeDelta=new Vector2(440,42);warning.fontSize=16;
            skillGlow.effectDistance=new Vector2(1,-1);
        }
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class CasualGoldSymbol:MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();float unit=rectTransform.rect.width/24f;
            for(int y=-11;y<=11;y++)for(int x=-11;x<=11;x++)
            {
                float d=x*x+y*y;if(d>121)continue;
                Color c=d>100?new Color(.3f,.19f,.09f):d>72?new Color(.9f,.61f,.21f):new Color(1,.8f,.3f);
                if(d>81&&y>0)c=new Color(1,.92f,.59f);
                if((x==-2||x==-1)&&Mathf.Abs(y)<6)c=new Color(1,.96f,.75f);
                if((y==5||y==-5)&&x>=-2&&x<=3)c=new Color(1,.96f,.75f);
                int n=vh.currentVertCount;vh.AddVert(new Vector3(x*unit,y*unit),c,Vector2.zero);vh.AddVert(new Vector3(x*unit,(y+1)*unit),c,Vector2.zero);vh.AddVert(new Vector3((x+1)*unit,(y+1)*unit),c,Vector2.zero);vh.AddVert(new Vector3((x+1)*unit,y*unit),c,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
            }
        }
    }
    public sealed class CasualTimingStrip:MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;
            for(float x=0;x<r.width;x+=8)
            {
                float t=x/400f;Color c=t<.4f?Color.Lerp(new Color(1,.83f,.34f),new Color(1,.39f,.23f),t/.4f):Color.Lerp(new Color(1,.39f,.23f),new Color(.91f,.12f,.29f),(t-.4f)/.6f);
                int n=vh.currentVertCount;float l=r.xMin+x,rr=l+Mathf.Min(8,r.width-x);
                vh.AddVert(new Vector3(l,r.yMin),c,Vector2.zero);vh.AddVert(new Vector3(l,r.yMax),c,Vector2.zero);vh.AddVert(new Vector3(rr,r.yMax),c,Vector2.zero);vh.AddVert(new Vector3(rr,r.yMin),c,Vector2.zero);
                vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
            }
        }
    }
}
