from pathlib import Path
p=Path('Assets/Scripts/WindClawVfx.cs');s=p.read_text(encoding='utf-8-sig')
s=s.replace('claw=Array.Find(Resources.LoadAll<Sprite>("VFX/WindClaw"),s=>s.name=="WindClawSlash");','claw=Resources.Load<Sprite>("VFX/WindClawSharp");')
s=s.replace('pool[i]=im;obj.SetActive(false);','pool[i]=im;if(i==3||i==4){im.type=Image.Type.Filled;im.fillMethod=Image.FillMethod.Vertical;im.fillOrigin=(int)Image.OriginVertical.Top;}obj.SetActive(false);')
s=s.replace('skill.hitInterval+.26f','skill.hitInterval+.34f').replace('scale=cut==0?1:1.08f','scale=cut==0?1:1.12f').replace('new Vector2(230,270)','new Vector2(250,290)')
s=s.replace('Show(3+cut,claw,center-direction*(65*(1-travel)),new Vector2(250,290)*scale,Color.white,rotation);','''Show(3+cut,claw,center,new Vector2(250,290)*scale,Color.white,rotation);pool[3+cut].fillAmount=Mathf.Clamp01(travel);
                    for(int n=0;n<3;n++)
                    {
                        Vector2 local=new Vector2((n-1)*65,(.5f-travel)*290)*scale;float a=rotation*Mathf.Deg2Rad;
                        Vector2 tip=center+new Vector2(local.x*Mathf.Cos(a)-local.y*Mathf.Sin(a),local.x*Mathf.Sin(a)+local.y*Mathf.Cos(a));
                        Show(7+cut*3+n,spark,tip,Vector2.one*(cut==0?24:32),Color.white,rotation);
                        Show(26+n,null,tip-direction*12,new Vector2(4,9),new Color(.5f,1,.9f),rotation);
                    }''')
s=s.replace('final<.22f','final<.23f').replace('(final-.06f)/.16f','(final-.085f)/.145f')
s=s.replace('Color color=h<.055f?Color.white:new Color(.32f,.91f,.80f,cut==0?.75f:.85f);','Color color=h<.15f?Color.white:new Color(.32f,.91f,.80f,.85f);')
s=s.replace('Show(3+cut,claw,center,new Vector2(250,290)*scale,color,rotation);','Show(3+cut,claw,center,new Vector2(250,290)*scale,color,rotation);pool[3+cut].fillAmount=1;')
s=s.replace('if(final>=0&&final<.17f)','if(final>=0&&final<.19f)')
s=s.replace('if(final>=.025f){float t=(final-.025f)/.145f;Show(13,ring,target,Vector2.one*Mathf.Lerp(80,235,Mathf.Clamp01(t*1.5f)),new Color(.65f,1,.92f,1-t));}','''if(final<.12f)Show(24,spark,target,new Vector2(175,195),new Color(1,1,1,final<.05f?1:1-(final-.05f)/.07f),0);
                float t=final/.19f;Show(13,ring,target,Vector2.one*Mathf.Lerp(65,265,Mathf.Clamp01(t*1.8f)),new Color(.65f,1,.92f,1-t));''')
a=s.index('            for(int i=14;i<Count;i++)');b=s.index('\n        }',a)
s=s[:a]+'''            for(int i=14;i<24;i++)
            {
                int n=i-14;float a=n*2.39996f;Vector2 dir=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
                if(age<gather&&n<8)Show(i,null,source+dir*38*(1-age/gather),Vector2.one*3,new Color(.4f,1,.85f));
                if(final>=0&&final<.08f)Show(i,null,target+dir*(8+final*340),new Vector2(6,10),new Color(.75f,1,.94f,1-final/.08f),a*Mathf.Rad2Deg);
                else if(final>=.08f&&final<.32f)
                {
                    float t=(final-.08f)/.24f;Vector2 origin=center+new Vector2((n%3-1)*55,(n/3-1)*47);
                    Show(i,null,origin+dir*(t*75)+Vector2.down*(t*t*24),new Vector2(n%2==0?9:7,n%3==0?20:14)*(1-t*.4f),new Color(.3f,.96f,.82f,1-t),45+n*31+t*70);
                }
                else if(final<0&&hitAt[0]>=0&&age-hitAt[0]<.09f&&n<6)
                {float h=age-hitAt[0];Show(i,null,target+dir*(12+h*200),new Vector2(4,8),new Color(.5f,1,.86f,1-h/.09f),a*Mathf.Rad2Deg);}
            }'''+s[b:]
p.write_text(s,encoding='utf-8')
p=Path('Assets/Scripts/WindHitFeedback.cs');s=p.read_text(encoding='utf-8-sig').replace('hold=final?.06f:0;recoil=final?7:2','hold=final?.05f:0;recoil=final?3:2');p.write_text(s,encoding='utf-8')
p=Path('Assets/Scripts/BattleAnimation.cs');s=p.read_text(encoding='utf-8-sig').replace('final?.067f:.025f','final?.083f:.050f').replace('finalClaw?.067f:.025f','finalClaw?.083f:.050f').replace('new Vector2(final?62:-64,78)','new Vector2(final?98:-98,88)').replace('n.text.fontSize=final?30:25','n.text.fontSize=final?28:24');p.write_text(s,encoding='utf-8')
