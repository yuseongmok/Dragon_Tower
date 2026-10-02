using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    public sealed class PixelHudFrame : MaskableGraphic
    {
        public Color edge=new Color(.28f,.49f,.45f);
        public bool prominent;
        public bool outlineOnly;
        [Range(0,1)] public float progress=1;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;float l=r.xMin,b=r.yMin,w=r.width,h=r.height;
            var bright=Color.Lerp(edge,Color.white,.35f);
            var shadow=Color.Lerp(edge,new Color(.01f,.035f,.055f),.72f);
            if(outlineOnly)
            {
                Box(vh,l+8,b,w-16,2,edge);Box(vh,l+8,b+h-2,w-16,2,edge);
                Box(vh,l,b+8,2,h-16,edge);Box(vh,l+w-2,b+8,2,h-16,edge);
                Box(vh,l+2,b+2,6,2,edge);Box(vh,l+w-8,b+2,6,2,edge);
                Box(vh,l+2,b+h-4,6,2,edge);Box(vh,l+w-8,b+h-4,6,2,edge);return;
            }
            if(prominent)
            {
                // Concentric pixel rings. Progress follows model cooldown, clockwise from twelve.
                float radius=Mathf.Min(w,h)*.5f;const float pixel=3;
                for(float y=-radius;y<radius;y+=pixel)for(float x=-radius;x<radius;x+=pixel)
                {
                    var p=new Vector2(x+pixel*.5f,y+pixel*.5f);float d=p.magnitude;Color c;
                    float angle=Mathf.Repeat(Mathf.Atan2(p.x,p.y)/(2*Mathf.PI),1);
                    bool charged=angle<=progress;
                    if(d>radius-1)continue;
                    if(d>radius-5){if(((int)(angle*32)%4)==0)continue;c=shadow;}
                    else if(d>radius-8)c=charged?edge:shadow;
                    else if(d>radius-12)c=color;
                    else if(d>radius-15)c=charged?bright:shadow;
                    else if(d>radius-19)c=shadow;
                    else c=color;
                    Box(vh,r.center.x+x,r.center.y+y,pixel,pixel,c);
                }
                return;
            }
            Box(vh,l+6,b,w-12,h,shadow);Box(vh,l+2,b+4,w-4,h-8,shadow);
            Box(vh,l+8,b+2,w-16,h-4,edge);Box(vh,l+4,b+6,w-8,h-12,edge);
            Box(vh,l+8,b+4,w-16,h-8,color);Box(vh,l+6,b+8,w-12,h-16,color);
            Box(vh,l+10,b+h-6,w-20,2,bright);
            Box(vh,l+9,b+8,w-18,1,shadow);Box(vh,l+8,b+9,1,h-18,shadow);
            foreach(float x in new[]{l+5,l+w-9})foreach(float y in new[]{b+5,b+h-9})Box(vh,x,y,4,4,edge);
        }
        static void Box(VertexHelper vh,float x,float y,float w,float h,Color c)
        {int n=vh.currentVertCount;vh.AddVert(new Vector3(x,y),c,Vector2.zero);vh.AddVert(new Vector3(x,y+h),c,Vector2.zero);vh.AddVert(new Vector3(x+w,y+h),c,Vector2.zero);vh.AddVert(new Vector3(x+w,y),c,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);}
    }
}


