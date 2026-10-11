using System;using System.Collections.Generic;using UnityEngine;using UnityEngine.UI;using TMPro;
namespace DragonTower {
 public sealed partial class CollectionFlow {
  static readonly Color GuideIvory=new Color32(255,240,207,255),GuideGold=new Color32(246,184,71,255),GuideMuted=new Color32(214,183,148,255);
  void ApplyCozyLobby(){
   // This is deliberately applied only to the newly-created lobby body.
   foreach(var im in body.GetComponentsInChildren<Image>())if(im.transform!=body&&im.name=="Dragon nameplate")im.gameObject.SetActive(false);
   foreach(var old in body.GetComponentsInChildren<Text>())old.enabled=false;
   foreach(string name in new[]{"Lobby badge","나의 탑 기록"}){var t=body.Find(name);if(t!=null)t.gameObject.SetActive(false);}
   if(portrait!=null){portrait.anchoredPosition=new Vector2(0,-371);}
   GuideImage("Lobby title","secondary",0,143,163,36);GuideText("드래곤 로비",0,143,146,29,20);
   var buttons=body.GetComponentsInChildren<Button>();
   foreach(var b in buttons){if(b==DragonButton||b.name=="Selected dragon")continue;string title=b.name;
    if(b==TowerButton||title=="첫 드래곤 부화"){GuideButton(b,"primary",0,624,390,65,title,"tower",true,27);}
    else if(title=="드래곤 도감")GuideButton(b,"secondary",Session.Selected==null?0:-100,691,Session.Selected==null?380:188,49,title,"book",true,22);
    else if(title=="드래곤 상태")GuideButton(b,"secondary",100,691,188,49,title,"dragon",true,22);
    else if(title=="업적 · 도전 기록")GuideButton(b,"secondary",0,748,344,49,title,"trophy",true,23);
    else if(title=="닉네임 설정"||title.StartsWith("플레이어 · "))GuideButton(b,"secondary",166,141,116,34,string.IsNullOrEmpty(Session.Profile.playerName)?"닉네임 설정":Session.Profile.playerName,"person",false,13,true);
    else if(title.StartsWith("보관 알"))GuideButton(b,"secondary",165,181,113,28,title,null,false,12);
   }
   GuideImage("Partner nameplate","nameplate",0,457,165,39);GuideText(Session.Selected==null?"첫 드래곤":Session.SelectedName(),0,457,130,28,23);
   if(DragonButton!=null){GuideNav(-129,true);GuideNav(129,false);}
   GuideImage("Tower records panel","panel",0,531,390,107);GuideIcon(body,"chart",-161,502,24);GuideText("나의 탑 기록",-61,501,166,25,22,GuideGold,TextAlignmentOptions.MidlineLeft);
   var p=Session.Profile;float[] xs={-105,0,105};string[] captions={"최고 점수","최고 층","최대 유대"};string[] values={p.bestScore.ToString("N0")+"점",p.bestFloor+"층",p.bestBond.ToString()};
   for(int i=0;i<3;i++){GuideText(captions[i],xs[i],526,98,22,17,GuideMuted);GuideText(values[i],xs[i],547,99,28,24);}
   for(int i=0;i<2;i++){var r=Rect("Record separator",body,i==0?-53:53,538,1,31);r.gameObject.AddComponent<Image>().color=new Color32(127,93,64,255);}
   GuideText(BestPartner(),0,571,342,23,15,GuideMuted);
   var footer=GuideText(notice.text,0,790,440,25,13,GuideMuted);footer.gameObject.AddComponent<LobbyGuideNotice>().source=notice;
   var line=Rect("Footer ornament line",body,0,811,180,1);line.gameObject.AddComponent<Image>().color=new Color32(151,102,49,255);var diamond=Rect("Footer gold diamond",body,0,811,7,7);diamond.localRotation=Quaternion.Euler(0,0,45);diamond.gameObject.AddComponent<Image>().color=GuideGold;
   var gear=FindFirstObjectByType<DragonTowerSettings>();if(gear!=null)foreach(var b in gear.GetComponentsInChildren<Button>(true))if(b.name=="설정"){body.gameObject.AddComponent<LobbyGuideGear>().Bind(b);break;}
  }
  void GuideNav(float x,bool left){var r=Rect(left?"Previous partner":"Next partner",body,x,400,42,48);var im=r.gameObject.AddComponent<Image>();im.color=Color.clear;var b=r.gameObject.AddComponent<Button>();b.targetGraphic=im;b.onClick.AddListener(ShowDragonSelect);var icon=GuideIcon(r,"arrow",0,24,23);if(left)icon.rectTransform.localScale=new Vector3(-1,1,1);}
  Image GuideImage(string name,string asset,float x,float y,float w,float h){var im=Rect(name,body,x,y,w,h).gameObject.AddComponent<Image>();LobbyGuideArt.Skin(im,asset,h);im.raycastTarget=false;return im;}
  Image GuideIcon(Transform parent,string asset,float x,float y,float size){var im=Rect("Guide icon "+asset,parent,x,y,size,size).gameObject.AddComponent<Image>();im.sprite=Resources.Load<Sprite>("UI/LobbyGuide/"+asset);im.preserveAspect=true;im.raycastTarget=false;return im;}
  TextMeshProUGUI GuideText(string value,float x,float y,float w,float h,int size,Color? color=null,TextAlignmentOptions alignment=TextAlignmentOptions.Center){return GuideTextAt(body,value,x,y,w,h,size,color??GuideIvory,alignment);}
  TextMeshProUGUI GuideTextAt(Transform parent,string value,float x,float y,float w,float h,int size,Color color,TextAlignmentOptions alignment=TextAlignmentOptions.Center){var t=Rect("Guide text",parent,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();t.font=Resources.Load<TMP_FontAsset>("UI/LobbyGuide/LobbyFont");t.text=value;t.richText=false;t.fontSize=size;t.enableAutoSizing=true;t.fontSizeMin=Mathf.Min(size,9);t.textWrappingMode=TextWrappingModes.NoWrap;t.fontSizeMax=size;t.color=color;t.alignment=alignment;t.raycastTarget=false;t.outlineColor=new Color32(40,22,13,255);t.outlineWidth=.18f;return t;}
  void GuideButton(Button b,string asset,float x,float y,float w,float h,string title,string icon,bool arrow,int size,bool pencil=false){
   var r=(RectTransform)b.transform;r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);
   var ancient=b.GetComponent<AncientButtonFeedback>();if(ancient!=null)ancient.enabled=false;var cozy=b.GetComponent<CozyButtonFeedback>();if(cozy!=null)cozy.enabled=false;
   foreach(Transform child in b.transform)child.gameObject.SetActive(false);
   var im=b.GetComponent<Image>();im.enabled=true;im.color=Color.clear;b.targetGraphic=im;b.transition=Selectable.Transition.None;
   var face=Rect("Guide button face",b.transform,0,h/2,w,h).gameObject.AddComponent<Image>();LobbyGuideArt.Skin(face,asset,h);face.raycastTarget=false;
   float textW=w-(icon!=null?54:24)-(arrow?18:pencil?4:0);GuideTextAt(b.transform,title,icon!=null?5:0,h/2,textW,h-10,size,GuideIvory);
   if(icon!=null)GuideIcon(b.transform,icon,-w/2+(w>250?40:24),h/2,w>250?29:w<140?19:27);
   if(arrow)GuideIcon(b.transform,"arrow",w/2-21,h/2,16);
   if(pencil)GuideIcon(b.transform,"pencil",w/2-15,h/2,17);
   b.gameObject.AddComponent<LobbyGuideButton>();
  }
 }
 public static class LobbyGuideArt {
  public static void Skin(Image image,string asset,float height){var src=Resources.Load<Sprite>("UI/LobbyGuide/"+asset);if(src==null)return;image.sprite=src;image.type=Image.Type.Sliced;image.pixelsPerUnitMultiplier=src.rect.height/height;image.color=Color.white;}
 }
 public sealed class LobbyGuideNotice:MonoBehaviour {
  public Text source;TextMeshProUGUI target;void Awake(){target=GetComponent<TextMeshProUGUI>();}void LateUpdate(){if(source!=null&&target.text!=source.text)target.text=source.text;}
 }
 public sealed class LobbyGuideButton:MonoBehaviour,UnityEngine.EventSystems.IPointerDownHandler,UnityEngine.EventSystems.IPointerUpHandler,UnityEngine.EventSystems.IPointerExitHandler {
  bool down;void Set(bool value){if(down==value)return;down=value;foreach(Transform c in transform)if(c.gameObject.activeSelf)((RectTransform)c).anchoredPosition+=Vector2.down*(value?2:-2);var face=transform.Find("Guide button face");if(face!=null)face.GetComponent<Image>().color=value?new Color(.82f,.82f,.82f):Color.white;}
  public void OnPointerDown(UnityEngine.EventSystems.PointerEventData e){if(GetComponent<Button>().IsInteractable())Set(true);}public void OnPointerUp(UnityEngine.EventSystems.PointerEventData e){Set(false);}public void OnPointerExit(UnityEngine.EventSystems.PointerEventData e){Set(false);}void OnDisable(){Set(false);}
 }
 public sealed class LobbyGuideGear:MonoBehaviour {
  readonly List<GameObject> originals=new List<GameObject>();GameObject art;
  public void Bind(Button b){foreach(Transform c in b.transform)if(c.gameObject.activeSelf){originals.Add(c.gameObject);c.gameObject.SetActive(false);}art=new GameObject("Lobby only gear skin",typeof(RectTransform),typeof(Image));var r=art.GetComponent<RectTransform>();r.SetParent(b.transform,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;LobbyGuideArt.Skin(art.GetComponent<Image>(),"secondary",48);art.GetComponent<Image>().raycastTarget=false;var icon=new GameObject("Gear icon",typeof(RectTransform),typeof(Image));var ir=icon.GetComponent<RectTransform>();ir.SetParent(r,false);ir.anchorMin=ir.anchorMax=Vector2.one*.5f;ir.sizeDelta=Vector2.one*30;var im=icon.GetComponent<Image>();im.sprite=Resources.Load<Sprite>("UI/LobbyGuide/gear");im.preserveAspect=true;im.raycastTarget=false;}
  void OnDisable(){foreach(var o in originals)if(o!=null)o.SetActive(true);if(art!=null){art.SetActive(false);Destroy(art);}}
 }
}
