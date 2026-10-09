using System;using System.Linq;using System.Collections;using UnityEngine;using UnityEngine.UI;
namespace DragonTower {public sealed partial class CollectionFlow {
 bool roomTransitionBusy,chestAnimating,chestFastForward;
 static Sprite[] chestSprites;
 ItemData[] ChestChoices(){if(database==null)return Array.Empty<ItemData>();if(towerRun.ChestOpened&&towerRun.ChestRewardIds!=null)return towerRun.ChestRewardIds.Select(id=>database.items.FirstOrDefault(x=>x!=null&&x.StableId==id)).Where(x=>x!=null).ToArray();return database.PickItems(3,towerRun.RewardSeed^towerRun.Floor);}
 Image RoomScenery(string resource){foreach(string n in new[]{"Ornate content frame","Dim tower backdrop"}){var t=body.Find(n);if(t!=null)t.gameObject.SetActive(false);}var r=Rect("Room scenery",body,0,425,480,850);r.SetSiblingIndex(1);var im=r.gameObject.AddComponent<Image>();im.sprite=Resources.Load<Sprite>(resource);im.raycastTarget=false;return im;}
 Image RoomImage(Transform parent,string name,Sprite sprite,float x,float y,float w,float h){var im=Rect(name,parent,x,y,w,h).gameObject.AddComponent<Image>();im.sprite=sprite;im.color=Color.white;im.preserveAspect=true;im.raycastTarget=false;return im;}
 static Sprite ChestSprite(int index){if(chestSprites==null){var t=Resources.Load<Texture2D>("UI/reward-chest");if(t==null)return null;chestSprites=new Sprite[2];for(int i=0;i<2;i++)chestSprites[i]=Sprite.Create(t,new Rect(i*t.width/2f,0,t.width/2f,t.height),new Vector2(.5f,.5f),100);}return chestSprites[index];}
 void ShowTreasureChamber(){Screen("아이템방");RoomScenery("UI/treasure-chamber");Label("상자를 터치하세요",0,180,420,42,24,Gold);
 var rootChest=Rect("Treasure chest touch",body,0,340,310,280);var hit=rootChest.gameObject.AddComponent<Image>();hit.color=Color.clear;RoomButton=rootChest.gameObject.AddComponent<Button>();RoomButton.targetGraphic=hit;RoomButton.onClick.AddListener(()=>OpenTreasure(rootChest));
 RoomImage(rootChest,"Chest sprite",ChestSprite(0),0,140,300,270);
 var dust=Rect("Treasure dust",body,0,425,480,850).gameObject.AddComponent<RewardRoomParticles>();dust.raycastTarget=false;dust.ambient=true;
 Label(CompactInventory(),0,647,420,55,14,AncientUi.Ivory);SecondRoomButton=Button("상자를 열지 않고 나간다",0,742,390,54,()=>{if(!chestAnimating)AdvanceFloor();});notice.text="상자를 열면 세 가지 아이템 중 하나를 선택합니다";
 }
 void OpenTreasure(RectTransform chest){if(chestAnimating){chestFastForward=true;return;}if(roomTransitionBusy||towerRun==null||towerRun.Phase!=FloorPhase.RoomEvent||towerRun.ChestOpened)return;
 var choices=ChestChoices();towerRun.OpenChest(choices.Select(x=>x.StableId).ToArray());SaveProgress();chestAnimating=true;SecondRoomButton.interactable=false;DragonTowerAudio.PlayChest();StartCoroutine(AnimateTreasure(chest,choices));}
 IEnumerator AnimateTreasure(RectTransform chest,ItemData[] choices){var owner=body;var heading=owner.Find("상자를 터치하세요").GetComponent<Text>();heading.text="봉인이 풀립니다";SecondRoomButton.interactable=false;var image=chest.Find("Chest sprite").GetComponent<Image>();var origin=chest.anchoredPosition;float elapsed=0;
 var highest=choices.OrderByDescending(x=>(int)x.grade).FirstOrDefault();Color tint=highest==null?AncientUi.Gold:DragonTowerTheme.Grade(highest.grade);
 var fx=Rect("Chest opening light",owner,0,350,400,380).gameObject.AddComponent<RewardRoomParticles>();fx.raycastTarget=false;fx.tint=tint;fx.power=highest==null?1:1+(int)highest.grade*.35f;fx.transform.SetSiblingIndex(chest.GetSiblingIndex());
 var icons=new Image[choices.Length];for(int i=0;i<choices.Length;i++){icons[i]=RoomImage(owner,"Emerging reward "+i,ItemIcon(choices[i]),(i-(choices.Length-1)*.5f)*78,350,58,58);icons[i].color=Color.clear;}
 var lockFlash=RoomImage(chest,"Unlock gleam",null,-20,173,24,30);lockFlash.color=Color.clear;
 notice.text="다시 터치하면 개봉 연출을 건너뜁니다";
 while(elapsed<1.85f&&owner==body&&!returningToLobby){elapsed+=Time.unscaledDeltaTime;if(chestFastForward)elapsed=1.85f;chest.anchoredPosition=origin+new Vector2(elapsed<.45f?Mathf.Sin(elapsed*75)*4:0,0);fx.age=elapsed;
 lockFlash.color=new Color(1,.88f,.5f,Mathf.Clamp01(1-Mathf.Abs(elapsed-.35f)/.22f)*.8f);
 if(elapsed>=.64f){heading.text="보물을 발견했습니다";image.sprite=ChestSprite(1);float lid=Mathf.SmoothStep(0,1,Mathf.Clamp01((elapsed-.64f)/.18f));image.rectTransform.localScale=new Vector3(1,.94f+.06f*lid,1);image.rectTransform.anchoredPosition=new Vector2(0,-140-(1-lid)*8);}
 float rise=Mathf.SmoothStep(0,1,Mathf.Clamp01((elapsed-.85f)/.55f));for(int i=0;i<icons.Length;i++){icons[i].rectTransform.anchoredPosition=new Vector2((i-(icons.Length-1)*.5f)*78,-(350-rise*82));icons[i].color=new Color(1,1,1,rise);}
 yield return null;}
 if(owner!=body||returningToLobby)yield break;ShowItemChoices();
 }
}}
