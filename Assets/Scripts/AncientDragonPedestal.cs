using UnityEngine;using UnityEngine.UI;
namespace DragonTower {
 [RequireComponent(typeof(CanvasRenderer))]
 public sealed class AncientDragonPedestal:MaskableGraphic {
  protected override void OnPopulateMesh(VertexHelper v){v.Clear();var r=rectTransform.rect;Ellipse(v,r.center+new Vector2(0,-5),r.width*.5f,r.height*.42f,AncientUi.Hex(0x141518));Ellipse(v,r.center,r.width*.48f,r.height*.43f,AncientUi.DarkGold);Ellipse(v,r.center+new Vector2(0,3),r.width*.46f,r.height*.36f,AncientUi.Stone);Ellipse(v,r.center+new Vector2(0,5),r.width*.38f,r.height*.25f,AncientUi.Background);}
  static void Ellipse(VertexHelper v,Vector2 p,float rx,float ry,Color c){for(float y=-ry;y<ry;y+=2){float width=Mathf.Floor(rx*Mathf.Sqrt(Mathf.Max(0,1-y*y/(ry*ry)))/2)*2;int n=v.currentVertCount;v.AddVert(new Vector3(p.x-width,p.y+y),c,Vector2.zero);v.AddVert(new Vector3(p.x-width,p.y+y+2),c,Vector2.zero);v.AddVert(new Vector3(p.x+width,p.y+y+2),c,Vector2.zero);v.AddVert(new Vector3(p.x+width,p.y+y),c,Vector2.zero);v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);}}
 }
}
