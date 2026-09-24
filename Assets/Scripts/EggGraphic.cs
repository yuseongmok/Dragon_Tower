using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    // Small code-drawn UI symbol; no extra texture to download.
    public sealed class EggGraphic : MaskableGraphic
    {
        public bool cracked;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            int[] widths={4,7,9,11,13,14,15,15,15,14,12,8};
            for(int row=0;row<widths.Length;row++)
            {
                float y=18-row*3;int w=widths[row];
                Quad(vh,-w,y,w*2,3,new Color(.10f,.12f,.18f));
                if(row>0&&row<11)
                {
                    Quad(vh,-w+2,y,w*2-4,3,new Color(.88f,.82f,.66f));
                    Quad(vh,-w+3,y,Mathf.Max(2,w-3),3,new Color(1,.95f,.79f));
                }
            }
            Quad(vh,3,5,5,4,new Color(.79f,.45f,.26f));Quad(vh,-9,-6,5,4,new Color(.68f,.39f,.25f));Quad(vh,5,-13,4,3,new Color(.68f,.39f,.25f));
            if(cracked)for(int i=0;i<5;i++)Quad(vh,(i%2==0?0:3),10-i*4,3,5,new Color(.13f,.13f,.18f));
        }
        void Quad(VertexHelper vh,float x,float y,float width,float height,Color c)
        {
            int i=vh.currentVertCount;float s=rectTransform.rect.width/40;
            vh.AddVert(new Vector3(x*s,y*s),c,Vector2.zero);vh.AddVert(new Vector3((x+width)*s,y*s),c,Vector2.zero);
            vh.AddVert(new Vector3((x+width)*s,(y+height)*s),c,Vector2.zero);vh.AddVert(new Vector3(x*s,(y+height)*s),c,Vector2.zero);
            vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);
        }
    }
}
