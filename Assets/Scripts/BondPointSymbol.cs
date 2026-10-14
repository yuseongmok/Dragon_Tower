using UnityEngine;
using UnityEngine.UI;
namespace DragonTower {
 // Shared, point-filtered UI asset: a turquoise bond heart in an antique gold seal.
 public static class BondPointSymbol {
  static Sprite sprite;
  public static Sprite Sprite {
   get {
    if(sprite!=null)return sprite;
    const int n=24;var pixels=new Color32[n*n];
    string[] heart={"..###...###..",".#####.#####.","#############","#############",".###########.","..#########..","...#######...","....#####....",".....###.....","......#......"};
    for(int y=0;y<n;y++)for(int x=0;x<n;x++){
     int dx=Mathf.Abs(x-11),dy=Mathf.Abs(y-11);int d=dx+dy;
     if(dx>10||dy>10||d>16)continue;
     pixels[y*n+x]=d>=15||dx==10||dy==10?new Color32(56,37,35,255):d>=13||dx>=8||dy>=8?(y>11?new Color32(255,223,151,255):new Color32(175,122,64,255)):new Color32(28,49,58,255);
    }
    for(int row=0;row<heart.Length;row++)for(int col=0;col<13;col++)if(heart[row][col]=='#')pixels[(17-row)*n+5+col]=row<3?new Color32(172,249,230,255):row<6?new Color32(76,204,183,255):new Color32(37,136,150,255);
    var tex=new Texture2D(n,n,TextureFormat.RGBA32,false){name="Bond point heart",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};tex.SetPixels32(pixels);tex.Apply(false,true);
    sprite=Sprite.Create(tex,new Rect(0,0,n,n),new Vector2(.5f,.5f),24);sprite.name="Bond point symbol";return sprite;
   }
  }
  public static Image Add(Transform parent,Vector2 position,float size=24){
   var go=new GameObject("Bond point symbol",typeof(RectTransform),typeof(Image));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.anchoredPosition=position;r.sizeDelta=new Vector2(size,size);var i=go.GetComponent<Image>();i.sprite=Sprite;i.raycastTarget=false;return i;
  }
 }
}
