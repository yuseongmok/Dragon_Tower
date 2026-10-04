using System;using System.IO;using System.Linq;using UnityEditor;using UnityEngine;
namespace DragonTower.Editor {
 public static class GenesisSetup {
  public const string Folder="Assets/Resources/VFX/Genesis",SkillPath="Assets/Data/Skills/skill_genesis.asset";
  public static void Install(){
   GenerateArt();var skill=AssetDatabase.LoadAssetAtPath<SkillData>(SkillPath);
   if(skill==null){skill=ScriptableObject.CreateInstance<SkillData>();AssetDatabase.CreateAsset(skill,SkillPath);}
   var dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon3.asset");
   skill.skillId="skill_genesis";skill.displayName="천지개벽";skill.description="블랜디 전용 · 지하로 잠입해 거대한 산맥을 분출시킵니다. 잠입 완료부터 복귀까지 무적. 정상 복귀 후 5초 동안 받는 최종 피해 50% 감소.";
   skill.exclusiveDragonId=dragon.StableId;skill.elementType=ElementType.Earth;skill.effectKind=SkillEffectKind.Earth;skill.rarity=ContentRarity.Legendary;skill.genesisPresentation=true;
   skill.damage=240;skill.cooldown=18;skill.hitCount=1;skill.initialHitDelay=1.15f;skill.hitInterval=.14f;skill.protectedCastDelay=.30f;skill.protectedCastDuration=2.25f;skill.completionDefenseDuration=5;skill.completionDefensePercent=50;skill.vfxDuration=3.2f;
   skill.icon=AssetDatabase.LoadAllAssetsAtPath(Folder+"/GenesisAtlas.png").OfType<Sprite>().First(s=>s.name=="Genesis3_0");
   dragon.signatureSkill=skill;EditorUtility.SetDirty(skill);EditorUtility.SetDirty(dragon);
   var db=AssetDatabase.LoadAssetAtPath<ContentDatabase>("Assets/Resources/ContentDatabase.asset");if(!db.skills.Contains(skill)){db.skills=db.skills.Concat(new[]{skill}).ToArray();EditorUtility.SetDirty(db);}AssetDatabase.SaveAssets();Debug.Log("GENESIS_INSTALLED");
  }
  static Color C(int r,int g,int b)=>new Color32((byte)r,(byte)g,(byte)b,255);
  static Color Stone(int row,int f,float x,float y){
   float u=(y+1)*.5f;
   float ridge=row==0?-.12f+.16f*u:row==1?.24f*u-.28f:.14f;
   float width=row==0?.91f*(1-u*.72f):row==1?.83f*(1-u*.52f):.76f*(1-u*.25f);
   float roof=row==0?Mathf.Max(.99f-Mathf.Abs(x+.10f)*.99f,Mathf.Max(.64f-Mathf.Abs(x+.62f)*1.25f,.76f-Mathf.Abs(x-.45f)*1.3f)):row==1?.86f-.38f*Mathf.Abs(x-.35f):.82f-Mathf.Abs(x)*.22f;
   if(u<.03f||u>roof||Mathf.Abs(x-ridge)>width)return Color.clear;
   float q=(x-ridge)/width;
   Color c=q<-.68f?C(29,25,25):q<-.18f?C(68,45,35):q<.42f?C(135,96,57):C(200,158,94);
   // Broad stratified geological faces, not a scaled shared stone blade.
   if(u<.33f&&q<.3f)c=C(80,56,40);
   if(u>.28f&&u<.55f&&q>-.4f&&q<.1f)c=C(172,128,74);
   if(q>.43f&&q<.49f)c=C(247,223,168);
   float chips=Mathf.Sin(Mathf.Floor(x*19)*71+Mathf.Floor(u*17)*37);
   if(chips>.87f&&q<.4f&&u>.09f)c=Color.Lerp(c,C(51,37,29),.27f);
   if(chips<-.87f&&q>.1f)c=Color.Lerp(c,C(226,192,130),.20f);
   float edge=roof-u;if(edge<.024f)c=C(244,218,158);
   float seam=Mathf.Min(Mathf.Abs(u-.34f-x*.13f),Mathf.Abs(u-.66f+x*.19f));
   if(seam<.009f+f*.002f)c=C(35,27,25);
   if(f>=2&&Mathf.Abs(x-.14f*Mathf.Sin(u*24)-ridge)<.008f*(f-1))return Color.clear;
   return c;
  }
  static Color Pixel(int row,int f,float x,float y){
   if(row<3)return Stone(row,f,x,y);
   float r=Mathf.Sqrt(x*x+y*y);if(r>.97f)return Color.clear;if(r>.87f)return C(230,190,111);
   Color bg=C(30,23,22);
   if(y<-.39f&&Mathf.Abs(y+.56f+.15f*Mathf.Sin(x*15))<.04f)return C(255,227,160);
   var rock=Stone(0,0,x*1.12f,(y+.10f)*1.33f);if(rock.a>0)return rock;
   if(y<-.5f)return C(83,54,35);return bg;
  }
  public static void GenerateArt(){Directory.CreateDirectory(Folder);AssetDatabase.Refresh();const int n=128,w=512,h=512;var tex=new Texture2D(w,h,TextureFormat.RGBA32,false);var pixels=new Color[w*h];
   for(int row=0;row<4;row++)for(int f=0;f<4;f++)for(int y=0;y<n;y++)for(int x=0;x<n;x++)pixels[(row*n+y)*w+f*n+x]=Pixel(row,f,(x-63.5f)/64,(y-63.5f)/64);
   tex.SetPixels(pixels);tex.Apply();string path=Folder+"/GenesisAtlas.png";File.WriteAllBytes(path,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);AssetDatabase.Refresh();
   var im=(TextureImporter)AssetImporter.GetAtPath(path);im.textureType=TextureImporterType.Sprite;im.spriteImportMode=SpriteImportMode.Multiple;im.filterMode=FilterMode.Point;im.mipmapEnabled=false;im.alphaIsTransparency=true;im.textureCompression=TextureImporterCompression.Uncompressed;im.npotScale=TextureImporterNPOTScale.None;im.spritePixelsPerUnit=100;im.maxTextureSize=512;
   var slices=new SpriteMetaData[16];for(int row=0;row<4;row++)for(int f=0;f<4;f++)slices[row*4+f]=new SpriteMetaData{name="Genesis"+row+"_"+f,rect=new Rect(f*n,row*n,n,n),pivot=Vector2.one*.5f,alignment=0};
#pragma warning disable 618
   im.spritesheet=slices;
#pragma warning restore 618
   im.SaveAndReimport();
  }
 }
}
