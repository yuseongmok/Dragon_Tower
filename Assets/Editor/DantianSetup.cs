using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class DantianSetup
    {
        [MenuItem("Dragon Tower/VFX/Install Dantian signature")]
        public static void Install()
        {
            const string sheet="Assets/Resources/VFX/Dantian.png";
            if(!File.Exists(sheet))
            {
                var tex=new Texture2D(256,128,TextureFormat.RGBA32,false);var pixels=new Color[256*128];
                // Small jagged rift strip and a distinct diagonal signature icon.
                for(int y=0;y<32;y++)for(int x=0;x<256;x++)
                {
                    float taper=Mathf.Pow(Mathf.Sin((x+.5f)/256*Mathf.PI),.25f);
                    float width=12*taper+(x/7%3-1),d=Mathf.Abs(y-15.5f);
                    if(d<width)pixels[y*256+x]=Color.white;
                }
                for(int y=0;y<64;y++)for(int x=0;x<64;x++)
                {
                    float d=Mathf.Abs(y-x),r=Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f));
                    if(r<30)pixels[(y+48)*256+x]=d<1?Color.white:d<2?WindVFXStyle.Pale:d<4?WindVFXStyle.Mint:d<7?WindVFXStyle.Cyan:d<11?WindVFXStyle.Teal:new Color(.012f,.04f,.085f);
                }
                tex.SetPixels(pixels);tex.Apply();File.WriteAllBytes(sheet,tex.EncodeToPNG());Object.DestroyImmediate(tex);AssetDatabase.Refresh();
                var im=(TextureImporter)AssetImporter.GetAtPath(sheet);im.textureType=TextureImporterType.Sprite;im.spriteImportMode=SpriteImportMode.Multiple;im.filterMode=FilterMode.Point;im.mipmapEnabled=false;im.alphaIsTransparency=true;im.textureCompression=TextureImporterCompression.Uncompressed;
#pragma warning disable 618
                im.spritesheet=new[]{new SpriteMetaData{name="DantianRift",rect=new Rect(0,0,256,32),pivot=Vector2.one*.5f,alignment=0},new SpriteMetaData{name="DantianIcon",rect=new Rect(0,48,64,64),pivot=Vector2.one*.5f,alignment=0}};
#pragma warning restore 618
                im.SaveAndReimport();
            }
            const string path="Assets/Data/Skills/skill_dantian.asset";var skill=AssetDatabase.LoadAssetAtPath<SkillData>(path);
            if(skill==null)
            {
                skill=ScriptableObject.CreateInstance<SkillData>();skill.skillId="skill_dantian";skill.displayName="단천";
                skill.description="제피르 전용 · 공간 절단 후 폭발 · 연출 중 무적";skill.exclusiveDragonId="zephyr";skill.rewardEligibilityPercent=.5f;
                skill.rarity=ContentRarity.Legendary;skill.elementType=ElementType.Wind;skill.effectKind=SkillEffectKind.Wind;
                skill.damage=180;skill.cooldown=12;skill.hitCount=1;skill.initialHitDelay=.45f;skill.protectedCastDuration=1.3f;skill.vfxDuration=1.3f;
                skill.icon=AssetDatabase.LoadAllAssetsAtPath(sheet).OfType<Sprite>().First(s=>s.name=="DantianIcon");AssetDatabase.CreateAsset(skill,path);
            }
            var db=AssetDatabase.LoadAssetAtPath<ContentDatabase>("Assets/Resources/ContentDatabase.asset");
            if(db!=null&&!db.skills.Contains(skill)){db.skills=db.skills.Concat(new[]{skill}).ToArray();EditorUtility.SetDirty(db);}
            AssetDatabase.SaveAssets();
        }
    }
}
