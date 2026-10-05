using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    // Content recipe only; shared runtime/animation systems are unchanged.
    public static class BoltSetup
    {
        const int Cell=320;
        class Pose {public List<int> pixels=new List<int>();public int l=99999,b=99999,r,t;}
        [MenuItem("Dragon Tower/Production/Install Bolt Character Sets")]
        public static void InstallAll()
        {
            var dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon4.asset");
            if(dragon==null||dragon.StableId!="volt")throw new Exception("Bolt data missing");
            var template=Resources.Load<DragonAnimationSet>("DragonAnimations/ZephyrBase");
            for(int stage=0;stage<3;stage++){
                string form=new[]{"Base","Middle","Final"}[stage],folder="Assets/Art/Bolt/"+form;
                var sockets=Extract(form,folder,stage);AssetDatabase.Refresh();
                EvolutionSpriteSetup.Install(dragon,stage,folder,"Assets/Resources/DragonAnimations/Bolt"+form+".asset","Bolt"+form,
                    stage==0?dragon.displayName:stage==1?dragon.intermediateName:dragon.finalName,
                    DragonAnimationArchetype.Biped,
                    template,false,new[]{184f,220f,244f}[stage],true);
                var set=dragon.LoadAnimationSet(stage);var attachments=new List<DragonSpriteAttachment>();
                for(int state=0;state<6;state++)for(int f=0;f<4;f++){
                    int i=state*4+f;var sprite=set.Get((DragonAnimationState)state).frames[f];
                    attachments.Add(new DragonSpriteAttachment{name=stage==2?"SpearTip":"CastingOrigin",sprite=sprite,normalizedPosition=sockets[i],useAsAttackOrigin=true,useAsSkillOrigin=true});
                    if(stage==2){foreach(string name in new[]{"WeaponTip","WeaponTrailEnd"})attachments.Add(new DragonSpriteAttachment{name=name,sprite=sprite,normalizedPosition=sockets[i]});
                        foreach(string name in new[]{"WeaponTrailStart","LeftWing","RightWing"})attachments.Add(new DragonSpriteAttachment{name=name,sprite=sprite,normalizedPosition=extraSockets[name][i]});}
                }
                set.attachments=attachments.ToArray();EditorUtility.SetDirty(set);
                Debug.Log("BOLT_INSTALLED "+form);
            }
            AssetDatabase.SaveAssets();
        }
        static readonly Dictionary<string,Vector2[]> extraSockets=new Dictionary<string,Vector2[]>();
        static Vector2[] Extract(string form,string folder,int stage)
        {
            var tex=new Texture2D(2,2,TextureFormat.RGBA32,false);tex.LoadImage(File.ReadAllBytes("Assets/Art/Bolt/Source/"+form+".png"));
            int w=tex.width,h=tex.height;var px=tex.GetPixels32();var seen=new bool[px.Length];var poses=new Pose[24];
            for(int start=0;start<px.Length;start++){
                if(seen[start]||px[start].a<=32)continue;var p=new Pose();var q=new Stack<int>();q.Push(start);seen[start]=true;
                while(q.Count>0){int k=q.Pop(),x=k%w,y=k/w;p.pixels.Add(k);p.l=Math.Min(p.l,x);p.r=Math.Max(p.r,x);p.b=Math.Min(p.b,y);p.t=Math.Max(p.t,y);
                    void Push(int n){if(n>=0&&n<px.Length&&!seen[n]&&px[n].a>32){seen[n]=true;q.Push(n);}}
                    if(x>0)Push(k-1);if(x<w-1)Push(k+1);Push(k-w);Push(k+w);
                }
                if(p.pixels.Count<500)continue;
                int col=Mathf.Clamp((p.l+p.r)/2/(w/4),0,3),row=Mathf.Clamp((h-1-(p.b+p.t)/2)/(h/6),0,5),index=row*4+col;
                if(poses[index]!=null)throw new Exception("Overlapping Bolt pose "+form+" "+index);poses[index]=p;
            }
            Directory.CreateDirectory(folder);var sockets=new Vector2[24];foreach(string name in new[]{"WeaponTrailStart","LeftWing","RightWing"})extraSockets[name]=new Vector2[24];
            for(int row=0;row<6;row++){
                var output=new Color32[Cell*Cell*4];int ow=Cell*2;
                for(int frame=0;frame<4;frame++){
                    int sourceFrame=frame;
                    var p=poses[row*4+sourceFrame];if(p==null)throw new Exception("Missing Bolt pose");
                    if(p.r-p.l>=Cell-12||p.t-p.b>=Cell-12)throw new Exception("Bolt pose exceeds packing cell");
                    int ox=frame%2*Cell+(Cell-(p.r-p.l+1))/2,oy=(1-frame/2)*Cell+12;
                    foreach(int k in p.pixels)output[(oy+k/w-p.b)*ow+ox+k%w-p.l]=px[k];
                    Vector2 Pack(Vector2 point)=>new Vector2((ox%Cell+(p.r-p.l)*point.x)/Cell,(12+(p.t-p.b)*point.y)/Cell);
                    var point=new Vector2(.87f,.65f);
                    if(row==2)point=new Vector2(.82f,.74f);
                    if(row==5)point=new Vector2(.92f,frame<2?.45f:.22f);
                    if(stage==2){
                        // Authored spear tips in source-cell coordinates, top-left origin.
                        var tips=new[]{new Vector2(228,61),new Vector2(232,62),new Vector2(230,62),new Vector2(225,62),
                        new Vector2(237,35),new Vector2(253,126),new Vector2(253,126),new Vector2(230,47),
                        new Vector2(239,53),new Vector2(237,4),new Vector2(239,9),new Vector2(225,54),
                        new Vector2(236,54),new Vector2(253,156),new Vector2(253,150),new Vector2(234,44),
                        new Vector2(236,59),new Vector2(209,37),new Vector2(234,54),new Vector2(234,60),
                        new Vector2(239,80),new Vector2(249,183),new Vector2(240,193),new Vector2(225,198)};
                        var at=tips[row*4+frame];var wanted=new Vector2(frame*w/4+at.x,h-row*h/6-at.y);float best=float.MaxValue;Vector2 nearest=wanted;
                        foreach(int k in p.pixels){var candidate=new Vector2(k%w,k/w);float distance=(candidate-wanted).sqrMagnitude;if(distance<best){best=distance;nearest=candidate;}}
                        point=new Vector2((nearest.x-p.l)/(p.r-p.l),(nearest.y-p.b)/(p.t-p.b));
                    }
                    sockets[row*4+frame]=Pack(point);
                    bool low=row==5&&frame>=2;
                    extraSockets["WeaponTrailStart"][row*4+frame]=Pack(new Vector2(row==1&&frame>0&&frame<3?.70f:.57f,low?.14f:.43f));
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
