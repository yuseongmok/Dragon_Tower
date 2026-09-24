using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace DragonTower.Editor
{
    [InitializeOnLoad]
    public static class CollectionValidation
    {
        const string Flag="DragonTower.CollectionCheck";
        static double readyAt;
        static int phase;
        static string Prefix=>Environment.GetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE");
        static CollectionValidation()
        {
            if(!SessionState.GetBool(Flag,false))return;
            EditorApplication.playModeStateChanged+=Changed;
            if(EditorApplication.isPlaying)Schedule();
        }
        static void Check(bool ok,string message){if(!ok)throw new Exception(message);Debug.Log("COLLECTION_CHECK "+message);}
        static DragonData[] Catalog()
        {
            var catalog=new DragonData[8];
            for(int i=0;i<catalog.Length;i++)catalog[i]=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon"+i+".asset");
            return catalog;
        }
        public static void Run()
        {
            if(string.IsNullOrEmpty(Prefix)||!Prefix.StartsWith("DragonTower.Test."))throw new Exception("An isolated test save prefix is required.");
            UnitChecks();
            EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");
            SessionState.SetBool(Flag,true);SessionState.SetInt(Flag+"Stage",0);
            EditorApplication.playModeStateChanged-=Changed;EditorApplication.playModeStateChanged+=Changed;
            EditorApplication.isPlaying=true;
        }
        static void DeleteOwn(string key){PlayerPrefs.DeleteKey(key+".A");PlayerPrefs.DeleteKey(key+".B");PlayerPrefs.Save();}
        static void UnitChecks()
        {
            var catalog=Catalog();
            for(int i=0;i<catalog.Length;i++)
            {
                Check(catalog[i]!=null&&catalog[i].evolutionSheet!=null,"Evolution sheet is linked for dragon "+i);
                Check(!string.IsNullOrWhiteSpace(catalog[i].intermediateName)&&!string.IsNullOrWhiteSpace(catalog[i].finalName),"Evolution names are linked for dragon "+i);
                Check(catalog[i].SpriteForStage(1)!=null&&catalog[i].SpriteForStage(2)!=null,"Both evolution sprites can be created for dragon "+i);
            }
            var database=ContentDatabase.Load();
            Check(database!=null&&database.augments!=null&&database.augments.Length==29,"All twenty-nine augment assets are registered");
            Check(database.skills!=null&&database.skills.Length==32,"All thirty-two skill assets are registered");
            foreach(var element in new[]{ElementType.Fire,ElementType.Ice,ElementType.Wind,ElementType.Earth,ElementType.Lightning,ElementType.Water,ElementType.Dark,ElementType.Light})
                Check(database.skills.Count(s=>s.elementType==element)==4,"Four replacement skills exist for "+element);
            Check(database.augments.Select(x=>x.StableId).Distinct().Count()==29,"Augment stable IDs are unique");
            Check(database.PickAugments(3,1234).Select(x=>x.StableId).Distinct().Count()==3,"Augment room offers three different choices");
            foreach(var augment in database.augments)Check(augment!=null&&!string.IsNullOrWhiteSpace(augment.description),"Augment has a readable description: "+augment.StableId);
            Check(catalog[0].skillEffect==SkillEffectKind.Fire&&catalog[1].skillEffect==SkillEffectKind.Frost&&catalog[2].skillEffect==SkillEffectKind.Wind,"Each dragon has its own skill effect kind");
            Check(catalog[0].elementType==ElementType.Fire&&catalog[1].elementType==ElementType.Ice&&catalog[2].elementType==ElementType.Wind,"Dragon combat elements are stored independently of labels");
            Check(ElementRules.Damage(20,ElementType.Water,ElementType.Fire)==30&&ElementRules.Damage(20,ElementType.Fire,ElementType.Water)==20,"Element advantage applies only in the attack direction");
            Check(ElementRules.HasAdvantage(ElementType.Dark,ElementType.Light)&&ElementRules.HasAdvantage(ElementType.Light,ElementType.Dark),"Dark and light are mutually effective");
            Check(TowerRun.Pick(1,0)==TowerRoomKind.Monster&&TowerRun.Pick(1,44)==TowerRoomKind.Monster,"Monster room weight boundary");
            Check(TowerRun.Pick(1,45)==TowerRoomKind.Item&&TowerRun.Pick(1,56)==TowerRoomKind.Item,"Item room weight boundary");
            Check(TowerRun.Pick(1,57)==TowerRoomKind.Augment&&TowerRun.Pick(1,66)==TowerRoomKind.Augment,"Augment room weight boundary");
            Check(TowerRun.Pick(1,67)==TowerRoomKind.Recovery&&TowerRun.Pick(1,76)==TowerRoomKind.Recovery,"Recovery room weight boundary");
            Check(TowerRun.Pick(1,77)==TowerRoomKind.Nest&&TowerRun.Pick(1,79)==TowerRoomKind.Nest,"Rare nest room weight boundary");
            Check(TowerRun.Pick(1,80)==TowerRoomKind.Gold&&TowerRun.Pick(1,91)==TowerRoomKind.Gold,"Gold room weight boundary");
            Check(TowerRun.Pick(1,92)==TowerRoomKind.Shop&&TowerRun.Pick(1,99)==TowerRoomKind.Shop,"Shop room weight boundary");
            var run=new TowerRun(100,0,50);
            for(int floor=2;floor<=10;floor++){run.ChooseRoom(0);run.AdvanceFloor(0,50);}
            Check(run.Floor==10&&run.Choices.Count==1&&run.Choices[0]==TowerRoomKind.Boss,"Every tenth floor offers only the boss");
            var fireRun=new TowerRun(100,0,50,ElementType.Fire);var fireReplacement=database.skills.First(s=>s.elementType==ElementType.Fire&&s.StableId!=catalog[0].skill.StableId);
            fireRun.ReplaceSkill(fireReplacement,false);Check(fireRun.CurrentSkill(catalog[0].skill)==fireReplacement,"Same-element skill replacement is stored for the run");
            bool rejected=false;try{fireRun.ReplaceSkill(database.skills.First(s=>s.elementType==ElementType.Ice),false);}catch(InvalidOperationException){rejected=true;}
            Check(rejected,"Different-element skill replacement is rejected");
            var growth=new TowerRun(100,0,50);growth.ChooseRoom(0);
            for(int i=0;i<4;i++)growth.RecordBattleVictory(42);
            Check(growth.Level==5&&growth.CurrentHP==42&&growth.PendingLevelAugments==1,"Each monster levels up and level five queues augment");
            var carried=new BattleModel(growth.BuildBattleStats(catalog[0].Snapshot()),BattleEnemyStats.Normal(),growth.CurrentHP);
            Check(carried.PlayerHP==42,"Remaining HP carries into the next battle");
            growth.AddAugment("test",true);Check(growth.PendingLevelAugments==0&&growth.Augments.Count==1,"Level augment selection is recorded");
            var evolution=new TowerRun(100,0,50);
            for(int i=1;i<20;i++)evolution.RecordBattleVictory(100);
            Check(evolution.Level==20&&evolution.PendingEvolutionStage==1,"Level twenty queues intermediate evolution");
            evolution.ConsumeEvolution();Check(evolution.PendingEvolutionStage==0,"Intermediate evolution is consumed once");
            for(int i=20;i<40;i++)evolution.RecordBattleVictory(100);
            Check(evolution.Level==40&&evolution.PendingEvolutionStage==2,"Level forty queues final evolution");
            evolution.ConsumeEvolution();Check(evolution.PendingEvolutionStage==0,"Final evolution is consumed once");
            growth.Heal(20);Check(growth.CurrentHP==62,"Healing changes run HP only when requested");
            growth.AddGold(50);Check(growth.SpendGold(30)&&growth.Gold==20,"Gold can pay shop cost");
            growth.AddItem("healing-potion");Check(growth.CurrentHP==97,"Healing item restores run HP");
            var bossBattle=new BattleModel(catalog[0].Snapshot(),new BattleEnemyStats{displayName="test boss",maxHP=360,damage=24,interval=2.2f});
            Check(bossBattle.EnemyHP==360&&bossBattle.CurrentEnemyMaxHP==360,"Battle accepts boss health");
            bossBattle.Tick(2.2f);Check(bossBattle.PlayerHP==catalog[0].maxHP-24,"Battle accepts boss attack settings");
            string nestKey=Prefix+".nest";
            try
            {
                var nestSession=new CollectionSession(new ProfileStore(nestKey),catalog);nestSession.Hatch(0);
                nestSession.RegisterHatchedDragon(catalog[1]);
                nestSession=new CollectionSession(new ProfileStore(nestKey),catalog);
                Check(nestSession.Profile.dragons.Count==2&&nestSession.Owns(catalog[1].StableId),"Nest hatch persists in collection and codex");
            }
            finally{DeleteOwn(nestKey);}
            for(int i=0;i<catalog.Length;i++)
            {
                string key=Prefix+".unit"+i;
                try
                {
                    var store=new ProfileStore(key);var s=new CollectionSession(store,catalog);
                    Check(s.Profile.eggs==1&&s.Profile.dragons.Count==0,"One starter egg for fresh profile "+i);
                    var d=s.Hatch(i);string id=s.Profile.selectedInstanceId;
                    Check(d==catalog[i]&&s.Profile.eggs==0&&s.Profile.dragons.Count==1,"Hatch catalog species "+i);
                    Check(s.Hatch(i)==null&&s.Profile.dragons.Count==1,"Repeated tap cannot duplicate hatch "+i);
                    s=new CollectionSession(new ProfileStore(key),catalog);
                    Check(s.Profile.selectedInstanceId==id&&s.Selected==d&&s.Profile.eggs==0,"Reload preserves hatch "+i);
                    Check(!s.Select("not-owned"),"Unowned selection rejected "+i);
                    Check(s.Owns(d.StableId),"Hatched species registered "+i);
                    var clone=UnityEngine.Object.Instantiate(d);clone.name="renamed asset";clone.displayName="새 표시 이름";
                    var changed=(DragonData[])catalog.Clone();changed[i]=clone;
                    Check(new CollectionSession(new ProfileStore(key),changed).Selected==clone,"Stable ID survives renamed asset and label "+i);
                    UnityEngine.Object.DestroyImmediate(clone);
                    // Two normal saves give us a backup with the same post-hatch state.
                    store=new ProfileStore(key);var p=store.Load();store.Save(p);
                    PlayerPrefs.SetString(key+".A","broken");PlayerPrefs.Save();
                    var recoveredStore=new ProfileStore(key);var recovered=recoveredStore.Load();
                    Check(recoveredStore.RecoveredBackup&&recovered.dragons.Count==1,"Corrupted slot recovers valid backup "+i);
                    PlayerPrefs.SetString(key+".B","also broken");PlayerPrefs.Save();bool failed=false;
                    try {new ProfileStore(key).Load();}catch(InvalidOperationException){failed=true;}
                    Check(failed&&PlayerPrefs.GetString(key+".A")=="broken","Both corrupt slots preserved, no reset "+i);
                    PlayerPrefs.SetString(key+".A","{\"version\":2}");PlayerPrefs.Save();failed=false;
                    try {new ProfileStore(key).Load();}catch(InvalidOperationException){failed=true;}
                    Check(failed&&PlayerPrefs.GetString(key+".A")=="{\"version\":2}","Future schema not overwritten "+i);
                }
                finally {DeleteOwn(key);}
            }
        }
        static void Changed(PlayModeStateChange state)
        {
            if(!SessionState.GetBool(Flag,false))return;
            if(state==PlayModeStateChange.EnteredPlayMode)Schedule();
            if(state==PlayModeStateChange.EnteredEditMode)
            {
                if(SessionState.GetInt(Flag+"Stage",0)==1)EditorApplication.isPlaying=true;
                else {DeleteOwn(Prefix);SessionState.SetBool(Flag,false);Debug.Log("COLLECTION_PLAYMODE_OK_NO_BUILD");EditorApplication.Exit(0);}
            }
        }
        static void Schedule(){phase=0;readyAt=EditorApplication.timeSinceStartup+.4;EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
        static void Tick()
        {
            if(!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<readyAt)return;
            try
            {
                var controller=UnityEngine.Object.FindFirstObjectByType<BattleController>();var flow=controller.Flow;
                if(SessionState.GetInt(Flag+"Stage",0)==0)
                {
                    if(phase==0)
                    {
                        Check(flow.ScreenName=="새로운 만남"&&flow.Session.Profile.eggs==1,"Real startup shows first egg");
                        Check(controller.CurrentBattle==null,"No battle runs behind egg screen");Capture(controller.view,"01-first-egg");
                        flow.HatchButton.onClick.Invoke();flow.HatchButton.onClick.Invoke();
                        Check(flow.Session.Profile.dragons.Count==1&&flow.Session.Profile.eggs==0,"Double click commits one hatch");
                        var interrupted=new CollectionSession(new ProfileStore(Prefix),Catalog());
                        Check(interrupted.Profile.selectedInstanceId==flow.Session.Profile.selectedInstanceId&&interrupted.Profile.eggs==0,"Closing during animation retains committed hatch");
                        SessionState.SetString(Flag+"Dragon",flow.Session.Profile.selectedInstanceId);
                        readyAt=EditorApplication.timeSinceStartup+1.3;phase=1;return;
                    }
                    if(phase==1)
                    {
                        Check(flow.ScreenName=="부화 성공!","Hatch animation reaches reveal");Capture(controller.view,"02-hatched");
                        flow.ContinueButton.onClick.Invoke();Check(flow.ScreenName=="드래곤 로비","Continue opens lobby");Capture(controller.view,"03-lobby");
                        flow.ShowCodex();Check(flow.ScreenName=="드래곤 도감","Codex opens");Capture(controller.view,"04-codex");
                        flow.ShowStatus();Check(flow.ScreenName=="드래곤 상태","Status opens");Capture(controller.view,"05-status");
                        flow.ShowLobby();flow.EnterTowerWithRoll(45,80);
                        Check(flow.ScreenName=="다음 길 선택"&&flow.CurrentRun.Choices.Count==2,"Tower offers two room choices");Capture(controller.view,"07-two-doors");
                        flow.RoomButton.onClick.Invoke();Check(flow.ScreenName=="아이템방","Item room opens after selecting its door");Capture(controller.view,"08-item-chest");
                        flow.RoomButton.onClick.Invoke();phase=2;readyAt=EditorApplication.timeSinceStartup+.8;return;
                    }
                    Check(flow.ScreenName=="아이템 선택"&&flow.ChoiceButtons.Length==3,"Chest reveals three item choices");Capture(controller.view,"09-item-choices");
                    flow.ChoiceButtons[0].onClick.Invoke();Check(flow.ScreenName=="방 완료"&&flow.CurrentRun.Items.Count==1,"Chosen item is recorded");
                    flow.RoomButton.onClick.Invoke();Check(flow.CurrentRun.Floor==2&&!flow.CurrentRun.RoomChosen,"Completed room advances to next two doors");
                    flow.ShowLobby();flow.EnterTowerWithRoll(57,0);flow.RoomButton.onClick.Invoke();
                    Check(flow.ScreenName=="증강방"&&flow.ChoiceButtons.Length==3,"Augment room immediately offers three choices");Capture(controller.view,"10-augment-choices");
                    flow.ChoiceButtons[1].onClick.Invoke();Check(flow.CurrentRun.Augments.Count==1,"Augment choice is recorded in run");
                    flow.ShowLobby();flow.EnterTowerWithRoll(67,0);flow.RoomButton.onClick.Invoke();
                    Check(flow.ScreenName=="회복방","Recovery room opens green cross interaction");Capture(controller.view,"11-recovery-room");
                    flow.ShowLobby();flow.EnterTowerWithRoll(77,0);flow.RoomButton.onClick.Invoke();
                    Check(flow.ScreenName=="드래곤 둥지","Nest room opens egg interaction");Capture(controller.view,"12-nest-room");
                    flow.ShowLobby();flow.EnterTowerWithRoll(80,0);flow.RoomButton.onClick.Invoke();
                    Check(flow.ScreenName=="골드방","Gold room opens pickup interaction");Capture(controller.view,"13-gold-room");
                    flow.ShowLobby();flow.EnterTowerWithRoll(92,0);flow.RoomButton.onClick.Invoke();
                    Check(flow.ScreenName=="상점방","Shop offers healing purchase and leave option");Capture(controller.view,"14-shop-room");
                    flow.ShowLobby();flow.EnterTowerWithRoll(0,50);
                    flow.RoomButton.onClick.Invoke();Check(flow.CurrentRun.Room==TowerRoomKind.Monster,"Monster door can be selected");Capture(controller.view,"10-monster-room");
                    flow.RoomButton.onClick.Invoke();
                    Check(controller.CurrentBattle.Dragon.displayName==flow.Session.Selected.displayName,"Tower uses selected dragon");
                    var skin=Resources.Load<BattleSkin>("PixelBattleSkin");
                    Check(skin!=null&&skin.floorMonsters!=null&&Array.IndexOf(skin.floorMonsters,controller.view.CurrentEnemySprite)>=0,"Monster room uses the floor one to nine roster");
                    controller.view.attackButton.onClick.Invoke();
                    int dealt=ElementRules.Damage(flow.Session.Selected.attackDamage,controller.CurrentBattle.Dragon.elementType,controller.CurrentBattle.Enemy.elementType);
                    Check(controller.CurrentBattle.EnemyHP==240-dealt,"Attack uses the selected combat elements");
                    controller.CurrentBattle.Tick(100);controller.view.StepAnimation(controller.CurrentBattle,.7f);controller.view.Show(controller.CurrentBattle);
                    Check(controller.view.resultPanel.activeSelf,"Defeat opens existing result");
                    controller.view.restartButton.onClick.Invoke();
                    Check(flow.ScreenName=="도전 종료"&&controller.CurrentBattle==null,"Defeat ends the tower run");
                    Capture(controller.view,"11-defeat-summary");
                    flow.RoomButton.onClick.Invoke();
                    Check(flow.ScreenName=="드래곤 로비","Defeat summary returns to lobby");
                    Check(flow.Session.Profile.dragons.Count==1&&flow.Session.Profile.eggs==0,"Defeat retains collection without new egg");
                    SessionState.SetInt(Flag+"Stage",1);
                }
                else
                {
                    Check(flow.ScreenName=="드래곤 로비","Second Play starts at saved lobby");
                    Check(flow.Session.Profile.selectedInstanceId==SessionState.GetString(Flag+"Dragon",""),"Second Play retains same individual dragon");
                    Check(flow.Session.Profile.eggs==0,"Starter gift not repeated");Capture(controller.view,"06-reloaded");
                    flow.EnterTowerWithRoll(0,50);flow.RoomButton.onClick.Invoke();flow.RoomButton.onClick.Invoke();
                    Check(controller.CurrentBattle.PlayerHP==flow.Session.Selected.maxHP,"New run starts at base HP");
                    SessionState.SetInt(Flag+"Stage",2);
                }
                EditorApplication.update-=Tick;EditorApplication.isPlaying=false;
            }
            catch(Exception e){Debug.LogException(e);SessionState.SetBool(Flag,false);DeleteOwn(Prefix);EditorApplication.Exit(1);}
        }
        static void Capture(BattleView view,string name)
        {
            var camera=UnityEngine.Object.FindFirstObjectByType<Camera>();var rt=new RenderTexture(480,850,24);rt.Create();
            camera.targetTexture=rt;camera.orthographic=true;camera.orthographicSize=425;
            var canvas=view.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            Canvas.ForceUpdateCanvases();view.frame.localScale=Vector3.one;view.frame.anchoredPosition=Vector2.zero;camera.Render();
            var old=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(480,850,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,480,850),0,0);image.Apply();
            Directory.CreateDirectory("Validation/Collection");File.WriteAllBytes("Validation/Collection/"+name+".png",image.EncodeToPNG());
            RenderTexture.active=old;camera.targetTexture=null;canvas.renderMode=RenderMode.ScreenSpaceOverlay;rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(image);
        }
    }
}
