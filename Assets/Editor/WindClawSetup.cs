using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class WindClawSetup
    {
        [MenuItem("Dragon Tower/VFX/Install Wind Claw")]
        public static void Install()
        {
            const string sheet="Assets/Resources/VFX/WindClaw.png";
            if(!File.Exists(sheet))
            {
                // Three distinct tapered claw cuts; independent of all crescent slash art.
                var texture=new Texture2D(192,96,TextureFormat.RGBA32,false);var pixels=new Color[192*96];
                for(int y=0;y<96;y++)for(int x=0;x<96;x++)pixels[y*192+x]=Claw(x,y);
                for(int y=0;y<96;y++)for(int x=0;x<96;x++)
                {
                    float px=x-47.5f,py=y-47.5f;Color a=Claw(47.5f+(px-py)*.7071f,47.5f+(px+py)*.7071f),b=Claw(47.5f+(px+py)*.7071f,47.5f+(-px+py)*.7071f);
                    pixels[y*192+96+x]=b.a>0?b:a;
                }
                texture.SetPixels(pixels);texture.Apply();File.WriteAllBytes(sheet,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);AssetDatabase.Refresh();
                var importer=(TextureImporter)AssetImporter.GetAtPath(sheet);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;
#pragma warning disable 618
                importer.spritesheet=new[]{new SpriteMetaData{name="WindClawSlash",rect=new Rect(0,0,96,96),alignment=0,pivot=new Vector2(.5f,.5f)},new SpriteMetaData{name="WindClawIcon",rect=new Rect(96,0,96,96),alignment=0,pivot=new Vector2(.5f,.5f)}};
#pragma warning restore 618
                importer.SaveAndReimport();
            }
            var sprites=AssetDatabase.LoadAllAssetsAtPath(sheet).OfType<Sprite>().ToArray();
            const string path="Assets/Data/Skills/skill_wind_claw.asset";var skill=AssetDatabase.LoadAssetAtPath<SkillData>(path);
            if(skill==null)
            {
                skill=ScriptableObject.CreateInstance<SkillData>();skill.skillId="skill_wind_claw";skill.displayName="귀참";skill.description="세 줄 바람 발톱으로 두 번 교차 절단";skill.rarity=ContentRarity.Unique;skill.elementType=ElementType.Wind;skill.effectKind=SkillEffectKind.Wind;skill.damage=42;skill.cooldown=6.5f;skill.hitCount=2;skill.hitInterval=.14f;skill.initialHitDelay=.18f;skill.statusEffect=CombatStatusEffect.None;AssetDatabase.CreateAsset(skill,path);
            }
            if(skill.icon==null){skill.icon=Array.Find(sprites,s=>s.name=="WindClawIcon");EditorUtility.SetDirty(skill);}
            var database=AssetDatabase.LoadAssetAtPath<ContentDatabase>("Assets/Resources/ContentDatabase.asset");
            if(database!=null&&!database.skills.Contains(skill)){database.skills=database.skills.Concat(new[]{skill}).ToArray();EditorUtility.SetDirty(database);}
            AssetDatabase.SaveAssets();
        }
        static Color Claw(float x,float y)
        {
            for(int i=0;i<3;i++)
            {
                float lo=i==0?12:i==1?4:16,hi=i==0?85:i==1?92:81;
                if(y<lo||y>hi)continue;float t=(y-lo)/(hi-lo);
                float center=24+i*24+5*Mathf.Pow(1-t,2),width=(i==1?6:4.8f)*Mathf.Pow(Mathf.Sin(t*Mathf.PI),.65f),d=Mathf.Abs(x-center);
                if(d<width){if(d>width-1.2f)return new Color(.02f,.26f,.30f);if(x<center-width*.1f)return new Color(.26f,.88f,.77f);return new Color(.89f,1,.97f);}
            }
            return Color.clear;
        }
    }
}
