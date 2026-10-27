using UnityEngine;using UnityEngine.UI;
namespace DragonTower {
 // Uses the existing Mir legendary dragon texture. Two articulated apparitions, no cloned skills or damage callbacks.
 public sealed class PrimordialDragonApparition:MaskableGraphic {
  Texture2D tex;Material owned;Vector2 center;float clock,opacity,span;bool dual;
  public override Texture mainTexture=>tex;
  public void Initialize(RectTransform parent){tex=Resources.Load<Texture2D>("MirLegendary/Dragon");owned=new Material(Resources.Load<Shader>("VFX/ContractDragon"));material=owned;raycastTarget=false;rectTransform.SetParent(parent,false);rectTransform.anchorMin=rectTransform.anchorMax=rectTransform.pivot=new Vector2(.5f,1);rectTransform.sizeDelta=new Vector2(480,850);}
  public void Show(Vector2 p,float time,float alpha,bool two,float size=1){center=p;clock=time;opacity=alpha;dual=two;span=size;SetVerticesDirty();}
  public void Clear(){opacity=0;SetVerticesDirty();}
  Vector2 Point(float u,int side){float a=Mathf.Lerp(-2.6f,.35f,u);return center+new Vector2(side*(130-Mathf.Cos(a)*105)*span,Mathf.Sin(a)*102*span+Mathf.Sin(u*7+clock*1.5f)*8);}
  protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();if(opacity<=0)return;int count=dual?2:1;for(int k=0;k<count;k++){int side=k==0?-1:1,start=vh.currentVertCount;const int nx=64,ny=6;for(int x=0;x<=nx;x++){float u=x/(float)nx;var p=Point(u,side);var tangent=(Point(u+.005f,side)-Point(u-.005f,side)).normalized;var n=new Vector2(-tangent.y,tangent.x)*side;for(int y=0;y<=ny;y++){float v=y/(float)ny;var q=p+n*((v-.39f)*150*span);q=new Vector2(Mathf.Round(q.x/2)*2,Mathf.Round(q.y/2)*2);vh.AddVert(q,new Color(k==0?1:0,1,1,opacity),new Vector2(u,v));}}for(int x=0;x<nx;x++)for(int y=0;y<ny;y++){int n=start+x*(ny+1)+y;vh.AddTriangle(n,n+ny+1,n+1);vh.AddTriangle(n+1,n+ny+1,n+ny+2);}}}
  protected override void OnDestroy(){base.OnDestroy();if(owned!=null)Destroy(owned);}
 }
}
