using System.Collections.Generic;using UnityEngine;using UnityEngine.UI;using UnityEngine.EventSystems;
namespace DragonTower {
 // Presentation only: the Button/root RectTransform and its raycast region never move.
 public sealed class AncientButtonFeedback:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler,ISelectHandler,IDeselectHandler,ISubmitHandler {
  Button button;AncientStoneSurface art;bool fixedSelection,focused,down,lastDisabled,lastSelected;float releaseAt;
  readonly Dictionary<RectTransform,Vector2> positions=new Dictionary<RectTransform,Vector2>();
  public void Bind(Button b,AncientStoneSurface surface,bool selected){ResetPose();button=b;art=surface;fixedSelection=selected;Refresh();}
  void SetDown(bool value){if(down==value)return;down=value;if(value){positions.Clear();foreach(Transform child in transform){var r=child as RectTransform;if(r==null||!child.gameObject.activeSelf)continue;positions[r]=r.anchoredPosition;r.anchoredPosition+=Vector2.down*2;}}else{foreach(var pair in positions)if(pair.Key!=null)pair.Key.anchoredPosition=pair.Value;positions.Clear();}if(art!=null){art.pressed=value;art.SetVerticesDirty();}}
  void ResetPose(){SetDown(false);releaseAt=0;}
  void Refresh(){if(button==null||art==null)return;bool disabled=!button.IsInteractable(),selected=fixedSelection||focused;if(disabled)ResetPose();if(disabled!=lastDisabled||selected!=lastSelected||art.disabled!=disabled||art.selected!=selected){art.disabled=disabled;art.selected=selected;art.SetVerticesDirty();}lastDisabled=disabled;lastSelected=selected;}
  void LateUpdate(){if(releaseAt>0&&Time.unscaledTime>=releaseAt)ResetPose();Refresh();}
  void OnDisable(){ResetPose();focused=false;}
  public void OnPointerDown(PointerEventData e){if(e.button==PointerEventData.InputButton.Left&&button!=null&&button.IsInteractable())SetDown(true);}
  public void OnPointerUp(PointerEventData e){ResetPose();}
  public void OnPointerExit(PointerEventData e){ResetPose();}
  public void OnSelect(BaseEventData e){focused=true;Refresh();}
  public void OnDeselect(BaseEventData e){focused=false;Refresh();}
  public void OnSubmit(BaseEventData e){if(button!=null&&button.IsInteractable()){SetDown(true);releaseAt=Time.unscaledTime+.1f;}}
 }
}
