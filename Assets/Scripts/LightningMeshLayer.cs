using UnityEngine;using UnityEngine.UI;
namespace DragonTower {
 // Preallocated geometry, pixel-snapped paths, one shared-material UI mesh per depth.
 // No generated raster art or per-spark GameObjects/textures.
 public sealed class LightningMeshLayer:MaskableGraphic {
  const int Capacity=12000;
  // Retain the high-water capacity; no allocations once a layer has warmed up.
  Vector2[] a=new Vector2[256],b=new Vector2[256],c=new Vector2[256],d=new Vector2[256];Color32[] colors=new Color32[256];int submittedCount;
  void EnsureCapacity(int required){if(required<=a.Length)return;int size=Mathf.Min(Capacity,Mathf.Max(required,a.Length*2));System.Array.Resize(ref a,size);System.Array.Resize(ref b,size);System.Array.Resize(ref c,size);System.Array.Resize(ref d,size);System.Array.Resize(ref colors,size);}
  public int Count{get;private set;}public int Overflow{get;private set;}
  public void Begin(){Count=0;Overflow=0;}
  public void End(){if(Count>0||submittedCount>0)SetVerticesDirty();submittedCount=Count;}
  static Vector2 Snap(Vector2 p)=>new Vector2(Mathf.Round(p.x/2)*2,Mathf.Round(p.y/2)*2);
  public void Quad(Vector2 p0,Vector2 p1,Vector2 p2,Vector2 p3,Color col){if(col.a<.005f)return;if(Count>=Capacity){Overflow++;return;}EnsureCapacity(Count+1);a[Count]=Snap(p0);b[Count]=Snap(p1);c[Count]=Snap(p2);d[Count]=Snap(p3);colors[Count++]=col;}
  public void Line(Vector2 p,Vector2 q,float width,Color col){var n=new Vector2(-(q-p).y,(q-p).x).normalized*width*.5f;Quad(p-n,p+n,q+n,q-n,col);}
  public void Diamond(Vector2 p,float w,float h,Color col){Quad(p+Vector2.left*w,p+Vector2.up*h,p+Vector2.right*w,p+Vector2.down*h,col);}
  protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();for(int i=0;i<Count;i++){int n=vh.currentVertCount;vh.AddVert(a[i],colors[i],Vector2.zero);vh.AddVert(b[i],colors[i],Vector2.zero);vh.AddVert(c[i],colors[i],Vector2.zero);vh.AddVert(d[i],colors[i],Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);}}
 }
}
