using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    // Independent art samples. Reads existing status only; never writes damage or timers.
    public sealed class FireStylePrototype:MonoBehaviour
    {
        public FireVfxStyle style;
        const int PerGroup=192;Image[][] pool;RectTransform[] roots;
        readonly int[] used=new int[4],last=new int[4];
        bool upperOverlay;float rootCenterY=30;BattleVfxHudVisibility hudVisibility;
        public void SetUpperOverlay(bool enabled){upperOverlay=enabled;if(enabled&&hudVisibility==null)hudVisibility=new BattleVfxHudVisibility();rootCenterY=enabled?95:30;foreach(var r in roots){r.anchoredPosition=new Vector2(0,rootCenterY);r.sizeDelta=new Vector2(480,enabled?660:530);}if(enabled)BattleVfxHudVisibility.BringHudForward(view);}
        Material Surface(bool glow)=>upperOverlay?(glow?hudVisibility.additive:hudVisibility.alpha):(glow?style.highlight:Graphic.defaultGraphicMaterial);
        BattleView view;Vector2 origin,center,feet;float age=20;bool running;
        public int PeakImages{get;private set;}public int ActiveCount{get;private set;}
        public int DroppedLayers{get;private set;}
        public int Capacity=>PerGroup*4;
        public bool BurnShown{get;private set;}
        public bool SampleActive=>running&&age<3.55f;
        public bool presentationEnabled=true;
        // Shared drawing API for real skill presentations; combat remains outside this renderer.
        public void BeginLayers(float time,Vector2 target,Vector2 ground){if(upperOverlay)hudVisibility.Update(view);age=time;center=target;feet=ground;for(int g=0;g<4;g++)used[g]=0;}
        public void EndLayers(){ActiveCount=0;for(int g=0;g<4;g++){for(int i=used[g];i<last[g];i++)pool[g][i].gameObject.SetActive(false);last[g]=used[g];ActiveCount+=used[g];}PeakImages=Mathf.Max(PeakImages,ActiveCount);}
        public void Layer(FireStyleModule m,float time,Vector2 p,Vector2 size,float alpha=1,float angle=0,bool back=false,bool glow=false,bool sequence=false)=>Draw(m,time,p,size,White(alpha),angle,back,glow,sequence);
        public void LayeredFlame(Vector2 p,Vector2 size,float time,float alpha=1,bool back=false,float hot=1,float angle=0)=>Flame(p,size,time,alpha,back,hot,angle);
        public void Status(bool active){BurnShown=active;if(active)Burn();}
        public void Initialize(BattleView owner)
        {
            if(pool!=null)return;view=owner;pool=new Image[4][];roots=new RectTransform[4];
            for(int g=0;g<4;g++){
                var go=new GameObject("Fire prototype "+g,typeof(RectTransform),typeof(RectMask2D));var r=go.GetComponent<RectTransform>();roots[g]=r;r.SetParent(view.frame,false);r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(0,30);r.sizeDelta=new Vector2(480,530);
                if(g<2)r.SetSiblingIndex(view.enemyArt.transform.GetSiblingIndex());
                pool[g]=new Image[PerGroup];
                for(int i=0;i<PerGroup;i++){var child=new GameObject("Fire pooled "+g+"-"+i,typeof(RectTransform),typeof(Image));var im=child.GetComponent<Image>();im.rectTransform.SetParent(r,false);im.raycastTarget=false;im.material=g%2==1?style.highlight:null;pool[g][i]=im;child.SetActive(false);}
            }
        }
        public void Play(Vector2 source,Vector2 target,Vector2 ground){Clear();origin=source;center=target;feet=ground;age=0;running=true;PeakImages=0;}
        public void Step(BattleModel model,float delta)
        {
            if(pool==null)return;if(model==null||model.Result!=BattleResult.Fighting||!presentationEnabled){Clear();return;}
            age+=Mathf.Max(0,delta);BurnShown=model.EnemyBurning;
            for(int g=0;g<4;g++)used[g]=0;
            if(SampleActive){if(age<1.5f)Living();if(age>=1.42f)Explosion(age-1.42f);}
            if(BurnShown)Burn();
            ActiveCount=0;for(int g=0;g<4;g++){for(int i=used[g];i<last[g];i++)pool[g][i].gameObject.SetActive(false);last[g]=used[g];ActiveCount+=used[g];}
            PeakImages=Mathf.Max(PeakImages,ActiveCount);
        }
        public void SpriteLayer(Sprite sprite,Vector2 p,Vector2 size,float alpha=1,float angle=0,bool back=false,bool glow=false,bool foreground=false){int g=foreground?3:(back?0:2)+(glow?1:0);if(sprite==null||alpha<=0)return;if(used[g]>=PerGroup){DroppedLayers++;return;}var im=pool[g][used[g]++];im.sprite=sprite;im.material=Surface(!foreground&&g%2==1);im.color=White(alpha);var r=im.rectTransform;r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(Mathf.Round(p.x/2)*2,Mathf.Round((p.y-rootCenterY)/2)*2);r.sizeDelta=size;r.localRotation=Quaternion.Euler(0,0,angle);if(!im.gameObject.activeSelf)im.gameObject.SetActive(true);}
        void Draw(FireStyleModule module,float clock,Vector2 p,Vector2 size,Color tint,float angle=0,bool back=false,bool glow=false,bool burstFrame=false)
        {
            int g=(back?0:2)+(glow?1:0);if(tint.a<=0)return;if(used[g]>=PerGroup){DroppedLayers++;return;}var im=pool[g][used[g]++];im.material=Surface(g%2==1);im.sprite=burstFrame?style.Frame(module,clock):style.Get(module,clock);im.color=tint;
            var r=im.rectTransform;r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.anchoredPosition=new Vector2(Mathf.Round(p.x/2)*2,Mathf.Round((p.y-rootCenterY)/2)*2);r.sizeDelta=size;r.localRotation=Quaternion.Euler(0,0,angle);
            if(!im.gameObject.activeSelf)im.gameObject.SetActive(true);
        }
        static Color White(float a)=>new Color(1,1,1,Mathf.Clamp01(a));
        void Flame(Vector2 p,Vector2 size,float clock,float a,bool back=false,float hot=1,float angle=0)
        {
            float sway=Mathf.Sin(clock*9)*size.x*.045f;
            Draw(FireStyleModule.DarkFlame,clock*.83f,p+new Vector2(sway,0),Vector2.Scale(size,new Vector2(1.10f,1)),White(a),angle-3,back);
            Draw(FireStyleModule.Body,clock,p+new Vector2(-sway,-size.y*.02f),Vector2.Scale(size,new Vector2(.91f,.93f)),White(a),angle+3,back);
            Draw(FireStyleModule.Inner,clock*1.27f+.21f,p+new Vector2(sway*.3f,-size.y*.085f),Vector2.Scale(size,new Vector2(.65f,.78f)),White(a),angle-5,back);
            Draw(FireStyleModule.Core,clock*1.53f+.37f,p+new Vector2(-sway*.3f,-size.y*.15f),Vector2.Scale(size,new Vector2(.48f,.48f)),White(a*hot),angle,back,true);
        }
        void Living()
        {
            float a=Mathf.Clamp01(age/.16f)*Mathf.Clamp01((1.5f-age)/.2f);var p=new Vector2(-95,8);
            Draw(FireStyleModule.Shockwave,0,p-new Vector2(0,80),new Vector2(125,30),White(a*.5f),0,true,true,true);
            Flame(p,new Vector2(155,205),age,a);
            Draw(FireStyleModule.Tongue,age*1.3f,p+new Vector2(40,40+Mathf.Sin(age*7)*12),new Vector2(66,116),White(a*.9f),-18);
            Draw(FireStyleModule.Heat,age*.8f,p+new Vector2(5,115),new Vector2(165,100),White(a*.28f),0,true);
            Flame(origin+new Vector2(0,10),new Vector2(30,44),age,a*.8f,false,.55f);
            for(int i=0;i<18;i++){float t=Mathf.Repeat(age*.7f+i*.137f,1),x=Mathf.Sin(i*2.4f+t*3)*65;bool back=i%3==0;float scale=back?12:24;
                Draw(FireStyleModule.Ember,age+i*.1f,p+new Vector2(x,-45+t*175),Vector2.one*scale,White(a*(1-t)),i*37,back,!back);}
        }
        void Explosion(float t)
        {
            if(t<.25f){float k=t/.25f;Flame(center,Vector2.one*Mathf.Lerp(82,34,k),age,1);for(int i=0;i<12;i++){float a=i*2.4f;Draw(FireStyleModule.Ember,age+i,center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(1-k)*130,new Vector2(24,45),White(k),a*Mathf.Rad2Deg);}}
            float h=t-.25f;if(h<0)return;
            if(h<.16f){Draw(FireStyleModule.Burst,h/.16f,center,Vector2.one*Mathf.Lerp(100,310,h/.16f),White(1-h/.16f),15,false,true,true);Draw(FireStyleModule.Core,age,center,new Vector2(125,170),White(1-h/.16f),90,false,true);}
            if(h<.60f){float e=1-Mathf.Pow(1-Mathf.Clamp01(h/.32f),3);Draw(FireStyleModule.Burst,h/.6f,center,new Vector2(100+e*470,80+e*370),White(1),-12,true,false,true);
                Draw(FireStyleModule.Burst,(h+.04f)/.5f,center+new Vector2(0,14),new Vector2(70+e*365,90+e*315),new Color(1,1,.8f,Mathf.Clamp01((.52f-h)/.20f)),25,false,false,true);
                Draw(FireStyleModule.Shockwave,h/.6f,feet+new Vector2(0,16),new Vector2(70+e*500,30+e*155),White(Mathf.Clamp01((.6f-h)/.3f)),0,false,true,true);}
            if(h<.95f)for(int i=0;i<9;i++){float a=i*2.4f,r=80+h*70;var p=center+new Vector2(Mathf.Cos(a)*r,Mathf.Sin(a)*r*.6f+40*h);Draw(FireStyleModule.Smoke,age+i*.17f,p,new Vector2(90+h*55,95+h*85),White(Mathf.Clamp01(h/.13f)*Mathf.Clamp01((.95f-h)/.4f)*.72f),i*30,true);}
            if(h<.70f)for(int i=0;i<9;i++){float a=(i*40+15)*Mathf.Deg2Rad;var p=center+new Vector2(Mathf.Cos(a)*Mathf.Lerp(30,160,h/.7f),Mathf.Sin(a)*100*h/.7f+35);
                Flame(p,new Vector2(92,145)*(1-h*.35f),age+i*.18f,Mathf.Clamp01((.7f-h)/.23f),i%3==0,.85f,a*Mathf.Rad2Deg-90);}
            if(h<1.25f)for(int i=0;i<style.explosionEmbers;i++){float a=i*2.39996f;bool back=i%3==0;float speed=back?110:190+(i%5)*17;float life=Mathf.Clamp01(h/(.65f+i%6*.1f));if(life>=1)continue;
                var p=center+new Vector2(Mathf.Cos(a),Mathf.Sin(a)*.65f)*(20+h*speed)+new Vector2(0,45*h-75*h*h);float size=(back?17:29+i%4*7)*(back?1-life*.45f:1+life*.7f);var color=Color.Lerp(Color.white,new Color(.42f,.14f,.14f),life*life);color.a=1-life*life;
                Draw(FireStyleModule.Ember,age+i*.12f,p,new Vector2(size*.65f,size),color,-a*Mathf.Rad2Deg,back,!back);}
            if(h>.35f&&h<1.85f){float fade=Mathf.Clamp01((h-.35f)/.18f)*Mathf.Clamp01((1.85f-h)/.6f);
                Draw(FireStyleModule.Heat,age,center+new Vector2(0,30+h*50),new Vector2(310,220),White(fade*.18f),0,true);
                Draw(FireStyleModule.Shockwave,.25f,feet,new Vector2(190,36),White(fade*.45f),0,true,true,true);
                for(int i=0;i<3;i++)Flame(feet+new Vector2((i-1)*57,20),new Vector2(45,77),age+i*.27f,fade*.8f,i==1,.6f);
                for(int i=0;i<12;i++){float u=Mathf.Repeat(age*.6f+i*.18f,1);Draw(FireStyleModule.Ember,age+i,feet+new Vector2(Mathf.Sin(i*3)*100,15+u*110),Vector2.one*(12+i%3*6),White(fade*(1-u)),i*30,i%2==0);}
            }
        }
        void Burn()
        {
            // Subordinate to attack art: short side flames keep the face and torso visible.
            Draw(FireStyleModule.Shockwave,.22f,feet,new Vector2(125,27),White(.28f),0,true,true,true);
            for(int i=0;i<3;i++)Flame(feet+new Vector2((i-1)*62,24+(i%2)*20),new Vector2(29,56),age*.75f+i*.31f,.78f,i==1,.35f);
            for(int i=0;i<8;i++){float u=Mathf.Repeat(age*.5f+i*.137f,1);Draw(FireStyleModule.Ember,age+i,feet+new Vector2(Mathf.Sin(i*2.4f+u)*72,u*108),new Vector2(10,17),White((1-u)*.7f),i*28,i%2==0);}
        }
        public void Clear(){running=false;age=20;BurnShown=false;ActiveCount=0;if(pool!=null)for(int g=0;g<4;g++){for(int i=0;i<last[g];i++)if(pool[g][i]!=null)pool[g][i].gameObject.SetActive(false);last[g]=used[g]=0;}}
        void OnDisable()=>Clear();
        void OnDestroy(){Clear();hudVisibility?.Dispose();if(roots!=null)foreach(var r in roots)if(r!=null)Destroy(r.gameObject);}
    }
}





