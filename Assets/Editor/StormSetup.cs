using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    // Content recipe only; shared runtime/animation systems are unchanged.
    public static class StormSetup
    {
        const int Cell=320;
        class Pose {public List<int> pixels=new List<int>();public int l=99999,b=99999,r,t;}
        [MenuItem("Dragon Tower/Production/Install Storm Character Sets")]
        public static void InstallAll()
        {
            var dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon12.asset");
            if(dragon==null||dragon.StableId!="storm")throw new Exception("Storm data missing");
            var template=Resources.Load<DragonAnimationSet>("DragonAnimations/ZephyrBase");
            for(int stage=0;stage<3;stage++){
                string form=new[]{"Base","Middle","Final"}[stage],folder="Assets/Art/Storm/"+form;
                var sockets=Extract(form,folder,stage);AssetDatabase.Refresh();
                EvolutionSpriteSetup.Install(dragon,stage,folder,"Assets/Resources/DragonAnimations/Storm"+form+".asset","Storm"+form,
                    stage==0?dragon.displayName:stage==1?dragon.intermediateName:dragon.finalName,
                    stage==0?DragonAnimationArchetype.Biped:DragonAnimationArchetype.Flying,
                    template,false,new[]{184f,220f,244f}[stage],true);
                var set=dragon.LoadAnimationSet(stage);var attachments=new List<DragonSpriteAttachment>();
                for(int state=0;state<6;state++)for(int f=0;f<4;f++){
                    int i=state*4+f;var sprite=set.Get((DragonAnimationState)state).frames[f];
                    attachments.Add(new DragonSpriteAttachment{name="AttackOrigin",sprite=sprite,normalizedPosition=sockets[i],useAsAttackOrigin=true});
                    foreach(string name in new[]{"LeftWing","RightWing","CastingCenter"})attachments.Add(new DragonSpriteAttachment{name=name,sprite=sprite,normalizedPosition=extraSockets[name][i],useAsSkillOrigin=name=="CastingCenter"});
                }
                set.attachments=attachments.ToArray();EditorUtility.SetDirty(set);
                Debug.Log("STORM_INSTALLED "+form);
            }
            AssetDatabase.SaveAssets();
        }
        static readonly Dictionary<string,Vector2[]> extraSockets=new Dictionary<string,Vector2[]>();
        static Vector2[] Extract(string form,string folder,int stage)
        {
            var tex=new Texture2D(2,2,TextureFormat.RGBA32,false);tex.LoadImage(File.ReadAllBytes("Assets/Art/Storm/Source/"+form+".png"));
            int w=tex.width,h=tex.height;var px=tex.GetPixels32();var seen=new bool[px.Length];var poses=new Pose[24];
            for(int start=0;start<px.Length;start++){
                if(seen[start]||px[start].a<=32)continue;var p=new Pose();var q=new Stack<int>();q.Push(start);seen[start]=true;
                while(q.Count>0){int k=q.Pop(),x=k%w,y=k/w;p.pixels.Add(k);p.l=Math.Min(p.l,x);p.r=Math.Max(p.r,x);p.b=Math.Min(p.b,y);p.t=Math.Max(p.t,y);
                    void Push(int n){if(n>=0&&n<px.Length&&!seen[n]&&px[n].a>32){seen[n]=true;q.Push(n);}}
                    if(x>0)Push(k-1);if(x<w-1)Push(k+1);Push(k-w);Push(k+w);
                }
                if(p.pixels.Count<500)continue;
                int col=Mathf.Clamp((p.l+p.r)/2/(w/4),0,3),row=Mathf.Clamp((h-1-(p.b+p.t)/2)/(h/6),0,5),index=row*4+col;
                if(poses[index]!=null)throw new Exception("Overlapping Storm pose "+form+" "+index);poses[index]=p;
            }
            Directory.CreateDirectory(folder);var sockets=new Vector2[24];foreach(string name in new[]{"CastingCenter","LeftWing","RightWing"})extraSockets[name]=new Vector2[24];
            for(int row=0;row<6;row++){
                var output=new Color32[Cell*Cell*4];int ow=Cell*2;
                for(int frame=0;frame<4;frame++){
                    int sourceFrame=frame;
                    var p=poses[row*4+sourceFrame];if(p==null)throw new Exception("Missing Storm pose");
                    if(p.r-p.l>=Cell-12||p.t-p.b>=Cell-12)throw new Exception("Storm pose exceeds packing cell");
                    int ox=frame%2*Cell+(Cell-(p.r-p.l+1))/2,oy=(1-frame/2)*Cell+12;
                    foreach(int k in p.pixels)output[(oy+k/w-p.b)*ow+ox+k%w-p.l]=px[k];
                    Vector2 Pack(Vector2 point)=>new Vector2((ox%Cell+(p.r-p.l)*point.x)/Cell,(12+(p.t-p.b)*point.y)/Cell);
                    var point=new Vector2(.87f,.65f);
                    if(row==2)point=new Vector2(.82f,.74f);
                    if(row==5)point=new Vector2(.92f,frame<2?.45f:.22f);
                    sockets[row*4+frame]=Pack(point);
                    bool low=row==5&&frame>=2;
                    extraSockets["CastingCenter"][row*4+frame]=Pack(new Vector2(.68f,low?.24f:row==2?.65f:.52f));
                    extraSockets["LeftWing"][row*4+frame]=Pack(new Vector2(.25f,low?.60f:.78f));
                    extraSockets["RightWing"][row*4+frame]=Pack(new Vector2(row==2&&frame>0&&frame<3?.81f:.58f,low?.40f:.63f));

                }
                var sheet=new Texture2D(ow,ow,TextureFormat.RGBA32,false);sheet.SetPixels32(output);sheet.Apply();
                File.WriteAllBytes(folder+"/"+((DragonAnimationState)row)+".png",sheet.EncodeToPNG());UnityEngine.Object.DestroyImmediate(sheet);
            }
            UnityEngine.Object.DestroyImmediate(tex);
            return sockets;
        }
    }
}
