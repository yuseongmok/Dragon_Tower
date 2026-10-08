using UnityEngine;using UnityEngine.UI;using UnityEngine.Sprites;
namespace DragonTower {
 // Deforms a copy of the arena texture only. Actor/HUD transforms and camera are untouched.
 public sealed class EclipseSpaceLayer:MaskableGraphic {
  Sprite source;float time,power,fracture=-1;Vector2 center;
  public override Texture mainTexture=>source==null?Texture2D.whiteTexture:source.texture;
  public void Bind(Image arena){source=arena.sprite;raycastTarget=false;var a=arena.rectTransform;rectTransform.SetParent(a.parent,false);rectTransform.anchorMin=a.anchorMin;rectTransform.anchorMax=a.anchorMax;rectTransform.pivot=a.pivot;rectTransform.anchoredPosition=a.anchoredPosition;rectTransform.sizeDelta=a.sizeDelta;rectTransform.localScale=a.localScale;rectTransform.SetSiblingIndex(a.GetSiblingIndex()+1);}
  public void Draw(Vector2 localCenter,float age,float strength,float shatter){center=localCenter;time=age;power=strength;fracture=shatter;enabled=strength>.001f||shatter>=0;SetVerticesDirty();}
  Vector2 Warp(Vector2 q){var d=q-center;float radius=d.magnitude;float influence=Mathf.Exp(-radius/245f);float bend=power*influence*(.12f+Mathf.Sin(radius*.023f-time*3)*.028f);return center+d*(1-bend)+new Vector2(-d.y,d.x)*bend*.62f;}
  protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();if(source==null)return;Rect r=rectTransform.rect;Vector4 uv=DataUtility.GetOuterUV(source);int cols=fracture<0?24:7,rows=fracture<0?40:11;
   for(int y=0;y<rows;y++)for(int x=0;x<cols;x++){int id=y*cols+x;float u=x/(float)cols,v=y/(float)rows,du=1f/cols,dv=1f/rows;var a=new Vector2(r.xMin+u*r.width,r.yMin+v*r.height);var b=a+new Vector2(du*r.width,0);var c=b+new Vector2(0,dv*r.height);var d=a+new Vector2(0,dv*r.height);var tint=Color.Lerp(Color.white,new Color(.45f,.32f,.68f,1),power*.55f);
    if(fracture<0){a=Warp(a);b=Warp(b);c=Warp(c);d=Warp(d);}else{float t=fracture,depth=.6f+(id%4)*.42f;var mid=(a+c)*.5f;var dir=(mid-center).normalized;bool inward=id%3==0;Vector2 move=dir*(inward?-1:1)*(t*100+t*t*250)*depth;float size=inward?Mathf.Max(0,1-t*1.8f):1+t*.25f*depth;float angle=t*(id%2==0?1:-1)*depth;var rot=Quaternion.Euler(0,0,angle*35);a=mid+(Vector2)(rot*((a-mid)*size))+move;b=mid+(Vector2)(rot*((b-mid)*size))+move;c=mid+(Vector2)(rot*((c-mid)*size))+move;d=mid+(Vector2)(rot*((d-mid)*size))+move;tint=Color.Lerp(new Color(.55f,.27f,.85f),Color.white,t*.4f);tint.a=Mathf.Clamp01((1.1f-t)/.8f);}
    int i=vh.currentVertCount;vh.AddVert(a,tint,new Vector2(Mathf.Lerp(uv.x,uv.z,u),Mathf.Lerp(uv.y,uv.w,v)));vh.AddVert(b,tint,new Vector2(Mathf.Lerp(uv.x,uv.z,u+du),Mathf.Lerp(uv.y,uv.w,v)));vh.AddVert(c,tint,new Vector2(Mathf.Lerp(uv.x,uv.z,u+du),Mathf.Lerp(uv.y,uv.w,v+dv)));vh.AddVert(d,tint,new Vector2(Mathf.Lerp(uv.x,uv.z,u),Mathf.Lerp(uv.y,uv.w,v+dv)));vh.AddTriangle(i,i+1,i+2);if(fracture<0||id%4!=0)vh.AddTriangle(i,i+2,i+3);
   }
  }
 }
}
