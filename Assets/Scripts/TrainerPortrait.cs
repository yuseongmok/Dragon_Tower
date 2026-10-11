using UnityEngine;using UnityEngine.UI;
namespace DragonTower {
 public sealed class TrainerPortrait:MonoBehaviour {
  static Texture2D atlas;static Sprite[] bodies,hairs;static Sprite outfit;Image skin,hair,clothes;
  static Sprite Slice(float x,float y,float w,float h){float scale=atlas.width/1280f;return Sprite.Create(atlas,new Rect(x*scale,atlas.height-(y+h)*scale,w*scale,h*scale),new Vector2(.5f,.5f),100);}
  static void Load(){if(atlas!=null)return;atlas=Resources.Load<Texture2D>("Trainer/trainer-paperdoll");if(atlas==null)return;atlas.filterMode=FilterMode.Point;bodies=new[]{Slice(98,60,234,402),Slice(508,60,234,402)};hairs=new[]{Slice(95,514,240,216),Slice(505,518,240,216),Slice(907,518,240,216)};outfit=Slice(82,860,268,346);}
  Image Layer(string name,float y,float w,float h){var go=new GameObject(name,typeof(RectTransform),typeof(Image));var r=go.GetComponent<RectTransform>();r.SetParent(transform,false);r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.anchoredPosition=new Vector2(0,-y);r.sizeDelta=new Vector2(w,h);var im=go.GetComponent<Image>();im.raycastTarget=false;return im;}
  public void Bind(TrainerProfile p){Load();if(atlas==null||p==null)return;if(skin==null){skin=Layer("Body / Skin",115,130,230);clothes=Layer("Outfit",151,140,148);hair=Layer("Hair",36,134,116);Layer("Head Accessory",40,140,100).color=Color.clear;Layer("Face Accessory",62,70,40).color=Color.clear;}
   skin.sprite=bodies[Mathf.Clamp(p.bodyType,0,1)];skin.color=p.skinTone==0?Color.white:p.skinTone==1?new Color(.80f,.67f,.56f):new Color(.57f,.40f,.32f);hair.sprite=hairs[Mathf.Clamp(p.hairStyle,0,2)];hair.rectTransform.anchoredPosition=new Vector2(0,p.hairStyle==2?-37:-36);clothes.sprite=outfit;
  }
 }
}
