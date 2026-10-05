using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    // Content recipe only; shared runtime/animation systems are unchanged.
    public static class DanteSetup
    {
        const int Cell=352;
        class Pose {public List<int> pixels=new List<int>();public int l=99999,b=99999,r,t;}
        [MenuItem("Dragon Tower/Production/Install Dante Character Sets")]
        public static void InstallAll()
        {
            var dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon7.asset");
            if(dragon==null||dragon.StableId!="dante")throw new Exception("Dante data missing");
            var template=Resources.Load<DragonAnimationSet>("DragonAnimations/ZephyrBase");
            for(int stage=0;stage<3;stage++){
                string form=new[]{"Base","Middle","Final"}[stage],folder="Assets/Art/Dante/"+form;
                var sockets=Extract(form,folder,stage);AssetDatabase.Refresh();
                EvolutionSpriteSetup.Install(dragon,stage,folder,"Assets/Resources/DragonAnimations/Dante"+form+".asset","Dante"+form,
                    stage==0?dragon.displayName:stage==1?dragon.intermediateName:dragon.finalName,
                    stage<2?DragonAnimationArchetype.Biped:DragonAnimationArchetype.LargeQuadruped,
                    template,false,new[]{174f,210f,234f}[stage],true);
                var set=dragon.LoadAnimationSet(stage);var attachments=new List<DragonSpriteAttachment>();
                for(int state=0;state<6;state++)for(int f=0;f<4;f++){
                    int i=state*4+f;var sprite=set.Get((DragonAnimationState)state).frames[f];
                    attachments.Add(new DragonSpriteAttachment{name="AttackOrigin",sprite=sprite,normalizedPosition=sockets[i],useAsAttackOrigin=true});
                    foreach(string name in new[]{"CastingCenter"})attachments.Add(new DragonSpriteAttachment{name=name,sprite=sprite,normalizedPosition=extraSockets[name][i],useAsSkillOrigin=name=="CastingCenter"});
                }
                set.attachments=attachments.ToArray();if(stage==2)PrepareStaff(set);EditorUtility.SetDirty(set);
                Debug.Log("DANTE_INSTALLED "+form);
            }
            AssetDatabase.SaveAssets();
        }
        static void PrepareStaff(DragonAnimationSet set)
        {
            const string path="Assets/Art/Dante/Staff.png";
            var input=new Texture2D(2,2,TextureFormat.RGBA32,false);input.LoadImage(File.ReadAllBytes("Assets/Art/Dante/Source/Staff.png"));
            var px=input.GetPixels32();int l=input.width,b=input.height,r=0,t=0;
            for(int y=0;y<input.height;y++)for(int x=0;x<input.width;x++)if(px[y*input.width+x].a>32){l=Math.Min(l,x);b=Math.Min(b,y);r=Math.Max(r,x);t=Math.Max(t,y);}
            // Pack into a small point-filtered battle texture; generated alpha is retained.
            int h=192,w=Mathf.RoundToInt((r-l+1f)/(t-b+1)*h);var target=new Texture2D(w,h,TextureFormat.RGBA32,false);var packed=new Color32[w*h];
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)packed[y*w+x]=px[(b+Mathf.Min(t-b,Mathf.FloorToInt((y+.5f)/h*(t-b+1))))*input.width+l+Mathf.Min(r-l,Mathf.FloorToInt((x+.5f)/w*(r-l+1)))];
            target.SetPixels32(packed);target.Apply();File.WriteAllBytes(path,target.EncodeToPNG());UnityEngine.Object.DestroyImmediate(input);UnityEngine.Object.DestroyImmediate(target);AssetDatabase.Refresh();
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=256;importer.spritePixelsPerUnit=100;importer.SaveAndReimport();
            var data=new FloatingWeaponData{sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path),size=new Vector2(150f*w/h,150),tip=new Vector2(.5f,.97f)};
            var poses=new List<FloatingWeaponPose>();
            for(int state=0;state<6;state++)for(int frame=0;frame<4;frame++){
                Vector2 p=new Vector2(149,-14);float angle=-7;bool hover=true;
                if(state==1){p=new[]{new Vector2(113,-10),new Vector2(126,3),new Vector2(131,6),new Vector2(149,-14)}[frame];angle=new[]{-12f,-57,-65,-7}[frame];}
                if(state==2){p=new[]{new Vector2(111,-4),new Vector2(102,43),new Vector2(96,57),new Vector2(149,-14)}[frame];angle=new[]{5f,-18,-24,-7}[frame];}
                if(state==3){p=new Vector2(frame==1?120:145,frame==1?-32:-14);angle=frame==1?21:-7;}
                if(state==4){p=new Vector2(frame==1?123:145,frame==1?-22:-14);angle=frame==1?19:-7;}
                if(state==5){p=new[]{new Vector2(110,-25),new Vector2(104,-50),new Vector2(80,-96),new Vector2(74,-100)}[frame];angle=new[]{15f,42,78,87}[frame];hover=false;}
                poses.Add(new FloatingWeaponPose{characterFrame=set.Get((DragonAnimationState)state).frames[frame],position=p,angle=angle,hover=hover});
            }
            data.poses=poses.ToArray();set.floatingWeapon=data;
        }
        static readonly Dictionary<string,Vector2[]> extraSockets=new Dictionary<string,Vector2[]>();
        static Vector2[] Extract(string form,string folder,int stage)
        {
            var tex=new Texture2D(2,2,TextureFormat.RGBA32,false);tex.LoadImage(File.ReadAllBytes("Assets/Art/Dante/Source/"+form+".png"));
            int w=tex.width,h=tex.height;var px=tex.GetPixels32();var seen=new bool[px.Length];var poses=new Pose[24];
            for(int start=0;start<px.Length;start++){
                if(seen[start]||px[start].a<=32)continue;var p=new Pose();var q=new Stack<int>();q.Push(start);seen[start]=true;
                while(q.Count>0){int k=q.Pop(),x=k%w,y=k/w;p.pixels.Add(k);p.l=Math.Min(p.l,x);p.r=Math.Max(p.r,x);p.b=Math.Min(p.b,y);p.t=Math.Max(p.t,y);
                    void Push(int n){if(n>=0&&n<px.Length&&!seen[n]&&px[n].a>32){seen[n]=true;q.Push(n);}}
                    if(x>0)Push(k-1);if(x<w-1)Push(k+1);Push(k-w);Push(k+w);
                }
                if(p.pixels.Count<500)continue;
                int col=Mathf.Clamp((p.l+p.r)/2/(w/4),0,3),row=Mathf.Clamp((h-1-(p.b+p.t)/2)/(h/6),0,5),index=row*4+col;
                if(poses[index]!=null)throw new Exception("Overlapping Dante pose "+form+" "+index);poses[index]=p;
            }
            Directory.CreateDirectory(folder);var sockets=new Vector2[24];foreach(string name in new[]{"CastingCenter"})extraSockets[name]=new Vector2[24];
            for(int row=0;row<6;row++){
                var output=new Color32[Cell*Cell*4];int ow=Cell*2;
                for(int frame=0;frame<4;frame++){
                    int sourceFrame=frame;
                    var p=poses[row*4+sourceFrame];if(p==null)throw new Exception("Missing Dante pose");
                    if(p.r-p.l>=Cell-12||p.t-p.b>=Cell-12)throw new Exception("Dante pose exceeds packing cell "+form+" "+row+" "+frame+" "+(p.r-p.l)+"x"+(p.t-p.b));
                    int ox=frame%2*Cell+(Cell-(p.r-p.l+1))/2,oy=(1-frame/2)*Cell+12;
                    foreach(int k in p.pixels)output[(oy+k/w-p.b)*ow+ox+k%w-p.l]=px[k];
                    Vector2 Pack(Vector2 point)=>new Vector2((ox%Cell+(p.r-p.l)*point.x)/Cell,(12+(p.t-p.b)*point.y)/Cell);
                    var point=new Vector2(.87f,.65f);
                    if(row==2)point=new Vector2(.82f,.74f);
                    if(row==5)point=new Vector2(.92f,frame<2?.45f:.22f);
                    sockets[row*4+frame]=Pack(point);
                    bool low=row==5&&frame>=2;
                    extraSockets["CastingCenter"][row*4+frame]=Pack(new Vector2(.68f,low?.24f:row==2?.65f:.52f));

                }
                var sheet=new Texture2D(ow,ow,TextureFormat.RGBA32,false);sheet.SetPixels32(output);sheet.Apply();
                File.WriteAllBytes(folder+"/"+((DragonAnimationState)row)+".png",sheet.EncodeToPNG());UnityEngine.Object.DestroyImmediate(sheet);
            }
            UnityEngine.Object.DestroyImmediate(tex);
            return sockets;
        }
    }
}

