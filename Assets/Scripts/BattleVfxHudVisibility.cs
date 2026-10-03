using UnityEngine;
using UnityEngine.UI;
namespace DragonTower {
 // Scoped to tall skill presentations; gameplay and HUD layout are untouched.
 public sealed class BattleVfxHudVisibility {
  public readonly Material alpha,additive;
  public BattleVfxHudVisibility(){var shader=Resources.Load<Shader>("VFX/HudSafeVfx");alpha=new Material(shader){name="HUD safe VFX alpha"};additive=new Material(shader){name="HUD safe VFX additive"};alpha.SetFloat("_DstBlend",10);additive.SetFloat("_DstBlend",1);}
  public void Update(BattleView view){var frame=view.frame;float top=frame.rect.yMax;var range=new Vector4(frame.TransformPoint(new Vector3(0,top-125,0)).y,frame.TransformPoint(new Vector3(0,top-180,0)).y,.24f,0);alpha.SetVector("_HudFadeRange",range);additive.SetVector("_HudFadeRange",range);}
  public static void BringHudForward(BattleView view){Lift(view.modeLabel,view.frame);Lift(view.floorLabel,view.frame);Lift(view.enemyName,view.frame);Lift(view.enemyHP,view.frame);Lift(view.enemyElementIcon,view.frame);if(view.enemyFill!=null&&view.enemyFill.transform.parent!=view.frame)view.enemyFill.transform.parent.SetAsLastSibling();}
  static void Lift(Graphic graphic,Transform frame){if(graphic!=null&&graphic.transform.parent==frame)graphic.transform.SetAsLastSibling();}
  public void Dispose(){Object.Destroy(alpha);Object.Destroy(additive);}
 }
}
