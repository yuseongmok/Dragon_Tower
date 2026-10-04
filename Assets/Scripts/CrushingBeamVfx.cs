using UnityEngine;using UnityEngine.UI;
namespace DragonTower {
 public sealed class CrushingBeamVfx:MonoBehaviour {
  BattleView view;BattleModel model;SkillData skill;EarthStylePrototype earth;BattleVfxHudVisibility materials;Material cracks;
  Graphic actor,enemy;Image[] pool;Sprite[] atlas=new Sprite[40];Image bodyGlow,enemyFlash;RectTransform rear,front;public RectTransform Numbers{get;private set;}
  Vector2 mouth,center,foot,target,ground;readonly Vector3[] corners=new Vector3[4];float clock,contact=100,failed=100,final=100;int used;bool committed,held;Vector2 restoreEnemy,holdEnemy;
  public bool Configured=>skill!=null&&skill.chargedBeam;public int HitCount=>Mathf.Max(2,skill==null?2:skill.hitCount);public bool DamageFromSkill{get;set;}public int HitsShown{get;private set;}public int PeakImages{get;private set;}public int DroppedLayers=>earth==null?0:earth.DroppedLayers;
  public bool Channeling=>Configured&&model!=null&&model.BeamPhase==ChargedBeamPhase.Beam;
  public bool Clean=>used==0&&!held&&(earth==null||earth.ActiveCount==0)&&(bodyGlow==null||!bodyGlow.gameObject.activeSelf);
  public Vector2 LastOrigin=>mouth;
  public void Initialize(BattleView v){view=v;}public void BindBattle(BattleModel b){model=b;}
  public void Configure(SkillData data,Graphic player,Graphic foe){Cancel();skill=data;actor=player;enemy=foe;if(!Configured)return;Ensure();bodyGlow.rectTransform.SetParent(actor.transform,false);enemyFlash.rectTransform.SetParent(enemy.transform,false);Fit(bodyGlow);Fit(enemyFlash);}
  static void Fit(Image image){image.rectTransform.anchorMin=Vector2.zero;image.rectTransform.anchorMax=Vector2.one;image.rectTransform.offsetMin=image.rectTransform.offsetMax=Vector2.zero;image.preserveAspect=true;}
  RectTransform Root(string name,bool back){var go=new GameObject(name,typeof(RectTransform),typeof(RectMask2D),typeof(Canvas));var r=go.GetComponent<RectTransform>();r.SetParent(view.frame,false);r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(0,95);r.sizeDelta=new Vector2(480,670);if(back)r.SetSiblingIndex(view.enemyArt.transform.GetSiblingIndex());return r;}
  void Ensure(){if(pool!=null)return;materials=new BattleVfxHudVisibility();cracks=new Material(Resources.Load<Shader>("VFX/TitanEnergyCracks"));var sprites=Resources.LoadAll<Sprite>("VFX/CrushingBeam/BeamAtlas");for(int row=0;row<5;row++)for(int f=0;f<8;f++)foreach(var s in sprites)if(s.name=="Beam"+row+"_"+f)atlas[row*8+f]=s;
   rear=Root("Crushing beam atmosphere",true);front=Root("Crushing beam energy",false);pool=new Image[640];for(int i=0;i<pool.Length;i++){var go=new GameObject("Beam pooled "+i,typeof(RectTransform),typeof(Image));pool[i]=go.GetComponent<Image>();pool[i].raycastTarget=false;pool[i].rectTransform.SetParent(front,false);go.SetActive(false);}
   earth=gameObject.AddComponent<EarthStylePrototype>();earth.style=Resources.Load<EarthSkillLibrary>("EarthSkills/Library").style;earth.Initialize(view);
   bodyGlow=Overlay("Titan charge cracks",cracks);enemyFlash=Overlay("Beam hit silhouette",Resources.Load<Material>("VFX/Wisp/Silhouette"));
   var n=new GameObject("Beam damage overlay",typeof(RectTransform),typeof(Canvas));Numbers=n.GetComponent<RectTransform>();Numbers.SetParent(view.frame,false);Numbers.anchorMin=Numbers.anchorMax=Numbers.pivot=new Vector2(.5f,1);Numbers.sizeDelta=new Vector2(480,850);BattleVfxHudVisibility.BringHudForward(view);
  }
  Image Overlay(string name,Material material){var go=new GameObject(name,typeof(RectTransform),typeof(Image));var image=go.GetComponent<Image>();image.raycastTarget=false;image.material=material;go.SetActive(false);return image;}
  public void Charge(){if(!Configured||model==null||model.BeamPhase!=ChargedBeamPhase.Charging)return;Cancel();clock=0;HitsShown=PeakImages=0;Anchors();earth.Play(EarthSample.HeavyFinalImpact,mouth,ground);}
  public void Fail(){if(!Configured)return;failed=0;committed=false;}
  public void StartBeam(){if(!Configured)return;committed=true;contact=100;Anchors();}
  public void Hit(){if(!Configured||!DamageFromSkill)return;HitsShown++;contact=0;if(HitsShown>=Mathf.Max(2,skill.hitCount)){final=0;holdEnemy=view.enemyArt.rectTransform.anchoredPosition;}Anchors();Render();}
  public void PrepareFrame(){if(held){view.enemyArt.rectTransform.anchoredPosition=restoreEnemy;held=false;}}
  void Anchors(){view.TryGetCharacterAnchor(DragonVisualAnchor.SkillOrigin,view.frame,out mouth);view.TryGetCharacterAnchor(DragonVisualAnchor.CharacterCenter,view.frame,out center);view.TryGetCharacterAnchor(DragonVisualAnchor.GroundPosition,view.frame,out foot);target=view.frame.InverseTransformPoint(view.enemyArt.rectTransform.TransformPoint(Vector2.zero));ground=target-new Vector2(0,70);}
  public void Step(BattleModel b,float dt){PrepareFrame();if(!Configured)return;if(b==null||b.Result==BattleResult.Defeat||(b.Result==BattleResult.Victory&&final>10)){Cancel();return;}clock+=dt;contact+=dt;failed+=dt;final+=dt;Anchors();Render();
   if(final<.085f){restoreEnemy=view.enemyArt.rectTransform.anchoredPosition;view.enemyArt.rectTransform.anchoredPosition=holdEnemy;held=true;}
   bool flash=contact<(final<1?.12f:.07f);enemyFlash.gameObject.SetActive(flash);if(flash){enemyFlash.sprite=(enemy as Image)?.sprite;enemyFlash.color=new Color(1,.94f,.69f,final<1?1:.8f);}
  }
  public void Cancel(){LegendaryScreenDimming.Set(this,Color.clear);PrepareFrame();committed=false;contact=failed=final=100;clock=0;HitsShown=0;if(pool!=null)foreach(var image in pool)if(image!=null)image.gameObject.SetActive(false);used=0;if(bodyGlow!=null)bodyGlow.gameObject.SetActive(false);if(enemyFlash!=null)enemyFlash.gameObject.SetActive(false);if(earth!=null)earth.Clear();}
  void OnDisable(){if(Configured)model?.CancelChargedBeam();Cancel();}
  void OnDestroy(){Cancel();materials?.Dispose();if(cracks!=null)Destroy(cracks);if(rear!=null)Destroy(rear.gameObject);if(front!=null)Destroy(front.gameObject);if(Numbers!=null)Destroy(Numbers.gameObject);if(bodyGlow!=null)Destroy(bodyGlow.gameObject);if(enemyFlash!=null)Destroy(enemyFlash.gameObject);if(earth!=null)Destroy(earth);}
  static float Fade(float t,float begin,float end)=>Mathf.Clamp01((end-t)/(end-begin));static Color W(float a=1)=>new Color(1,1,1,Mathf.Clamp01(a));
  Sprite S(int row,float time)=>atlas[row*8+Mathf.FloorToInt(Mathf.Repeat(time*14,8))];
  void Draw(Sprite sprite,Vector2 p,Vector2 size,Color color,float angle=0,bool back=false,bool glow=false){if(color.a<.003f||used>=pool.Length)return;var im=pool[used++];im.sprite=sprite;im.color=color;im.material=glow?materials.additive:materials.alpha;var r=im.rectTransform;var parent=back?rear:front;if(r.parent!=parent)r.SetParent(parent,false);r.SetAsLastSibling();r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(Mathf.Round(p.x/2)*2,Mathf.Round((p.y-95)/2)*2);r.sizeDelta=size;r.localRotation=Quaternion.Euler(0,0,angle);im.gameObject.SetActive(true);}
  void Ring(Vector2 p,float size,float alpha,float angle=0,bool back=false){Draw(S(1,clock),p,new Vector2(size,size*.54f),new Color(1,.64f,.19f,alpha),angle,back);Draw(S(1,clock+.15f),p,new Vector2(size*.91f,size*.47f),new Color(1,.96f,.63f,alpha*.7f),angle,back,true);}
  void Core(Vector2 p,float size,float power){Draw(S(2,clock),p,Vector2.one*size,W(power));Draw(S(2,clock+.23f),p,Vector2.one*size*.66f,W(power*.70f),clock*73,false,true);}
  void Render(){LegendaryScreenDimming.Set(this,Color.clear);if(pool==null)return;int previous=used;used=0;materials.Update(view);earth.BeginLayers(clock,true);var phase=model==null?ChargedBeamPhase.Idle:model.BeamPhase;bodyGlow.gameObject.SetActive(false);
   if(phase==ChargedBeamPhase.Charging)Charging((float)model.ChargeProgress);
   if(phase==ChargedBeamPhase.Priming){float q=Mathf.Clamp01((float)model.BeamPhaseElapsed/Mathf.Max(.01f,skill.beamIgnitionDelay));Atmosphere(q);Charging(.8f+.2f*q);earth.Crack(foot,.25f,1.65f,1);Ring(foot,310*(1-q*.25f),1,0,true);}
   if(phase==ChargedBeamPhase.Beam){float t=(float)model.BeamPhaseElapsed;Atmosphere(1);ChargeSkin(.85f+.15f*Mathf.Clamp01(t/5));Beam(t,1);OngoingImpact(t);GroundPressure(t);if(t<.20f)earth.Impulse(t,16,.20f);else earth.Impulse(Mathf.Repeat(t,.16f),1.3f,.16f);}
   if(final<1.5f){Atmosphere(Fade(final,.18f,.9f));FinalBurst(final);if(final<.25f)Beam(skill.beamDuration,Fade(final,0,.25f));earth.Impulse(final,22,.30f);}
   if(failed<.55f)Failure(failed);
   earth.EndLayers();for(int i=used;i<previous;i++)pool[i].gameObject.SetActive(false);PeakImages=Mathf.Max(PeakImages,used+earth.ActiveCount);
  }
  void Atmosphere(float a){LegendaryScreenDimming.Set(this,new Color(.015f,.014f,.017f,.78f*a));}
  void ChargeSkin(float q){if(q<=0)return;bodyGlow.gameObject.SetActive(true);bodyGlow.sprite=(actor as Image)?.sprite;actor.rectTransform.GetWorldCorners(corners);cracks.SetVector("_ActorRect",new Vector4(corners[0].x,corners[0].y,corners[2].x,corners[2].y));cracks.SetFloat("_Charge",q);bodyGlow.color=Color.Lerp(new Color(1,.25f,.015f,.65f),new Color(1,1,.86f,1),q);}
  void Charging(float q){ChargeSkin(q);float strength=Mathf.Clamp01(q*1.4f);
   // Whole-body induction: orbiting fragments rise from the ground, never converge on the mouth.
   earth.Crack(foot,.25f,1+q*.65f,strength);
   for(int i=0;i<26;i++){float u=Mathf.Repeat(clock*(.22f+q*.18f)+i*.137f,1),a=i*2.399f+clock*(1+q);float z=Mathf.Sin(a);var p=foot+new Vector2(Mathf.Cos(a)*(90+q*45),u*(110+q*150)+z*18);earth.Rock(p,Vector2.one*(7+i%4*5)*(z>0?1.15f:.65f),clock*.2f+i*.13f,strength*(1-u*.6f),z>0?2:0,i*31+clock*35);Draw(S(0,clock+i*.1f),p,new Vector2(20+q*30,3+q*3),new Color(1,.73f,.28f,strength*.65f),90,z<0,true);}
   for(int i=0;i<3;i++){float u=Mathf.Repeat(clock*.7f+i/3f,1);Orbit(foot+Vector2.up*u*(120+q*140),110+q*35,21,clock*2+i, strength*(1-u)*.75f);}
   for(int i=0;i<5;i++)earth.Dust(foot+new Vector2((i-2)*43,8),new Vector2(65+q*30,26),Mathf.Repeat(clock*.3f+i*.2f,1),strength*.4f,0);
   earth.Impulse(Mathf.Repeat(clock,.2f),q*.75f,.2f);
  }
  // Project circular flow into ellipses: the near half is brighter/thicker and occludes the actor.
  void Orbit(Vector2 p,float radius,float depth,float phase,float alpha){for(int i=0;i<24;i++){float a=i*Mathf.PI*2/24+phase,z=Mathf.Sin(a);if(Mathf.Sin(a*3+phase)<-.55f)continue;var at=p+new Vector2(Mathf.Cos(a)*radius,z*depth);float angle=Mathf.Atan2(Mathf.Cos(a)*depth,-Mathf.Sin(a)*radius)*Mathf.Rad2Deg;float near=(z+1)*.5f;Draw(S(0,clock),at,new Vector2(22+near*13,3+near*5),Color.Lerp(new Color(.55f,.26f,.07f,alpha*.4f),new Color(1,.97f,.7f,alpha),near),angle,z<0,z>0);}}
  void Beam(float t,float fade){
   Vector2 origin=new Vector2(0,Mathf.Max(foot.y,-223));float top=front.anchoredPosition.y+front.rect.height*.5f;
   float over=Mathf.Clamp01((t-(skill.beamDuration-.85f))/.70f),width=(285+over*55)*(1+.018f*Mathf.Sin(clock*31));
   float rise=Mathf.Clamp01(t/.075f),height=(top-origin.y+100)*Mathf.Max(.02f,rise);width*=Mathf.Max(.06f,fade);var p=origin+Vector2.up*(height*.5f);
   // Parallel-sided cylindrical volume, not a scaled fan. Offset highlight gives curved cross-section.
   Draw(S(0,clock*.8f),p,new Vector2(height,width*1.14f),new Color(.22f,.13f,.07f,fade),90,true);
   Draw(S(0,clock),p,new Vector2(height,width),new Color(1,.38f,.04f,fade),90,true);
   Draw(S(0,clock+.18f),p+Vector2.left*12,new Vector2(height,width*.78f),new Color(1,.77f,.23f,fade),90,true);
   Draw(S(0,clock+.31f),p+Vector2.left*22,new Vector2(height,width*.44f),new Color(1,1,.86f,fade),90,true,true);
   Draw(S(0,clock+.42f),p+Vector2.right*38,new Vector2(height,width*.5f),new Color(1,.72f,.24f,fade*.16f),90,false,true);
   // Continuous axial streams and rotating pressure bands share a fixed radius at every height.
   for(int i=0;i<7;i++){float u=Mathf.Repeat(clock*.65f+i/7f,1);Orbit(origin+Vector2.up*(u*height),width*.49f,30,clock*3+i*.8f,fade*(.55f+over*.2f));}
   for(int i=0;i<28;i++){float a=i*2.399f+clock*1.8f,z=Mathf.Sin(a),u=Mathf.Repeat(clock*.95f+i*.137f,1);var at=origin+new Vector2(Mathf.Cos(a)*width*.49f,u*height+z*22);Draw(S(0,clock+i),at,new Vector2(55+i%4*22,3+(z+1)*3),new Color(1,.94f,.62f,fade*(z>0?.85f:.3f)),90,z<0,z>0);if(i%4==0)earth.Rock(at,Vector2.one*(12+(z+1)*8),clock+i*.13f,fade,z>0?2:0,i*31+clock*95);}
   earth.Crack(origin,.22f,1.75f,fade);Orbit(origin,width*.57f,34,clock*2,fade);for(int i=0;i<5;i++)earth.Dust(origin+new Vector2((i-2)*55,7),new Vector2(95,45),Mathf.Repeat(clock*.6f+i*.2f,1),fade*.65f,0);
  }
  void OngoingImpact(float t){
   // Contact is conveyed by the existing enemy flash and shards; no circular hit-point or core.
   if(contact<.18f){float f=1-contact/.18f;for(int i=0;i<6;i++){var p=target+new Vector2((i-2.5f)*32,contact*310+i%2*26);Draw(S(0,clock+i),p,new Vector2(45,4),new Color(1,.97f,.72f,f),75+i*5,false,true);}earth.Debris(target,contact,.55f,HitsShown,false);}
  }
  void GroundPressure(float t){for(int i=0;i<6;i++){float u=(i+1)/6f;var p=Vector2.Lerp(foot,ground,u);float pulse=Mathf.Repeat(t*1.4f-i*.16f,1);earth.Crack(p,.18f,.55f+u*.55f,.9f);earth.Layer(EarthModule.GroundSlab,pulse,p+new Vector2(0,Mathf.Sin(pulse*Mathf.PI)*24),new Vector2(110,40),.8f,0,i*17);earth.Dust(p+new Vector2((i%2==0?-1:1)*75,pulse*55),new Vector2(100,55),pulse,(1-pulse)*.7f,0);if(i%2==0)earth.Shockwave(p,pulse*.9f,.65f,i);}}
  void FinalBurst(float t){float h=Mathf.Max(0,t-.08f),e=1-Mathf.Pow(1-Mathf.Clamp01(h/.22f),3),fade=Fade(t,.42f,1.45f);Draw(S(3,clock),target,Vector2.one*(160+e*510),new Color(.25f,.17f,.095f,fade),17,true);Draw(S(3,clock+.13f),target,Vector2.one*(90+e*430),W(fade),-clock*32);Draw(S(3,clock+.3f),target,Vector2.one*(65+e*290),new Color(1,1,.86f,fade*.86f),clock*51,false,true);Core(target,120+e*200,Fade(t,.12f,.55f));Ring(target,140+e*550,fade,0);earth.Impact(ground,h,2.3f,87);for(int i=0;i<3;i++){earth.Rise(ground+new Vector2((i-1)*110,-15+i%2*24),h+.14f,i==1?1.18f:.82f,i+30);earth.Debris(target+new Vector2((i-1)*70,0),h-i*.025f,1.35f+i*.15f,77+i,true);}for(int j=0;j<10;j++){float a=j*2.399f;var p=target+new Vector2(Mathf.Cos(a)*h*200,Mathf.Sin(a)*h*160+35);earth.Dust(p,Vector2.one*(95+h*110),h/1.3f,fade*.7f,j%3==0?0:2);}}
  void Failure(float t){float fade=Fade(t,.10f,.55f);for(int i=0;i<18;i++){float a=i*2.399f;var p=center+new Vector2(Mathf.Cos(a)*(65+t*100),Mathf.Sin(a)*65-180*t*t);earth.Rock(p,Vector2.one*(7+i%3*5),t+i*.13f,fade,1,i*23);earth.Layer(EarthModule.Grain,0,p+Vector2.up*7,Vector2.one*5,fade,1);}earth.Crack(foot,.25f,1.4f,fade);}

 }
}
