from pathlib import Path
p=Path('Assets/Scripts/WindClawVfx.cs');s=p.read_text(encoding='utf-8-sig')
s=s.replace('const int Count=30;','const int Count=32;Material highlight;').replace('goldFrames','coreFrames').replace('"VFX/WindClawLayered"','"VFX/WindClawHierarchy"').replace('"ClawGold"','"ClawCore"')
s=s.replace('public bool Drawing {get;private set;}','public bool Drawing {get;private set;}\n        public bool FinalLayersVisible=>pool!=null&&pool[24].gameObject.activeSelf&&pool[30].gameObject.activeSelf&&pool[31].gameObject.activeSelf&&pool[13].gameObject.activeSelf;')
s=s.replace('pool=new Image[Count];','highlight=new Material(Resources.Load<Shader>("VFX/WindClawHighlight"));pool=new Image[Count];')
s=s.replace('"WindClawWarmRim":i>=26?','"WindClawHotCore":i==30?"WindClawMintBurst":i==31?"WindClawCyanBurst":i>=26?')
s=s.replace('pool[i]=im;obj.SetActive(false);','pool[i]=im;if(i==24||i==25||i==29)im.material=highlight;obj.SetActive(false);')
s=s.replace('pool[6].transform.SetSiblingIndex(0);','pool[6].transform.SetSiblingIndex(0);pool[24].transform.SetAsLastSibling();')
s=s.replace('void OnDisable(){Clear();}','void OnDisable(){Clear();}\n        void OnDestroy(){if(highlight!=null)Destroy(highlight);}')
s=s.replace('new Vector2(260*Mathf.Lerp(.68f,1,sweep),290)*scale','new Vector2(260*Mathf.Lerp(.80f,1,sweep),290*Mathf.Lerp(.88f,1,sweep))*scale')
s=s.replace('Layer(5+cut,bodyFrames[frame],position+new Vector2(-mirror*7,-5),size*1.04f,new Color(.08f,.35f,.68f,.65f),angle-8*mirror,mirror);','Layer(5+cut,bodyFrames[Mathf.Max(0,frame-2)],position+new Vector2(-mirror*3,-3),size*1.015f,new Color(.08f,.30f,.52f,cut==0?.27f:.36f),angle-3*mirror,mirror);')
s=s.replace('Layer(3+cut,bodyFrames[frame],position,size,Color.white,angle,mirror);','Layer(3+cut,bodyFrames[Mathf.Max(0,frame-1)],position,size,new Color(.82f,1,1,cut==0?.9f:1),angle,mirror);')
s=s.replace('Layer(cut==0?25:29,coreFrames[Mathf.Max(0,frame-1)],position+new Vector2(mirror*9,-5),size,new Color(1,1,1,.72f),angle-11*mirror,mirror);','Layer(cut==0?25:29,coreFrames[frame],position+direction*2,size*1.004f,new Color(1,1,1,cut==0?.65f:.95f),angle+mirror,mirror);')
s=s.replace('final<.23f','final<.18f')
s=s.replace('float fade=final<0?1:1-Mathf.Clamp01((final-.085f)/.145f);\n                    Color color=h<.06f?Color.white:new Color(.45f,.93f,.95f,.82f);color.a*=fade;','''float fade=final<0?1:1-Mathf.Clamp01((final-.04f)/.14f);
                    Color color;
                    if(final<0)color=h<.04f?Color.white:new Color(.35f,.85f,.89f,.66f);
                    else if(final<.04f)color=Color.white;
                    else if(final<.09f)color=Color.Lerp(new Color(.65f,1,.85f),new Color(.16f,.78f,.94f),(final-.04f)/.05f);
                    else color=Color.Lerp(new Color(.16f,.78f,.94f),new Color(.025f,.28f,.38f),(final-.09f)/.09f);
                    color.a*=fade;''')
s=s.replace('if(h<.10f)Layer(5+cut,claw,center+new Vector2(-mirror*7,-5),size*1.04f,new Color(.08f,.35f,.68f,(1-h/.1f)*.6f),baseRotation-8*mirror,mirror);','float trailLife=cut==0?.11f:.15f;if(h<trailLife)Layer(5+cut,claw,center+new Vector2(-mirror*3,-3-h*10),size*1.015f,new Color(.08f,.30f,.52f,(1-h/trailLife)*.32f),baseRotation-3*mirror,mirror);')
s=s.replace('if(h<.16f)Layer(cut==0?25:29,coreFrames[7],center+new Vector2(mirror*(9+h*40),-5-h*15),size,new Color(1,1,1,(1-h/.16f)*.72f),baseRotation-mirror*(11+h*55),mirror);','float coreAge=final>=0?final:h;float coreLife=cut==0?.035f:.055f;if(coreAge<coreLife)Layer(cut==0?25:29,coreFrames[7],center,size,new Color(1,1,1,(1-coreAge/coreLife)*(cut==0?.65f:.95f)),baseRotation,mirror);')
a=s.index('            if(final>=0&&final<.19f)');b=s.index('            for(int i=14;i<24;i++)',a)
s=s[:a]+'''            if(final>=0&&final<.20f)
            {
                if(final<.11f)root.anchoredPosition=new Vector2(Mathf.Round(4*Mathf.Sin(final/.11f*Mathf.PI)),-132);
                // All layers begin on the confirmed second hit, with staggered expansion/decay.
                if(final<.065f)Show(24,spark,target,Vector2.one*Mathf.Lerp(105,140,Mathf.Clamp01(final/.025f)),new Color(1,1,1,1-final/.065f),0);
                if(final<.12f){float t=final/.12f;Show(30,spark,target,Vector2.one*Mathf.Lerp(75,190,Mathf.Clamp01((final-.008f)/.05f)),new Color(.65f,1,.84f,(1-t)*.78f),25);}
                if(final<.16f){float t=final/.16f;Show(31,shared.Frame(WindModule.WindHitSpark,.4f),target,Vector2.one*Mathf.Lerp(90,225,Mathf.Clamp01((final-.016f)/.075f)),new Color(.06f,.73f,.91f,(1-t)*.56f),-15);}
                float wave=final/.20f;Show(13,ring,target,Vector2.one*Mathf.Lerp(90,270,Mathf.Clamp01((final-.012f)/.10f)),new Color(.025f,.52f,.55f,(1-wave)*.85f));
            }
'''+s[b:]
p.write_text(s,encoding='utf-8')
p=Path('Assets/Editor/WindClawValidation.cs');s=p.read_text(encoding='utf-8-sig');s=s.replace('bool cross=false,draw=false;','bool cross=false,draw=false,layers=false;');s=s.replace('cross|=vfx.CrossVisible;','cross|=vfx.CrossVisible;layers|=vfx.FinalLayersVisible;');s=s.replace('Check(draw,"Claw is progressively drawn before contact");','Check(draw,"Claw is progressively drawn before contact");Check(layers,"Four impact depths coincide on the final contact");');p.write_text(s,encoding='utf-8')
