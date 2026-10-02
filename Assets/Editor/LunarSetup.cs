using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class LunarSetup
    {
        public const string SkillPath="Assets/Data/Skills/skill_luna_transcendence.asset";
        public static void Install()
        {
            const string folder="Assets/Resources/VFX/Lunar";Directory.CreateDirectory(folder);AssetDatabase.Refresh();var tex=new Texture2D(768,480,TextureFormat.RGBA32,false);var pixels=new Color[768*480];
            for(int r=0;r<4;r++)for(int f=0;f<8;f++)for(int y=0;y<96;y++)for(int x=0;x<96;x++)pixels[(r*96+y)*768+f*96+x]=Pixel(r,f,x,y);
            for(int y=0;y<96;y++)for(int x=0;x<96;x++){var c=Pixel(0,2,x,y);float px=(x-56)/15f,py=(y-40)/27f;if(Mathf.Abs(px)+Mathf.Abs(py)<1)c=px<0?new Color(.65f,.9f,1):Color.white;float d=Vector2.Distance(new Vector2(x,y),new Vector2(48,48));if(c.a==0&&d<44)c=d>41?new Color(.67f,.73f,1):new Color(.025f,.04f,.12f);pixels[(384+y)*768+x]=c;}
            tex.SetPixels(pixels);tex.Apply();File.WriteAllBytes(folder+"/Frames.png",tex.EncodeToPNG());Object.DestroyImmediate(tex);AssetDatabase.Refresh();var im=(TextureImporter)AssetImporter.GetAtPath(folder+"/Frames.png");im.textureType=TextureImporterType.Sprite;im.spriteImportMode=SpriteImportMode.Multiple;im.filterMode=FilterMode.Point;im.mipmapEnabled=false;im.textureCompression=TextureImporterCompression.Uncompressed;im.spritePixelsPerUnit=100;im.maxTextureSize=1024;im.npotScale=TextureImporterNPOTScale.None;
            var slices=new SpriteMetaData[33];for(int r=0;r<4;r++)for(int f=0;f<8;f++)slices[r*8+f]=new SpriteMetaData{name="Lunar"+r+"_"+f,rect=new Rect(f*96,r*96,96,96),alignment=9,pivot=Vector2.one*.5f};slices[32]=new SpriteMetaData{name="LunarIcon",rect=new Rect(0,384,96,96),alignment=9,pivot=Vector2.one*.5f};
#pragma warning disable 618
            im.spritesheet=slices;
#pragma warning restore 618
            im.SaveAndReimport();var skill=AssetDatabase.LoadAssetAtPath<SkillData>(SkillPath);if(skill==null){skill=ScriptableObject.CreateInstance<SkillData>();AssetDatabase.CreateAsset(skill,SkillPath);}
            skill.skillId="skill_luna_transcendence";skill.displayName="초월";skill.description="루나 전용 · 달빛과 냉기를 압축해 폭발. 발동 시 둔화 중인 적에게 추가 달빛 파쇄.";skill.exclusiveDragonId="luna";skill.rarity=ContentRarity.Legendary;skill.elementType=ElementType.Ice;skill.effectKind=SkillEffectKind.Frost;skill.lunarPresentation=true;skill.damage=220;skill.cooldown=16;skill.hitCount=1;skill.initialHitDelay=.95f;skill.hitInterval=.28f;skill.slowBonusDamagePercent=50;skill.slowBonusDelay=.16f;skill.vfxDuration=1.9f;skill.protectedCastDuration=0;skill.statusEffect=CombatStatusEffect.None;skill.rewardEligibilityPercent=100;
            skill.icon=AssetDatabase.LoadAllAssetsAtPath(folder+"/Frames.png").OfType<Sprite>().First(s=>s.name=="LunarIcon");EditorUtility.SetDirty(skill);var luna=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon1.asset");luna.signatureSkill=skill;EditorUtility.SetDirty(luna);var db=AssetDatabase.LoadAssetAtPath<ContentDatabase>("Assets/Resources/ContentDatabase.asset");if(!db.skills.Contains(skill)){db.skills=db.skills.Concat(new[]{skill}).ToArray();EditorUtility.SetDirty(db);}AssetDatabase.SaveAssets();Debug.Log("LUNAR_INSTALLED Luna signature only");
        }
        static Color Pixel(int row,int f,int x,int y)
        {
            float px=(x-48)/44f,py=(y-48)/44f,r=Mathf.Sqrt(px*px+py*py),a=Mathf.Atan2(py,px),t=f/7f;Color white=new Color(.96f,.98f,1),pale=new Color(.71f,.86f,1),silver=new Color(.52f,.61f,.92f),navy=new Color(.08f,.15f,.36f),cyan=new Color(.22f,.70f,.95f);
            if(row==0){float inner=Mathf.Sqrt((px-.34f)*(px-.34f)+(py-.16f)*(py-.16f));if(r>.88f||inner<.77f)return Color.clear;float crescentEdge=Mathf.Min(.88f-r,inner-.77f);Color c=crescentEdge<.026f?white:crescentEdge<.065f?pale:px<-.5f?silver:pale;if((x*3+y*7+f*3)%37==0)c=white;if(r>.85f&&px>0)c=navy;return c;}
            if(row==1){float width=.08f+.06f*Mathf.Sin(py*5+t*6.28f)+.12f*Mathf.Pow(Mathf.Abs(py),4),q=Mathf.Abs(px);if(q>width||Mathf.Abs(py)>.98f)return Color.clear;var c=q<width*.25f?white:q<width*.6f?pale:silver;c.a=q<width*.6f?1:.55f;return c;}
            if(row==2){float ring=Mathf.Min(Mathf.Abs(r-.78f),Mathf.Abs(r-.61f));if(ring<.018f)return pale;if(r>.63f&&r<.75f&&Mathf.Sin(a*16+t*2)>.82f)return white;if(Mathf.Abs(px)<.018f&&r<.65f)return silver;if(Mathf.Abs(py)<.018f&&r<.65f)return silver;return Color.clear;}
            float edge=(.27f+t*.66f)*(1+.12f*Mathf.Sin(a*12)+.09f*Mathf.Sin(a*24+t*8)),hole=Mathf.Max(0,t-.2f)*.87f;
            if(r<edge&&r>hole){Color c=r>edge-.035f?white:r>edge-.10f?pale:r>edge-.17f?silver:navy;if(Mathf.Sin(a*12)>.65f)c=cyan;c.a=1-t*.75f;return c;}return Color.clear;
        }
    }
}

