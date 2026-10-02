using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    // T=0 is the existing damage cue. This component never applies damage or moves actors.
    public sealed class GaleStrikeVfx : MonoBehaviour
    {
        WindSkillRecipe recipe;Image[] pool;RectTransform root,stormRoot;
        Vector2 source,target;float age=10,width,height;bool enabledForSkill;
        const float End=.600f;
        public bool Active=>enabledForSkill&&recipe!=null&&age<End;
        public int Capacity=>pool==null?0:pool.Length;
        public int ActiveCount {get{int n=0;if(pool!=null)foreach(var image in pool)if(image.gameObject.activeSelf)n++;return n;}}
        public void Initialize(RectTransform parent)
        {
            recipe=Resources.Load<WindSkillRecipe>("VFX/GaleStrike");if(recipe==null)return;
            root=Root("Gale Strike modules (pooled)",parent,132,518);
            stormRoot=Root("Storm safe area",root,0,228);
            pool=new Image[16];
            for(int i=0;i<pool.Length;i++)
            {
                var child=new GameObject(i<6?"Wind module "+i:"Residual pixel "+(i-6),typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
                var image=child.GetComponent<Image>();var r=image.rectTransform;r.SetParent(i==0?root:stormRoot,false);r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.pivot=Vector2.one*.5f;
                image.raycastTarget=false;pool[i]=image;child.SetActive(false);
            }
        }
        static RectTransform Root(string name,Transform parent,float top,float height)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(RectMask2D));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.pivot=new Vector2(.5f,1);r.anchoredPosition=new Vector2(0,-top);r.sizeDelta=new Vector2(480,height);return r;
        }
        public void Configure(bool zephyr,SkillData skill){enabledForSkill=zephyr&&recipe!=null&&skill!=null&&skill.StableId==recipe.skillId;Clear();}
        public void Clear(){age=10;if(pool!=null)foreach(var image in pool)image.gameObject.SetActive(false);}
        void OnDisable(){Clear();}
        public bool Play(Vector2 from,Vector2 to,Vector2 silhouette)
        {
            if(!enabledForSkill||recipe==null)return false;
            Clear();source=from;target=to;width=Mathf.Min(silhouette.x*1.2f,288);
            // Fit the existing body center inside the enemy-only area; never cover attack warnings.
            height=Mathf.Max(16,Mathf.Min(silhouette.y*1.25f,2*Mathf.Min(-to.y-132,360+to.y)));
            age=0;Render();return true;
        }
        public void Step(float dt){if(!Active)return;age+=Mathf.Max(0,dt);if(age>=End-.00001f){Clear();return;}Render();}
        void Draw(int index,WindModule module,Vector2 at,Vector2 size,float opacity,int frame=0,float rotation=0)
        {
            var image=pool[index];image.sprite=recipe.Frame(module,frame/8f);image.color=new Color(1,1,1,opacity);
            var r=image.rectTransform;r.anchoredPosition=new Vector2(Mathf.Round(at.x/2)*2,Mathf.Round((at.y+132)/2)*2);r.sizeDelta=size;r.localRotation=Quaternion.Euler(0,0,rotation);image.gameObject.SetActive(true);
        }
        void Render()
        {
            for(int i=0;i<pool.Length;i++)pool[i].gameObject.SetActive(false);
            if(age<.067f)
            {
                Vector2 path=target-source;
                Draw(0,WindModule.WindTrail,(source+target)*.5f,new Vector2(165,path.magnitude/1.84f*2),1-age/.067f,0,-Mathf.Atan2(path.x,path.y)*Mathf.Rad2Deg);
                float flash=age<.033f?1:1-(age-.033f)/.034f;
                Draw(1,WindModule.WindHitSpark,target,Vector2.one*(width/1.2f*.40f/.54f),flash);
            }
            if(age>=.033f&&age<.333f)
            {
                float grow=Mathf.Lerp(.7f,1,Mathf.Clamp01((age-.033f)/.067f));
                float compress=Mathf.Clamp01((age-.267f)/.066f);
                Vector2 size=new Vector2(width*grow*Mathf.Lerp(1,.75f,compress),height*grow*Mathf.Lerp(1,.85f,compress));
                // Scale about the foot instead of making the funnel slide around the target.
                Vector2 center=target+new Vector2(0,(size.y-height)*.5f);
                int frame=((int)((age-.033f)*15))%8;
                Draw(2,WindModule.TornadoOuter,center,size,1,frame);
                Draw(3,WindModule.TornadoCore,center,new Vector2(size.x*.52f,size.y),.85f,frame);
            }
            if(age>=.333f&&age<.467f)
            {
                float t=age-.333f;
                if(t<.033f)Draw(4,WindModule.WindHitSpark,target,Vector2.one*width*.48f,1);
                float scale=Mathf.Lerp(.45f,1,Mathf.Clamp01(t/.067f));
                Draw(5,WindModule.WindShockwave,target,Vector2.one*(width*1.1f*scale/.9f),t<.067f?1:1-(t-.067f)/.067f);
            }
            if(age>=.400f)
            {
                float t=(age-.4f)/.2f;
                for(int i=0;i<10;i++)
                {
                    float angle=(20+i*15.5f)*Mathf.Deg2Rad;
                    Vector2 velocity=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                    var image=pool[i+6];image.sprite=null;image.color=new Color(.28f,.95f,.82f,1-t);
                    image.rectTransform.anchoredPosition=target+new Vector2((i-4.5f)*width*.045f,132)+velocity*(12+t*42);
                    image.rectTransform.anchoredPosition=new Vector2(Mathf.Round(image.rectTransform.anchoredPosition.x/2)*2,Mathf.Round(image.rectTransform.anchoredPosition.y/2)*2);
                    image.rectTransform.sizeDelta=Vector2.one*(i%3==0?4:2);image.gameObject.SetActive(true);
                }
            }
        }
    }
}

