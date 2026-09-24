using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    // Original geometric placeholder art, drawn directly into a small UI mesh.
    public class DragonGraphic : MaskableGraphic
    {
        public bool enemy;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Color dark = Color.Lerp(color, new Color(.06f,.10f,.17f), .48f);
            Color light = Color.Lerp(color, Color.white, .42f);
            Poly(vh,dark, -13,-32, -70,-12, -91,52, -46,28, -21,48, 0,8);
            Poly(vh,color, -60,0, -85,45, -46,23, -23,41, -29,-8);
            Poly(vh,dark, 1,-34, 46,-45, 72,-14, 55,-60, 14,-61);
            Ellipse(vh,color, 0,-27,32,41);
            Ellipse(vh,light, 9,-35,17,27);
            Poly(vh,dark, -18,-51,-35,-68,-12,-70,2,-51);
            Poly(vh,color, 17,-53,8,-71,38,-70,31,-61);
            Poly(vh,light, 3,40,-9,76,16,61,27,36);
            Poly(vh,dark, 27,46,37,79,48,47);
            Ellipse(vh,color, 24,26,34,32);
            Ellipse(vh,color, 45,12,29,17);
            Poly(vh,light, 44,4,57,-4,59,7);
            Ellipse(vh,new Color(.06f,.09f,.15f), 36,30,7,10);
            Ellipse(vh,Color.white, 38,34,2.5f,3);
            Poly(vh,dark, 24,43,43,38,41,44);
            Ellipse(vh,dark, 63,17,3,3);
            Poly(vh,color, -8,-13,21,-11,34,-24,15,-26);
        }
        void Poly(VertexHelper vh, Color c, params float[] points)
        {
            int start=vh.currentVertCount;
            for(int i=0;i<points.Length;i+=2)
                vh.AddVert(new Vector3(points[i]*(enemy?-1:1)*rectTransform.rect.width/200f,points[i+1]*rectTransform.rect.height/200f),c,Vector2.zero);
            for(int i=1;i<points.Length/2-1;i++) vh.AddTriangle(start,start+i,start+i+1);
        }
        void Ellipse(VertexHelper vh,Color c,float x,float y,float rx,float ry)
        {
            float[] p=new float[48];
            for(int i=0;i<24;i++){float a=i*Mathf.PI*2/24;p[i*2]=x+Mathf.Cos(a)*rx;p[i*2+1]=y+Mathf.Sin(a)*ry;}
            Poly(vh,c,p);
        }
    }
}
