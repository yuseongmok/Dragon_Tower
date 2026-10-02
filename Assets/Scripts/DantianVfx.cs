using System;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    // One pooled portrait overlay. The existing background is sampled once with a tiny
    // UV displacement; no capture texture, post processing, global time or camera mutation.
    public sealed class DantianVfx : MonoBehaviour
    {
        public Action Cancelled;
        const int Count=38;
        Image[] pool;Image arena,playerImage;Material split,originalMaterial;bool materialApplied;
        RectTransform root,frame;Sprite rift,spark,ring;WindSkillRecipe shared;SkillData skill;
        readonly Vector3[] corners=new Vector3[4];Vector2 source,target;
        readonly Vector2 direction=new Vector2(.447214f,.894427f),normal=new Vector2(-.894427f,.447214f);
        float age=10,impact=-1;
        public bool Configured=>skill!=null&&skill.StableId=="skill_dantian";
        public bool Active=>Configured&&age<skill.protectedCastDuration;
        public bool Rupturing=>Active&&impact>=0;
        public float Age=>age;
        public int Capacity=>Count;
        public RectTransform Overlay=>root;
        public bool Clean=>root==null||!root.gameObject.activeSelf&&!materialApplied;
        public void Initialize(BattleView view)
        {
            frame=view.frame;arena=frame.Find("Arena").GetComponent<Image>();
            split=new Material(Resources.Load<Shader>("VFX/DantianSplit"));
            var atlas=Resources.LoadAll<Sprite>("VFX/Dantian");rift=Array.Find(atlas,s=>s.name=="DantianRift");
            spark=Resources.Load<Sprite>("VFX/GaleSlashImpact");ring=Resources.Load<Sprite>("VFX/GaleSlashShockwave");shared=Resources.Load<WindSkillRecipe>("VFX/GaleStrike");
            var go=new GameObject("Dantian signature overlay (pooled)",typeof(RectTransform),typeof(RectMask2D));root=go.GetComponent<RectTransform>();root.SetParent(frame,false);
            go.AddComponent<DantianOverlayGuard>().owner=this;
            root.anchorMin=root.anchorMax=new Vector2(.5f,1);root.pivot=new Vector2(.5f,1);root.sizeDelta=new Vector2(480,850);root.anchoredPosition=Vector2.zero;
            pool=new Image[Count];for(int i=0;i<Count;i++){var obj=new GameObject(i==0?"Dantian darkening":i==1?"Dantian caster":i<8?"Dantian rift layer":i<14?"Dantian rupture":i<34?"Dantian fragment":"Dantian compression",typeof(RectTransform),typeof(Image));var im=obj.GetComponent<Image>();im.rectTransform.SetParent(root,false);im.rectTransform.anchorMin=im.rectTransform.anchorMax=new Vector2(.5f,1);im.raycastTarget=false;pool[i]=im;obj.SetActive(false);}ClearVisual();
        }
        public void Configure(SkillData data,Graphic caster){Cancel();skill=data;playerImage=caster as Image;}
        public void Cast(Vector2 from,Vector2 to)
        {
            if(!Configured)return;if(!isActiveAndEnabled){Cancelled?.Invoke();return;}ClearVisual();source=from;target=to;age=0;impact=-1;originalMaterial=arena.material;arena.material=split;materialApplied=true;root.SetAsLastSibling();root.gameObject.SetActive(true);Render();
        }
        public void Hit(){if(Active&&impact<0){impact=age;Render();}}
        public void Step(BattleModel battle)
        {
            if(!Active)return;
            if(!battle.ProtectedSkillActive){Cancel();return;}
            age=skill.protectedCastDuration-(float)battle.ProtectedSkillRemaining;Render();
        }
        public void Cancel(){bool wasActive=Active;ClearVisual();if(wasActive)Cancelled?.Invoke();}
        void ClearVisual()
        {
            age=10;impact=-1;if(pool!=null)foreach(var im in pool)if(im!=null)im.gameObject.SetActive(false);
            if(materialApplied&&arena!=null)arena.material=originalMaterial;materialApplied=false;
            if(root!=null)root.gameObject.SetActive(false);
        }
        void OnDisable(){Cancel();}
        void OnDestroy(){Cancel();if(split!=null)Destroy(split);if(root!=null)Destroy(root.gameObject);}
        void Show(int i,Sprite sprite,Vector2 point,Vector2 size,Color color,float angle=0)
        {
            var im=pool[i];im.sprite=sprite;im.color=color;im.rectTransform.anchoredPosition=new Vector2(Mathf.Round(point.x/2)*2,Mathf.Round(point.y/2)*2);im.rectTransform.sizeDelta=size;im.rectTransform.localRotation=Quaternion.Euler(0,0,angle);im.gameObject.SetActive(true);
        }
        void Render()
        {
            foreach(var im in pool)im.gameObject.SetActive(false);if(!Active)return;
            float duration=skill.protectedCastDuration,recovery=Mathf.Clamp01((age-(duration-.30f))/.30f);
            float dark=.68f*Mathf.Clamp01(age/.10f)*(1-recovery),h=impact<0?-1:age-impact;
            if(h>=0)dark*=Mathf.Lerp(1,.60f,Mathf.Clamp01(h/.08f));
            Show(0,null,new Vector2(0,-425),new Vector2(480,850),new Color(.003f,.012f,.028f,dark));
            split.SetVector("_Line",new Vector4(normal.x,normal.y,Vector2.Dot(normal,target+new Vector2(0,425)),0));
            split.SetFloat("_Split",h<0?0:Mathf.Round(4*Mathf.Sin(Mathf.PI*Mathf.Clamp01(h/.30f))));split.SetFloat("_Saturation",1-dark*.65f);
            split.SetVector("_Impulse",h>=0&&h<.12f?new Vector4(Mathf.Round(Mathf.Sin(h*110)*5*(1-h/.12f)),Mathf.Round(2*(1-h/.12f)),0,0):Vector4.zero);
            // Keep the current caster sprite readable above the dimmer during compression.
            if(age<.31f&&playerImage!=null)
            {
                playerImage.rectTransform.GetWorldCorners(corners);var a=frame.InverseTransformPoint(corners[0]);var b=frame.InverseTransformPoint(corners[2]);
                Show(1,playerImage.sprite,new Vector2((a.x+b.x)*.5f,(a.y+b.y)*.5f-frame.rect.yMax),new Vector2(b.x-a.x,b.y-a.y),new Color(1,1,1,Mathf.Clamp01(age/.05f)));pool[1].preserveAspect=true;
            }
            if(age>=.15f&&age<.30f)
            {
                float t=(age-.15f)/.15f;Show(34,spark,source+new Vector2(12,25),Vector2.one*Mathf.Lerp(95,22,t),WindVFXStyle.Alpha(WindVFXStyle.Pale,t));
                for(int i=0;i<16;i++){float a=i*2.39996f;Vector2 radial=new Vector2(Mathf.Cos(a)*215,Mathf.Sin(a)*340);Show(14+i,null,source+Vector2.Lerp(radial,Vector2.zero,t*t),new Vector2(4,10),WindVFXStyle.Alpha(i%3==0?WindVFXStyle.Pale:WindVFXStyle.Cyan,.9f),-a*Mathf.Rad2Deg);}
                for(int i=0;i<3;i++)Show(35+i,shared.Frame(WindModule.WindTrail,t),source+new Vector2((i-1)*50,90*(1-t)),new Vector2(18,140*(1-t)),WindVFXStyle.Alpha(WindVFXStyle.Mint,t),i*70-70);
            }
            if(age>=.30f)
            {
                float angle=Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg;
                float cut=Mathf.Clamp01((age-.30f)/.035f),fade=1-recovery;
                float open=h<0?0:Mathf.Sin(Mathf.PI*Mathf.Clamp01(h/.35f));
                Vector2 center=target;
                // The dark gap appears after the white edge, then opens at confirmed damage.
                if(age>=.32f)Show(2,rift,center,new Vector2(1300,12+open*44),WindVFXStyle.Alpha(new Color(.008f,.025f,.07f),fade),angle);
                if(age>=.315f)Show(3,rift,center+normal*2,new Vector2(1260,9+open*32),WindVFXStyle.Alpha(WindVFXStyle.Teal,fade),angle);
                if(age>=.31f)Show(4,rift,center,new Vector2(1220,6+open*22),WindVFXStyle.Alpha(WindVFXStyle.Cyan,fade),angle);
                Show(5,rift,center-normal*2,new Vector2(1180*cut,4+open*12),WindVFXStyle.Alpha(WindVFXStyle.Mint,fade*(h<0?1:1-Mathf.Clamp01((h-.42f)/.30f))),angle);
                Show(6,rift,center,new Vector2(1180*cut,2+open*5),WindVFXStyle.Alpha(WindVFXStyle.Pale,fade*(h<0?1:1-Mathf.Clamp01((h-.22f)/.30f))),angle);
                if(h<.16f)Show(7,rift,center,new Vector2(1200*cut,1.7f+open*2),WindVFXStyle.Alpha(Color.white,fade),angle);
                if(age<.38f)Show(35,shared.Frame(WindModule.WindTrail,.3f),(source+target)*.5f,new Vector2(25,Vector2.Distance(source,target)),WindVFXStyle.Alpha(WindVFXStyle.Pale,1-(age-.30f)/.08f),-Mathf.Atan2((target-source).x,(target-source).y)*Mathf.Rad2Deg);
            }
            // Between the cut and real hit: only the rift remains. No shake/flash/debris.
            if(h>=0)
            {
                for(int i=0;i<5;i++)
                {
                    float t=h-i*.013f;if(t<0||t>.23f)continue;
                    Vector2 p=target+direction*((i-2)*145);float size=(i==2?240:155)*Mathf.Lerp(.35f,1,Mathf.Clamp01(t/.055f));
                    Show(8+i,spark,p,new Vector2(size,size*.7f),WindVFXStyle.Alpha(i%2==0?WindVFXStyle.Pale:WindVFXStyle.Cyan,1-t/.23f),63);
                }
                if(h<.27f)Show(13,ring,target,Vector2.one*Mathf.Lerp(70,360,Mathf.Clamp01(h/.14f)),WindVFXStyle.Alpha(WindVFXStyle.Teal,1-h/.27f));
                // Delayed energy jets use the same five pooled rupture slots. No second hit.
                if(h>=.25f&&h<.48f)
                {
                    float t=(h-.25f)/.23f;
                    for(int i=0;i<5;i++)
                    {
                        float side=i%2==0?1:-1;Vector2 p=target+direction*((i-2)*150)+normal*(side*(18+t*100));
                        Show(8+i,shared.Frame(WindModule.WindTrail,t),p,new Vector2(60*(1-t*.6f),230*(1-t*.3f)),WindVFXStyle.Alpha(i%2==0?WindVFXStyle.Mint:WindVFXStyle.Cyan,(1-t)*.85f),side>0?63:243);
                    }
                }
                for(int i=0;i<20;i++)
                {
                    float start=i%4*.025f,t=(h-start)/(.50f+(i%3)*.10f);if(t<0||t>=1)continue;
                    float side=i%2==0?1:-1;Vector2 p=target+direction*((i-9.5f)*40)+normal*(side*(12+t*(60+i%4*35)))+Vector2.down*(t*t*25);
                    float size=i%5==0?11:5+i%3*2;Show(14+i,null,p,new Vector2(size,size*2)*(1-t*.4f),WindVFXStyle.Alpha(i%5==0?WindVFXStyle.Pale:i%2==0?WindVFXStyle.Mint:WindVFXStyle.Cyan,(1-t)*(1-recovery)),63+side*t*70);
                }
            }
        }
    }
    public sealed class DantianOverlayGuard : MonoBehaviour
    {
        public DantianVfx owner;
        void OnDisable(){if(owner!=null&&owner.Active)owner.Cancel();}
    }
}
