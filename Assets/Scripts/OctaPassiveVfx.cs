using UnityEngine;
namespace DragonTower {
 // Read-only presentation of the persistent summon. Never rolls or deals damage.
 public sealed class OctaPassiveVfx:MonoBehaviour {
  BattleView view;BattleModel owner;LightningMeshLayer mesh;BattleVfxHudVisibility materials;
  double appeared;bool visible;int lastDamage;double strikeAt=-10;
  public int QuadCount=>mesh==null?0:mesh.Count;
  public void Initialize(BattleView v){view=v;materials=new BattleVfxHudVisibility();var go=new GameObject("Octa passive summoned tentacle",typeof(RectTransform),typeof(CanvasRenderer),typeof(LightningMeshLayer));var r=go.GetComponent<RectTransform>();r.SetParent(v.frame,false);r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.sizeDelta=new Vector2(480,850);mesh=go.GetComponent<LightningMeshLayer>();mesh.raycastTarget=false;mesh.material=materials.alpha;BattleVfxHudVisibility.BringHudForward(v);}
  public void Render(BattleModel b){if(mesh==null)return;mesh.Begin();
   if(owner!=b){owner=b;visible=false;lastDamage=0;strikeAt=-10;}
   bool active=b!=null&&b.NewPassives&&b.Dragon.passiveMechanic==DragonPassiveMechanic.Tentacle&&b.Result==BattleResult.Fighting&&b.TentacleRemaining>.0001&&view.IsBattleUncovered;
   if(!active){visible=false;mesh.End();return;}
   if(!visible){appeared=b.Time;visible=true;}if(lastDamage!=b.PassiveDamage){strikeAt=b.Time;lastDamage=b.PassiveDamage;}
   materials.Update(view);float age=(float)(b.Time-appeared),growth=Mathf.Clamp01(age/.18f)*Mathf.Clamp01((float)b.TentacleRemaining/.18f);
   float strike=Mathf.Clamp01(1-(float)(b.Time-strikeAt)/.24f);var target=(Vector2)view.frame.InverseTransformPoint(view.enemyArt.rectTransform.TransformPoint(Vector2.zero));var root=target+new Vector2(-76,-65);
   Color ink=new Color(.06f,.10f,.20f),body=new Color(.22f,.46f,.62f),edge=new Color(.51f,.83f,.84f),sucker=new Color(.78f,.68f,.88f);
   for(int i=0;i<24;i++){float a=i*Mathf.PI/12,c=(i+1)*Mathf.PI/12;mesh.Line(root+new Vector2(Mathf.Cos(a)*28,Mathf.Sin(a)*7),root+new Vector2(Mathf.Cos(c)*28,Mathf.Sin(c)*7),3,body);}
   Vector2 last=root;
   for(int i=1;i<=28;i++){float t=i/28f;var pos=root+new Vector2(Mathf.Sin(t*4+age*2)*16+t*t*(40+strike*50),t*(104-strike*36))*growth;float w=(21-17*t)*growth;mesh.Line(last,pos,w+4,ink);mesh.Line(last,pos,w,body);mesh.Line(last+Vector2.left*3,pos+Vector2.left*3,Mathf.Max(1,w*.18f),edge);if(i%3==0)mesh.Diamond(pos+Vector2.right*w*.25f,Mathf.Max(1,3-t*2),Mathf.Max(1,4-t*2),sucker);last=pos;}
   if(strike>0){var c=edge;c.a=strike;for(int i=0;i<5;i++){float a=i*1.256f;var d=new Vector2(Mathf.Cos(a),Mathf.Sin(a));mesh.Line(target+d*8,target+d*(16+strike*10),2,c);}}
   mesh.End();
  }
  void OnDisable(){visible=false;if(mesh!=null){mesh.Begin();mesh.End();}}
  void OnDestroy(){if(mesh!=null)Destroy(mesh.gameObject);materials?.Dispose();}
 }
}
