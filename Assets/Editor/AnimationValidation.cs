using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace DragonTower.Editor
{
    // Editor-only rendering and behavior checks. Does not create a player or Web build.
    public static class AnimationValidation
    {
        static BattleView view;
        static BattleModel battle;
        static DragonData data;
        static Camera camera;
        static RenderTexture target;
        static void Check(bool ok,string message){if(!ok)throw new Exception(message);Debug.Log("ANIMATION_CHECK "+message);}
        public static void Run()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");
            view=UnityEngine.Object.FindFirstObjectByType<BattleView>();view.ApplyPixelSkin();
            data=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon0.asset");
            camera=UnityEngine.Object.FindFirstObjectByType<Camera>();target=new RenderTexture(480,850,24);target.Create();
            camera.targetTexture=target;camera.orthographic=true;camera.orthographicSize=425;
            var canvas=view.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            Directory.CreateDirectory("Validation/Animation");
            Reset();var home=view.playerArt.rectTransform.anchoredPosition;Step(.25f);
            Check(view.playerArt.rectTransform.anchoredPosition!=home,"Idle pose changes");Capture("01-idle");
            battle.Attack();Step(.09f);Capture("02-attack");
            Check(view.enemyArt.rectTransform.localScale!=Vector3.one,"Enemy animation active");
            Reset();battle.Skill();Step(.19f);Capture("03-skill");
            Check(UnityEngine.Object.FindObjectsByType<Image>(FindObjectsSortMode.None).Length>10,"Effect images active");
            var dragons=new DragonData[8];for(int i=0;i<dragons.Length;i++)dragons[i]=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon"+i+".asset");
            string[] skillNames={"03a-fire-skill","03b-frost-skill","03c-wind-skill","03d-earth-skill","03e-lightning-skill","03f-water-skill","03g-dark-skill","03h-light-skill"};
            for(int i=0;i<dragons.Length;i++)
            {
                data=dragons[i];Reset();battle.Skill();Step(.19f);Capture(skillNames[i]);
                Check(data.skillEffect==(SkillEffectKind)i,data.displayName+" uses its own skill effect kind");
                Check(data.skill!=null&&data.skill.battleVfx!=null,data.displayName+" has asset VFX");
            }
            var skin=AssetDatabase.LoadAssetAtPath<BattleSkin>("Assets/Resources/PixelBattleSkin.asset");
            Check(skin!=null&&skin.floorMonsters!=null&&skin.floorMonsters.Length==5,"Five regular monster sprites installed");
            for(int i=0;i<skin.floorMonsters.Length;i++)
            {
                view.SetEncounter(new BattleEnemyStats{displayName="Monster "+i,maxHP=240,damage=18,interval=2.4f},i+1,false,skin.floorMonsters[i]);
                var image=view.enemyArt.transform.Find("Pixel sprite").GetComponent<Image>();
                Check(image.sprite==skin.floorMonsters[i],"Regular monster sprite "+i+" can be shown");Capture("monster-"+i);
            }
            Check(skin.ancientGolemBoss!=null,"Ancient golem boss sprite installed");
            view.SetEncounter(new BattleEnemyStats{displayName="고대 룬 골렘",maxHP=360,damage=24,interval=2.2f},10,true,skin.ancientGolemBoss);
            Check(view.enemyArt.transform.Find("Pixel sprite").GetComponent<Image>().sprite==skin.ancientGolemBoss,"Floor ten uses ancient golem boss sprite");Capture("monster-boss-10");
            view.SetEncounter(BattleEnemyStats.Normal(),1,false,skin.floorMonsters[0]);
            data=dragons[0];
            Reset();Step(2.12f);Capture("04-enemy-windup");battle.Dodge();Step(.29f);Capture("05-dodge");
            Check(battle.PlayerHP==data.maxHP,"Dodge prevents damage");
            Reset();Step(2.46f);Capture("06-enemy-hit");Check(battle.PlayerHP==data.maxHP-18,"Enemy damage and reaction");
            var motion=view.GetComponent<BattleAnimation>();int pool=motion.PoolObjectCount;
            for(int i=0;i<180;i++){view.PlayCue(CombatCue.Skill,38);view.StepAnimation(battle,.016f);}
            Check(motion.PoolObjectCount==pool,"Effect pool stays bounded during spam");
            var assetVfx=view.GetComponent<BattleAssetVfx>();int assetInstances=assetVfx.InstanceCount;
            Check(view.frame.Find("Battle effects (pooled)").childCount==pool+1,"No extra UI effect objects created");
            Check(assetVfx.InstanceCount==assetInstances,"Asset VFX instance is reused during spam");
            Reset();Check(view.frame.Find("Battle effects (pooled)").GetComponentsInChildren<Graphic>().Length==1,"Restart clears active particles and numbers");
            Check(view.playerArt.rectTransform.localScale==Vector3.one,"Restart resets transforms");
            battle.Tick(30);view.Show(battle);Check(!view.resultPanel.activeSelf,"Final hit animation precedes result");
            view.StepAnimation(battle,.7f);view.Show(battle);Check(view.resultPanel.activeSelf,"Result shown after short animation");
            Capture("07-defeat");
            Reset();Directory.CreateDirectory("Validation/Animation/Frames");
            for(int i=0;i<140;i++)
            {
                if(i==18||i==28||i==38)battle.Attack();
                if(i==52)battle.Skill();
                if(i==92)battle.Dodge();
                Step(.05f);Capture("Frames/frame-"+i.ToString("000"));
            }
            camera.targetTexture=null;target.Release();UnityEngine.Object.DestroyImmediate(target);
            Debug.Log("ANIMATION_VALIDATION_OK_NO_BUILD");
        }
        static void Reset()
        {
            view.SetDragonArt(data);battle=new BattleModel(data.Snapshot());battle.Cue+=view.PlayCue;
            battle.Feedback+=text=>view.message.text=text;view.message.text="애니메이션 테스트";view.Show(battle);
        }
        static void Step(float duration)
        {
            while(duration>0){float dt=Mathf.Min(.016f,duration);battle.Tick(dt);view.StepAnimation(battle,dt);view.Show(battle);duration-=dt;}
        }
        static void Capture(string name)
        {
            Canvas.ForceUpdateCanvases();view.frame.localScale=Vector3.one;view.frame.anchoredPosition=Vector2.zero;
            camera.Render();var old=RenderTexture.active;RenderTexture.active=target;
            var texture=new Texture2D(480,850,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,480,850),0,0);texture.Apply();
            File.WriteAllBytes("Validation/Animation/"+name+".png",texture.EncodeToPNG());
            RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(texture);
        }
    }
}
