using UnityEngine;using UnityEngine.UI;
namespace DragonTower {
 public sealed class HydraHeadLayer:MaskableGraphic {
  readonly Vector2[] centers=new Vector2[3];readonly float[] sizes=new float[3],angles=new float[3],jaws=new float[3],alphas=new float[3];Material owned;Texture2D atlas;float age;
  public override Texture mainTexture=>atlas;
  public void Initialize(BattleView view){atlas=Resources.Load<Texture2D>("Hydra/HeadParts");owned=new Material(Resources.Load<Shader>("Hydra/HeadFlow"));material=owned;raycastTarget=false;rectTransform.SetParent(view.frame,false);rectTransform.anchorMin=rectTransform.anchorMax=rectTransform.pivot=Vector2.one*.5f;rectTransform.sizeDelta=new Vector2(480,850);BattleVfxHudVisibility.BringHudForward(view);}
  public void SetRift(Vector2 center,float phase){owned.SetVector("_RiftCenter",center);owned.SetFloat("_Phase",phase);}
  public void Begin(float time){age=time;for(int i=0;i<3;i++)alphas[i]=0;}
  public void Head(int i,Vector2 center,float size,float angle,float jaw,float alpha){centers[i]=center;sizes[i]=size;angles[i]=angle;jaws[i]=jaw;alphas[i]=alpha;}
  public void End(){owned.SetFloat("_FlowTime",age);SetVerticesDirty();}public void Clear(){for(int i=0;i<3;i++)alphas[i]=0;SetVerticesDirty();}
  protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();if(atlas==null)return;for(int j=0;j<3;j++){int i=j==2?0:j+1;if(alphas[i]<=0)continue;bool flip=i==2;float angle=angles[i];Part(vh,i,new Rect(0,0,1223,486),new Vector2(610,186),angle+(flip?1:-1)*jaws[i],new Vector2(0,-8),flip);Part(vh,i,new Rect(0,511,1223,775),new Vector2(610,205),angle,Vector2.zero,flip);}}
  void Part(VertexHelper vh,int i,Rect rect,Vector2 pivot,float angle,Vector2 offset,bool flip){int start=vh.currentVertCount;float scale=sizes[i]/470;for(int k=0;k<4;k++){float x=k==0||k==3?0:rect.width,y=k<2?0:rect.height;var v=(new Vector2(x,y)-pivot)*scale;v.x*=flip?-1:1;float c=Mathf.Cos(angle),s=Mathf.Sin(angle);var q=centers[i]+offset+new Vector2(v.x*c-v.y*s,v.x*s+v.y*c);q=new Vector2(Mathf.Round(q.x/2)*2,Mathf.Round(q.y/2)*2);vh.AddVert(q,new Color(1,1,1,alphas[i]),new Vector2((rect.x+x)/1223f,(rect.y+y)/1286f));}vh.AddTriangle(start,start+1,start+2);vh.AddTriangle(start,start+2,start+3);}
  protected override void OnDestroy(){base.OnDestroy();if(owned!=null)Destroy(owned);}
 }
}
