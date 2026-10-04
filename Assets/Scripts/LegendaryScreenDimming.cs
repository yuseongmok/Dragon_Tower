using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower {
 // A single pooled, non-interactive full-canvas grade. No battle-area mask or HUD fade shader.
 public sealed class LegendaryScreenDimming : MonoBehaviour {
  struct Request { public Component owner; public Color color; }
  readonly Dictionary<int,Request> requests=new Dictionary<int,Request>();
  readonly List<int> expired=new List<int>();Image overlay;
  public bool Clean=>overlay==null||!overlay.enabled;
  public static void Set(Component owner,Color color){if(owner==null)return;var screen=owner.GetComponent<LegendaryScreenDimming>();if(screen==null)screen=owner.gameObject.AddComponent<LegendaryScreenDimming>();screen.Apply(owner,color);}
  void Apply(Component owner,Color color){if(overlay==null){var view=GetComponent<BattleView>();if(view==null)return;var canvas=view.GetComponentInParent<Canvas>();var go=new GameObject("Legendary full screen dim",typeof(RectTransform),typeof(Canvas),typeof(Image));var r=go.GetComponent<RectTransform>();r.SetParent(canvas.transform,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;var c=go.GetComponent<Canvas>();c.overrideSorting=true;c.sortingOrder=32700;overlay=go.GetComponent<Image>();overlay.raycastTarget=false;}
   if(color.a<=0)requests.Remove(owner.GetInstanceID());else requests[owner.GetInstanceID()]=new Request{owner=owner,color=color};Refresh();}
  void Refresh(){Color strongest=Color.clear;expired.Clear();foreach(var pair in requests){var r=pair.Value;if(r.owner==null||(r.owner is Behaviour b&&!b.isActiveAndEnabled)){expired.Add(pair.Key);continue;}if(r.color.a>strongest.a)strongest=r.color;}foreach(int id in expired)requests.Remove(id);if(overlay==null)return;
   // Full-screen grading also affects bright VFX, so keep the strongest grade readable.
   strongest.a=Mathf.Min(.46f,strongest.a);overlay.color=strongest;overlay.enabled=strongest.a>.001f;
  }
  void LateUpdate()=>Refresh();
  void OnDisable(){requests.Clear();if(overlay!=null)overlay.enabled=false;}
  void OnDestroy(){if(overlay!=null)Destroy(overlay.gameObject);}
 }
}
