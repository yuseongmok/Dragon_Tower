using UnityEngine;
using UnityEngine.UI;
namespace DragonTower {
 // One reusable, bounded mesh. Reads model time; never schedules or deals damage.
 public sealed class BossSkillVfx:MonoBehaviour {
  BattleView view;LightningMeshLayer mesh;Text label;
  KrakenBossSkillVfx kraken;
  VulcanBossSkillVfx vulcan; GarudaBossSkillVfx garuda;
  EarthVfxStyle earth;readonly Image[] rocks=new Image[40];int used;
  readonly Color dark=new Color(.18f,.14f,.12f),stone=new Color(.51f,.44f,.34f),edge=new Color(.85f,.73f,.52f);
  public int QuadCount=>(mesh==null?0:mesh.Count)+(kraken==null?0:kraken.QuadCount)+(vulcan==null?0:vulcan.QuadCount)+(garuda==null?0:garuda.QuadCount);
  public int DroppedQuads=>(kraken==null?0:kraken.DroppedQuads)+(vulcan==null?0:vulcan.DroppedQuads)+(garuda==null?0:garuda.DroppedQuads);
  public int PeakQuads{get;private set;}
  public Vector2 WavePosition{get;private set;}
  public void Initialize(BattleView owner){
   if(mesh!=null)return;view=owner;
   var go=new GameObject("Boss skill warning and wave (pooled)",typeof(RectTransform),typeof(CanvasRenderer),typeof(LightningMeshLayer));
   var r=go.GetComponent<RectTransform>();r.SetParent(view.frame,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,1);r.sizeDelta=new Vector2(480,850);
   mesh=go.GetComponent<LightningMeshLayer>();mesh.raycastTarget=false;
   label=new GameObject("Boss skill warning",typeof(RectTransform),typeof(Text)).GetComponent<Text>();
   label.rectTransform.SetParent(r,false);label.rectTransform.anchorMin=label.rectTransform.anchorMax=label.rectTransform.pivot=new Vector2(.5f,1);
   label.rectTransform.anchoredPosition=new Vector2(0,-408);label.rectTransform.sizeDelta=new Vector2(400,26);
   label.font=view.font;label.fontSize=16;label.alignment=TextAnchor.MiddleCenter;label.color=edge;label.raycastTarget=false;
   earth=Resources.Load<EarthSkillLibrary>("EarthSkills/Library")?.style;
   for(int i=0;i<rocks.Length;i++){var im=new GameObject("Pooled earth fragment "+i,typeof(RectTransform),typeof(Image)).GetComponent<Image>();im.rectTransform.SetParent(r,false);im.rectTransform.anchorMin=im.rectTransform.anchorMax=new Vector2(.5f,1);im.raycastTarget=false;im.enabled=false;rocks[i]=im;}
   kraken=new KrakenBossSkillVfx();kraken.Initialize(view);
   vulcan=new VulcanBossSkillVfx();vulcan.Initialize(view); garuda=new GarudaBossSkillVfx();garuda.Initialize(view);
   label.transform.SetAsLastSibling();
   BattleVfxHudVisibility.BringHudForward(view);Clear();
  }
  public void Clear(){if(mesh!=null){mesh.Begin();mesh.End();}kraken?.Clear();vulcan?.Clear();garuda?.Clear();if(label!=null)label.enabled=false;foreach(var im in rocks)if(im!=null)im.enabled=false;used=0;}
  void OnDisable()=>Clear();
  void OnDestroy(){if(mesh!=null)Destroy(mesh.gameObject);kraken?.Destroy();vulcan?.Destroy();garuda?.Destroy();}
  void Block(Vector2 p,float w,float h,Color c){mesh.Quad(p+new Vector2(-w,-h),p+new Vector2(-w,h),p+new Vector2(w,h),p+new Vector2(w,-h),c);}
  void Sprite(EarthModule module,Vector2 p,Vector2 size,float phase=0,float alpha=1){if(earth==null||used>=rocks.Length)return;var im=rocks[used++];im.sprite=earth.Frame(module,phase);im.color=new Color(1,1,1,alpha);im.rectTransform.anchoredPosition=new Vector2(Mathf.Round(p.x/2)*2,Mathf.Round(p.y/2)*2);im.rectTransform.sizeDelta=size;im.enabled=true;}
  void Stone(Vector2 p,float w,float h){if(earth!=null){Sprite(EarthModule.Rock,p,new Vector2(w*3,h*3),(used%7)/8f);return;}Block(p,w+2,h+2,dark);Block(p,w,h,stone);Block(p+new Vector2(-w*.25f,h-2),w*.65f,2,edge);}
  void Crack(Vector2 p,float scale){for(int i=0;i<5;i++){var a=p+new Vector2((i%2==0?-6:6)*scale,-i*9*scale);var b=p+new Vector2((i%2==1?-6:6)*scale,-(i+1)*9*scale);mesh.Line(a,b,4,dark);if(i==2)mesh.Line(a,a+new Vector2(16,-8)*scale,3,dark);}}
  public void Render(BattleModel b){
   if(mesh==null)return;mesh.Begin();label.enabled=false;foreach(var im in rocks)im.enabled=false;used=0;
   kraken?.Clear();
   vulcan?.Clear();garuda?.Clear();
   if(b!=null&&b.GarudaDecisionBattle&&b.Result==BattleResult.Fighting&&view.IsBattleUncovered){garuda.Render(b);label.enabled=b.BossPreparation;label.text=b.BossSkillWarning;label.color=WindVFXStyle.Mint;mesh.End();PeakQuads=Mathf.Max(PeakQuads,QuadCount);return;}
   if(b!=null&&b.BossSkillBusy&&(b.ActiveBossSkill.attackVfx=="forge_barrier"||b.ActiveBossSkill.attackVfx=="vulcan_meteor")&&view.IsBattleUncovered){
    label.enabled=b.BossPreparation;label.color=new Color(1,.76f,.35f);
    label.text=b.BossSkillWarning; // Existing enemy status already shows barrier HP and deadline.
    vulcan.Render(b);mesh.End();PeakQuads=Mathf.Max(PeakQuads,QuadCount);return;
   }
   if(b!=null&&b.BossSkillBusy&&b.ActiveBossSkill.attackVfx=="kraken_tentacle"&&view.IsBattleUncovered){
    label.enabled=b.BossPreparation;label.text=b.BossSkillWarning;label.color=new Color(.55f,.92f,.96f);
    kraken.Render(b);mesh.End();PeakQuads=Mathf.Max(PeakQuads,QuadCount);return;
   }
   if(b==null||!b.BossSkillBusy||b.ActiveBossSkill.attackVfx!="earth_wave"||!view.IsBattleUncovered){mesh.End();return;}
   var motion=view.GetComponent<BattleAnimation>();Vector2 source=motion.BossSkillEnemyGround,target=motion.BossSkillPlayerGround;
   label.enabled=b.BossPreparation;label.text=b.BossSkillWarning;label.color=edge;
   int playerIndex=view.playerArt.transform.GetSiblingIndex();int depth=playerIndex+(b.BossState==BossSkillState.Recovery?1:0);if(mesh.transform.GetSiblingIndex()<playerIndex)depth--;mesh.transform.SetSiblingIndex(depth);
   if(b.BossPreparation){
    var icon=new Vector2(-194,-420);mesh.Diamond(icon,10,10,edge);mesh.Diamond(icon,7,7,dark);Block(icon+new Vector2(0,2),1,3,edge);Block(icon+new Vector2(0,-4),1,1,edge);
    for(int i=-2;i<=2;i++){Crack(source+new Vector2(i*22,5),.65f);float t=(float)b.Time*2+i;Sprite(EarthModule.DustPuff,source+new Vector2(i*23,8+Mathf.Sin(t)*5),new Vector2(28,18),Mathf.Repeat(t,.9f),.6f);}
    // Three descending chevrons indicate direction, not the normal attack gauge.
    for(int j=0;j<3;j++){var p=Vector2.Lerp(source,target,.15f+j*.12f);float w=7+j*2;mesh.Line(p+new Vector2(-w,4),p+new Vector2(0,-4),2,edge);mesh.Line(p+new Vector2(0,-4),p+new Vector2(w,4),2,edge);}
   }
   if(b.BossState==BossSkillState.Travel){
    float t=b.BossPhaseProgress;WavePosition=Vector2.Lerp(source,target,t);
    Sprite(EarthModule.DustWave,WavePosition+Vector2.up*14,new Vector2(230+100*t,66),t,.72f);
    for(int i=-4;i<=4;i++){var p=WavePosition+new Vector2(i*(23+t*8),Mathf.Abs(i)*4+(i%2)*7);Crack(p+Vector2.up*36,.85f);Stone(p,13+t*6,11+(4-Mathf.Abs(i))*3);Sprite(EarthModule.DustPuff,p+new Vector2(-5,29),new Vector2(48,30),t,.65f);}
    mesh.Line(WavePosition+new Vector2(-105-38*t,-16),WavePosition+new Vector2(105+38*t,-16),5,edge);
    // Landing brackets remain fixed even while the dragon performs a dodge animation.
    for(int sign=-1;sign<=1;sign+=2){var p=target+new Vector2(sign*92,-5);mesh.Line(p,p+Vector2.up*14,2,edge);mesh.Line(p,p+new Vector2(-sign*12,0),2,edge);}
   }
   float impact=(float)(b.Time-b.BossImpactAt);
   if(b.BossState==BossSkillState.Recovery&&impact>=0&&impact<.4f){
    WavePosition=target;float t=impact/.4f;
    Sprite(EarthModule.Impact,target+Vector2.up*22,new Vector2(240,115),t,1-t);
    for(int i=0;i<16;i++){float a=i*2.39996f;var p=target+new Vector2(Mathf.Cos(a)*(35+90*t),Mathf.Sin(a)*(15+24*t)+40*Mathf.Sin(t*Mathf.PI));Stone(p,Mathf.Max(1,8*(1-t)),Mathf.Max(1,6*(1-t)));}
    for(int i=-3;i<=3;i++)Crack(target+new Vector2(i*35,14),1.2f*(1-t));
   }
   mesh.End();PeakQuads=Mathf.Max(PeakQuads,mesh.Count);
  }
 }
}

