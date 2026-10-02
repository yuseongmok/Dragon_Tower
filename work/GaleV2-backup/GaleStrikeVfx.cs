using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    // Presentation only: a fixed set of atlas-backed images, with no per-cast object creation.
    public sealed class GaleStrikeVfx : MonoBehaviour
    {
        WindSkillRecipe recipe;Image[] pool;RectTransform root;
        Vector2 source,target;float age=10;bool enabledForSkill;
        public bool Active=>enabledForSkill&&recipe!=null&&age<recipe.duration;
        public int Capacity=>pool==null?0:pool.Length;
        public int ActiveCount {get{int n=0;if(pool!=null)foreach(var image in pool)if(image.gameObject.activeSelf)n++;return n;}}
        public bool FinalFreeze=>Active&&age>=recipe.finalImpact&&age<recipe.finalImpact+.035f;
        public float HitFlash {get{
            if(!Active)return 0;
            float final=age-recipe.finalImpact;if(final>=0&&final<.095f)return 1-final/.095f;
            if(age>.28f&&age<.79f)return Mathf.Repeat(age-.28f,.14f)<.035f?.42f:0;
            return 0;
        }}
        public Vector2 EnemyOffset {get{
            if(!Active)return Vector2.zero;float t=age-recipe.finalImpact;
            if(t>=0&&t<.16f)return new Vector2(Mathf.Sin(t*85)*3,Mathf.Sin(t/.16f*Mathf.PI)*7);
            if(age>.28f&&age<.79f)return new Vector2(Mathf.Round(Mathf.Sin(age*90)*2),0);
            return Vector2.zero;
        }}
        public void Initialize(RectTransform parent)
        {
            recipe=Resources.Load<WindSkillRecipe>("VFX/GaleStrike");if(recipe==null)return;
            var go=new GameObject("Gale Strike modules (pooled)",typeof(RectTransform),typeof(RectMask2D));
            root=go.GetComponent<RectTransform>();root.SetParent(parent,false);root.anchorMin=root.anchorMax=new Vector2(.5f,1);root.pivot=new Vector2(.5f,1);root.anchoredPosition=new Vector2(0,-132);root.sizeDelta=new Vector2(480,518);
            pool=new Image[recipe.clips.Length];
            for(int i=0;i<pool.Length;i++)
            {
                var child=new GameObject(recipe.clips[i].module.ToString(),typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
                var image=child.GetComponent<Image>();var r=image.rectTransform;r.SetParent(root,false);r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.pivot=Vector2.one*.5f;
                image.raycastTarget=false;pool[i]=image;child.SetActive(false);
            }
        }
        public void Configure(bool zephyr,SkillData skill){enabledForSkill=zephyr&&recipe!=null&&skill!=null&&skill.StableId==recipe.skillId;Clear();}
        public void Clear(){age=10;if(pool!=null)foreach(var image in pool)image.gameObject.SetActive(false);}
        public bool Play(Vector2 from,Vector2 to){if(!enabledForSkill||recipe==null)return false;Clear();source=from;target=to;age=0;return true;}
        public void Step(float dt)
        {
            if(!Active)return;age+=dt;
            for(int i=0;i<pool.Length;i++)
            {
                var clip=recipe.clips[i];var image=pool[i];float t=(age-clip.start)/clip.duration;
                bool visible=age<recipe.duration&&t>=0&&t<1;image.gameObject.SetActive(visible);if(!visible)continue;
                float frame=clip.loop?Mathf.Repeat((age-clip.start)*1.8f,1):t;
                image.sprite=recipe.Frame(clip.module,frame);
                Vector2 at=Vector2.LerpUnclamped(source,target,Mathf.Lerp(clip.anchor,clip.endAnchor,t))+clip.offset+new Vector2(0,132);
                image.rectTransform.anchoredPosition=new Vector2(Mathf.Round(at.x/2)*2,Mathf.Round(at.y/2)*2);
                image.rectTransform.sizeDelta=clip.size*Mathf.Lerp(clip.startScale,clip.endScale,t);
                float fade=Mathf.Min(1,(1-t)*5);image.color=new Color(1,1,1,clip.opacity*fade);
            }
        }
    }
}
