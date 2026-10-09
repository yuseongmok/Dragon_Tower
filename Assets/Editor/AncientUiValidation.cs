using System;using System.Linq;using System.Reflection;using UnityEngine;using UnityEngine.UI;using UnityEngine.EventSystems;using UnityEditor;using UnityEditor.Build.Reporting;
namespace DragonTower.Editor {
 public static class AncientUiValidation {
  static void Check(bool ok,string label){if(!ok)throw new Exception("ANCIENT_UI_FAIL "+label);}
  static Button Find(string name)=>UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(x=>x.name==name&&x.gameObject.activeInHierarchy);
  public static void Verify(CollectionFlow flow,Action<string> capture){
   flow.ShowLobby();var b=Find("드래곤 도감");var root=(RectTransform)b.transform;var pos=root.anchoredPosition;var size=root.rect.size;var art=b.GetComponentInChildren<AncientStoneSurface>();var normal=art.rectTransform.anchoredPosition;var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left};
   ExecuteEvents.Execute(b.gameObject,pointer,ExecuteEvents.pointerDownHandler);Check(art.pressed&&art.rectTransform.anchoredPosition==normal+Vector2.down*2,"surface pressed two pixels");Check(root.anchoredPosition==pos&&root.rect.size==size,"fixed touch target");capture("pressed");
   ExecuteEvents.Execute(b.gameObject,pointer,ExecuteEvents.pointerUpHandler);Check(!art.pressed&&art.rectTransform.anchoredPosition==normal,"release restores");
   b.interactable=false;b.GetComponent<AncientButtonFeedback>().SendMessage("LateUpdate");ExecuteEvents.Execute(b.gameObject,pointer,ExecuteEvents.pointerDownHandler);Check(art.disabled&&!art.pressed,"disabled cannot depress");capture("disabled");b.interactable=true;b.GetComponent<AncientButtonFeedback>().SendMessage("LateUpdate");
   ExecuteEvents.Execute(b.gameObject,new BaseEventData(EventSystem.current),ExecuteEvents.selectHandler);Check(art.selected,"selected highlight");ExecuteEvents.Execute(b.gameObject,new BaseEventData(EventSystem.current),ExecuteEvents.deselectHandler);
   ExecuteEvents.Execute(b.gameObject,pointer,ExecuteEvents.pointerClickHandler);Check(flow.ScreenName=="드래곤 도감","codex click");Find("▶").onClick.Invoke();Check(UnityEngine.Object.FindObjectsByType<Text>(FindObjectsSortMode.None).Any(x=>x.text=="2 / 3"),"page next");Find("◀").onClick.Invoke();Find("로비로 돌아가기").onClick.Invoke();Check(flow.ScreenName=="드래곤 로비","return lobby");Find("드래곤 상태").onClick.Invoke();Find("이름 변경").onClick.Invoke();Check(flow.ScreenName=="파트너 이름 변경","rename entry unchanged");
   var input=UnityEngine.Object.FindFirstObjectByType<InputField>();input.text="가나다라마바사아자차카타";Find("이름 저장").onClick.Invoke();Check(flow.Session.SelectedName()==input.text,"custom name saved");flow.ShowLobby();capture("long-name");Check(UnityEngine.Object.FindObjectsByType<Text>(FindObjectsSortMode.None).Any(x=>x.text=="가나다라마바사아자차카타"&&x.resizeTextForBestFit),"long name fit");
   var restored=new CollectionSession(new ProfileStore(Environment.GetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE")),ContentDatabase.Load().dragons);Check(restored.SelectedName()=="가나다라마바사아자차카타","saved custom name reload");flow.Session.ResetDragonName(flow.Session.Profile.selectedInstanceId);
   foreach(var graphic in UnityEngine.Object.FindObjectsByType<AncientStoneSurface>(FindObjectsSortMode.None))Check(!graphic.raycastTarget&&graphic.GetComponent<CanvasRenderer>()!=null,"nonblocking rendered decoration");
   Debug.Log("ANCIENT_UI_INTERACTION_OK press release disabled selected fixed hit region navigation paging rename persistence long name");
  }
  public static void Build(){var report=BuildPipeline.BuildPlayer(new[]{"Assets/Scenes/Battle.unity"},"C:/Users/PC/Documents/Codex/AncientUI-20261009/WebGL",BuildTarget.WebGL,BuildOptions.None);Debug.Log("ANCIENT_UI_WEBGL_"+report.summary.result);EditorApplication.Exit(report.summary.result==BuildResult.Succeeded?0:1);}
 }
}
