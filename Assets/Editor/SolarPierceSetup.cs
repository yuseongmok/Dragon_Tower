using System;using System.IO;using System.Linq;using UnityEditor;using UnityEngine;
namespace DragonTower.Editor {
 public static class SolarPierceSetup {
  public const string Folder="Assets/Resources/VFX/SolarPierce",SkillPath="Assets/Data/Skills/skill_solar_pierce.asset";
  public static void Install(){GenerateArt();InstallData();}
  public static void GenerateArt(){Directory.CreateDirectory(Folder);AssetDatabase.Refresh();const int n=128,w=1024,h=768;var tex=new Texture2D(w,h,TextureFormat.RGBA32,false);var pixels=new Color[w*h];for(int row=0;row<6;row++)for(int f=0;f<8;f++)for(int y=0;y<n;y++)for(int x=0;x<n;x++)pixels[(row*n+y)*w+f*n+x]=Pixel(row,f,x,y);tex.SetPixels(pixels);tex.Apply();File.WriteAllBytes(Folder+"/SolarAtlas.png",tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);AssetDatabase.Refresh();var imp=(TextureImporter)AssetImporter.GetAtPath(Folder+"/SolarAtlas.png");imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Multiple;imp.filterMode=FilterMode.Point;imp.mipmapEnabled=false;imp.alphaIsTransparency=true;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.npotScale=TextureImporterNPOTScale.None;imp.spritePixelsPerUnit=100;imp.maxTextureSize=1024;var slices=new SpriteMetaData[48];for(int r=0;r<6;r++)for(int f=0;f<8;f++)slices[r*8+f]=new SpriteMetaData{name="Solar"+r+"_"+f,rect=new Rect(f*n,r*n,n,n),pivot=Vector2.one*.5f,alignment=0};
#pragma warning disable 618
 imp.spritesheet=slices;
#pragma warning restore 618
 imp.SaveAndReimport();}
  static void InstallData(){var skill=AssetDatabase.LoadAssetAtPath<SkillData>(SkillPath);if(skill==null){skill=ScriptableObject.CreateInstance<SkillData>();AssetDatabase.CreateAsset(skill,SkillPath);}skill.skillId="skill_solar_pierce";skill.displayName="솔라 피어스";skill.description="솔 전용 · 태양의 지속 타격과 불사조 충돌 후 최대 HP 20% 재생";skill.exclusiveDragonId="sol";skill.elementType=ElementType.Fire;skill.effectKind=SkillEffectKind.Fire;skill.rarity=ContentRarity.Legendary;skill.solarPresentation=true;skill.damage=18;skill.cooldown=16;skill.initialHitDelay=.32f;skill.hitCount=6;skill.hitInterval=.30f;skill.finalHitDamageMultiplier=8;skill.completionHealPercent=20;skill.completionHealDelay=.24f;skill.vfxDuration=3.1f;skill.icon=AssetDatabase.LoadAllAssetsAtPath(Folder+"/SolarAtlas.png").OfType<Sprite>().First(s=>s.name=="Solar4_0");EditorUtility.SetDirty(skill);var sol=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon8.asset");skill.exclusiveDragonId=sol.StableId;sol.signatureSkill=skill;EditorUtility.SetDirty(sol);var db=AssetDatabase.LoadAssetAtPath<ContentDatabase>("Assets/Resources/ContentDatabase.asset");if(!db.skills.Contains(skill)){db.skills=db.skills.Concat(new[]{skill}).ToArray();EditorUtility.SetDirty(db);}AssetDatabase.SaveAssets();Debug.Log("SOLAR_INSTALLED owner="+sol.StableId);}
  static Color Gold(float t){return t<.16f?new Color(1,1,.92f):t<.34f?new Color(1,.94f,.62f):t<.56f?new Color(1,.72f,.16f):t<.78f?new Color(1,.34f,.055f):new Color(.58f,.055f,.065f);}
  static Color Pixel(int row,int f,int x,int y){float px=(x-63.5f)/59,py=(y-63.5f)/59,t=f*Mathf.PI/4,r=Mathf.Sqrt(px*px+py*py),a=Mathf.Atan2(py,px);
   if(row==5){float edge=.84f+.05f*Mathf.Sin(a*9+t)+.06f*Mathf.Sin(a*15-t*2);if(r>edge)return Color.clear;float flow=Mathf.Sin(px*11+py*7+t)+Mathf.Sin(py*15-px*5-t*2);float q=r*.78f+flow*.085f;return Gold(q);}
   if(row==0){
    // Project plasma coordinates onto a sphere; fixed upper-left lighting and limb fade.
    if(r>=1)return Color.clear;float z=Mathf.Sqrt(Mathf.Max(0,1-r*r));float longitude=Mathf.Atan2(px,z)+t*.18f,latitude=Mathf.Asin(py);
    float flow=Mathf.Sin(longitude*15+Mathf.Sin(latitude*8+t)*1.8f+t)+Mathf.Sin(latitude*19-longitude*7-t*.5f);
    float light=Mathf.Clamp01(z*.72f-px*.35f+py*.26f);float heat=Mathf.Clamp01(.18f+light*.64f+flow*.028f);
    Color c=Color.Lerp(new Color(1,.23f,.025f),new Color(1,.72f,.12f),Mathf.Clamp01(heat*1.6f));c=Color.Lerp(c,new Color(1,.98f,.78f),Mathf.SmoothStep(0,1,Mathf.Clamp01((heat-.48f)/.48f)));
    float edge=Mathf.Clamp01((1-r)/.22f);c.a=edge*edge*(3-2*edge);return c;
   }
   if(row==1){float u=(py+1)*.5f;if(u<0||u>1)return Color.clear;float bend=Mathf.Sin(u*3+t*.3f)*.18f;float width=Mathf.Sin(Mathf.PI*u)*(.50f+.09f*Mathf.Sin(u*8-t));float q=Mathf.Abs(px-bend)/Mathf.Max(.001f,width);if(q>1)return Color.clear;return Gold(q*.9f+u*.08f);}
   if(row==2){
    // Turbulent flame ribbon: no eyes, beak, or solid bird anatomy.
    float u=(py+1)*.5f;if(u<=0||u>=1)return Color.clear;
    float center=.16f*Mathf.Sin(u*7-t)+.09f*Mathf.Sin(u*15+t);
    float width=Mathf.Pow(Mathf.Sin(u*Mathf.PI),.7f)*(.28f+.11f*Mathf.Sin(u*12-t*2));
    float q=Mathf.Abs(px-center)/Mathf.Max(.001f,width);if(q>=1)return Color.clear;
    float veins=Mathf.Sin(u*22+px*16-t*2)*.08f;var c=Gold(Mathf.Clamp01(q*.88f+veins));c.a=Mathf.SmoothStep(0,1,Mathf.Clamp01((1-q)*3))*Mathf.Min(1,u*7);return c;
   }
   if(row==3){float radius=.20f+f/7f*.69f;float wave=.04f*Mathf.Sin(a*13-t*2)+.025f*Mathf.Sin(a*23+t);if(Mathf.Abs(r-radius-wave)<.038f)return Gold(Mathf.Repeat(a/6.28f+t*.12f,1)*.65f);return Color.clear;}
   if(row==4){if(r>1)return Color.clear;var bg=new Color(.12f,.035f,.08f);if(r>.93f)return Gold(.35f);if(r<.32f)return Gold(r*1.7f);float ax=Mathf.Abs(px);float wing=.09f+ax*.72f;bool feather=ax>.22f&&ax<.85f&&py>wing-.32f&&py<wing+.08f&&Mathf.Repeat(ax*11+py*2,1)>.18f;if(feather)return Gold(Mathf.Abs(py-wing)*2.7f);if(Mathf.Abs(px)<.10f&&py>-.58f&&py<.1f)return Gold(.32f);return bg;}
   return Color.clear;
  }
 }
}
