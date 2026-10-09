using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DragonTower
{
    public sealed partial class CollectionFlow
    {
        RectTransform stairWorld;
        TowerDoorVisual[] towerDoors;
        Text stairFloor;
        static Sprite[] towerDoorSprites;

        static Sprite DoorSprite(bool boss)
        {
            if(towerDoorSprites==null)
            {
                var texture=Resources.Load<Texture2D>("UI/tower-doors");
                if(texture==null)return null;
                towerDoorSprites=new Sprite[2];
                for(int i=0;i<2;i++)towerDoorSprites[i]=Sprite.Create(texture,new Rect(i*texture.width/2f,0,texture.width/2f,texture.height),new Vector2(.5f,.5f),100);
            }
            return towerDoorSprites[boss?1:0];
        }

        Color DoorColor(TowerRoomKind kind)
        {
            switch(kind)
            {
                case TowerRoomKind.Monster:case TowerRoomKind.Boss:return AncientUi.Hex(0xE27563);
                case TowerRoomKind.Augment:return AncientUi.Hex(0x7DD6CE);
                case TowerRoomKind.Recovery:return AncientUi.Hex(0x92D391);
                case TowerRoomKind.Nest:return AncientUi.Hex(0xAACBE9);
                default:return AncientUi.Hex(0xEBC580);
            }
        }
        Sprite DoorMark(TowerRoomKind kind)
        {
            switch(kind)
            {
                case TowerRoomKind.Item:return ChestSprite(0);
                case TowerRoomKind.Augment:return RewardIcon(14);
                case TowerRoomKind.Gold:case TowerRoomKind.Shop:return null;
                case TowerRoomKind.Recovery:return RewardIcon(2);
                case TowerRoomKind.Nest:return null;
                case TowerRoomKind.Boss:return RewardIcon(13);
                default:return RewardIcon(0);
            }
        }
        void ShowStairChoices()
        {
            Screen("다음 길 선택");bool boss=towerRun.Choices.Count==1&&towerRun.Choices[0]==TowerRoomKind.Boss;
            stairWorld=Rect("Tower stair space",body,0,425,480,850);stairWorld.SetSiblingIndex(1);
            var scenery=RoomScenery(boss?"UI/boss-stairway":"UI/tower-stairways");scenery.transform.SetParent(stairWorld,false);
            towerDoors=new TowerDoorVisual[towerRun.Choices.Count];
            for(int i=0;i<towerDoors.Length;i++)
            {
                int index=i;var room=towerRun.Choices[i];float x=boss?0:(i==0?-103:103);
                var area=Rect("Door choice "+i,stairWorld,x,boss?235:225,boss?300:190,boss?350:220);
                var hit=area.gameObject.AddComponent<Image>();hit.color=Color.clear;
                var button=area.gameObject.AddComponent<Button>();button.targetGraphic=hit;button.transition=Selectable.Transition.None;
                button.onClick.AddListener(()=>SelectTowerRoom(index));
                var door=area.gameObject.AddComponent<TowerDoorVisual>();door.Build(DoorSprite(boss),DoorMark(room),DoorColor(room),boss,room);towerDoors[i]=door;
                if(i==0)RoomButton=button;else SecondRoomButton=button;
                float labelY=boss?530:370;
                var caption=Rect("Door caption "+i,stairWorld,x,labelY,boss?330:198,boss?100:114);
                var surface=caption.gameObject.AddComponent<Image>();AncientUi.Frame(surface,AncientSurfaceKind.Information);surface.raycastTarget=false;
                CardText(caption,"Room name",RoomName(room),0,25,boss?310:182,32,22,DoorColor(room),TextAnchor.MiddleCenter);
                var detail=CardText(caption,"Room reward",RoomDetail(room),0,72,boss?310:172,52,15,AncientUi.Ivory,TextAnchor.UpperCenter);detail.resizeTextForBestFit=true;detail.resizeTextMinSize=14;detail.resizeTextMaxSize=15;
            }
            var dust=Rect("Stair dust",stairWorld,0,425,480,850).gameObject.AddComponent<RewardRoomParticles>();dust.raycastTarget=false;dust.ambient=true;
            var floorBacking=Rect("Floor readability",body,0,145,244,34).gameObject.AddComponent<Image>();floorBacking.color=new Color(.10f,.09f,.09f,.8f);floorBacking.raycastTarget=false;
            stairFloor=Label((towerRun.Floor==1?"탑 입구":(towerRun.Floor-1)+"층")+"  →  "+towerRun.Floor+"층",0,145,420,34,23,AncientUi.Ivory);
            var status=Panel("Tower travel status",body,0,710,424,98,AncientUi.Stone,AncientUi.DarkGold);
            CardText(status.transform,"Travel stats","유대 "+towerRun.Level+"     HP "+towerRun.CurrentHP+" / "+towerRun.MaxHP,0,29,398,28,19,AncientUi.Ivory,TextAnchor.MiddleCenter);
            CardText(status.transform,"Travel gold","GOLD "+towerRun.Gold+"     ·     "+towerRun.Progress.score+"점",0,65,398,28,17,Gold,TextAnchor.MiddleCenter);
            notice.text=boss?"수호자의 문 · 준비되면 문을 터치하세요":"계단 위의 문을 터치해 길을 선택하세요";
        }
        IEnumerator EnterTowerDoor(int index)
        {
            var owner=body;var world=stairWorld;var doors=towerDoors;var floor=stairFloor;
            bool boss=towerRun.Room==TowerRoomKind.Boss;
            float elapsed=0,duration=boss?1.45f:1.25f;
            var start=world.anchoredPosition;float x=doors[index].GetComponent<RectTransform>().anchoredPosition.x;
            var veil=Rect("Door light transition",owner,0,425,480,850).gameObject.AddComponent<Image>();veil.raycastTarget=false;veil.color=Color.clear;
            while(elapsed<duration&&owner==body&&!returningToLobby)
            {
                elapsed+=Time.unscaledDeltaTime;float open=Mathf.SmoothStep(0,1,Mathf.Clamp01((elapsed-.17f)/.55f));doors[index].SetOpening(open);
                float move=Mathf.SmoothStep(0,1,Mathf.Clamp01((elapsed-.52f)/(duration-.52f)));world.localScale=Vector3.one*(1+move*.42f);
                world.anchoredPosition=start+new Vector2(-x*move*.7f,-88*move);
                if(boss&&elapsed>.45f&&elapsed<.65f)world.anchoredPosition+=new Vector2(Mathf.Sin(elapsed*85)*3,Mathf.Cos(elapsed*73)*2);
                if(elapsed>.65f)floor.text=towerRun.Floor+"층 · "+RoomName(towerRun.Room);
                veil.color=new Color(.94f,.83f,.61f,Mathf.Clamp01((elapsed-duration+.22f)/.22f));
                yield return null;
            }
            if(owner!=body||returningToLobby)yield break;
            ShowChosenRoom();
        }
    }
}
