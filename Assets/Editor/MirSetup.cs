using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    // Mir content import only. All motion uses the existing six-state sprite player.
    public static class MirSetup
    {
        const int Cell=320;
        static readonly Vector2[][] sockets=new Vector2[3][];
        [MenuItem("Dragon Tower/Production/Install Mir Character Sets")]
        public static void InstallAll()
        {
            var d=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon10.asset");
            if(d==null||d.StableId!="mir")throw new Exception("Expected existing Mir data");
            var template=Resources.Load<DragonAnimationSet>("DragonAnimations/ZephyrBase");
            for(int stage=0;stage<3;stage++){
                string form=new[]{"Base","Middle","Final"}[stage],folder="Assets/Art/Mir/"+form;
                Extract(form,folder,stage);AssetDatabase.Refresh();
                EvolutionSpriteSetup.Install(d,stage,folder,"Assets/Resources/DragonAnimations/Mir"+form+".asset","Mir"+form,
                    d.NameForStage(stage),stage==0?DragonAnimationArchetype.SmallQuadruped:DragonAnimationArchetype.Serpentine,
                    template,false,new[]{174f,225f,244f}[stage],true);
                var set=d.LoadAnimationSet(stage);var points=new List<DragonSpriteAttachment>();
                for(int state=0;state<6;state++){
                    var clip=set.Get((DragonAnimationState)state);
                    for(int f=0;f<4;f++){
                        int i=state*4+f;
                        // Floating bodies have an actual air gap; death settles back to ground.
                        float lift=stage==0?0:state==5?new[]{22f,14f,5f,0f}[f]:22f;
                        if(stage>0&&state==0)lift+=new[]{0f,2f,0f,-2f}[f];
                        clip.offsets[f].y+=lift/256f;
                        foreach(string name in new[]{"AttackOrigin","DragonHead","CastingCenter"}){
                            Vector2 point=sockets[name=="CastingCenter"?1:0][i];
                            points.Add(new DragonSpriteAttachment{name=name,sprite=clip.frames[f],normalizedPosition=point,
                                useAsAttackOrigin=name=="AttackOrigin",useAsSkillOrigin=name=="CastingCenter"});
                        }
                    }
                    EditorUtility.SetDirty(clip);
                }
                set.anchors.characterCenter.y+=stage==0?0:22;
                set.anchors.hitPosition.y+=stage==0?0:22;
                set.attachments=points.ToArray();EditorUtility.SetDirty(set);
            }
            d.basicPresentation=DragonBasicPresentation.PixelWind;
            d.elementSkillPool=AssetDatabase.LoadAssetAtPath<ElementSkillPool>("Assets/Data/SkillPools/WindShared.asset");
            EditorUtility.SetDirty(d);AssetDatabase.SaveAssets();Debug.Log("MIR_INSTALLED");
        }
        class Pose { public List<int> pixels=new List<int>();public int l=99999,b=99999,r,t; }
        static Pose[] FindPoses(Color32[] px,int w,int h){
            var seen=new bool[px.Length];var poses=new Pose[24];
            for(int start=0;start<px.Length;start++){
                if(seen[start]||px[start].a<=100)continue;var p=new Pose();var q=new Stack<int>();q.Push(start);seen[start]=true;
                while(q.Count>0){int k=q.Pop(),x=k%w,y=k/w;p.pixels.Add(k);p.l=Math.Min(p.l,x);p.r=Math.Max(p.r,x);p.b=Math.Min(p.b,y);p.t=Math.Max(p.t,y);
                    void Push(int n){if(n>=0&&n<px.Length&&!seen[n]&&px[n].a>100){seen[n]=true;q.Push(n);}}
                    if(x>0)Push(k-1);if(x<w-1)Push(k+1);Push(k-w);Push(k+w);
                }
                if(p.pixels.Count<500)continue;
                int index=Mathf.Clamp((h-1-(p.b+p.t)/2)/(h/6),0,5)*4+Mathf.Clamp((p.l+p.r)/2/(w/4),0,3);
                if(poses[index]!=null)throw new Exception("Overlapping Mir component "+index);
                if(p.r-p.l>Cell-8||p.t-p.b>Cell-8)throw new Exception("Oversize Mir component "+index);
                poses[index]=p;
            }
            for(int i=0;i<24;i++)if(poses[i]==null)throw new Exception("Missing Mir pose "+i);
            return poses;
        }        static void Extract(string form,string folder,int stage)
        {
            var tex=new Texture2D(2,2,TextureFormat.RGBA32,false);tex.LoadImage(File.ReadAllBytes("Assets/Art/Mir/Source/"+form+".png"));
            if(tex.width!=1024||tex.height!=1536)throw new Exception("Unexpected Mir source grid");
            var px=tex.GetPixels32();var poses=FindPoses(px,tex.width,tex.height);Directory.CreateDirectory(folder);sockets[0]=new Vector2[24];sockets[1]=new Vector2[24];
            for(int row=0;row<6;row++){
                var output=new Color32[Cell*Cell*4];
                for(int f=0;f<4;f++){
                    var p0=poses[row*4+f];int l=p0.l,r=p0.r,b=p0.b,t=p0.t;
                    int ox=f%2*Cell+(Cell-(r-l+1))/2,oy=(1-f/2)*Cell+8;
                    foreach(int k in p0.pixels){int x=k%1024,y=k/1024;output[(oy+y-b)*Cell*2+ox+x-l]=px[k];}
                    Vector2 Pack(float x,float y)=>new Vector2((ox%Cell+(r-l)*x)/Cell,(8+(t-b)*y)/Cell);
                    bool down=row==5&&f>=2;
                    float hy=down?.30f:row==2&&f==2?.82f:stage==0?.62f:.70f;
                    sockets[0][row*4+f]=Pack(.92f,hy);
                    sockets[1][row*4+f]=stage==1?Pack(.91f,down?.20f:row==2&&f==2?.67f:.39f):Pack(.85f,down?.24f:row==2?.62f:.50f);
                }
                var sheet=new Texture2D(Cell*2,Cell*2,TextureFormat.RGBA32,false);sheet.SetPixels32(output);sheet.Apply();
                File.WriteAllBytes(folder+"/"+((DragonAnimationState)row)+".png",sheet.EncodeToPNG());UnityEngine.Object.DestroyImmediate(sheet);
            }
            UnityEngine.Object.DestroyImmediate(tex);
        }
    }
}
