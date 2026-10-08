using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    // Content recipe only; shared runtime/animation systems are unchanged.
    public static class NovaSetup
    {
        const int Cell=400;
        class Pose {public List<int> pixels=new List<int>();public int l=99999,b=99999,r,t;}
        [MenuItem("Dragon Tower/Production/Install Nova Character Sets")]
        public static void InstallAll()
        {
            var dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon6.asset");
            if(dragon==null||dragon.StableId!="nova")throw new Exception("Nova data missing");
            var template=Resources.Load<DragonAnimationSet>("DragonAnimations/ZephyrBase");
            for(int stage=0;stage<3;stage++){
                string form=new[]{"Base","Middle","Final"}[stage],folder="Assets/Art/Nova/"+form;
                var sockets=Extract(form,folder,stage);AssetDatabase.Refresh();
                EvolutionSpriteSetup.Install(dragon,stage,folder,"Assets/Resources/DragonAnimations/Nova"+form+".asset","Nova"+form,
                    stage==0?dragon.displayName:stage==1?dragon.intermediateName:dragon.finalName,
                    DragonAnimationArchetype.Biped,
                    template,false,new[]{160f,208f,250f}[stage],true);
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
                    foreach(string name in new[]{"CastingCenter","CosmicCore"})attachments.Add(new DragonSpriteAttachment{name=name,sprite=sprite,normalizedPosition=extraSockets[name][i],useAsSkillOrigin=name=="CastingCenter"});
                }
                set.attachments=attachments.ToArray();EditorUtility.SetDirty(set);
                Debug.Log("NOVA_INSTALLED "+form);
            }
            AssetDatabase.SaveAssets();
        }
        // Authored chest core centers in the approved Final source sheet (top-left pixels).
        static readonly Vector2[] FinalCore={new Vector2(211,138),new Vector2(467,141),new Vector2(721,139),new Vector2(977,139),new Vector2(202,394),new Vector2(455,391),new Vector2(739,405),new Vector2(980,390),new Vector2(191,631),new Vector2(415,627),new Vector2(674,618),new Vector2(977,632),new Vector2(199,894),new Vector2(451,878),new Vector2(738,904),new Vector2(979,870),new Vector2(195,1094),new Vector2(460,1088),new Vector2(712,1120),new Vector2(978,1105),new Vector2(177,1371),new Vector2(438,1390),new Vector2(666,1437),new Vector2(926,1443)};
        static readonly Dictionary<string,Vector2[]> extraSockets=new Dictionary<string,Vector2[]>();
        static Vector2[] Extract(string form,string folder,int stage)
        {
            var tex=new Texture2D(2,2,TextureFormat.RGBA32,false);tex.LoadImage(File.ReadAllBytes("Assets/Art/Nova/Source/"+form+".png"));
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
            Directory.CreateDirectory(folder);var sockets=new Vector2[24];foreach(string name in new[]{"CastingCenter","CosmicCore","BodyCenter"})extraSockets[name]=new Vector2[24];
            for(int row=0;row<6;row++){
                var output=new Color32[Cell*Cell*4];int ow=Cell*2;
                for(int frame=0;frame<4;frame++){
                    int sourceFrame=frame;
                    var p=poses[row*4+sourceFrame];if(p==null)throw new Exception("Missing Nova pose");
                    if(p.r-p.l>=Cell-12||p.t-p.b>=Cell-12)throw new Exception("Nova pose exceeds packing cell");
                    int ox=frame%2*Cell+(Cell-(p.r-p.l+1))/2,oy=(1-frame/2)*Cell+12;
                    foreach(int k in p.pixels)output[(oy+k/w-p.b)*ow+ox+k%w-p.l]=px[k];
                    Vector2 Pack(Vector2 point)=>new Vector2((ox%Cell+(p.r-p.l)*point.x)/Cell,(12+(p.t-p.b)*point.y)/Cell);
                    // Content sockets follow the hands and chest of each Nova pose.
                    bool low=row==5&&frame>=2;
                    var point=new Vector2(.88f,low?.22f:.44f);
                    if(row==1)point=new Vector2(new[]{.83f,.96f,.98f,.88f}[frame],.43f);
                    if(row==2)point=new Vector2(new[]{.82f,.84f,.90f,.87f}[frame],new[]{.37f,.50f,.48f,.42f}[frame]);
                    sockets[row*4+frame]=Pack(point);
                    var core=new Vector2(stage==2?.62f:.64f,low?.20f:row==2&&frame==2?.44f:.40f);
                    if(stage==2){var q=FinalCore[row*4+frame];core=new Vector2((q.x-p.l)/(p.r-p.l),(h-1-q.y-p.b)/(p.t-p.b));}
                    extraSockets["CastingCenter"][row*4+frame]=Pack(core);
                    extraSockets["CosmicCore"][row*4+frame]=Pack(core);
                    extraSockets["BodyCenter"][row*4+frame]=Pack(core);


                }
                var sheet=new Texture2D(ow,ow,TextureFormat.RGBA32,false);sheet.SetPixels32(output);sheet.Apply();
                File.WriteAllBytes(folder+"/"+((DragonAnimationState)row)+".png",sheet.EncodeToPNG());UnityEngine.Object.DestroyImmediate(sheet);
            }
            UnityEngine.Object.DestroyImmediate(tex);
            return sockets;
        }
    }
}





