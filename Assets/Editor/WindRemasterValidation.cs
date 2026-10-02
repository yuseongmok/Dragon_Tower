using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower.Editor
{
    // Run alongside the existing real-combat damage/cooldown/repeated-cast validations.
    public static class WindRemasterValidation
    {
        static void Check(bool ok,string text){if(!ok)throw new Exception("WIND_STYLE: "+text);UnityEngine.Debug.Log("WIND_STYLE_CHECK "+text);}
        public static void Validate(BattleView view,DragonData dragon)
        {
            var atlas=WindVFXStyle.Load();Check(atlas.Length==30,"All thirty shared slices load");
            foreach(var sprite in atlas)Check(sprite.texture==atlas[0].texture&&sprite.texture.filterMode==FilterMode.Point,"One point-filtered texture");
            var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(atlas[0]));
            Check(!importer.mipmapEnabled&&atlas[0].texture.width==512&&atlas[0].texture.height==384,"Bounded atlas without mipmaps");
            var common=view.GetComponent<GaleStrikeVfx>();var blade=view.GetComponent<WindBladeVfx>();var slash=view.GetComponent<GaleSlashVfx>();
            Vector2 from=new Vector2(-26,-532),to=new Vector2(30,-264);
            SkillData Skill(string id)=>AssetDatabase.LoadAssetAtPath<SkillData>("Assets/Data/Skills/"+id+".asset");
            common.Configure(true,dragon.skill);
            Measure("Common","Gale Strike modules (pooled)",()=>{common.Clear();common.Play(from,to,new Vector2(240,200));},common.Step,common.Clear);
            var data=Skill("skill_falling_flower");blade.Configure(data);float age=0;int hit=0;
            Measure("Blade","Wind Blade modules (pooled)",()=>{age=0;hit=0;blade.Cast(from,to);},dt=>{age+=dt;blade.Step(dt);if(hit<3&&age>=data.initialHitDelay+hit*data.hitInterval){blade.Hit();hit++;}},blade.Clear);
            data=Skill("skill_gale_slash");slash.Configure(data);
            Measure("Slash","Gale Slash pooled visual layer",()=>{age=0;hit=0;slash.Cast(from,to);},dt=>{age+=dt;slash.Step(dt);if(hit==0&&age>=data.initialHitDelay){slash.Hit();hit++;}},slash.Clear);
            view.SetDragonArt(dragon);UnityEngine.Debug.Log("WIND_REMASTER_VALIDATION_OK");
            void Measure(string label,string rootName,Action cast,Action<float> step,Action clear)
            {
                Transform root=null;foreach(var t in view.frame.GetComponentsInChildren<Transform>(true))if(t.name==rootName)root=t;
                Check(root!=null,label+" visual root exists");var images=root.GetComponentsInChildren<Image>(true);int count=images.Length,peak=0;
                foreach(var image in images)Check(!image.raycastTarget&&image.material==Graphic.defaultGraphicMaterial,label+" shares standard non-additive UI material");
                for(int n=0;n<20;n++){cast();for(int f=0;f<45;f++)step(1f/60);clear();}
                var watch=new Stopwatch();long before=GC.GetAllocatedBytesForCurrentThread();watch.Start();
                // Presentation CPU/managed allocation only, not GPU/browser/device profiling.
                for(int n=0;n<100;n++)
                {
                    cast();for(int f=0;f<45;f++){step(1f/60);int active=0;foreach(var image in images)if(image.gameObject.activeSelf)active++;peak=Math.Max(peak,active);}clear();
                }
                watch.Stop();long bytes=GC.GetAllocatedBytesForCurrentThread()-before;
                Check(peak>0,label+" workload actually rendered sprites");
                Check(root.GetComponentsInChildren<Image>(true).Length==count,label+" pool never grows");
                foreach(var image in images)Check(!image.gameObject.activeSelf,label+" cleanup leaves no sprite");
                UnityEngine.Debug.Log($"WIND_STYLE_BUDGET {label}: pool={count}, peak={peak}, measuredBytes={bytes}, 4500StepsMs={watch.Elapsed.TotalMilliseconds:F2}");
            }
        }
    }
}
