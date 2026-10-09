using System;
using System.IO;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;

namespace DragonTower.Editor
{
    // Samples the production presentation coroutine at 30 Hz for a stable review video.
    [InitializeOnLoad] public static class RewardRoomReview
    {
        const string Flag="RewardRoomReview";
        const string Out="C:/Users/PC/Documents/Codex/RewardRooms-20261009/";
        static CollectionFlow flow;static TowerRun run;static IEnumerator animation;
        static int stage,frame;static FieldInfo elapsed;
        static RewardRoomReview(){if(SessionState.GetBool(Flag,false))EditorApplication.update+=Tick;}
        public static void Run(){Environment.SetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE","DragonTower.Test.RoomReview");Directory.CreateDirectory(Out+"frames");SessionState.SetBool(Flag,true);EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");EditorApplication.EnterPlaymode();}
        static object Call(string n,params object[] a)=>typeof(CollectionFlow).GetMethod(n,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).Invoke(flow,a);
        static void Capture(string name)=>typeof(AugmentPresentationValidation).GetMethod("Capture",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{Out+name+".png"});
        static void SetRun(int floor,params TowerRoomKind[] choices)
        {
            var d=flow.Session.Selected;run=new TowerRun(d.maxHP,0,1,d.elementType,d.alternateSkillElement,d.StableId,d);run.AttachInstance(flow.Session.Profile.selectedInstanceId);var save=run.Capture();save.Floor=floor;save.choices=choices;run=TowerRun.Restore(save,ContentDatabase.Load());typeof(CollectionFlow).GetField("towerRun",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(flow,run);
        }
        static void StartSample(IEnumerator iterator){animation=iterator;animation.MoveNext();elapsed=animation.GetType().GetFields(BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance).First(f=>f.Name.Contains("elapsed"));frame=1;}
        static bool Sample(string prefix,int total)
        {
            elapsed.SetValue(animation,frame/30f-Time.unscaledDeltaTime);bool more=animation.MoveNext();Capture("frames/"+prefix+frame.ToString("D3"));frame++;return !more||frame>total;
        }
        static void Tick()
        {
            try
            {
                if(!EditorApplication.isPlaying)return;
                if(flow==null)
                {
                    flow=UnityEngine.Object.FindFirstObjectByType<CollectionFlow>();if(flow?.Session==null){flow=null;return;}if(flow.Session.Selected==null)flow.Session.Hatch(0);
                    SetRun(9,TowerRoomKind.Item,TowerRoomKind.Augment);Call("ShowTowerChoices");Capture("stairs-two-doors");Capture("frames/door000");run.BeginFloorRoom(0,18421);StartSample((IEnumerator)Call("EnterTowerDoor",0));return;
                }
                if(stage==0&&Sample("door",39))
                {
                    Call("ShowItemRoom");Capture("chest-closed");Capture("frames/chest000");var choices=(ItemData[])Call("ChestChoices");run.OpenChest(choices.Select(x=>x.StableId).ToArray());var chest=flow.RoomButton.GetComponent<RectTransform>();StartSample((IEnumerator)Call("AnimateTreasure",chest,choices));stage=1;
                }
                else if(stage==1)
                {
                    bool done=Sample("chest",58);if(frame==35)Capture("chest-opening");if(done){SetRun(10,TowerRoomKind.Boss);Call("ShowTowerChoices");Capture("stairs-boss-door");Capture("frames/boss000");run.BeginFloorRoom(0,18421);StartSample((IEnumerator)Call("EnterTowerDoor",0));stage=2;}
                }
                else if(stage==2&&Sample("boss",45))
                {
                    Debug.Log("ROOM_REVIEW_CAPTURE_OK");SessionState.SetBool(Flag,false);EditorApplication.Exit(0);stage=3;
                }
            }
            catch(Exception e){Debug.LogException(e);SessionState.SetBool(Flag,false);EditorApplication.Exit(1);}
        }
        public static void Build()
        {
            var report=BuildPipeline.BuildPlayer(new[]{"Assets/Scenes/Battle.unity"},Out+"WebGL",BuildTarget.WebGL,BuildOptions.None);
            Debug.Log("REWARD_ROOM_WEBGL_"+report.summary.result);EditorApplication.Exit(report.summary.result==BuildResult.Succeeded?0:1);
        }
    }
}
