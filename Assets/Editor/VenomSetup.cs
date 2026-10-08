using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    // Content recipe only; shared runtime/animation systems are unchanged.
    public static class VenomSetup
    {
        const int Cell=400;
        class Pose {public List<int> pixels=new List<int>();public int l=99999,b=99999,r,t;}
        [MenuItem("Dragon Tower/Production/Install Venom Character Sets")]
        public static void InstallAll()=>InstallFrom(0);
        public static void InstallEvolutions()=>InstallFrom(1);
        static void InstallFrom(int firstStage)
        {
            var dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon14.asset");
            if(dragon==null||dragon.StableId!="venom")throw new Exception("Venom data missing");
            var template=Resources.Load<DragonAnimationSet>("DragonAnimations/ZephyrBase");
            for(int stage=firstStage;stage<3;stage++){
                string form=new[]{"Base","Middle","Final"}[stage],folder="Assets/Art/Venom/"+form;
                var sockets=Extract(form,folder,stage);AssetDatabase.Refresh();
                EvolutionSpriteSetup.Install(dragon,stage,folder,"Assets/Resources/DragonAnimations/Venom"+form+".asset","Venom"+form,
                    stage==0?dragon.displayName:stage==1?dragon.intermediateName:dragon.finalName,
                    stage==0?DragonAnimationArchetype.SmallQuadruped:DragonAnimationArchetype.LargeQuadruped,
                    template,false,new[]{170f,220f,250f}[stage],true);
                var set=dragon.LoadAnimationSet(stage);var attachments=new List<DragonSpriteAttachment>();
                Vector2 IdlePoint(Vector2 p)=>((p-Vector2.one*.5f)*set.idle.displayScale+set.idle.offsets[0])*256f;
                set.anchors.characterCenter=IdlePoint(extraSockets["BodyCenter"][0]);
                set.anchors.hitPosition=set.anchors.characterCenter;
                set.anchors.attackOrigin=IdlePoint(sockets[0]);
                set.anchors.skillOrigin=IdlePoint(extraSockets["CastingCenter"][0]);
                // A folded recovery wing is narrower than Idle. Keep physical pixel size
                // consistent instead of magnifying the entire attack to match that width.
                foreach(DragonAnimationState state in Enum.GetValues(typeof(DragonAnimationState))){
                    var clip=set.Get(state);float ratio=set.idle.displayScale/clip.displayScale;
                    var ground=set.anchors.groundPosition/256f;
                    for(int f=0;f<clip.offsets.Length;f++)clip.offsets[f]=ground-(ground-clip.offsets[f])*ratio;
                    clip.displayScale=set.idle.displayScale;EditorUtility.SetDirty(clip);
                }
                for(int state=0;state<6;state++)for(int f=0;f<4;f++){
                    int i=state*4+f;var sprite=set.Get((DragonAnimationState)state).frames[f];
                    attachments.Add(new DragonSpriteAttachment{name="AttackOrigin",sprite=sprite,normalizedPosition=sockets[i],useAsAttackOrigin=true});
                    foreach(string name in stage==0?new[]{"CastingCenter","MainMouth"}:stage==1?new[]{"CastingCenter","MainMouth","LeftMouth"}:new[]{"CastingCenter","MainMouth","LeftMouth","RightMouth"})attachments.Add(new DragonSpriteAttachment{name=name,sprite=sprite,normalizedPosition=extraSockets[name][i],useAsSkillOrigin=name=="CastingCenter"});
                }
                set.attachments=attachments.ToArray();EditorUtility.SetDirty(set);
                Debug.Log("VENOM_INSTALLED "+form);
            }
            AssetDatabase.SaveAssets();
        }
        // Mouth centers authored against each source pose; follows the existing per-sprite socket pipeline.
        static readonly Dictionary<string,Vector2[]> MouthPixels=new Dictionary<string,Vector2[]> {
            {"BaseMain", new[]{new Vector2(229f,149f),new Vector2(469f,148f),new Vector2(725f,149f),new Vector2(982f,148f),new Vector2(252f,440f),new Vector2(503f,391f),new Vector2(762f,421f),new Vector2(982f,389f),new Vector2(251f,698f),new Vector2(506f,575f),new Vector2(724f,578f),new Vector2(987f,693f),new Vector2(246f,937f),new Vector2(497f,943f),new Vector2(759f,962f),new Vector2(980f,894f),new Vector2(221f,1125f),new Vector2(467f,1132f),new Vector2(739f,1166f),new Vector2(981f,1133f),new Vector2(225f,1404f),new Vector2(487f,1420f),new Vector2(742f,1455f),new Vector2(982f,1453f)}},
            {"MiddleMain", new[]{new Vector2(223f,112f),new Vector2(482f,108f),new Vector2(743f,108f),new Vector2(998f,104f),new Vector2(226f,360f),new Vector2(494f,357f),new Vector2(750f,376f),new Vector2(996f,358f),new Vector2(235f,649f),new Vector2(435f,575f),new Vector2(735f,595f),new Vector2(996f,625f),new Vector2(239f,884f),new Vector2(480f,878f),new Vector2(743f,883f),new Vector2(1000f,888f),new Vector2(222f,1119f),new Vector2(483f,1140f),new Vector2(742f,1141f),new Vector2(997f,1134f),new Vector2(222f,1371f),new Vector2(482f,1404f),new Vector2(736f,1433f),new Vector2(1001f,1427f)}},
            {"MiddleLeft", new[]{new Vector2(223f,166f),new Vector2(487f,167f),new Vector2(744f,171f),new Vector2(1000f,169f),new Vector2(224f,423f),new Vector2(489f,413f),new Vector2(756f,433f),new Vector2(988f,431f),new Vector2(235f,702f),new Vector2(488f,614f),new Vector2(770f,651f),new Vector2(999f,681f),new Vector2(241f,943f),new Vector2(485f,939f),new Vector2(743f,942f),new Vector2(1003f,945f),new Vector2(222f,1181f),new Vector2(487f,1189f),new Vector2(743f,1194f),new Vector2(1001f,1193f),new Vector2(223f,1429f),new Vector2(483f,1462f),new Vector2(737f,1466f),new Vector2(1003f,1469f)}},
            {"FinalMain", new[]{new Vector2(183f,55f),new Vector2(440f,55f),new Vector2(701f,68f),new Vector2(951f,55f),new Vector2(206f,301f),new Vector2(487f,331f),new Vector2(750f,357f),new Vector2(951f,312f),new Vector2(157f,523f),new Vector2(447f,522f),new Vector2(687f,550f),new Vector2(953f,564f),new Vector2(195f,829f),new Vector2(497f,861f),new Vector2(723f,877f),new Vector2(953f,805f),new Vector2(180f,1034f),new Vector2(390f,1031f),new Vector2(721f,1063f),new Vector2(952f,1048f),new Vector2(194f,1323f),new Vector2(441f,1339f),new Vector2(737f,1460f),new Vector2(929f,1489f)}},
            {"FinalLeft", new[]{new Vector2(153f,111f),new Vector2(412f,111f),new Vector2(670f,117f),new Vector2(921f,114f),new Vector2(145f,348f),new Vector2(410f,367f),new Vector2(668f,376f),new Vector2(921f,370f),new Vector2(139f,594f),new Vector2(406f,584f),new Vector2(650f,600f),new Vector2(919f,618f),new Vector2(169f,883f),new Vector2(456f,891f),new Vector2(716f,902f),new Vector2(912f,850f),new Vector2(145f,1071f),new Vector2(400f,1099f),new Vector2(668f,1123f),new Vector2(915f,1106f),new Vector2(153f,1385f),new Vector2(408f,1409f),new Vector2(701f,1481f),new Vector2(864f,1474f)}},
            {"FinalRight", new[]{new Vector2(234f,105f),new Vector2(490f,106f),new Vector2(751f,105f),new Vector2(1000f,108f),new Vector2(241f,359f),new Vector2(473f,387f),new Vector2(733f,409f),new Vector2(1000f,369f),new Vector2(208f,584f),new Vector2(492f,589f),new Vector2(750f,605f),new Vector2(1000f,617f),new Vector2(232f,880f),new Vector2(510f,894f),new Vector2(766f,908f),new Vector2(1000f,855f),new Vector2(220f,1082f),new Vector2(472f,1068f),new Vector2(751f,1117f),new Vector2(1004f,1101f),new Vector2(237f,1381f),new Vector2(490f,1401f),new Vector2(764f,1473f),new Vector2(1004f,1480f)}},
        };
        static readonly Dictionary<string,Vector2[]> extraSockets=new Dictionary<string,Vector2[]>();
        static Vector2[] Extract(string form,string folder,int stage)
        {
            var tex=new Texture2D(2,2,TextureFormat.RGBA32,false);tex.LoadImage(File.ReadAllBytes("Assets/Art/Venom/Source/"+form+".png"));
            int w=tex.width,h=tex.height;var px=tex.GetPixels32();var seen=new bool[px.Length];var poses=new Pose[24];
            for(int start=0;start<px.Length;start++){
                if(seen[start]||px[start].a<=32)continue;var p=new Pose();var q=new Stack<int>();q.Push(start);seen[start]=true;
                while(q.Count>0){int k=q.Pop(),x=k%w,y=k/w;p.pixels.Add(k);p.l=Math.Min(p.l,x);p.r=Math.Max(p.r,x);p.b=Math.Min(p.b,y);p.t=Math.Max(p.t,y);
                    void Push(int n){if(n>=0&&n<px.Length&&!seen[n]&&px[n].a>32){seen[n]=true;q.Push(n);}}
                    if(x>0)Push(k-1);if(x<w-1)Push(k+1);Push(k-w);Push(k+w);
                }
                if(p.pixels.Count<500)continue;
                int col=Mathf.Clamp((p.l+p.r)/2/(w/4),0,3),row=Mathf.Clamp((h-1-(p.b+p.t)/2)/(h/6),0,5),index=row*4+col;
                if(poses[index]==null||p.pixels.Count>poses[index].pixels.Count)poses[index]=p;
            }
            Directory.CreateDirectory(folder);var sockets=new Vector2[24];foreach(string name in new[]{"CastingCenter","MainMouth","LeftMouth","RightMouth","BodyCenter"})extraSockets[name]=new Vector2[24];
            for(int row=0;row<6;row++){
                var output=new Color32[Cell*Cell*4];int ow=Cell*2;
                for(int frame=0;frame<4;frame++){
                    int sourceFrame=frame;
                    var p=poses[row*4+sourceFrame];if(p==null)throw new Exception("Missing Venom pose");
                    if(p.r-p.l>=Cell-12||p.t-p.b>=Cell-12)throw new Exception("Venom pose exceeds packing cell");
                    int ox=frame%2*Cell+(Cell-(p.r-p.l+1))/2,oy=(1-frame/2)*Cell+12;
                    foreach(int k in p.pixels)output[(oy+k/w-p.b)*ow+ox+k%w-p.l]=px[k];
                    Vector2 Pack(Vector2 point)=>new Vector2((ox%Cell+(p.r-p.l)*point.x)/Cell,(12+(p.t-p.b)*point.y)/Cell);
                    bool low=row==5&&frame>=2;
                    Vector2 Mouth(string name){var q=MouthPixels[form+name][row*4+frame];return new Vector2((q.x-p.l)/(p.r-p.l),(h-1-q.y-p.b)/(p.t-p.b));}
                    var main=Mouth("Main");
                    var left=stage>0?Mouth("Left"):main;
                    var right=stage==2?Mouth("Right"):main;
                    sockets[row*4+frame]=Pack(main);
                    extraSockets["CastingCenter"][row*4+frame]=Pack(main);
                    extraSockets["MainMouth"][row*4+frame]=Pack(main);
                    extraSockets["LeftMouth"][row*4+frame]=Pack(left);
                    extraSockets["RightMouth"][row*4+frame]=Pack(right);
                    extraSockets["BodyCenter"][row*4+frame]=Pack(new Vector2(.58f,low?.20f:.33f));


                }
                var sheet=new Texture2D(ow,ow,TextureFormat.RGBA32,false);sheet.SetPixels32(output);sheet.Apply();
                File.WriteAllBytes(folder+"/"+((DragonAnimationState)row)+".png",sheet.EncodeToPNG());UnityEngine.Object.DestroyImmediate(sheet);
            }
            UnityEngine.Object.DestroyImmediate(tex);
            return sockets;
        }
    }
}



