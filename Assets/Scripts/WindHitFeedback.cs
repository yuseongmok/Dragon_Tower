using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    // Local presentation stop only. Combat time, input and animation clocks keep running.
    public sealed class WindHitFeedback : MonoBehaviour
    {
        struct Pose
        {
            public Vector2 position;public Vector3 scale;public Quaternion rotation;
            public static Pose Read(RectTransform r)=>new Pose{position=r.anchoredPosition,scale=r.localScale,rotation=r.localRotation};
            public void Apply(RectTransform r){r.anchoredPosition=position;r.localScale=scale;r.localRotation=rotation;}
        }
        RectTransform player,enemy;Image playerImage,flash;Material material;Sprite heldSprite,normalSprite;
        Pose heldPlayer,heldEnemy,normalPlayer,normalEnemy;float age=10,hold=.06f,recoil=7,flashLength=.065f,strength=.98f,duration=.18f;bool applied,playerHeld;Vector2 recoilDirection;
        public bool Active=>age<duration;
        public bool Holding=>age<hold;
        public bool FlashVisible=>flash!=null&&flash.gameObject.activeSelf;
        public void Initialize()
        {
            var go=new GameObject("Wind hit silhouette flash",typeof(RectTransform),typeof(Image));flash=go.GetComponent<Image>();flash.raycastTarget=false;
            material=new Material(Resources.Load<Shader>("VFX/GaleImpactSilhouette"));flash.material=material;go.SetActive(false);
        }
        public void Bind(RectTransform p,RectTransform e,Graphic playerGraphic,Graphic enemyGraphic)
        {
            Clear();player=p;enemy=e;playerImage=playerGraphic as Image;
            flash.rectTransform.SetParent(enemyGraphic.transform,false);flash.rectTransform.anchorMin=Vector2.zero;flash.rectTransform.anchorMax=Vector2.one;flash.rectTransform.offsetMin=flash.rectTransform.offsetMax=Vector2.zero;
            var image=enemyGraphic as Image;flash.sprite=image!=null?image.sprite:null;flash.preserveAspect=image!=null&&image.preserveAspect;
        }
        public void Hit(bool final)
        {
            Clear();hold=final?.05f:0;recoil=final?3:2;recoilDirection=new Vector2(final?-1:1,-.25f);flashLength=final?.065f:.035f;strength=final?.98f:.65f;duration=final?.18f:.09f;heldPlayer=Pose.Read(player);heldEnemy=Pose.Read(enemy);heldSprite=playerImage!=null?playerImage.sprite:null;age=0;UpdateFlash();
        }
        void UpdateFlash(){bool on=age<flashLength;flash.gameObject.SetActive(on);if(on)flash.color=new Color(.86f,1,.96f,strength*(1-Mathf.Clamp01((age-flashLength*.65f)/(flashLength*.35f))));}
        public void Step(float delta,bool fighting,bool allowPlayerHold)
        {
            // Called after normal poses have been recomputed, so no offsets accumulate.
            applied=false;playerHeld=false;
            if(!fighting){Clear();return;}if(!Active)return;
            age+=delta;normalPlayer=Pose.Read(player);normalEnemy=Pose.Read(enemy);normalSprite=playerImage!=null?playerImage.sprite:null;
            if(Holding)
            {
                heldEnemy.Apply(enemy);
                if(allowPlayerHold){heldPlayer.Apply(player);if(playerImage!=null)playerImage.sprite=heldSprite;playerHeld=true;}
                applied=true;
            }
            else if(age<duration)
            {
                float t=(age-hold)/(duration-hold);
                // Direction follows the closing claw; no physical displacement.
                enemy.anchoredPosition+=recoilDirection*(recoil*Mathf.Sin(Mathf.PI*Mathf.Clamp01(t)));applied=true;
            }
            UpdateFlash();
        }
        public void Clear()
        {
            if(applied){if(enemy!=null)normalEnemy.Apply(enemy);if(playerHeld&&player!=null){normalPlayer.Apply(player);if(playerImage!=null)playerImage.sprite=normalSprite;}}
            applied=playerHeld=false;age=10;if(flash!=null)flash.gameObject.SetActive(false);
        }
        void OnDisable(){Clear();}
        void OnDestroy(){if(material!=null)Destroy(material);if(flash!=null)Destroy(flash.gameObject);}
    }
}

