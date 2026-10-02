using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class WispSetup
    {
        public const string SkillPath="Assets/Data/Skills/skill_aurora_wisp.asset";
        public static void Install()
        {
            const string folder="Assets/Resources/VFX/Wisp";Directory.CreateDirectory(folder);AssetDatabase.Refresh();
            var tex=new Texture2D(768,576,TextureFormat.RGBA32,false);var pixels=new Color[768*576];
            for(int row=0;row<5;row++)for(int f=0;f<8;f++)for(int y=0;y<96;y++)for(int x=0;x<96;x++)pixels[(row*96+y)*768+f*96+x]=Pixel(row,f,x,y);
            for(int y=0;y<96;y++)for(int x=0;x<96;x++){float r=Vector2.Distance(new Vector2(x,y),new Vector2(48,48));var c=Pixel(0,2,x,y);if(c.a<.1f&&r<44)c=r>40?new Color(.65f,.65f,1):new Color(.04f,.055f,.17f);pixels[(480+y)*768+x]=c;}
            tex.SetPixels(pixels);tex.Apply();File.WriteAllBytes(folder+"/Frames.png",tex.EncodeToPNG());Object.DestroyImmediate(tex);AssetDatabase.Refresh();
            var imp=(TextureImporter)AssetImporter.GetAtPath(folder+"/Frames.png");imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Multiple;imp.filterMode=FilterMode.Point;imp.mipmapEnabled=false;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.spritePixelsPerUnit=100;imp.maxTextureSize=1024;imp.npotScale=TextureImporterNPOTScale.None;
            var slices=new SpriteMetaData[41];for(int r=0;r<5;r++)for(int f=0;f<8;f++)slices[r*8+f]=new SpriteMetaData{name="Wisp"+r+"_"+f,rect=new Rect(f*96,r*96,96,96),alignment=9,pivot=Vector2.one*.5f};slices[40]=new SpriteMetaData{name="WispIcon",rect=new Rect(0,480,96,96),alignment=9,pivot=Vector2.one*.5f};
#pragma warning disable 618
            imp.spritesheet=slices;
#pragma warning restore 618
            imp.SaveAndReimport();
            if(AssetDatabase.LoadAssetAtPath<Material>(folder+"/Silhouette.mat")==null)AssetDatabase.CreateAsset(new Material(Resources.Load<Shader>("VFX/GaleImpactSilhouette")),folder+"/Silhouette.mat");
            var skill=AssetDatabase.LoadAssetAtPath<SkillData>(SkillPath);if(skill==null){skill=ScriptableObject.CreateInstance<SkillData>();AssetDatabase.CreateAsset(skill,SkillPath);}
            skill.skillId="skill_aurora_wisp";skill.displayName="위습";skill.description="오로라 전용 · 영혼 관통 난무 후 집중 폭발. 연출 중과 종료 후 3초 동안 회피 쿨타임 제한 해방.";skill.exclusiveDragonId="aurora";skill.rarity=ContentRarity.Legendary;skill.elementType=ElementType.Ice;skill.effectKind=SkillEffectKind.Frost;skill.wispPresentation=true;skill.icePresentation=false;
            skill.damage=12;skill.cooldown=14;skill.hitCount=13;skill.initialHitDelay=.38f;skill.hitInterval=.13f;skill.finalHitDamageMultiplier=5;skill.dodgeFreeCastDuration=2.5f;skill.dodgeFreeAfterDuration=3;skill.vfxDuration=2.5f;skill.protectedCastDuration=0;skill.statusEffect=CombatStatusEffect.None;skill.rewardEligibilityPercent=100;
            skill.icon=AssetDatabase.LoadAllAssetsAtPath(folder+"/Frames.png").OfType<Sprite>().First(s=>s.name=="WispIcon");EditorUtility.SetDirty(skill);
            var aurora=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon9.asset");aurora.signatureSkill=skill;EditorUtility.SetDirty(aurora);
            var db=AssetDatabase.LoadAssetAtPath<ContentDatabase>("Assets/Resources/ContentDatabase.asset");if(!db.skills.Contains(skill)){db.skills=db.skills.Concat(new[]{skill}).ToArray();EditorUtility.SetDirty(db);}AssetDatabase.SaveAssets();Debug.Log("WISP_INSTALLED Aurora signature only");
        }
        static Color Pixel(int row,int f,int x,int y)
        {
            float px=(x-48)/44f,py=(y-48)/44f,t=f/8f,r=Mathf.Sqrt(px*px+py*py),a=Mathf.Atan2(py,px);
            var white=new Color(.96f,1,1);var pale=new Color(.64f,.96f,1);var cyan=new Color(.23f,.68f,1);var blue=new Color(.26f,.32f,.78f);var violet=new Color(.57f,.42f,.91f);
            if(row==0){float sway=Mathf.Sin(py*6-t*6.283f)*.13f;float head=Mathf.Sqrt((px-sway)*(px-sway)+(py-.25f)*(py-.25f)*1.2f);float body=Mathf.Abs(px-sway);float taper=Mathf.Max(0,(py+.98f)*.28f);float shape=head<.55f?head/.55f:py<.25f&&body<taper?body/taper:2;
                if(shape<1){var c=shape<.27f?white:shape<.52f?pale:shape<.74f?cyan:Mathf.Sin(py*8+t*6.28f)>0?violet:blue;c.a=shape>.8f?.65f:1;return c;}return Color.clear;}
            if(row==1){float s=Mathf.Sin(py*7-t*6.283f)*.22f,edge=.07f+(py+1)*.19f,q=Mathf.Abs(px-s);if(q<edge&&Mathf.Abs(py)<.96f){var c=q<edge*.3f?pale:q<edge*.6f?cyan:violet;c.a=(py+1)*.35f;return c;}return Color.clear;}
            if(row==2){float phase=f/7f,edge=(.30f+phase*.65f)*(1+.12f*Mathf.Sin(a*7-phase*12)+.10f*Mathf.Sin(a*13+phase*5)),hole=Mathf.Max(0,phase-.35f)*.70f;if(r>hole&&r<edge){var c=r<edge*.30f?white:r<edge*.48f?pale:r<edge*.67f?cyan:r<edge*.82f?blue:violet;c.a=1-phase*.8f;return c;}return Color.clear;}
            if(row==3){float q=Mathf.Sqrt(px*px+py*py*2),radius=.15f+f/7f*.77f;if(Mathf.Abs(q-radius)<.04f+.018f*Mathf.Sin(a*11+f)){var c=py<0?pale:violet;c.a=1-f/8f;return c;}return Color.clear;}
            if(row==4){float q=Mathf.Min(Mathf.Abs(px),Mathf.Abs(py));if(r<.18f)return white;if(r<.9f&&q<.018f){pale.a=(1-r)*(1-t*.6f);return pale;}if(r<.45f&&Mathf.Abs(Mathf.Abs(px)-Mathf.Abs(py))<.025f){cyan.a=1-r;return cyan;}}
            return Color.clear;
        }
    }
}
