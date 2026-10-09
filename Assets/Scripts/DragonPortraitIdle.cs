using System;using System.Linq;using UnityEngine;using UnityEngine.UI;
namespace DragonTower {
 [Serializable] public sealed class PortraitBoundsEntry {public string id;public int stage;public Rect bounds;}
 [Serializable] public sealed class PortraitBoundsCatalogue {public PortraitBoundsEntry[] entries;}
 // UI-only framing. One fixed envelope for the entire Idle prevents frame-by-frame size pumping.
 public sealed class DragonPortraitIdle:MonoBehaviour {
  static PortraitBoundsCatalogue catalogue;
  Image image;DragonIdleFrames idle;RectTransform actor;Vector2 center;float scale,clock;FloatingWeaponVisual weapon;
  public float FitScale=>scale;
  public void Bind(DragonData dragon,int stage,Vector2 fit){
   image=GetComponent<Image>();var set=dragon.LoadAnimationSet(stage);idle=set!=null?set.idle:dragon.LegacyAnimation(DragonAnimationState.Idle,stage);
   if(catalogue==null){var json=Resources.Load<TextAsset>("UI/portrait-bounds");catalogue=json!=null?JsonUtility.FromJson<PortraitBoundsCatalogue>(json.text):new PortraitBoundsCatalogue();}
   var entry=catalogue.entries?.FirstOrDefault(x=>x.id==dragon.StableId&&x.stage==stage);var bounds=entry!=null?entry.bounds:new Rect(-100,-100,200,200);
   scale=Mathf.Min(fit.x/Mathf.Max(1,bounds.width),fit.y/Mathf.Max(1,bounds.height));center=bounds.center;
   // Keep the original hit target/slot fixed while the child art uses normalized visible bounds.
   var r=new GameObject("Idle artwork",typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>();r.SetParent(transform,false);r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.sizeDelta=Vector2.one*256;
   var child=r.GetComponent<Image>();child.sprite=image.sprite;child.color=image.color;child.preserveAspect=true;child.raycastTarget=false;
   image.color=new Color(1,1,1,0);image=child;actor=r;
   var root=new GameObject("Idle rig",typeof(RectTransform)).GetComponent<RectTransform>();root.SetParent(transform,false);root.anchorMin=root.anchorMax=root.pivot=Vector2.one*.5f;root.sizeDelta=Vector2.one*256;root.localScale=Vector3.one*scale;root.anchoredPosition=-center*scale;actor.SetParent(root,false);
   if(set!=null&&set.floatingWeapon!=null&&set.floatingWeapon.sprite!=null){weapon=new FloatingWeaponVisual();weapon.Bind(set.floatingWeapon,image,root);}
   Step(0);
  }
  public void Step(float dt){if(image==null||actor==null)return;clock+=dt;if(idle!=null){var sprite=idle.At(clock);if(sprite!=null)image.sprite=sprite;actor.localScale=Vector3.one*Mathf.Max(.1f,idle.displayScale);actor.anchoredPosition=idle.OffsetFor(image.sprite)*256;}weapon?.Step(dt);}
  void Update(){Step(Mathf.Min(Time.unscaledDeltaTime,.1f));}
 }
}
