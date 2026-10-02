using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class EvolutionSpriteSetup
    {
        public static void Install(DragonData dragon,int stage,string Art,string SetPath,string spritePrefix,string displayName,DragonAnimationArchetype archetype,DragonAnimationSet timingTemplate=null,bool idleOnly=false,float visualWidth=0,bool prepareAnchors=false)
        {
            if(dragon==null||stage<0||stage>2)throw new InvalidOperationException("Invalid evolution target");
            var set=AssetDatabase.LoadAssetAtPath<DragonAnimationSet>(SetPath);
            if(set==null){set=ScriptableObject.CreateInstance<DragonAnimationSet>();AssetDatabase.CreateAsset(set,SetPath);}
            set.archetype=archetype;
            var source=timingTemplate!=null?timingTemplate:dragon.LoadAnimationSet(0);
            if(source==null||source.idle==null)throw new InvalidOperationException("A validated timing/ground template is required");float idleWidth=0;
            Vector2 targetFoot=Foot(source.idle.frames[0])/source.idle.frames[0].rect.size;
            foreach(DragonAnimationState state in Enum.GetValues(typeof(DragonAnimationState)))
            {
                if(idleOnly&&state!=DragonAnimationState.Idle)continue;
                string path=Art+"/"+state+".png";var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                if(importer==null)throw new InvalidOperationException("Missing sheet: "+path);
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
                importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;
                importer.textureCompression=TextureImporterCompression.Uncompressed;importer.npotScale=TextureImporterNPOTScale.None;
                importer.spritePixelsPerUnit=100;importer.isReadable=true;importer.maxTextureSize=512;
                importer.GetSourceTextureWidthAndHeight(out int w,out int h);
                var slices=new SpriteMetaData[4];
                for(int i=0;i<4;i++)slices[i]=new SpriteMetaData{name=spritePrefix+"_"+state+"_"+i,rect=new Rect(i%2*w/2,(1-i/2)*h/2,w/2,h/2),pivot=new Vector2(.5f,.5f),alignment=0};
                #pragma warning disable 618
                importer.spritesheet=slices;
                #pragma warning restore 618
                importer.SaveAndReimport();
                var frames=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
                if(frames.Length!=4)throw new InvalidOperationException("Expected four frames: "+state);
                string clipPath=Art+"/"+state+".asset";var clip=AssetDatabase.LoadAssetAtPath<DragonIdleFrames>(clipPath);
                if(clip==null){clip=ScriptableObject.CreateInstance<DragonIdleFrames>();AssetDatabase.CreateAsset(clip,clipPath);}
                clip.frames=frames;clip.durations=(float[])source.Get(state).durations.Clone();clip.offsets=new Vector2[4];
                // Normalize sheet whitespace, not dragon growth. Idle stays at the existing 256px actor size.
                float width=OpaqueBounds(frames[state==DragonAnimationState.Death?0:3]).width/frames[0].rect.width;
                if(state==DragonAnimationState.Idle)idleWidth=OpaqueBounds(frames[0]).width/frames[0].rect.width;
                float targetWidth=visualWidth>0?visualWidth/256f:idleWidth;
                clip.displayScale=state==DragonAnimationState.Idle?(visualWidth>0?targetWidth/idleWidth:1):targetWidth/width;
                for(int i=0;i<4;i++)
                {
                    var foot=Foot(frames[i])/frames[i].rect.size;
                    clip.offsets[i]=targetFoot-Vector2.one*.5f-(foot-Vector2.one*.5f)*clip.displayScale;
                }
                switch(state){case DragonAnimationState.Idle:set.idle=clip;break;case DragonAnimationState.Attack:set.attack=clip;break;case DragonAnimationState.Skill:set.skill=clip;break;case DragonAnimationState.Dodge:set.dodge=clip;break;case DragonAnimationState.Hit:set.hit=clip;break;case DragonAnimationState.Death:set.death=clip;break;}
                if(prepareAnchors&&state==DragonAnimationState.Idle)
                {
                    var b=OpaqueBounds(frames[0]);var size=frames[0].rect.size;
                    Vector2 Point(float x,float y)=>((new Vector2((b.xMin+b.width*x)/size.x,(b.yMin+b.height*y)/size.y)-Vector2.one*.5f)*clip.displayScale+clip.offsets[0])*256f;
                    set.anchors=new DragonVisualAnchors{attackOrigin=Point(.9f,.60f),skillOrigin=Point(.86f,.69f),characterCenter=Point(.57f,.43f),hitPosition=Point(.61f,.44f),groundPosition=(targetFoot-Vector2.one*.5f)*256f};
                }
                EditorUtility.SetDirty(clip);
                // Pixels are needed only during import/registration, never during battle.
                importer.isReadable=false;importer.SaveAndReimport();
            }
            if(stage==0)
            {
                dragon.animationSetPath=SetPath.Substring("Assets/Resources/".Length).Replace(".asset","");
                dragon.battleSprite=set.idle.frames[0];
                EditorUtility.SetDirty(dragon);EditorUtility.SetDirty(set);AssetDatabase.SaveAssets();return;
            }
            var forms=(dragon.evolutionForms??Array.Empty<DragonEvolutionForm>()).Where(f=>f!=null&&f.stage!=stage).ToList();
            var old=dragon.Form(stage);
            forms.Add(new DragonEvolutionForm{stage=stage,displayName=displayName,
                battleSprite=set.idle.frames[0],animationSetPath=SetPath.Substring("Assets/Resources/".Length).Replace(".asset",""),
                defaultSkill=old?.defaultSkill,healthMultiplier=old?.healthMultiplier??0,attackMultiplier=old?.attackMultiplier??0});
            dragon.evolutionForms=forms.OrderBy(f=>f.stage).ToArray();EditorUtility.SetDirty(dragon);EditorUtility.SetDirty(set);AssetDatabase.SaveAssets();
            Debug.Log("EVOLUTION_INSTALLED: "+spritePrefix+", stage "+stage+", existing timing and multipliers");
        }
        public static RectInt OpaqueBounds(Sprite sprite)
        {
            var r=sprite.rect;int w=(int)r.width,h=(int)r.height;var p=sprite.texture.GetPixels((int)r.x,(int)r.y,w,h);
            int l=w,b=h,right=0,top=0;
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)if(p[y*w+x].a>.5f){l=Math.Min(l,x);b=Math.Min(b,y);right=Math.Max(right,x+1);top=Math.Max(top,y+1);}
            return new RectInt(l,b,right-l,top-b);
        }
        static Vector2 Foot(Sprite sprite)
        {
            var r=sprite.rect;var bounds=OpaqueBounds(sprite);int w=(int)r.width;var p=sprite.texture.GetPixels((int)r.x,(int)r.y,w,(int)r.height);
            float sum=0,count=0;int band=Math.Max(2,Mathf.RoundToInt(w*.025f));
            for(int y=bounds.yMin;y<Math.Min(bounds.yMin+band,(int)r.height);y++)for(int x=0;x<w;x++)if(p[y*w+x].a>.5f){sum+=x;count++;}
            return new Vector2(count>0?sum/count:w*.5f,bounds.yMin);
        }
    }
}
