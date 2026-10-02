from pathlib import Path
p=Path('Assets/Scripts/WindClawVfx.cs')
s=Path('work/WindClawV4-backup/WindClawVfx.cs').read_text()
s=s.replace('const int Count=30;', 'const int Count=32;')
s=s.replace('public int HitsShown', 'public bool FinalLayersVisible=>pool!=null&&pool[24].gameObject.activeSelf&&pool[30].gameObject.activeSelf&&pool[31].gameObject.activeSelf&&pool[13].gameObject.activeSelf;\n        public int HitsShown')
s=s.replace('i>=26?"WindClawTipDebris"', 'i==30?"WindClawMintBurst":i==31?"WindClawCyanBurst":i>=26?"WindClawTipDebris"')
start=s.index('            if(final>=0&&final<.19f)')
end=s.index('            for(int i=14;',start)
s=s[:start]+'''            // The confirmed final hit keeps its contact spark; the burst follows 20ms later.
            // Presentation only: no extra damage, hit cue or global time change.
            if(final>=0&&final<.22f)
            {
                if(final<.11f)root.anchoredPosition=new Vector2(Mathf.Round(4*Mathf.Sin(final/.11f*Mathf.PI)),-132);
                if(final<.12f)Show(24,spark,target,new Vector2(175,195),new Color(1,1,1,final<.05f?1:1-(final-.05f)/.07f),0);
                float burst=final-.02f;
                if(burst>=0)
                {
                    if(burst<.12f)Show(30,spark,target,Vector2.one*Mathf.Lerp(80,205,Mathf.Clamp01(burst/.055f)),new Color(.65f,1,.84f,(1-burst/.12f)*.85f),25);
                    if(burst<.16f)Show(31,shared.Frame(WindModule.WindHitSpark,.4f),target,Vector2.one*Mathf.Lerp(95,240,Mathf.Clamp01(burst/.075f)),new Color(.06f,.73f,.91f,(1-burst/.16f)*.65f),-15);
                    Show(13,ring,target,Vector2.one*Mathf.Lerp(65,275,Mathf.Clamp01(burst/.085f)),new Color(.65f,1,.92f,1-burst/.20f));
                }
            }
'''+s[end:]
p.write_text(s,encoding='utf-8')
p=Path('Assets/Editor/WindClawValidation.cs');s=p.read_text(encoding='utf-8-sig').replace('Four impact depths coincide on the final contact','Contact spark overlaps the following final burst');p.write_text(s,encoding='utf-8')
