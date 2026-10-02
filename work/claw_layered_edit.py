from pathlib import Path
p=Path('Assets/Scripts/WindClawVfx.cs');s=p.read_text(encoding='utf-8-sig')
s=s.replace('Sprite claw,spark,ring;','Sprite claw,spark,ring;readonly Sprite[] bodyFrames=new Sprite[8],goldFrames=new Sprite[8];')
s=s.replace('public int HitsShown=>hits;','public bool Drawing {get;private set;}\n        public int HitsShown=>hits;')
s=s.replace('claw=Resources.Load<Sprite>("VFX/WindClawSharp");','var frames=Resources.LoadAll<Sprite>("VFX/WindClawLayered");for(int f=0;f<8;f++){int index=f;bodyFrames[f]=Array.Find(frames,s=>s.name=="ClawBody"+index);goldFrames[f]=Array.Find(frames,s=>s.name=="ClawGold"+index);}claw=bodyFrames[7];')
s=s.replace('if(i==3||i==4){im.type=Image.Type.Filled;im.fillMethod=Image.FillMethod.Vertical;im.fillOrigin=(int)Image.OriginVertical.Top;}','')
s=s.replace('i==24?"WindClawFinalImpact":i>=26?','i==24?"WindClawFinalImpact":i==25||i==29?"WindClawWarmRim":i>=26?')
s=s.replace('pool[i]=im;obj.SetActive(false);}','pool[i]=im;obj.SetActive(false);}pool[5].transform.SetSiblingIndex(0);pool[6].transform.SetSiblingIndex(0);')
s=s.replace('public void Clear(){age=10;','public void Clear(){Drawing=false;age=10;')
s=s.replace('if(pool==null)return;foreach(var im in pool)','Drawing=false;if(pool==null)return;foreach(var im in pool)')
a=s.index('            for(int cut=0;cut<2;cut++)');b=s.index('            if(final>=0&&final<.19f)',a)
s=s[:a]+'''            for(int cut=0;cut<2;cut++)
            {
                float due=skill.initialHitDelay+cut*skill.hitInterval,flight=Mathf.Min(.08f,skill.initialHitDelay),travel=(age-(due-flight))/Mathf.Max(.001f,flight),h=hitAt[cut]<0?-1:age-hitAt[cut];
                float baseRotation=cut==0?42:-42,scale=cut==0?1:1.12f,mirror=cut==0?1:-1;
                Vector2 direction=cut==0?new Vector2(.707f,-.707f):new Vector2(-.707f,-.707f);
                if(travel>=0&&travel<1&&h<0)
                {
                    Drawing=true;
                    float sweep=1-Mathf.Pow(1-travel,1.7f),angle=baseRotation+(cut==0?-26:26)*(1-sweep);
                    Vector2 size=new Vector2(260*Mathf.Lerp(.68f,1,sweep),290)*scale,position=center-direction*(26*(1-sweep));
                    int frame=Mathf.Clamp((int)(sweep*7),0,6);
                    Show(1,shared.Frame(WindModule.WindTrail,travel),Vector2.Lerp(source,center,sweep),new Vector2(46,150),new Color(.3f,.9f,1,.65f),-Mathf.Atan2((center-source).x,(center-source).y)*Mathf.Rad2Deg);
                    Layer(2,bodyFrames[frame],source+direction*20,new Vector2(70,80),new Color(.45f,.9f,1,1-travel),angle,mirror);
                    Layer(5+cut,bodyFrames[frame],position+new Vector2(-mirror*7,-5),size*1.04f,new Color(.08f,.35f,.68f,.65f),angle-8*mirror,mirror);
                    Layer(3+cut,bodyFrames[frame],position,size,Color.white,angle,mirror);
                    Layer(cut==0?25:29,goldFrames[Mathf.Max(0,frame-1)],position+new Vector2(mirror*9,-5),size,new Color(1,1,1,.72f),angle-11*mirror,mirror);
                    for(int n=0;n<3;n++)
                    {
                        Vector2 tip=ClawPoint(n,(frame+1)/8f);tip=new Vector2((tip.x/96-.5f)*size.x*mirror,(tip.y/128-.5f)*size.y);
                        float a=angle*Mathf.Deg2Rad;tip=position+new Vector2(tip.x*Mathf.Cos(a)-tip.y*Mathf.Sin(a),tip.x*Mathf.Sin(a)+tip.y*Mathf.Cos(a));
                        Show(7+cut*3+n,spark,tip,Vector2.one*(cut==0?24:32),Color.white,angle);
                        Show(26+n,null,tip-direction*10,new Vector2(3,8),new Color(.5f,.95f,1),angle);
                    }
                }
                if(h>=0&&(final<0||final<.23f))
                {
                    float fade=final<0?1:1-Mathf.Clamp01((final-.085f)/.145f);
                    Color color=h<.06f?Color.white:new Color(.45f,.93f,.95f,.82f);color.a*=fade;
                    Vector2 size=new Vector2(260,290)*scale;
                    Layer(3+cut,claw,center,size,color,baseRotation,mirror);
                    if(h<.10f)Layer(5+cut,claw,center+new Vector2(-mirror*7,-5),size*1.04f,new Color(.08f,.35f,.68f,(1-h/.1f)*.6f),baseRotation-8*mirror,mirror);
                    if(h<.16f)Layer(cut==0?25:29,goldFrames[7],center+new Vector2(mirror*(9+h*40),-5-h*15),size,new Color(1,1,1,(1-h/.16f)*.72f),baseRotation-mirror*(11+h*55),mirror);
                    if(h<.12f)for(int n=0;n<3;n++)
                    {
                        Vector2 line=new Vector2((n-1)*43,0);float a=baseRotation*Mathf.Deg2Rad;line=new Vector2(line.x*Mathf.Cos(a),line.x*Mathf.Sin(a));
                        Show(7+cut*3+n,spark,target+line,new Vector2(cut==0?55:90,cut==0?68:105),new Color(1,1,1,1-Mathf.Max(0,h-(cut==0?.02f:.05f))/.07f),baseRotation);
                    }
                }
            }
'''+s[b:]
# The ribbon is sampled in local space; source and runtime tips use the same curve.
pos=s.index('        void Render()')
s=s[:pos]+'''        void Layer(int i,Sprite sprite,Vector2 point,Vector2 size,Color color,float angle,float mirror)
        {Show(i,sprite,point,size,color,angle);pool[i].rectTransform.localScale=new Vector3(mirror,1,1);}
        static Vector2 ClawPoint(int claw,float t)
        {
            float x=claw*20;Vector2 a=new Vector2(8+x,claw==1?123:claw==0?111:103),b=new Vector2(-2+x,65),c=new Vector2(16+x,-13),d=new Vector2(49+x,claw==1?18:claw==0?25:32);
            float u=1-t;return a*u*u*u+b*3*u*u*t+c*3*u*t*t+d*t*t*t;
        }
'''+s[pos:]
p.write_text(s,encoding='utf-8')
p=Path('Assets/Editor/WindClawValidation.cs');s=p.read_text(encoding='utf-8-sig');s=s.replace('draw|=Array.Exists(images,im=>im.name=="WindClawSlash"&&im.gameObject.activeSelf&&im.fillAmount>0&&im.fillAmount<1);','draw|=vfx.Drawing;');p.write_text(s,encoding='utf-8')
