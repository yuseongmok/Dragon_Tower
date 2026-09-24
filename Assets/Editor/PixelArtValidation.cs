using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class PixelArtValidation
    {
        // Run in an isolated project copy via batchmode; never replaces the user's open scene.
        public static void ValidateAndBuild()
        {
            PixelArtSetup.Install();
            PrototypeSetup.Verify();
            EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");
            var view=UnityEngine.Object.FindFirstObjectByType<BattleView>();
            var data=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon0.asset");
            view.ApplyPixelSkin();view.SetDragonArt(data);
            var battle=new BattleModel(data.Snapshot());
            view.Bind(()=>battle.Attack(),()=>battle.Skill(),()=>battle.Dodge(),()=>{});
            view.Show(battle);
            var skin=Resources.Load<BattleSkin>("PixelBattleSkin");
            foreach(var sprite in new[]{skin.babyDragon,skin.rockSlime,skin.towerBackground})
                Debug.Log("PIXEL_TEXTURE "+sprite.name+" "+sprite.texture.width+"x"+sprite.texture.height+" readable="+sprite.texture.isReadable+" filter="+sprite.texture.filterMode);
            var camera=UnityEngine.Object.FindFirstObjectByType<Camera>();
            var target=new RenderTexture(480,850,24);target.Create();camera.targetTexture=target;
            camera.orthographic=true;camera.orthographicSize=425;
            var canvas=view.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            Canvas.ForceUpdateCanvases();view.frame.localScale=Vector3.one;view.frame.anchoredPosition=Vector2.zero;
            camera.Render();
            var old=RenderTexture.active;RenderTexture.active=target;
            var capture=new Texture2D(480,850,TextureFormat.RGB24,false);capture.ReadPixels(new Rect(0,0,480,850),0,0);capture.Apply();
            Directory.CreateDirectory("Validation");File.WriteAllBytes("Validation/pixel-battle.png",capture.EncodeToPNG());
            RenderTexture.active=old;camera.targetTexture=null;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(capture);
            view.attackButton.onClick.Invoke();if(battle.EnemyHP!=BattleModel.EnemyMaxHP-data.attackDamage)throw new Exception("Attack button wiring failed");
            view.skillButton.onClick.Invoke();if(battle.EnemyHP!=BattleModel.EnemyMaxHP-data.attackDamage-data.skill.damage)throw new Exception("Skill button wiring failed");
            view.dodgeButton.onClick.Invoke();if(battle.DodgeUntil<=0)throw new Exception("Dodge button wiring failed");
            Debug.Log("PIXEL_UI_BINDINGS_OK");
            // Reload to discard test-only camera changes before building the saved scene.
            EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");
            PrototypeSetup.BuildWeb();
        }
    }
}
