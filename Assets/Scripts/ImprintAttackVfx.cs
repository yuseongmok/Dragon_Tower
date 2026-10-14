using UnityEngine;
namespace DragonTower {
 // Presentation only: no BattleModel reference, random draws, timers or damage callbacks.
 // Shares the legendary mesh pool, HUD-safe shader and read-only skill palette assets.
 public sealed class ImprintAttackVfx:MonoBehaviour {
  const int Capacity=3;
  readonly UnityEngine.UI.RawImage[] dragons=new UnityEngine.UI.RawImage[6];readonly UnityEngine.UI.Image[] meteors=new UnityEngine.UI.Image[3];
  struct Pulse {public bool active;public float age;public int tier;public DragonPassiveMechanic motif;public Vector2 target;public Color ink,main,light;}
  readonly Pulse[] pulses=new Pulse[Capacity];int cursor;BattleView view;LightningMeshLayer mesh;BattleVfxHudVisibility materials;
  DarkVfxStyle dark;LightVfxStyle light;WaterVfxStyle water;LightningVfxStyle lightning;FireVfxStyle fire;IceVfxStyle ice;EarthVfxStyle earth;
  public int ActiveCount {get{int n=0;foreach(var p in pulses)if(p.active)n++;return n;}}
  public int QuadCount=>mesh==null?0:mesh.Count;public int PeakQuads{get;private set;}public int Overflow=>mesh==null?0:mesh.Overflow;
  public static int Tier(int level)=>level<3?1:level<6?2:level<9?3:4;
  public void Initialize(BattleView owner){if(mesh!=null)return;view=owner;materials=new BattleVfxHudVisibility();
   var go=new GameObject("Imprint basic attack pool",typeof(RectTransform),typeof(UnityEngine.CanvasRenderer),typeof(LightningMeshLayer));var r=go.GetComponent<RectTransform>();r.SetParent(view.frame,false);r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.sizeDelta=new Vector2(480,850);mesh=go.GetComponent<LightningMeshLayer>();mesh.raycastTarget=false;mesh.material=materials.alpha;BattleVfxHudVisibility.BringHudForward(view);
   var dragonTexture=Resources.Load<Texture2D>("MirLegendary/Dragon");var meteorSprite=Resources.Load<FireSkillLibrary>("FireSkills/Library")?.meteor;
   for(int i=0;i<6;i++){var im=new GameObject("Pooled small divine dragon "+i,typeof(RectTransform),typeof(UnityEngine.UI.RawImage)).GetComponent<UnityEngine.UI.RawImage>();im.rectTransform.SetParent(view.frame,false);im.rectTransform.anchorMin=im.rectTransform.anchorMax=Vector2.one*.5f;im.texture=dragonTexture;im.raycastTarget=false;im.material=materials.alpha;im.enabled=false;dragons[i]=im;}
   for(int i=0;i<3;i++){var im=new GameObject("Pooled small meteor "+i,typeof(RectTransform),typeof(UnityEngine.UI.Image)).GetComponent<UnityEngine.UI.Image>();im.rectTransform.SetParent(view.frame,false);im.rectTransform.anchorMin=im.rectTransform.anchorMax=Vector2.one*.5f;im.sprite=meteorSprite;im.preserveAspect=true;im.raycastTarget=false;im.material=materials.alpha;im.enabled=false;meteors[i]=im;}BattleVfxHudVisibility.BringHudForward(view);   dark=Resources.Load<DarkSkillLibrary>("DarkSkills/Library")?.style;light=Resources.Load<LightSkillLibrary>("LightSkills/Library")?.style;water=Resources.Load<WaterSkillLibrary>("WaterSkills/Library")?.style;lightning=Resources.Load<LightningSkillLibrary>("LightningSkills/Library")?.style;fire=Resources.Load<FireSkillLibrary>("FireSkills/Library")?.style;ice=Resources.Load<IceSkillLibrary>("IceSkills/Library")?.style;earth=Resources.Load<EarthSkillLibrary>("EarthSkills/Library")?.style;
  }
  public void Play(DragonData dragon,int level){if(view==null||dragon==null)return;var target=(Vector2)view.frame.InverseTransformPoint(view.enemyArt.rectTransform.TransformPoint(Vector2.zero));PlayAt(dragon,level,target);}
  public void PlayAt(DragonData dragon,int level,Vector2 target){if(mesh==null||dragon==null||Tier(level)==1)return;
   var p=new Pulse{active=true,tier=Tier(level),motif=dragon.passiveMechanic,target=target,main=dragon.color,ink=new Color(.035f,.055f,.09f),light=new Color(.92f,1,1)};
   switch(p.motif){
    case DragonPassiveMechanic.JetStream:case DragonPassiveMechanic.StormAffinity:p.main=WindVFXStyle.Mint;p.ink=WindVFXStyle.Deep;p.light=WindVFXStyle.Pale;break;
    case DragonPassiveMechanic.VoidAccelerator:case DragonPassiveMechanic.Mirage:if(dark!=null){p.main=dark.purple;p.ink=dark.core;p.light=dark.pale;}break;
    case DragonPassiveMechanic.HydraVenom:if(dark!=null){p.main=dark.toxic;p.ink=dark.blackPurple;p.light=dark.poisonPale;}break;
    case DragonPassiveMechanic.TimeRift:case DragonPassiveMechanic.HolyLight:case DragonPassiveMechanic.Phoenix:if(light!=null){p.main=light.gold;p.ink=light.deep;p.light=light.ivory;}break;
    case DragonPassiveMechanic.RuyiOrb:case DragonPassiveMechanic.Tentacle:case DragonPassiveMechanic.FrozenBlade:if(water!=null){p.main=water.aqua;p.ink=water.deep;p.light=water.foam;}break;
    case DragonPassiveMechanic.ElectricShock:if(lightning!=null){p.main=lightning.cyan;p.ink=lightning.navy;p.light=lightning.core;}break;
    case DragonPassiveMechanic.FlameBreath:if(fire!=null){p.main=fire.orange;p.ink=fire.deepCrimson;p.light=fire.whiteHot;}break;
    case DragonPassiveMechanic.FreezingGaze:if(ice!=null){p.main=ice.cyan;p.ink=ice.deepNavy;p.light=ice.white;}break;
    case DragonPassiveMechanic.HardenedShell:case DragonPassiveMechanic.IronArmor:if(earth!=null){p.main=earth.sand;p.ink=earth.shadow;p.light=earth.edge;}break;
   }
   pulses[cursor]=p;cursor=(cursor+1)%Capacity;Render();
  }
  public void Step(float dt){for(int i=0;i<Capacity;i++)if(pulses[i].active){pulses[i].age+=Mathf.Max(0,dt);if(pulses[i].age>=.46f)pulses[i].active=false;}Render();}
  public void Clear(){for(int i=0;i<Capacity;i++)pulses[i].active=false;if(mesh!=null){mesh.Begin();mesh.End();}foreach(var im in dragons)if(im!=null)im.enabled=false;foreach(var im in meteors)if(im!=null)im.enabled=false;}
  void OnDisable()=>Clear();void OnDestroy(){Clear();materials?.Dispose();if(mesh!=null)Destroy(mesh.gameObject);foreach(var im in dragons)if(im!=null)Destroy(im.gameObject);foreach(var im in meteors)if(im!=null)Destroy(im.gameObject);}
  static Vector2 D(float a)=>new Vector2(Mathf.Cos(a),Mathf.Sin(a));
  Color ink,main,pale;Vector2 center;float scale,alpha;
  Vector2 P(Vector2 p)=>center+p*scale;
  Color Fade(Color c){c.a*=alpha;return c;}
  void Line(Vector2 a,Vector2 b,float w,Color c)=>mesh.Line(P(a),P(b),w*scale,Fade(c));
  void Dot(Vector2 p,float r,Color c)=>mesh.Diamond(P(p),r*scale,r*scale,Fade(c));
  void Ring(Vector2 c,float r,float aspect,float turn,Color col,float width=2,float arc=6.283f){for(int i=0;i<32;i++){float a=turn+i*arc/32,b=turn+(i+1)*arc/32;Line(c+new Vector2(Mathf.Cos(a)*r,Mathf.Sin(a)*r*aspect),c+new Vector2(Mathf.Cos(b)*r,Mathf.Sin(b)*r*aspect),width,col);}}
  void Slash(float angle,float offset,float t){Vector2 d=D(angle),n=D(angle+1.57f);for(int i=0;i<15;i++){float u=(i-7)/7f,v=(i-6)/7f;Vector2 a=d*u*48+n*(offset+(1-u*u)*12),b=d*v*48+n*(offset+(1-v*v)*12);Line(a,b,8*(1-Mathf.Abs(u))+.5f,main);Line(a+n*2,b+n*2,2,pale);}}
  void Gear(Vector2 pos,float angle){Ring(pos,19,1,angle,main,5);Ring(pos,9,1,angle,pale);for(int i=0;i<10;i++){var d=D(angle+i*.6283f);Line(pos+d*17,pos+d*25,6,main);}Line(pos-D(angle)*8,pos+D(angle)*8,3,pale);}
  void Dragon(Vector2 pos,float turn){Vector2 last=pos;for(int i=0;i<19;i++){float a=turn+i*.14f;var q=pos+D(a)*(12+i*.8f);Line(last,q,Mathf.Max(2,9-i*.35f),ink);Line(last,q,Mathf.Max(1,6-i*.25f),main);if(i%4==0)Dot(q+D(a)*3,2,pale);last=q;}var head=pos+D(turn)*12;Dot(head,8,main);Dot(head+new Vector2(3,2),2,pale);Line(head+new Vector2(-3,5),head+new Vector2(-8,12),2,pale);Line(head+new Vector2(3,5),head+new Vector2(5,12),2,pale);}
  void Render(){if(mesh==null)return;materials.Update(view);mesh.Begin();foreach(var im in dragons)im.enabled=false;foreach(var im in meteors)im.enabled=false;for(int slot=0;slot<Capacity;slot++){var p=pulses[slot];if(!p.active)continue;float t=p.age;
   // Match the existing basic hit flash at +0.06 s. No new gameplay event is emitted.
   if(t<.06f)continue;float u=(t-.06f)/.40f;center=p.target;scale=p.tier==2?.67f:p.tier==3?.86f:1f;alpha=Mathf.Clamp01((1-u)*2);ink=p.ink;main=p.main;pale=p.light;
   if(p.tier>=3){float a=alpha;alpha*=.22f;center+=new Vector2(-8,5);Draw(p.motif,u,p.tier);center=p.target;alpha=a;}
   Draw(p.motif,u,p.tier);
   if(p.motif==DragonPassiveMechanic.RuyiOrb&&dragons[0].texture!=null){for(int j=0;j<2;j++){var im=dragons[slot*2+j];float a=u*5+j*Mathf.PI;im.enabled=true;im.color=new Color(1,1,1,alpha);im.rectTransform.anchoredPosition=center+D(a)*30*scale;im.rectTransform.sizeDelta=new Vector2(104,35)*scale;im.rectTransform.localRotation=Quaternion.Euler(0,0,a*Mathf.Rad2Deg+90);}}
   if(p.motif==DragonPassiveMechanic.FlameBreath&&meteors[slot].sprite!=null){var im=meteors[slot];im.enabled=true;im.color=new Color(1,1,1,alpha);im.rectTransform.anchoredPosition=center+Vector2.up*(1-u)*12;im.rectTransform.sizeDelta=Vector2.one*48*scale;}
   int particles=p.tier==2?4:p.tier==3?8:12;for(int j=0;j<particles;j++){float a=j*2.39996f;Dot(D(a)*(22+u*40)+(j%2==0?Vector2.up:Vector2.down)*3,1.4f*(1-u),j%3==0?pale:main);}
   if(p.tier==4){Ring(Vector2.zero,19+u*40,.45f,u,main,1.5f);for(int j=0;j<4;j++)Dot(D(j*1.57f)*(42+u*10),2,pale);}
  }mesh.End();PeakQuads=Mathf.Max(PeakQuads,mesh.Count);}
  void Draw(DragonPassiveMechanic motif,float u,int tier){float turn=u*5;switch(motif){
   case DragonPassiveMechanic.TimeRift:Gear(new Vector2(-25,8),turn);Gear(new Vector2(25,-8),-turn);break;
   case DragonPassiveMechanic.JetStream:Slash(.68f,0,u);if(tier>=3)Slash(.68f,-13,u);break;
   case DragonPassiveMechanic.VoidAccelerator:for(int i=0;i<10;i++)Ring(Vector2.zero,i*2,1,0,ink,3);Ring(Vector2.zero,22,1,turn,main,4);Ring(Vector2.zero,34,.3f,.3f+turn,pale,2);break;
   case DragonPassiveMechanic.FreezingGaze:for(int i=0;i<9;i++)Ring(Vector2.zero,31-i*1.3f,1,.5f+u,pale,i<3?2:1,3.6f);break;
   case DragonPassiveMechanic.RuyiOrb:if(dragons[0].texture==null){Dragon(D(turn)*22,turn);Dragon(D(turn+Mathf.PI)*22,turn+Mathf.PI);}break;
   case DragonPassiveMechanic.HydraVenom:for(int side=-1;side<=1;side+=2)for(int j=0;j<3;j++){float x=(j-1)*22,y=side*(28-u*17);Line(new Vector2(x,y),new Vector2(x+4,side*4),8,main);Line(new Vector2(x-2,y),new Vector2(x+3,side*4),3,pale);}break;
   case DragonPassiveMechanic.HardenedShell:Line(new Vector2(-42,-34),new Vector2(30,26),12,ink);Line(new Vector2(-42,-34),new Vector2(30,26),7,main);Line(new Vector2(-42,-34),new Vector2(30,26),2,pale);Ring(new Vector2(30,26),14+u*10,.6f,turn,pale);break;
   case DragonPassiveMechanic.StormAffinity:for(int j=0;j<8;j++)Ring(new Vector2(Mathf.Sin(j*.8f+turn)*6,-34+j*8),7+j*4,.23f,turn+j*.4f,j%2==0?main:pale,2,5.1f);break;
   case DragonPassiveMechanic.FrozenBlade:Slash(.78f,0,u);Slash(2.36f,0,u);break;
   case DragonPassiveMechanic.Tentacle:{Vector2 last=new Vector2(-32,-36);for(int j=0;j<24;j++){float f=j/23f;var q=new Vector2(-32+f*65,-36+Mathf.Sin(f*2.8f+u)*54);Line(last,q,12-f*9,ink);Line(last,q,8-f*6,main);if(j%4==0)Dot(q+Vector2.down*3,2,pale);last=q;}break;}
   case DragonPassiveMechanic.IronArmor:for(int j=-1;j<=1;j++)Slash(.95f,j*17,u);break;
   case DragonPassiveMechanic.Phoenix:Ring(Vector2.zero,18,1,turn,main,8);for(int j=0;j<8;j++)Line(D(j*.785f+turn)*23,D(j*.785f+turn)*31,3,pale);for(int j=0;j<4;j++){var pos=new Vector2((j<2?-1:1)*(28+j%2*8),-12+j%2*22);Line(pos,pos+new Vector2(j<2?-10:10,23),4,main);for(int k=0;k<4;k++)Line(pos+Vector2.up*k*5,pos+new Vector2(j<2?-12:12,k*5+6),2,pale);}break;
   case DragonPassiveMechanic.Mirage:Ring(new Vector2(0,8),19,1,0,main,9,Mathf.PI);for(int j=-1;j<=1;j++)Line(new Vector2(j*11,8),new Vector2(j*15+Mathf.Sin(turn+j)*5,-22-u*12),8,main);Dot(new Vector2(-7,10),3,pale);Dot(new Vector2(7,10),3,pale);break;
   case DragonPassiveMechanic.HolyLight:Ring(new Vector2(-25,-25),24,.4f,turn,main,3);for(int j=0;j<6;j++)Dot(new Vector2(-25,-25)+new Vector2(Mathf.Cos(j*1.047f+turn)*20,Mathf.Sin(j*1.047f+turn)*8),2,pale);Line(new Vector2(-25,-25),new Vector2(29,29),7,main);Line(new Vector2(-25,-25),new Vector2(29,29),2,pale);break;
   case DragonPassiveMechanic.FlameBreath:for(int j=0;j<5;j++)Line(new Vector2(-12+j*5,12),new Vector2(-35+j*6,55-j*3),5,main);Dot(Vector2.zero,17,ink);Dot(new Vector2(-2,3),12,main);Dot(new Vector2(-3,5),6,pale);break;
   case DragonPassiveMechanic.ElectricShock:{Vector2 last=new Vector2(10,64);for(int j=1;j<=6;j++){var q=new Vector2(j==6?0:j%2==0?15:-12,64-j*11);Line(last,q,8,main);Line(last,q,2,pale);last=q;}break;}
  }}
 }
}
