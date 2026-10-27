using UnityEngine;
using UnityEngine.UI;
namespace DragonTower {
// Opening bounds follow the painted frame; the atlas contributes only door material.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class TowerDoorAperture : MaskableGraphic {
 public float arch;
 static readonly Rect[] normal={new Rect(65,89,54,91),new Rect(182,89,55,94),new Rect(59,84,51,82),new Rect(187,85,51,82),new Rect(65,101,47,70),new Rect(186,101,47,70),new Rect(63,109,48,83),new Rect(192,109,47,83),new Rect(56,113,44,60),new Rect(203,113,45,61),new Rect(71,96,44,100),new Rect(189,96,44,100),new Rect(71,81,49,83),new Rect(194,80,52,84),new Rect(64,111,48,80),new Rect(188,112,48,80),new Rect(61,94,43,80),new Rect(195,90,46,88),new Rect(59,123,51,88),new Rect(191,123,52,88)};
 static readonly Rect[] bosses={new Rect(114,78,69,139),new Rect(108,78,79,139),new Rect(111,78,77,139),new Rect(111,100,77,128),new Rect(113,102,77,134),new Rect(114,105,71,125),new Rect(114,92,71,127),new Rect(113,107,74,120),new Rect(112,105,73,133),new Rect(111,111,76,126)};
 public static Rect Layout(int floor,bool boss,int choice){int z=Mathf.Clamp((floor-1)/10,0,9);return boss?bosses[z]:normal[z*2+Mathf.Clamp(choice,0,1)];}
 protected override void OnPopulateMesh(VertexHelper v){
  v.Clear();var r=rectTransform.rect;v.AddVert(Vector3.zero,color,Vector2.zero);
  var points=new System.Collections.Generic.List<Vector2>();points.Add(new Vector2(r.xMin,r.yMin));points.Add(new Vector2(r.xMax,r.yMin));
  if(arch<=0){points.Add(new Vector2(r.xMax,r.yMax));points.Add(new Vector2(r.xMin,r.yMax));}
  else for(int i=0;i<=24;i++){float a=i*Mathf.PI/24;points.Add(new Vector2(Mathf.Cos(a)*r.width*.5f,r.yMax-r.height*arch+Mathf.Sin(a)*r.height*arch));}
  for(int i=0;i<points.Count;i++)v.AddVert(points[i],color,Vector2.zero);
  for(int i=0;i<points.Count;i++)v.AddTriangle(0,i+1,(i+1)%points.Count+1);
 }
}}


