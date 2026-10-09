using UnityEngine;
namespace DragonTower {
 public sealed partial class ItemProcVfx {
  LightningStylePrototype electric;LightStylePrototype holy;WaterStylePrototype water;DarkStylePrototype dark;EarthSkillLibrary earth;
  int ElementLayers=>(electric!=null?electric.ActiveCount:0)+(holy!=null?holy.ActiveCount:0)+(water!=null?water.ActiveCount:0)+(dark!=null?dark.ActiveCount:0);
  public int DroppedElementLayers=>(electric!=null?electric.DroppedLayers:0)+(holy!=null?holy.DroppedLayers:0)+(water!=null?water.DroppedLayers:0)+(dark!=null?dark.DroppedLayers:0);
  void EnsureElement(ItemProcArt art){
   if(art==ItemProcArt.Earth&&earth==null)earth=Resources.Load<EarthSkillLibrary>("EarthSkills/Library");
   if(art==ItemProcArt.Lightning&&electric==null){electric=gameObject.AddComponent<LightningStylePrototype>();electric.style=Resources.Load<LightningSkillLibrary>("LightningSkills/Library").style;electric.Initialize(view);}
   if(art==ItemProcArt.Light&&holy==null){holy=gameObject.AddComponent<LightStylePrototype>();holy.style=Resources.Load<LightSkillLibrary>("LightSkills/Library").style;holy.Initialize(view);}
   if(art==ItemProcArt.Water&&water==null){water=gameObject.AddComponent<WaterStylePrototype>();water.style=Resources.Load<WaterSkillLibrary>("WaterSkills/Library").style;water.Initialize(view);}
   if(art==ItemProcArt.Dark&&dark==null){dark=gameObject.AddComponent<DarkStylePrototype>();dark.style=Resources.Load<DarkSkillLibrary>("DarkSkills/Library").style;dark.Initialize(view);}
  }
  void BeginElements(float t){electric?.BeginLayers(t);holy?.BeginLayers(t);water?.BeginLayers(t);dark?.BeginLayers(t);}
  void EndElements(){electric?.EndLayers();holy?.EndLayers();water?.EndLayers();dark?.EndLayers();}
  void ClearElements(){electric?.Clear();holy?.Clear();water?.Clear();dark?.Clear();}
  void DestroyElements(){if(electric!=null)Destroy(electric);if(holy!=null)Destroy(holy);if(water!=null)Destroy(water);if(dark!=null)Destroy(dark);}
  void E(EarthModule m,Vector2 p,Vector2 size,float t,float alpha=1,float rotation=0)=>Draw(earth==null?null:earth.style.Frame(m,Mathf.Clamp01(t)),p,size,A(alpha),rotation);
  void ElementCast(Cast c,float age,double time){
   float scale=Mathf.Max(.35f,c.rule.visualScale);var p=c.to;int seed=Mathf.FloorToInt(age*24);float charge=Mathf.Clamp01(age/Mathf.Max(.05f,c.rule.initialDelay));
   if(c.impacts.Count<c.rule.hitCount){switch(c.rule.art){
    case ItemProcArt.Lightning:electric.Residual(p,age,scale*.65f,seed);break;
    case ItemProcArt.Light:holy.Circle(p+Vector2.up*65*scale,58*scale,new Vector3(62,15,0),age,1,scale>1?2:0,1);break;
    case ItemProcArt.Water:water.Mass(Vector2.Lerp(c.from,p,charge),28*scale,age,1);break;
    case ItemProcArt.Dark:if(c.rule.status==CombatStatusEffect.Poison)dark.ToxicMass(p,25*scale*charge,age,1);else dark.DarkVolume(p,35*scale*charge,age,1);break;
    case ItemProcArt.Earth:E(EarthModule.Crack,p+Vector2.down*35,new Vector2(130,45)*scale,charge,1);break;
   }}
   for(int i=0;i<c.impacts.Count;i++){float t=(float)(time-c.impacts[i]);if(t<0||t>1.3f)continue;float f=Mathf.Clamp01(1-t/1.1f);switch(c.rule.art){
    case ItemProcArt.Lightning:if(t<.14f)electric.Bolt(p+new Vector2((i%3-1)*120,220),p,scale,1-t/.14f,seed+i*71,1,true);electric.Impact(p,t,scale,seed+i,false);electric.Residual(p,t,scale,seed);break;
    case ItemProcArt.Light:holy.Beam(p+Vector2.up*140,p,t);holy.Impact(p,t,scale);break;
    case ItemProcArt.Water:water.Impact(p,t,scale);break;
    case ItemProcArt.Dark:if(c.rule.status==CombatStatusEffect.Poison)dark.ToxicImpact(p,t);else dark.DarkImpact(p,t,scale);break;
    case ItemProcArt.Earth:
     E(EarthModule.Crack,p+Vector2.down*35,new Vector2(200,65)*scale,t,f);
     E(EarthModule.Impact,p,new Vector2(130,100)*scale,t*5,Mathf.Clamp01(1-t/.25f));
     E(EarthModule.DustWave,p+Vector2.down*32,new Vector2(190,65)*scale*(.4f+t*2),t,f*.7f);
     for(int j=0;j<18;j++){float a=j*2.399f;var q=p+new Vector2(Mathf.Cos(a)*t*(100+j%3*30),t*(100+j%4*25)-t*t*220)*scale;E(EarthModule.Rock,q,Vector2.one*(8+j%4*4)*scale,t+j*.13f,f,j*31+t*80);}break;
   }}
  }
 }
}
