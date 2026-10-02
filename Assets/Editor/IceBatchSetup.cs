using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class IceBatchSetup
    {
        public static readonly string[] Paths={"Assets/Data/Skill1.asset","Assets/Data/Skills/skill_ice_slash.asset","Assets/Data/Skills/skill_ice_slam.asset","Assets/Data/Skills/skill_moon_combo.asset"};
        public static void Install(int through)
        {
            const string folder="Assets/Resources/IceSkills";Directory.CreateDirectory(folder);AssetDatabase.Refresh();
            var lib=AssetDatabase.LoadAssetAtPath<IceSkillLibrary>(folder+"/Library.asset");if(lib==null){lib=ScriptableObject.CreateInstance<IceSkillLibrary>();AssetDatabase.CreateAsset(lib,folder+"/Library.asset");}
            lib.style=AssetDatabase.LoadAssetAtPath<IceVfxStyle>(IceStyleSetup.StylePath);
            var t=new Texture2D(256,256,TextureFormat.RGBA32,false);var pixels=new Color[256*256];
            for(int k=0;k<16;k++)for(int y=0;y<64;y++)for(int x=0;x<64;x++)pixels[(k/4*64+y)*256+k%4*64+x]=Pixel(k,x,y,lib.style);
            t.SetPixels(pixels);t.Apply();File.WriteAllBytes(folder+"/Atlas.png",t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);AssetDatabase.Refresh();
            var imp=(TextureImporter)AssetImporter.GetAtPath(folder+"/Atlas.png");imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Multiple;imp.filterMode=FilterMode.Point;imp.mipmapEnabled=false;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.spritePixelsPerUnit=100;imp.isReadable=false;imp.maxTextureSize=256;
            var slices=new SpriteMetaData[16];for(int i=0;i<16;i++)slices[i]=new SpriteMetaData{name="Ice"+i,rect=new Rect(i%4*64,i/4*64,64,64),alignment=9,pivot=Vector2.one*.5f};
#pragma warning disable 618
            imp.spritesheet=slices;
#pragma warning restore 618
            imp.SaveAndReimport();var sprites=AssetDatabase.LoadAllAssetsAtPath(folder+"/Atlas.png").OfType<Sprite>().ToArray();lib.sprites=slices.Select(m=>sprites.First(p=>p.name==m.name)).ToArray();EditorUtility.SetDirty(lib);
            for(int i=0;i<=through;i++)InstallSkill(i,lib);AssetDatabase.SaveAssets();
        }
        static void InstallSkill(int i,IceSkillLibrary lib)
        {
            var s=AssetDatabase.LoadAssetAtPath<SkillData>(Paths[i]);s.displayName=new[]{"아이스샷","아이스볼","서리숨결","아이시클"}[i];s.icePresentation=true;s.iceKind=(IceSkillKind)i;s.rarity=i==0?ContentRarity.Common:i==3?ContentRarity.Unique:ContentRarity.Rare;
            s.icon=lib.Get(new[]{1,3,5,9}[i]);s.hitCount=new[]{3,2,5,3}[i];s.hitInterval=new[]{.11f,.24f,.16f,.18f}[i];s.initialHitDelay=new[]{.20f,.28f,.18f,.20f}[i];
            s.statusEffect=CombatStatusEffect.Slow;s.statusChancePercent=100;s.statusDuration=s.statusDuration>0?s.statusDuration:3;s.statusPower=new[]{20,60,30,60}[i];s.statusOnHit=true;
            s.description=s.displayName+" · "+s.hitCount+" Hit · 공격 간격 둔화 "+s.statusPower+"%";EditorUtility.SetDirty(s);
        }
        static bool Poly(int x,int y,params Vector2[] p){bool inside=false;for(int i=0,j=p.Length-1;i<p.Length;j=i++)if((p[i].y>y)!=(p[j].y>y)&&x<(p[j].x-p[i].x)*(y-p[i].y)/(p[j].y-p[i].y)+p[i].x)inside=!inside;return inside;}
        static Vector2 V(int x,int y)=>new Vector2(x,y);
        static Color Pixel(int k,int x,int y,IceVfxStyle s)
        {
            if(k==0||k==1){float half=(58-y)*.21f;bool inside=y>7&&y<59&&Mathf.Abs(x-32)<half;if(inside){if(Mathf.Abs(x-32)<1.2f)return s.white;if(x<27)return s.deepBlue;if(x>33)return s.cyan;return s.iceBlue;}}
            if(k==2||k==3){float dx=x-32,dy=y-32,r=dx*dx+dy*dy;if(r<27*27){if(r>24*24)return y>32?s.cyan:s.deepNavy;if(r>20*20)return x<28?s.deepBlue:s.iceBlue;if(Mathf.Abs(dx+dy*.5f)<2&&y>27)return s.white;if(x>35&&y>35)return s.pale;if(x<25&&y<34)return s.deepBlue;return (x+y)/9%2==0?s.cyan:s.iceBlue;}}
            if(k==4||k==5){float half=7+y*.29f+((y/5)%2)*3;float dx=Mathf.Abs(x-32);if(y>5&&y<59&&dx<half){if((x/3+y/4)%11==0)return Color.clear;if(dx>half-4)return new Color(s.deepBlue.r,s.deepBlue.g,s.deepBlue.b,.7f);if(dx<2&&y%11<6)return s.pale;if(dx<half*.35f)return s.cyan;return s.iceBlue;}}
            if(k>=6&&k<=9){int variant=k==9?2:k-6;bool shape=variant==0?Poly(x,y,V(18,7),V(38,59),V(48,18),V(38,6)):variant==1?Poly(x,y,V(9,8),V(19,45),V(29,31),V(34,60),V(51,18),V(46,5)):Poly(x,y,V(7,8),V(13,39),V(25,28),V(34,60),V(46,28),V(53,46),V(59,9));if(shape){float ridge=30+(y-25)*.15f;if(Mathf.Abs(x-ridge)<1.4f&&y>13)return s.white;if(x<ridge-6)return x<20?s.deepNavy:s.deepBlue;if(x<ridge)return s.iceBlue;if(x<ridge+7)return s.pale;if(y<20+(x-35)*.35f)return s.deepBlue;if(y>40&&x>43)return s.iceBlue;return s.cyan;}}
            return Color.clear;
        }
    }
}
