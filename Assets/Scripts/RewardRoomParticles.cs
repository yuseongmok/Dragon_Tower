using UnityEngine;using UnityEngine.UI;
namespace DragonTower {
[RequireComponent(typeof(CanvasRenderer))]public sealed class RewardRoomParticles:MaskableGraphic {
 public float age,power=1;public Color tint=new Color(1,.72f,.25f);public bool ambient;
 void Update(){if(ambient)age+=Time.unscaledDeltaTime;SetVerticesDirty();}
 protected override void OnPopulateMesh(VertexHelper v){v.Clear();var r=rectTransform.rect;if(ambient){for(int i=0;i<18;i++){float x=Mathf.Sin(i*31.3f)*r.width*.45f,y=Mathf.Repeat(i*47+age*(7+i%4),r.height)-r.height*.5f;Quad(v,x,y,2,2,new Color(1,.78f,.4f,.12f+.12f*Mathf.Sin(age+i)));}return;}
 float light=Mathf.SmoothStep(0,1,Mathf.Clamp01((age-.35f)/.45f));for(int i=0;i<9;i++){float angle=(i-4)*.12f+Mathf.Sin(age+i)*.025f;Vector2 a=new Vector2(-5,-5),b=new Vector2(5,-5),tip=new Vector2(Mathf.Sin(angle)*160,170);int n=v.currentVertCount;Color c=tint;c.a=light*.28f*power;v.AddVert(a,c,Vector2.zero);v.AddVert(b,c,Vector2.zero);c.a=0;v.AddVert(tip+Vector2.right*15,c,Vector2.zero);v.AddVert(tip-Vector2.right*15,c,Vector2.zero);v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);}
 for(int i=0;i<24*power;i++){float t=Mathf.Clamp01((age-.42f-i*.004f)/1.2f),a=i*2.4f;float x=Mathf.Cos(a)*t*135,y=-5+Mathf.Sin(a)*t*75+t*65;Color c=tint;c.a=light*(1-t);Quad(v,x,y,2+i%3,2+i%3,c);}
 if(power>1.5f){float radius=Mathf.Clamp01((age-.7f)/.65f)*165;for(int i=0;i<48;i++){float a=i*Mathf.PI/24;Color c=tint;c.a=Mathf.Clamp01(1-(age-.7f)/.65f)*light*.6f;Quad(v,Mathf.Cos(a)*radius,Mathf.Sin(a)*radius*.3f-5,4,2,c);}}
 }
 static void Quad(VertexHelper v,float x,float y,float w,float h,Color c){int n=v.currentVertCount;v.AddVert(new Vector3(x,y),c,Vector2.zero);v.AddVert(new Vector3(x,y+h),c,Vector2.zero);v.AddVert(new Vector3(x+w,y+h),c,Vector2.zero);v.AddVert(new Vector3(x+w,y),c,Vector2.zero);v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);}
}}
