using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower.Editor
{
    // Run only in an isolated copy. Tests real Awake/Start/Update across two Play sessions.
    [InitializeOnLoad]
    public static class DragonVisibilityValidation
    {
        const string Key="DragonTower.VisibilityCheck";
        static double readyAt;
        static DragonVisibilityValidation()
        {
            if(!SessionState.GetBool(Key,false))return;
            EditorApplication.playModeStateChanged+=Changed;
            if(EditorApplication.isPlaying)Schedule();
        }
        public static void Run()
        {
            string testKey=Environment.GetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE");
            if(string.IsNullOrEmpty(testKey)||!testKey.StartsWith("DragonTower.Test."))throw new Exception("Isolated test save key required.");
            EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");
            SessionState.SetBool(Key,true);SessionState.SetInt(Key+"Stage",0);
            EditorApplication.playModeStateChanged-=Changed;EditorApplication.playModeStateChanged+=Changed;
            EditorApplication.isPlaying=true;
        }
        static void Changed(PlayModeStateChange state)
        {
            if(!SessionState.GetBool(Key,false))return;
            if(state==PlayModeStateChange.EnteredPlayMode)Schedule();
            if(state==PlayModeStateChange.EnteredEditMode)
            {
                if(SessionState.GetInt(Key+"Stage",0)==1)EditorApplication.isPlaying=true;
                else
                {
                    string testKey=Environment.GetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE");
                    PlayerPrefs.DeleteKey(testKey+".A");PlayerPrefs.DeleteKey(testKey+".B");PlayerPrefs.Save();
                    SessionState.SetBool(Key,false);Debug.Log("DRAGON_VISIBILITY_PLAYMODE_OK_NO_BUILD");EditorApplication.Exit(0);
                }
            }
        }
        static void Schedule(){readyAt=EditorApplication.timeSinceStartup+1;EditorApplication.update-=Check;EditorApplication.update+=Check;}
        static void Check()
        {
            if(!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<readyAt)return;
            EditorApplication.update-=Check;
            try
            {
                var controller=UnityEngine.Object.FindFirstObjectByType<BattleController>();
                if(controller.Flow.Session.Selected==null)controller.Flow.Session.Hatch(0);
                controller.Flow.ShowLobby();controller.Flow.EnterTower();
                var view=UnityEngine.Object.FindFirstObjectByType<BattleView>();
                var dragons=new DragonData[8];for(int i=0;i<dragons.Length;i++)dragons[i]=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon"+i+".asset");
                Require(dragons[0].element=="불꽃"&&dragons[1].element=="얼음"&&dragons[2].element=="바람","Preserves user element labels");
                foreach(var dragon in dragons)
                {
                    Require(dragon.battleSprite!=null,dragon.displayName+" data has its own sprite");
                    view.SetDragonArt(dragon);CheckImage(view,dragon);Capture(view,dragon.StableId);
                }
                var original=dragons[0];
                var temporary=UnityEngine.Object.Instantiate(original);
                foreach(var label in new[]{"FIRE","불꽃","표시 이름 변경"})
                {temporary.element=label;view.SetDragonArt(temporary);CheckImage(view,temporary);}
                temporary.battleSprite=null;view.SetDragonArt(temporary);
                Require(view.playerArt.enabled,"Missing image falls back to placeholder");
                view.SetDragonArt(original);CheckImage(view,original);
                UnityEngine.Object.Destroy(temporary);
                SessionState.SetInt(Key+"Stage",SessionState.GetInt(Key+"Stage",0)+1);
                EditorApplication.isPlaying=false;
            }
            catch(Exception ex){Debug.LogException(ex);SessionState.SetBool(Key,false);EditorApplication.Exit(1);}
        }
        static void CheckImage(BattleView view,DragonData data)
        {
            var image=view.playerArt.transform.Find("Pixel sprite").GetComponent<Image>();
            Canvas.ForceUpdateCanvases();
            Require(image.gameObject.activeInHierarchy&&image.enabled&&image.sprite==data.battleSprite&&image.color.a>0,"Visible sprite: "+data.element);
            Require(!view.playerArt.enabled,"Placeholder disabled while sprite visible");
            Require(image.rectTransform.rect.width>0&&image.rectTransform.rect.height>0,"Sprite has nonzero layout");
        }
        static void Require(bool valid,string message){if(!valid)throw new Exception(message);Debug.Log("VISIBILITY_CHECK "+message);}
        static void Capture(BattleView view,string speciesId)
        {
            var camera=UnityEngine.Object.FindFirstObjectByType<Camera>();var target=new RenderTexture(480,850,24);target.Create();
            camera.targetTexture=target;camera.orthographic=true;camera.orthographicSize=425;
            var canvas=view.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            Canvas.ForceUpdateCanvases();view.frame.localScale=Vector3.one;view.frame.anchoredPosition=Vector2.zero;camera.Render();
            var old=RenderTexture.active;RenderTexture.active=target;var texture=new Texture2D(480,850,TextureFormat.RGB24,false);
            texture.ReadPixels(new Rect(0,0,480,850),0,0);texture.Apply();Directory.CreateDirectory("Validation");
            File.WriteAllBytes("Validation/dragon-visibility-"+speciesId+".png",texture.EncodeToPNG());
            RenderTexture.active=old;camera.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(texture);
        }
    }
}
