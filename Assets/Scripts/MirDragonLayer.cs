using UnityEngine;using UnityEngine.UI;
namespace DragonTower {
 // One subdivided, path-deformed texture. Tail samples an earlier part of the flight.
 public sealed class MirDragonLayer:MaskableGraphic {
  Material owned;Texture2D tex;MirLegendaryVfx owner;float age,alpha,gold,trail;
  public override Texture mainTexture=>tex;
  public void Initialize(BattleView view,MirLegendaryVfx fx,int order){owner=fx;tex=Resources.Load<Texture2D>("MirLegendary/Dragon");owned=new Material(Resources.Load<Shader>("MirLegendary/DragonFlow"));material=owned;raycastTarget=false;rectTransform.SetParent(view.frame,false);rectTransform.anchorMin=rectTransform.anchorMax=rectTransform.pivot=Vector2.one*.5f;rectTransform.sizeDelta=new Vector2(480,850);var canvas=gameObject.AddComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=order;}
  public void Show(float time,float opacity,float empowered,float delay=0){age=time;alpha=opacity;gold=empowered;trail=delay;owned.SetFloat("_FlowTime",time);owned.SetFloat("_Gold",gold);SetVerticesDirty();}
  public void Clear(){alpha=0;SetVerticesDirty();}
  protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();if(alpha<=0||owner==null)return;const int nx=100,ny=12;for(int x=0;x<=nx;x++){
    float u=x/(float)nx;Vector2 center=owner.BodyPoint(age-trail,1-u);Vector2 forward=(owner.BodyPoint(age-trail,Mathf.Max(0,1-u-.008f))-owner.BodyPoint(age-trail,1-u+.008f)).normalized;var normal=new Vector2(-forward.y,forward.x);
    for(int y=0;y<=ny;y++){float v=y/(float)ny;var q=center+normal*((v-.39f)*220);vh.AddVert(q,new Color(1,1,1,alpha),new Vector2(u,v));}
   }
   for(int x=0;x<nx;x++)for(int y=0;y<ny;y++){int n=x*(ny+1)+y;vh.AddTriangle(n,n+ny+1,n+1);vh.AddTriangle(n+1,n+ny+1,n+ny+2);}
  }
  protected override void OnDestroy(){base.OnDestroy();if(owned!=null)Destroy(owned);}
 }
}
