using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower.Editor
{
    public static class ZephyrIdleSetup
    {
        const string Sheet="Assets/Art/ZephyrIdle/zephyr-idle.png";
        [InitializeOnLoadMethod]
        static void Listen() { EditorApplication.playModeStateChanged+=OnPlay; }
        static int waitFrames;
        static void OnPlay(PlayModeStateChange state)
        {
            if(state!=PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool("ZephyrValidation",false))return;
            foreach(var c in UnityEngine.Object.FindObjectsByType<BattleController>(FindObjectsSortMode.None)) c.enabled=false;
            waitFrames=0;EditorApplication.update+=PlayCheck;
        }
        static void PlayCheck()
        {
            if(++waitFrames<12)return;
            EditorApplication.update-=PlayCheck;SessionState.SetBool("ZephyrValidation",false);
            try { var d=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon2.asset");Validate(d,d.idleFrames);Finish(0); }
            catch(Exception e){Debug.LogException(e);Finish(1);}
        }
        static void Finish(int code) { if(Application.isBatchMode)EditorApplication.Exit(code);else EditorApplication.isPlaying=false; }
        [MenuItem("Dragon Tower/Zephyr/Install and validate Idle")]
        public static void Run()
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(Sheet);
            importer.textureType=TextureImporterType.Sprite;
            importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.filterMode=FilterMode.Point;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled=false;importer.alphaIsTransparency=true;
            importer.isReadable=true;
            importer.maxTextureSize=2048;
            importer.GetSourceTextureWidthAndHeight(out int w,out int h);
            var slices=new SpriteMetaData[4];
            for(int i=0;i<4;i++)slices[i]=new SpriteMetaData {name="Zephyr_Idle_"+i,rect=new Rect((i%2)*w/2,(1-i/2)*h/2,w/2,h/2),pivot=new Vector2(.5f,.5f),alignment=0};
            #pragma warning disable 618
            importer.spritesheet=slices;
            #pragma warning restore 618
            importer.SaveAndReimport();
            var frames=AssetDatabase.LoadAllAssetsAtPath(Sheet).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
            Check(frames.Length==4,"Four idle frames imported");
            const string clipPath="Assets/Art/ZephyrIdle/ZephyrIdle.asset";
            var clip=AssetDatabase.LoadAssetAtPath<DragonIdleFrames>(clipPath);
            if(clip==null){clip=ScriptableObject.CreateInstance<DragonIdleFrames>();AssetDatabase.CreateAsset(clip,clipPath);}
            clip.frames=frames;clip.durations=new[]{.6f,.2f,.12f,.28f};
            clip.offsets=new Vector2[4];
            var anchors=frames.Select(FootAnchor).ToArray();
            for(int i=0;i<4;i++)clip.offsets[i]=(anchors[0]-anchors[i])/(w/2f);
            EditorUtility.SetDirty(clip);
            var dragon=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon2.asset");
            Check(dragon.StableId=="zephyr","Only Zephyr configured");
            dragon.idleFrames=clip;EditorUtility.SetDirty(dragon);AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");
            Environment.SetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE","DragonTower.ZephyrIdle.Validation");
            SessionState.SetBool("ZephyrValidation",true);EditorApplication.isPlaying=true;
        }
        internal static Vector2 FootAnchor(Sprite sprite)
        {
            var r=sprite.rect;var pixels=sprite.texture.GetPixels((int)r.x,(int)r.y,(int)r.width,(int)r.height);
            int width=(int)r.width, bottom=0;
            for(int y=0;y<(int)r.height;y++)
            { bool found=false;for(int x=0;x<width;x++)if(pixels[y*width+x].a>.5f){found=true;break;}if(found){bottom=y;break;} }
            float sum=0,count=0;
            for(int y=bottom;y<Math.Min(bottom+20,(int)r.height);y++)for(int x=0;x<width;x++)if(pixels[y*width+x].a>.5f){sum+=x;count++;}
            return new Vector2(count>0?sum/count:width/2f,bottom);
        }
        static void Check(bool ok,string message){if(!ok)throw new Exception(message);Debug.Log("ZEPHYR_CHECK "+message);}
        static void Validate(DragonData dragon,DragonIdleFrames clip)
        {
            var view=UnityEngine.Object.FindFirstObjectByType<BattleView>();view.SetDragonArt(dragon);
            var collection=view.frame.Find("Collection screens");if(collection!=null)collection.gameObject.SetActive(false);
            view.SetEncounter(BattleEnemyStats.Normal(),1,false);
            var battle=new BattleModel(dragon.Snapshot());battle.Cue+=view.PlayCue;
            var player=view.playerArt.transform.Find("Pixel sprite").GetComponent<Image>();
            var camera=UnityEngine.Object.FindFirstObjectByType<Camera>();
            var target=new RenderTexture(480,850,24);target.Create();camera.targetTexture=target;
            camera.orthographic=true;camera.orthographicSize=425;
            camera.transform.position=new Vector3(0,0,-10);camera.transform.rotation=Quaternion.identity;
            camera.nearClipPlane=.1f;camera.farClipPlane=100;camera.cullingMask=-1;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.gray;
            var canvas=view.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=camera;
            var canvasRect=(RectTransform)canvas.transform;canvasRect.position=Vector3.zero;canvasRect.localScale=Vector3.one;canvasRect.sizeDelta=new Vector2(480,850);
            ShaderUtil.allowAsyncCompilation=false;
            Directory.CreateDirectory("Validation/ZephyrIdle");
            float[] steps={.01f,.60f,.20f,.12f};
            for(int i=0;i<4;i++)
            {
                view.StepAnimation(battle,steps[i]);view.Show(battle);
                Check(player.sprite==clip.frames[i],"Battle screen renders frame "+i);
                Capture(view,camera,target,"idle-"+i);
            }
            Check(view.playerArt.rectTransform.localScale==Vector3.one,"Frame idle avoids stretching pixels");
            battle.Attack();view.StepAnimation(battle,.05f);
            Check(player.sprite==(dragon.attackFrames!=null?dragon.attackFrames.frames[0]:clip.frames[0]),"Attack takes priority over idle");
            Check(Math.Abs(dragon.Snapshot().attackCooldown-.3f)<.001f,"Attack cooldown remains 0.3s");
            view.SetDragonArt(dragon,10);
            Check(player.sprite==dragon.BattleSpriteAtLevel(10),"Evolution art preserved");
            view.StepAnimation(battle,.7f);
            Check(player.sprite==dragon.BattleSpriteAtLevel(10),"Evolution not overwritten by base idle");
            var other=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon0.asset");view.SetDragonArt(other);
            view.StepAnimation(new BattleModel(other.Snapshot()),.7f);
            Check(player.sprite==other.BattleSpriteAtLevel(1),"Other dragons preserved");
            Check(clip.At(1.201f)==clip.frames[0],"Idle loops");
            if(dragon.attackFrames!=null)ValidateAttack(view,dragon,player,camera,target);
            if(dragon.skillFrames!=null)ValidateSkill(view,dragon,player,camera,target);
            if(dragon.dodgeFrames!=null)ValidateDodge(view,dragon,player,camera,target);
            if(dragon.hitFrames!=null)ValidateHit(view,dragon,player,camera,target);
            if(dragon.deathFrames!=null)ValidateDeath(view,dragon,player,camera,target);
            ValidateWind(view,dragon,player,camera,target);
            BattleInputValidation.Validate(view,dragon,camera);
            Capture(view,camera,target,"hud-final");
            PrototypeSetup.Verify();
            CombatMechanicsChecks.VerifyInEditor();
            camera.targetTexture=null;target.Release();UnityEngine.Object.DestroyImmediate(target);
            File.WriteAllText("Validation/ZephyrIdle/result.txt","ZEPHYR_IDLE_VALIDATION_OK");
            Debug.Log("ZEPHYR_IDLE_VALIDATION_OK");
        }
        static void ValidateAttack(BattleView view,DragonData dragon,Image player,Camera camera,RenderTexture target)
        {
            var clip=dragon.attackFrames;view.SetDragonArt(dragon);
            var b=new BattleModel(dragon.Snapshot());b.Cue+=view.PlayCue;
            Check(b.Attack(),"Initial attack accepted");int hp=b.EnemyHP;
            Check(!b.Attack()&&b.EnemyHP==hp,"Spam cannot duplicate damage");
            for(int i=0;i<4;i++)
            {
                float dt=i==0?.001f:.06f;b.Tick(dt);view.StepAnimation(b,dt);view.Show(b);
                Check(player.sprite==clip.frames[i],"Attack frame "+i+" at combat time "+b.Time);
                Capture(view,camera,target,"attack-"+i);
            }
            b.Tick(.06f);view.StepAnimation(b,.06f);view.Show(b);
            Check(System.Array.IndexOf(dragon.idleFrames.frames,player.sprite)>=0,"Attack returns to Idle");Capture(view,camera,target,"attack-return");
            Check(!b.Attack(),"Attack still blocked before 0.3s");
            b.Tick(.061f);Check(b.Attack(),"Next attack accepted after cooldown");view.StepAnimation(b,.001f);
            Check(player.sprite==clip.frames[0],"Next accepted attack restarts animation");
            view.PlayCue(CombatCue.Dodge,0);view.StepAnimation(b,.01f);
            Check(System.Array.IndexOf(clip.frames,player.sprite)<0,"Dodge interrupts attack sprite");
            view.SetDragonArt(dragon);view.PlayCue(CombatCue.Attack,9);view.PlayCue(CombatCue.Skill,20);view.StepAnimation(b,.01f);
            Check(System.Array.IndexOf(clip.frames,player.sprite)<0,"Skill interrupts attack sprite");
            view.SetDragonArt(dragon);view.PlayCue(CombatCue.Attack,9);view.PlayCue(CombatCue.EnemyHit,10);view.StepAnimation(b,.01f);
            Check(System.Array.IndexOf(clip.frames,player.sprite)<0,"Hit interrupts attack sprite");
            view.StepAnimation(b,.3f);Check(System.Array.IndexOf(clip.frames,player.sprite)<0,"Interrupted attack cannot resume later");
            view.SetDragonArt(dragon,10);view.PlayCue(CombatCue.Attack,9);view.StepAnimation(b,.07f);
            Check(player.sprite==dragon.BattleSpriteAtLevel(10),"Evolved attack keeps evolution art");
            var other=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon0.asset");view.SetDragonArt(other);view.PlayCue(CombatCue.Attack,9);view.StepAnimation(b,.07f);
            Check(player.sprite==other.battleSprite,"Other dragon attack unchanged");
            var fastStats=dragon.Snapshot();fastStats.attackCooldown=.15f;
            b=new BattleModel(fastStats);b.Cue+=view.PlayCue;view.SetDragonArt(dragon);
            b.Attack();b.Tick(.151f);view.StepAnimation(b,.151f);Check(b.Attack(),"Faster cooldown permits retrigger");view.StepAnimation(b,.001f);
            Check(player.sprite==clip.frames[0],"Fast attack restarts from first frame");
            view.SetDragonArt(dragon);var lethal=dragon.Snapshot();lethal.attackDamage=10000;
            var won=new BattleModel(lethal);won.Cue+=view.PlayCue;won.Attack();view.StepAnimation(won,.01f);
            Check(System.Array.IndexOf(clip.frames,player.sprite)<0,"Battle result ends attack playback");
            Debug.Log("ZEPHYR_ATTACK_VALIDATION_OK");
        }
        static void ValidateSkill(BattleView view,DragonData dragon,Image player,Camera camera,RenderTexture target)
        {
            var clip=dragon.skillFrames;view.SetDragonArt(dragon);
            var enemy=BattleEnemyStats.Normal();enemy.maxHP=10000;enemy.interval=100;
            var b=new BattleModel(dragon.Snapshot(),enemy);b.Cue+=view.PlayCue;
            var control=new BattleModel(dragon.Snapshot(),enemy);
            Check(b.Skill()&&control.Skill(),"Skill accepted");
            Check(b.EnemyHP==control.EnemyHP&&b.SkillReady==control.SkillReady,"Skill damage and cooldown unchanged");
            double ready=b.SkillReady;int hp=b.EnemyHP;
            Check(!b.Skill()&&b.EnemyHP==hp&&b.SkillReady==ready,"Skill spam cannot duplicate damage or restart cooldown");
            float[] steps={.001f,.08f,.10f,.10f};
            for(int i=0;i<4;i++)
            {
                b.Tick(steps[i]);control.Tick(steps[i]);view.StepAnimation(b,steps[i]);view.Show(b);
                Check(player.sprite==clip.frames[i],"Skill frame "+i);
                Check(b.EnemyHP==control.EnemyHP,"Presentation does not alter skill ticks");
                Capture(view,camera,target,"skill-"+i);
            }
            b.Tick(.12f);view.StepAnimation(b,.12f);view.Show(b);
            Check(System.Array.IndexOf(dragon.idleFrames.frames,player.sprite)>=0,"Skill returns to Idle");
            Check(player.rectTransform.localScale==Vector3.one,"Skill display scale resets on Idle");Capture(view,camera,target,"skill-return");
            view.SetDragonArt(dragon);view.PlayCue(CombatCue.Skill,20);view.PlayCue(CombatCue.Attack,9);view.StepAnimation(b,.09f);
            Check(player.sprite==clip.frames[1],"Attack cue does not overwrite active skill pose");
            view.PlayCue(CombatCue.SkillHit,10);view.StepAnimation(b,.1f);
            Check(player.sprite==clip.frames[2],"Repeated skill hit does not restart casting");
            foreach(var cue in new[]{CombatCue.Dodge,CombatCue.EnemyHit,CombatCue.PlayerStatusHit})
            {
                view.SetDragonArt(dragon);view.PlayCue(CombatCue.Skill,20);view.PlayCue(cue,10);view.StepAnimation(b,.01f);
                Check(System.Array.IndexOf(clip.frames,player.sprite)<0,cue+" interrupts skill pose");
                view.StepAnimation(b,.45f);Check(System.Array.IndexOf(clip.frames,player.sprite)<0,"Interrupted skill cannot resume");
            }
            view.SetDragonArt(dragon,10);view.PlayCue(CombatCue.Skill,20);view.StepAnimation(b,.19f);
            Check(player.sprite==dragon.BattleSpriteAtLevel(10)&&player.rectTransform.localScale==Vector3.one,"Evolution art and scale preserved");
            var other=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon0.asset");view.SetDragonArt(other);view.PlayCue(CombatCue.Skill,20);view.StepAnimation(b,.19f);
            Check(player.sprite==other.battleSprite,"Other dragon skill preserved");
            view.SetDragonArt(dragon);view.PlayCue(CombatCue.Skill,20);view.StepAnimation(b,.19f);view.SetSkillPresentation(dragon.skill);
            Check(player.sprite==dragon.idleFrames.frames[0]&&player.rectTransform.localScale==Vector3.one,"Reset clears skill pose and scale");
            var disabled=dragon.Snapshot();disabled.skillDisabled=true;var sealedBattle=new BattleModel(disabled,enemy);
            Check(!sealedBattle.Skill(),"Disabled skill cannot cast");
            var wonStats=dragon.Snapshot();wonStats.attackDamage=10000;var won=new BattleModel(wonStats);won.Attack();
            view.PlayCue(CombatCue.Skill,20);view.StepAnimation(won,.1f);
            Check(System.Array.IndexOf(clip.frames,player.sprite)<0,"Battle result ends skill pose");
            Debug.Log("ZEPHYR_SKILL_VALIDATION_OK");
        }
        static void ValidateDodge(BattleView view,DragonData dragon,Image player,Camera camera,RenderTexture target)
        {
            var clip=dragon.dodgeFrames;
            foreach(int direction in new[]{-1,1})
            {
                view.SetDragonArt(dragon);var motion=view.GetComponent<BattleAnimation>();motion.SetDodgeDirection(direction);
                var home=view.playerArt.rectTransform.anchoredPosition;
                var b=new BattleModel(dragon.Snapshot());b.Cue+=view.PlayCue;
                Check(b.Dodge(),"Dodge accepted "+direction);
                Check(Math.Abs(b.DodgeReady-1.4)<.001&&Math.Abs(b.DodgeUntil-.42)<.001,"Dodge cooldown and window preserved");
                double ready=b.DodgeReady;Check(!b.Dodge()&&b.DodgeReady==ready,"Dodge spam rejected");
                float[] steps={.001f,.07f,.14f,.11f};
                for(int i=0;i<4;i++)
                {
                    b.Tick(steps[i]);view.StepAnimation(b,steps[i]);view.Show(b);
                    Check(player.sprite==clip.frames[i],"Dodge frame "+i+" direction "+direction);
                    if(i==1)Check((view.playerArt.rectTransform.anchoredPosition.x-home.x)*direction>0,"Dodge moves in selected direction");
                    Capture(view,camera,target,(direction<0?"dodge-left-":"dodge-right-")+i);
                }
                b.Tick(.101f);view.StepAnimation(b,.101f);view.Show(b);
                Check(System.Array.IndexOf(dragon.idleFrames.frames,player.sprite)>=0,"Dodge returns to Idle");
                Check(view.playerArt.rectTransform.anchoredPosition==home&&player.rectTransform.localScale==Vector3.one,"Dodge restores position and scale");
                Capture(view,camera,target,"dodge-return");
                b.Tick(.979f);Check(b.Dodge(),"Dodge ready again after 1.4s");
            }
            view.SetDragonArt(dragon);var enemy=BattleEnemyStats.Normal();enemy.interval=.2f;
            var dodged=new BattleModel(dragon.Snapshot(),enemy);var exposed=new BattleModel(dragon.Snapshot(),enemy);
            dodged.Cue+=view.PlayCue;dodged.Dodge();dodged.Tick(.21f);exposed.Tick(.21f);view.StepAnimation(dodged,.21f);
            Check(dodged.PlayerHP==dragon.maxHP&&exposed.PlayerHP<dragon.maxHP,"Enemy strike avoided during dodge window");
            view.SetDragonArt(dragon);var b2=new BattleModel(dragon.Snapshot());view.PlayCue(CombatCue.Dodge,0);view.PlayCue(CombatCue.Skill,20);view.PlayCue(CombatCue.Attack,9);view.StepAnimation(b2,.08f);
            Check(player.sprite==clip.frames[1],"Dodge pose has priority over offensive poses");
            view.PlayCue(CombatCue.PlayerStatusHit,1);view.StepAnimation(b2,.01f);
            Check(System.Array.IndexOf(clip.frames,player.sprite)<0,"Damage interrupts dodge presentation");
            view.StepAnimation(b2,.5f);Check(System.Array.IndexOf(clip.frames,player.sprite)<0,"Interrupted dodge cannot resume");
            view.SetDragonArt(dragon,10);view.PlayCue(CombatCue.Dodge,0);view.StepAnimation(b2,.08f);
            Check(player.sprite==dragon.BattleSpriteAtLevel(10),"Evolved dodge art unchanged");
            var other=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon0.asset");view.SetDragonArt(other);view.PlayCue(CombatCue.Dodge,0);view.StepAnimation(b2,.08f);
            Check(player.sprite==other.battleSprite,"Other dragon dodge art unchanged");
            view.SetDragonArt(dragon);var longStats=dragon.Snapshot();longStats.dodgeDuration=.84f;var longer=new BattleModel(longStats);longer.Cue+=view.PlayCue;longer.Dodge();view.StepAnimation(longer,.43f);
            Check(player.sprite==clip.frames[2],"Dodge presentation follows configured duration");
            view.SetDragonArt(dragon);Check(player.sprite==dragon.idleFrames.frames[0]&&player.rectTransform.localScale==Vector3.one,"Reset clears dodge pose");
            var lethal=dragon.Snapshot();lethal.attackDamage=10000;var won=new BattleModel(lethal);won.Attack();view.PlayCue(CombatCue.Dodge,0);view.StepAnimation(won,.1f);
            Check(System.Array.IndexOf(clip.frames,player.sprite)<0,"Result ends dodge sprite");
            Debug.Log("ZEPHYR_DODGE_VALIDATION_OK");
        }
        static void ValidateHit(BattleView view,DragonData dragon,Image player,Camera camera,RenderTexture target)
        {
            var clip=dragon.hitFrames;view.SetDragonArt(dragon);
            var enemy=BattleEnemyStats.Normal();enemy.interval=.2f;
            var b=new BattleModel(dragon.Snapshot(),enemy);b.Cue+=view.PlayCue;
            var control=new BattleModel(dragon.Snapshot(),enemy);
            b.Tick(.201f);control.Tick(.201f);
            Check(b.PlayerHP==control.PlayerHP&&b.PlayerHP<dragon.maxHP,"Real enemy hit damage unchanged");
            var home=view.playerArt.rectTransform.anchoredPosition;
            float[] steps={.001f,.04f,.07f,.07f};
            for(int i=0;i<4;i++)
            {
                view.StepAnimation(b,steps[i]);view.Show(b);
                Check(player.sprite==clip.frames[i],"Hit frame "+i);Capture(view,camera,target,"hit-"+i);
            }
            view.StepAnimation(b,.061f);view.Show(b);
            Check(System.Array.IndexOf(dragon.idleFrames.frames,player.sprite)>=0,"Hit returns to Idle");
            Check(view.playerArt.rectTransform.anchoredPosition==home&&player.rectTransform.localScale==Vector3.one&&player.color==Color.white,"Hit restores position scale and tint");Capture(view,camera,target,"hit-return");
            view.PlayCue(CombatCue.EnemyHit,10);view.StepAnimation(b,.12f);view.PlayCue(CombatCue.EnemyHit,10);view.StepAnimation(b,.001f);
            Check(player.sprite==clip.frames[0],"Repeated hit restarts flinch");
            view.SetDragonArt(dragon);view.PlayCue(CombatCue.PlayerStatusHit,1);view.StepAnimation(b,.05f);
            Check(player.sprite==clip.frames[1],"Status damage uses Hit");
            view.PlayCue(CombatCue.Attack,9);view.PlayCue(CombatCue.Skill,20);view.PlayCue(CombatCue.Dodge,0);view.StepAnimation(b,.01f);
            Check(player.sprite==clip.frames[1],"Hit presentation has priority without changing combat input");
            view.SetDragonArt(dragon);var avoided=new BattleModel(dragon.Snapshot(),enemy);avoided.Cue+=view.PlayCue;avoided.Dodge();avoided.Tick(.201f);view.StepAnimation(avoided,.1f);
            Check(avoided.PlayerHP==dragon.maxHP&&System.Array.IndexOf(clip.frames,player.sprite)<0,"Successful dodge does not show Hit");
            view.SetDragonArt(dragon,10);view.PlayCue(CombatCue.EnemyHit,10);view.StepAnimation(b,.05f);
            Check(player.sprite==dragon.BattleSpriteAtLevel(10),"Evolution hit art preserved");
            var other=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon0.asset");view.SetDragonArt(other);view.PlayCue(CombatCue.EnemyHit,10);view.StepAnimation(b,.05f);
            Check(player.sprite==other.battleSprite,"Other dragon hit art preserved");
            view.SetDragonArt(dragon);view.PlayCue(CombatCue.EnemyHit,10);view.StepAnimation(b,.05f);view.SetSkillPresentation(dragon.skill);
            Check(player.sprite==dragon.idleFrames.frames[0]&&player.color==Color.white,"Battle reset clears Hit");
            enemy.damage=10000;var defeated=new BattleModel(dragon.Snapshot(),enemy);defeated.Cue+=view.PlayCue;defeated.Tick(.201f);view.StepAnimation(defeated,.1f);
            Check(defeated.Result==BattleResult.Defeat&&System.Array.IndexOf(clip.frames,player.sprite)<0,"Lethal hit yields to defeat presentation");
            Debug.Log("ZEPHYR_HIT_VALIDATION_OK");
        }
        static void ValidateDeath(BattleView view,DragonData dragon,Image player,Camera camera,RenderTexture target)
        {
            var clip=dragon.deathFrames;view.SetDragonArt(dragon);
            var enemy=BattleEnemyStats.Normal();enemy.damage=10000;enemy.interval=.2f;
            var b=new BattleModel(dragon.Snapshot(),enemy);b.Cue+=view.PlayCue;
            b.Tick(.201f);view.Show(b);
            Check(b.Result==BattleResult.Defeat&&b.PlayerHP==0,"Lethal damage still causes defeat");
            Check(!view.resultPanel.activeSelf,"Result waits for death animation");
            float[] steps={.001f,.10f,.14f,.16f};
            for(int i=0;i<4;i++)
            {
                view.StepAnimation(b,steps[i]);view.Show(b);
                Check(player.sprite==clip.frames[i],"Death frame "+i);
                Check(!view.resultPanel.activeSelf,"Death pose visible before result");
                Capture(view,camera,target,"death-"+i);
            }
            view.PlayCue(CombatCue.Attack,9);view.PlayCue(CombatCue.Skill,20);view.PlayCue(CombatCue.Dodge,0);view.PlayCue(CombatCue.EnemyHit,1);
            view.StepAnimation(b,.20f);view.Show(b);
            Check(player.sprite==clip.frames[3]&&!view.resultPanel.activeSelf,"Final pose held and combat cues cannot replace Death");
            view.StepAnimation(b,.05f);view.Show(b);
            Check(view.resultPanel.activeSelf,"Result appears at existing 0.65 second delay");Capture(view,camera,target,"death-result");
            view.StepAnimation(b,2);
            Check(player.sprite==clip.frames[3]&&player.color.a==1,"Death does not loop or fade to invisible");
            Check(!b.Attack()&&!b.Skill()&&!b.Dodge(),"Defeated combat cannot accept actions");
            view.SetDragonArt(dragon);var fresh=new BattleModel(dragon.Snapshot());view.Show(fresh);
            Check(player.sprite==dragon.idleFrames.frames[0]&&player.rectTransform.localScale==Vector3.one&&!view.resultPanel.activeSelf,"New battle restores Idle scale and UI");
            var lethal=dragon.Snapshot();lethal.attackDamage=10000;var won=new BattleModel(lethal);won.Cue+=view.PlayCue;won.Attack();view.StepAnimation(won,.3f);
            Check(System.Array.IndexOf(clip.frames,player.sprite)<0,"Victory never plays player Death");
            view.SetDragonArt(dragon,10);view.StepAnimation(b,.3f);
            Check(player.sprite==dragon.BattleSpriteAtLevel(10),"Evolution death keeps evolution art");
            var other=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon0.asset");view.SetDragonArt(other);view.StepAnimation(b,.3f);
            Check(player.sprite==other.battleSprite&&player.color.a<1,"Other dragon keeps previous defeat fade");
            Debug.Log("ZEPHYR_DEATH_VALIDATION_OK");
        }
        static void ValidateWind(BattleView view,DragonData dragon,Image player,Camera camera,RenderTexture target)
        {
            view.SetDragonArt(dragon);var motion=view.GetComponent<BattleAnimation>();var wind=view.GetComponent<ZephyrWindVfx>();var asset=view.GetComponent<BattleAssetVfx>();
            Check(motion.UsesPixelWindSkill&&asset.InstanceCount==0,"Zephyr wind skill bypasses 3D prefab");
            var cues=new[]{CombatCue.Attack,CombatCue.Skill,CombatCue.Dodge,CombatCue.EnemyHit};
            string[] names={"wind-attack","wind-skill","wind-dodge","wind-hit"};
            for(int i=0;i<cues.Length;i++)
            {
                view.SetDragonArt(dragon);var enemy=BattleEnemyStats.Normal();enemy.damage=10;enemy.interval=100;
                var b=new BattleModel(dragon.Snapshot(),enemy);b.Cue+=view.PlayCue;
                if(i==0)b.Attack();else if(i==1)b.Skill();else if(i==2)b.Dodge();else view.PlayCue(cues[i],10);
                float[] moments=i==1?new[]{.07f,.13f,.15f,.15f}:new[]{.07f,.05f,.06f,.06f};
                for(int f=0;f<4;f++){b.Tick(moments[f]);view.StepAnimation(b,moments[f]);view.Show(b);Capture(view,camera,target,names[i]+"-"+f);}
                view.StepAnimation(b,1);Check(wind.ActiveCount==0,"Wind effects expire: "+names[i]);
            }
            view.SetDragonArt(dragon);int objects=wind.Capacity;int children=view.frame.GetComponentsInChildren<Transform>(true).Length;
            var spam=new BattleModel(dragon.Snapshot());
            for(int i=0;i<200;i++){view.PlayCue(CombatCue.Skill,20);view.StepAnimation(spam,.016f);}
            Check(wind.Capacity==objects&&wind.ActiveCount<=objects&&children==view.frame.GetComponentsInChildren<Transform>(true).Length,"Wind spam uses fixed pool");
            view.SetDragonArt(dragon);Check(wind.ActiveCount==0,"Battle reset clears every wind effect");
            foreach(var g in view.frame.Find("Battle effects (pooled)/Zephyr pixel wind").GetComponentsInChildren<Graphic>(true))Check(!g.raycastTarget,"Wind cannot intercept combat input");
            Check(view.frame.Find("Battle effects (pooled)/Zephyr pixel wind").GetComponent<RectMask2D>()!=null,"Wind clipped to combat stage");
            view.SetDragonArt(dragon,10);Check(motion.UsesPixelWindSkill,"Zephyr evolution shares wind style");
            var other=AssetDatabase.LoadAssetAtPath<DragonData>("Assets/Data/Dragon0.asset");view.SetDragonArt(other);
            Check(!motion.UsesPixelWindSkill&&asset.InstanceCount==1,"Other dragon keeps existing skill VFX");
            view.SetDragonArt(dragon);view.SetSkillPresentation(other.skill);
            Check(!motion.UsesPixelWindSkill&&asset.InstanceCount==1,"Non-wind learned skill keeps its own VFX");
            view.SetSkillPresentation(dragon.skill);Check(motion.UsesPixelWindSkill&&asset.InstanceCount==0,"Switching back to wind clears old prefab");
            var stats=dragon.Snapshot();var battle=new BattleModel(stats);var reference=new BattleModel(dragon.Snapshot());battle.Cue+=view.PlayCue;
            battle.Attack();reference.Attack();battle.Skill();reference.Skill();battle.Dodge();reference.Dodge();
            Check(battle.EnemyHP==reference.EnemyHP&&battle.AttackReady==reference.AttackReady&&battle.SkillReady==reference.SkillReady&&battle.DodgeReady==reference.DodgeReady,"Wind presentation preserves damage and cooldowns");
            Debug.Log("ZEPHYR_WIND_VALIDATION_OK");
        }
        static void Capture(BattleView view,Camera camera,RenderTexture target,string name)
        {
            Canvas.ForceUpdateCanvases();view.frame.localScale=Vector3.one;view.frame.anchoredPosition=Vector2.zero;
            camera.Render();var old=RenderTexture.active;RenderTexture.active=target;
            var texture=new Texture2D(480,850,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,480,850),0,0);texture.Apply();
            Check(texture.GetPixels32().Count(p=>p.r>8||p.g>8||p.b>8)>10000,"Rendered image contains visible pixels");
            File.WriteAllBytes("Validation/ZephyrIdle/"+name+".png",texture.EncodeToPNG());
            RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(texture);
        }
    }
}

