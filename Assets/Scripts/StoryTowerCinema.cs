using UnityEngine;
using UnityEngine.UI;
namespace DragonTower {
 // Whole-illustration keyframes: preserves the story art instead of overlaying battle sprites.
 public sealed class StoryTowerCinema:MonoBehaviour {
  public float Duration=>duration;float duration;
  int fixedPose=-1;Image lower,upper;Sprite[] frames;
  Image Layer(Transform parent,string label){
   var r=new GameObject(label,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);
   r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(0,-425);r.sizeDelta=new Vector2(480,850);
   var i=r.gameObject.AddComponent<Image>();i.raycastTarget=false;return i;
  }
  public void Build(RectTransform parent,bool building){Build(parent,building?4:5);}
  public void Build(RectTransform parent,int page){
   string path=page==4?"tower-construction-illustrated":page==6?"corruption":page==7?"guardians":page==8?"promise":"contract-illustrated";Build(parent,path,page==4||page==6?8f:10f);
  }
  public void Build(RectTransform parent,string path,float seconds,int pose=-1){
   duration=seconds;fixedPose=pose;var t=Resources.Load<Texture2D>("Story/"+path);
   lower=Layer(parent,"Story painted keyframe");upper=Layer(parent,"Story painted transition");
   if(t==null){Debug.LogError("Missing illustrated story atlas");return;}
   frames=new Sprite[4];float w=t.width/2f,h=t.height/2f;
   for(int n=0;n<4;n++)frames[n]=Sprite.Create(t,new Rect((n%2)*w+6,(1-n/2)*h+6,w-12,h-12),new Vector2(.5f,.5f),100);
   // Cover without stretching if the generated atlas has slight aspect variation.
   float width=Mathf.Max(480,850*w/h),height=Mathf.Max(850,480*h/w);
   lower.rectTransform.sizeDelta=upper.rectTransform.sizeDelta=new Vector2(width,height);
   Render(0);
  }
  public void Render(float seconds){
   if(frames==null)return;
   float p=fixedPose>=0?fixedPose:Mathf.Clamp01(seconds/Mathf.Max(.1f,Duration-1.2f))*3;int a=Mathf.Min(3,Mathf.FloorToInt(p)),b=Mathf.Min(3,a+1);
   lower.sprite=frames[a];upper.sprite=frames[b];
   // Holds each illustration before a brief dissolve into the small pose change.
   upper.color=new Color(1,1,1,Mathf.SmoothStep(0,1,Mathf.InverseLerp(.58f,1,p-a)));
  }
  void OnDestroy(){if(frames!=null)foreach(var s in frames)if(s!=null)Destroy(s);}
 }
}
