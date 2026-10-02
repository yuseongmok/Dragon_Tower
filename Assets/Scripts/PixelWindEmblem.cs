using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    // Small theme-tinted pixel spiral, shared by the wind skill and player badge.
    public sealed class PixelWindEmblem : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();float scale=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)/32f;
            for(int y=-16;y<16;y++)for(int x=-16;x<16;x++)
            {
                Vector2 point=new Vector2(x+.5f,y+.5f);float distance=100;
                for(int step=0;step<160;step++)
                {
                    float t=step/159f,angle=t*Mathf.PI*4.5f;
                    var spiral=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*(1+t*12);
                    distance=Mathf.Min(distance,Vector2.Distance(point,spiral));
                }
                if(distance>1.45f)continue;
                Color c=distance<.7f?Color.Lerp(color,Color.white,.25f):color;
                int n=vh.currentVertCount;float px=x*scale,py=y*scale;
                vh.AddVert(new Vector3(px,py),c,Vector2.zero);vh.AddVert(new Vector3(px,py+scale),c,Vector2.zero);
                vh.AddVert(new Vector3(px+scale,py+scale),c,Vector2.zero);vh.AddVert(new Vector3(px+scale,py),c,Vector2.zero);
                vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
            }
        }
    }
}
