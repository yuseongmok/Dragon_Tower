using UnityEngine;
namespace DragonTower {
 // Presentation only: two preallocated, two-pixel-snapped meshes, no hit or timer callbacks.
 public sealed class KrakenBossSkillVfx {
  BattleView view;LightningMeshLayer floor,body;KrakenProfile palette;
  readonly Color navy=new Color(.025f,.08f,.16f),cyan=new Color(.12f,.72f,.79f),foam=new Color(.83f,.97f,.96f);
  public int QuadCount=>(floor==null?0:floor.Count)+(body==null?0:body.Count);
  public int DroppedQuads=>(floor==null?0:floor.Overflow)+(body==null?0:body.Overflow);
  public void Initialize(BattleView owner){view=owner;palette=Resources.Load<KrakenProfile>("VFX/Kraken/Profile");floor=Layer("Kraken floor water (pooled)");body=Layer("Kraken curved tentacle (pooled)");}
  LightningMeshLayer Layer(string name){var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(LightningMeshLayer));var r=go.GetComponent<RectTransform>();r.SetParent(view.frame,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,1);r.sizeDelta=new Vector2(480,850);var m=go.GetComponent<LightningMeshLayer>();m.raycastTarget=false;return m;}
  public void Clear(){if(floor!=null){floor.Begin();floor.End();}if(body!=null){body.Begin();body.End();}}
  public void Destroy(){if(floor!=null)Object.Destroy(floor.gameObject);if(body!=null)Object.Destroy(body.gameObject);}
  static Color Alpha(Color c,float a){c.a=a;return c;}
  static Vector2 Ellipse(float a,float r)=>new Vector2(Mathf.Cos(a)*r,Mathf.Sin(a)*r*.29f);
  void Ring(Vector2 p,float r,float phase,float width,Color c){
   for(int i=0;i<36;i++){if((i+Mathf.FloorToInt(phase*2))%9==0)continue;float a=i*Mathf.PI/18+phase,b=a+.15f;float ripple=1+.035f*Mathf.Sin(i*1.7f+phase*3);floor.Line(p+Ellipse(a,r*ripple),p+Ellipse(b,r*ripple),width,c);}
  }
  void Disc(Vector2 p,float r,Color c){for(int i=0;i<24;i++)floor.Quad(p,p+Ellipse(i*Mathf.PI/12,r),p+Ellipse((i+1)*Mathf.PI/12,r),p,c);}
  void PlaceDepth(LightningMeshLayer layer,bool front){int n=view.playerArt.transform.GetSiblingIndex()+(front?1:0);if(layer.transform.GetSiblingIndex()<view.playerArt.transform.GetSiblingIndex())n--;layer.transform.SetSiblingIndex(n);}
  public void Render(BattleModel b){
   floor.Begin();body.Begin();PlaceDepth(floor,false);PlaceDepth(body,true);
   var motion=view.GetComponent<BattleAnimation>();var p=motion.BossSkillPlayerGround;var boss=motion.BossSkillEnemyGround;
   float t=b.BossPhaseProgress;bool prep=b.BossPreparation;
   if(prep){
    bool warning=b.BossState==BossSkillState.Warning;float charge=warning?t*.22f:.22f+t*.78f;
    float rotation=warning?t*1.4f:1.4f+2*t+5*t*t;
    float contraction=warning?0:Mathf.Clamp01((t-.76f)/.24f);
    float radius=Mathf.Lerp(88,40,contraction);
    Disc(p,64,Alpha(navy,.24f+charge*.3f));
    for(int i=0;i<3;i++)Ring(p,radius-i*12,rotation+i*1.2f,2+charge,Alpha(i==0?foam:cyan,.28f+charge*.6f));
    if(!warning){
     // A submerged hooked silhouette, visible before the actual damage frame.
     for(int i=0;i<14;i++){float u=i/13f;var q=p+new Vector2(-43+u*77,-13+Mathf.Sin(u*4.5f)*8);floor.Diamond(q,(12-u*9)*t,5*t,Alpha(navy,.95f*t));}
     for(int i=0;i<12;i++){float a=i*2.39996f+rotation;float h=Mathf.Repeat(i*.173f+t*2,1);floor.Diamond(p+Ellipse(a,radius)+Vector2.up*(4+18*h),2,3,Alpha(foam,charge*(1-h)));}
    }
    // Boss-side bubbles and surface ripples communicate preparation without new guidance text.
    Ring(boss,50,rotation*.4f,2,Alpha(cyan,.55f));
    for(int i=0;i<8;i++){float f=Mathf.Repeat(t+i*.137f,1);var q=boss+new Vector2((i%2==0?-1:1)*(40+i*4),20+f*65);floor.Diamond(q,3,4,Alpha(cyan,(1-f)*.65f));}
   }else if(b.BossSkillHasImpacted){
    var s=b.ActiveBossSkill;
    float age=b.BossState==BossSkillState.Travel?t*s.damageTime:s.damageTime+t*(s.lingerTime+s.recovery);
    float retract=Mathf.Clamp01((age-s.damageTime-s.lingerTime)/Mathf.Max(.01f,s.recovery*.65f));
    float height=(.18f+.82f*Mathf.Clamp01(age/s.damageTime))*(1-retract);
    float fade=1-Mathf.Clamp01((age-.7f)/.9f);
    Disc(p,62,Alpha(navy,.72f*fade));Ring(p,70+age*18,age*3,3,Alpha(cyan,fade*.8f));Ring(p,48+age*14,-age*2,2,Alpha(foam,fade*.6f));
    if(height>.005f)Tentacle(p+Vector2.right*32,height,age);
    float burst=Mathf.Clamp01(age/.75f);
    if(age<.75f)for(int i=0;i<22;i++){
     float a=i*2.39996f,spread=20+burst*(58+(i%4)*10);
     var q=p+new Vector2(Mathf.Cos(a)*spread,Mathf.Sin(a)*spread*.23f+Mathf.Sin(burst*Mathf.PI)*(24+i%5*8));
     body.Diamond(q,Mathf.Max(1,4*(1-burst)),Mathf.Max(2,7*(1-burst)),Alpha(i%3==0?foam:cyan,1-burst));
    }
    if(age<.4f)for(int i=0;i<12;i++){float a=i*Mathf.PI/6;var q=p+Ellipse(a,32+age*90);body.Line(q,q+new Vector2(Mathf.Cos(a)*12,12+Mathf.Abs(Mathf.Sin(a))*14)*(1-age/.4f),5,Alpha(foam,1-age/.4f));}
   }
   floor.End();body.End();
  }
  Vector2 Curve(float u,float age){
   // Broad hook with a downward-curled, living tip rather than a pointed blade.
   if(u<.7f){float t=u/.7f,v=1-t;return 3*v*v*t*new Vector2(-36,65)+3*v*t*t*new Vector2(28,180)+t*t*t*new Vector2(80,180);}
   else{float t=(u-.7f)/.3f,v=1-t;return v*v*v*new Vector2(80,180)+3*v*v*t*new Vector2(137,180)+3*v*t*t*new Vector2(132,102)+t*t*t*new Vector2(83+Mathf.Sin(age*12)*6,129+Mathf.Cos(age*9)*5);}
  }
  void Tentacle(Vector2 root,float height,float age){
   Color shadow=palette!=null?palette.shadow:navy,skin=palette!=null?palette.body:new Color(.02f,.23f,.33f),face=palette!=null?palette.face:cyan;
   Color cups=palette!=null?palette.cups:new Color(.53f,.27f,.59f),lip=palette!=null?palette.cupLight:new Color(.88f,.57f,.76f);
   // Four continuous tapered ribbons form an ink edge, dark underside, teal body and highlight.
   for(int pass=0;pass<4;pass++)for(int i=0;i<32;i++){
    float u=i/32f,v=(i+1)/32f;Vector2 a=Curve(u,age),z=Curve(v,age);a.y*=height;z.y*=height;
    var n=new Vector2(-(z-a).y,(z-a).x).normalized;
    float w=Mathf.Lerp(26,2,u),w2=Mathf.Lerp(26,2,v),scale=pass==0?1.16f:pass==1?1:pass==2?.68f:.18f;
    float offset=pass==2?-.18f:pass==3?-.52f:0;
    var p=root+a+n*w*offset;var q=root+z+n*w2*offset;
    body.Quad(p-n*w*scale,p+n*w*scale,q+n*w2*scale,q-n*w2*scale,pass==0?shadow:pass==1?cups:pass==2?skin:face);
   }
   for(int i=0;i<9;i++){
    float u=.09f+i*.092f;var q=Curve(u,age);q.y*=height;var next=Curve(u+.015f,age);next.y*=height;var tangent=(next-q).normalized;var n=new Vector2(-tangent.y,tangent.x);
    float r=Mathf.Lerp(7,2,u);var center=root+q+n*(Mathf.Lerp(26,2,u)*.65f);
    for(int j=0;j<10;j++){float a=j*Mathf.PI/5,z=(j+1)*Mathf.PI/5;var p=center+n*Mathf.Cos(a)*r*.6f+tangent*Mathf.Sin(a)*r;var e=center+n*Mathf.Cos(z)*r*.6f+tangent*Mathf.Sin(z)*r;body.Line(p,e,3,lip);}
    body.Diamond(center,r*.35f,r*.55f,shadow);
   }
  }
 }
}
