using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class GaleStrikeSetup
    {
        const string Folder="Assets/Resources/VFX",Sheet=Folder+"/WindModules.png";
        [MenuItem("Dragon Tower/VFX/Install Gale Strike sprites")]
        public static void Install(){Build(false);}
        [MenuItem("Dragon Tower/VFX/Rebuild Gale Strike sprites and recipe")]
        public static void Rebuild(){Build(true);}
        static void Build(bool rebuild)
        {
            var existing=AssetDatabase.LoadAssetAtPath<WindSkillRecipe>(Folder+"/GaleStrike.asset");if(existing!=null&&!rebuild)return;
            Directory.CreateDirectory(Folder);var atlas=new Texture2D(512,576,TextureFormat.RGBA32,false);var pixels=new Color[512*576];
            for(int kind=0;kind<9;kind++)for(int frame=0;frame<8;frame++)for(int y=0;y<64;y++)for(int x=0;x<64;x++)
                pixels[(kind*64+y)*512+frame*64+x]=Pixel(kind,(x-31.5f)/32,(y-31.5f)/32,frame/8f);
            atlas.SetPixels(pixels);atlas.Apply();File.WriteAllBytes(Sheet,atlas.EncodeToPNG());UnityEngine.Object.DestroyImmediate(atlas);AssetDatabase.Refresh();
            var importer=(TextureImporter)AssetImporter.GetAtPath(Sheet);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=1024;importer.alphaIsTransparency=true;
            var slices=new SpriteMetaData[72];for(int kind=0;kind<9;kind++)for(int frame=0;frame<8;frame++)slices[kind*8+frame]=new SpriteMetaData{name=((WindModule)kind)+"_"+frame,rect=new Rect(frame*64,kind*64,64,64),pivot=Vector2.one*.5f,alignment=0};
            #pragma warning disable 618
            importer.spritesheet=slices;
            #pragma warning restore 618
            importer.SaveAndReimport();var sprites=AssetDatabase.LoadAllAssetsAtPath(Sheet).OfType<Sprite>().ToArray();
            var recipe=existing!=null?existing:ScriptableObject.CreateInstance<WindSkillRecipe>();recipe.frames=slices.Select(s=>sprites.First(sprite=>sprite.name==s.name)).ToArray();
            recipe.clips=new[]{
                Clip(WindModule.WindGather,0,.18f,180,152,0,0,.95f,.65f),
                Clip(WindModule.WindPixelParticles,0,.19f,166,150,0,0,1,.6f),
                Clip(WindModule.WindTrail,.12f,.19f,110,220,.20f,.88f,.7f,1),
                Clip(WindModule.WindSlash,.19f,.20f,210,154,.85f,1,.55f,1.1f),
                Clip(WindModule.TornadoOuter,.23f,.66f,370,340,1,1,.66f,1,true,.88f),
                Clip(WindModule.TornadoCore,.23f,.66f,206,320,1,1,.6f,1,true,.8f),
                Clip(WindModule.WindPixelParticles,.25f,.66f,362,320,1,1,.7f,1.05f,true),
                Clip(WindModule.WindHitSpark,.29f,.12f,92,92,1,1,.5f,1),
                Clip(WindModule.WindHitSpark,.43f,.12f,90,90,1,1,.5f,1),
                Clip(WindModule.WindHitSpark,.57f,.12f,108,108,1,1,.5f,1),
                Clip(WindModule.WindHitSpark,.71f,.12f,98,98,1,1,.5f,1),
                Clip(WindModule.TornadoOuter,.79f,.1f,370,340,1,1,1,.65f),
                Clip(WindModule.WindHitSpark,.86f,.19f,228,200,1,1,.5f,1.15f),
                Clip(WindModule.WindShockwave,.86f,.28f,390,290,1,1,.3f,1),
                Clip(WindModule.WindSlash,.88f,.22f,334,260,1,1,.6f,1.2f),
                Clip(WindModule.WindPixelParticles,.88f,.44f,360,300,1,1,.6f,1.12f),
                Clip(WindModule.WindAfterimage,1.02f,.33f,346,292,1,1,.9f,1.1f,false,.55f)
            };
            recipe.clips[7].offset=new Vector2(-55,12);recipe.clips[8].offset=new Vector2(43,-24);recipe.clips[9].offset=new Vector2(-12,43);recipe.clips[10].offset=new Vector2(49,12);
            recipe.clips[4].offset=recipe.clips[5].offset=recipe.clips[6].offset=recipe.clips[11].offset=new Vector2(0,-24);
            if(existing==null)AssetDatabase.CreateAsset(recipe,Folder+"/GaleStrike.asset");else EditorUtility.SetDirty(recipe);AssetDatabase.SaveAssets();
        }
        static WindModuleClip Clip(WindModule module,float start,float duration,float w,float h,float anchor,float endAnchor,float startScale,float endScale,bool loop=false,float opacity=1)
        {return new WindModuleClip{module=module,start=start,duration=duration,size=new Vector2(w,h),anchor=anchor,endAnchor=endAnchor,startScale=startScale,endScale=endScale,loop=loop,opacity=opacity};}
        static Color Pixel(int kind,float x,float y,float t)
        {
            float s=0,r=Mathf.Sqrt(x*x+y*y),a=Mathf.Atan2(y,x);
            if(kind==0)
            {
                float target=.82f-t*.5f;
                float wave=Mathf.Repeat(a/(Mathf.PI*2)+t*1.5f, .25f);
                if(r<target&&r>target-.10f&&wave<.17f)s=.8f;
                if(Mathf.Abs(x)<.03f&&Mathf.Abs(y)<.16f*(1-t))s=1;
            }
            else if(kind==1)
            {
                float center=y*.16f+Mathf.Sin(y*9+t*8)*.045f;
                float width=(.05f+(.9f-y)*.13f);
                if(Mathf.Abs(y)<.92f&&Mathf.Abs(x-center)<width){float d=Mathf.Abs(x-center)/width;s=d<.22f?1:d<.55f?.8f:.42f;}
                if(Mathf.Abs(x-center-.27f)<.025f&&y<.5f&&y>-.65f)s=.7f;
            }
            else if(kind==2)
            {
                float ang=Mathf.DeltaAngle(a*Mathf.Rad2Deg,t*150+35);
                if(r>.70f&&r<.85f&&ang>-130&&ang<80)s=r>.8f?1:.75f;
                if(r>.48f&&r<.53f&&ang>-90&&ang<50)s=.45f;
            }
            else if(kind==3)
            {
                float center=Mathf.Sin(y*8-t*11)*(.04f+.08f*(y+1));
                float width=.08f+.13f*(y+1);
                if(Mathf.Abs(y)<.87f&&Mathf.Abs(x-center)<width&&Mathf.Repeat(y*3+t*2,1)>.22f)s=Mathf.Abs(x-center)<width*.27f?1:.65f;
                if(Mathf.Abs(y)<.8f&&Mathf.Abs(x+center*.7f)<.035f)s=1;
            }
            else if(kind==4||kind==8)
            {
                for(int band=0;band<6;band++)
                {
                    float cy=-.72f+band*.28f,width=.22f+band*.13f;
                    float shift=Mathf.Sin(t*Mathf.PI*2+band*.8f)*.055f;
                    float dy=(y-cy)/(.10f+band*.021f);
                    float radial=Mathf.Sqrt((x-shift)*(x-shift)/(width*width)+dy*dy);
                    float angle=Mathf.Atan2(dy,(x-shift)/width);
                    float gap=Mathf.Abs(Mathf.DeltaAngle(angle*Mathf.Rad2Deg,t*360+band*64));
                    if(radial>.48f&&radial<1&&gap>28)s=Mathf.Max(s,dy>.15f?(radial>.89f?1:.76f):.38f);
                }
                if(kind==8&&Mathf.Repeat(a*3+t,1)<.55f)s=0;
            }
            else if(kind==5)
            {
                float edge=.15f+.62f*Mathf.Pow(Mathf.Abs(Mathf.Cos(a*4)),14);
                if(r<edge*(.7f+t*.4f)&&r>.08f+t*.18f)s=r<edge*.55f?1:.78f;
                if(t<.3f&&Mathf.Abs(x)+Mathf.Abs(y)<.18f)s=1;
            }
            else if(kind==6)
            {
                if(r>.76f&&r<.9f)s=r>.86f?1:.7f;
                if(r>.61f&&r<.65f&&Mathf.Sin(a*5+t*8)>-.25f)s=.4f;
            }
            else if(kind==7)
            {
                for(int i=0;i<22;i++)
                {
                    float angle=i*2.39996f+t*2.4f;float radius=.35f+(i%5)*.12f;
                    float px=Mathf.Cos(angle)*radius,py=Mathf.Repeat(i*.173f+t*.9f,1)*1.8f-.9f;
                    float size=i%4==0?.045f:.025f;
                    if(Mathf.Abs(x-px)<size&&Mathf.Abs(y-py)<size*(i%3==0?2:1))s=i%3==0?1:.65f;
                }
            }
            if(s==0)return Color.clear;
            return s>.9f?new Color(.88f,1,.96f,1):s>.7f?new Color(.30f,.98f,.81f,1):s>.5f?new Color(.12f,.80f,.77f,1):new Color(.035f,.39f,.43f,.85f);
        }
    }
}

