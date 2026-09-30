using System;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    // Fixed pools keep status updates allocation-free apart from the short timer labels.
    public sealed class BattleStatusHud : MonoBehaviour
    {
        sealed class Badge { public RectTransform rect; public Image panel; public Text text; public BattleStatusIcon icon; public bool active; public double appeared; public int kind=-1; }
        RectTransform root; readonly Badge[] enemies=new Badge[5],players=new Badge[7];
        static readonly Color[] Colors={new Color(1,.40f,.16f),new Color(1,.85f,.20f),new Color(.35f,.87f,1),new Color(.86f,.57f,1),new Color(.55f,1,.32f),new Color(1,.35f,.46f),new Color(.90f,.65f,1),new Color(1,.65f,.45f)};
        static RectTransform Rect(Transform parent,string name,float w,float h)
        {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.sizeDelta=new Vector2(w,h);return r;}
        public void Initialize(BattleView view)
        {
            root=Rect(view.frame,"Combat status indicators",480,850);root.pivot=new Vector2(.5f,1);root.anchoredPosition=Vector2.zero;
            root.SetSiblingIndex(view.resultPanel.transform.GetSiblingIndex());
            Create(enemies,view.font);Create(players,view.font);
        }
        void Create(Badge[] badges,Font font)
        {
            for(int i=0;i<badges.Length;i++)
            {
                var b=new Badge();b.rect=Rect(root,"Status badge",106,25);b.panel=b.rect.gameObject.AddComponent<Image>();b.panel.color=new Color(.025f,.035f,.06f,.96f);b.panel.raycastTarget=false;
                DragonTowerTheme.Frame(b.panel,new Color(.5f,.6f,.7f));
                var ir=Rect(b.rect,"Status symbol",21,21);ir.anchoredPosition=new Vector2(-40,-12.5f);b.icon=ir.gameObject.AddComponent<BattleStatusIcon>();b.icon.raycastTarget=false;
                var tr=Rect(b.rect,"Status and remaining time",79,24);tr.anchoredPosition=new Vector2(12,-12.5f);b.text=tr.gameObject.AddComponent<Text>();b.text.font=font;b.text.fontSize=12;b.text.alignment=TextAnchor.MiddleCenter;b.text.raycastTarget=false;
                b.rect.gameObject.SetActive(false);badges[i]=b;
            }
        }
        public void Show(BattleModel b)
        {
            root.gameObject.SetActive(b.Result==BattleResult.Fighting);if(b.Result!=BattleResult.Fighting)return;
            int e=0,p=0;
            Add(enemies,ref e,b.EnemyBurning,0,"화상",b.EnemyBurnRemaining,b.Time,181);
            Add(enemies,ref e,b.EnemyParalyzed,1,"마비",b.EnemyParalyzeRemaining,b.Time,181);
            Add(enemies,ref e,b.EnemySlowed,2,"둔화",b.EnemySlowRemaining,b.Time,181);
            Add(enemies,ref e,b.EnemyStunned,3,"기절",b.EnemyStunRemaining,b.Time,181);
            Add(enemies,ref e,b.EnemyPoisoned,4,"중독",double.PositiveInfinity,b.Time,181);
            Add(players,ref p,b.PlayerBurning,0,"화상",b.PlayerBurnRemaining,b.Time,579);
            Add(players,ref p,b.PlayerParalyzed,1,"마비",b.PlayerParalyzeRemaining,b.Time,579);
            Add(players,ref p,b.PlayerSlowed,2,"둔화",b.PlayerSlowRemaining,b.Time,579);
            Add(players,ref p,b.PlayerStunned,3,"기절",b.StunRemaining,b.Time,579);
            Add(players,ref p,b.PlayerRending,5,"찰과상",b.PlayerRendRemaining,b.Time,579);
            Add(players,ref p,b.PlayerSkillSealed,6,"스킬봉인",b.SkillSealRemaining,b.Time,579);
            Add(players,ref p,b.PlayerAttackSealed,7,"공격봉인",b.AttackSealRemaining,b.Time,579);
            Hide(enemies,e);Hide(players,p);
        }
        static void Hide(Badge[] badges,int from){for(int i=from;i<badges.Length;i++){badges[i].active=false;badges[i].rect.gameObject.SetActive(false);}}
        static void Add(Badge[] badges,ref int count,bool enabled,int kind,string label,double remaining,double time,float y)
        {
            if(!enabled)return;int index=count++;var b=badges[index];
            if(!b.active||b.kind!=kind){b.appeared=time;b.active=true;b.kind=kind;b.rect.gameObject.SetActive(true);b.icon.Kind=kind;}
            b.rect.anchoredPosition=new Vector2(-165+(index%4)*110,-y-(index/4)*28);
            b.rect.localScale=Vector3.one*(1+.12f*Mathf.Max(0,1-(float)(time-b.appeared)*3));
            b.icon.color=Colors[kind];b.text.color=Colors[kind];b.text.text=label+" "+(double.IsInfinity(remaining)?"상시":remaining.ToString("0.0")+"s");
        }
        public static bool HasStatus(BattleModel b,bool player)=>player?(b.PlayerBurning||b.PlayerRending||b.PlayerParalyzed||b.PlayerSlowed||b.PlayerStunned||b.PlayerSkillSealed||b.PlayerAttackSealed):(b.EnemyBurning||b.EnemyParalyzed||b.EnemySlowed||b.EnemyStunned||b.EnemyPoisoned);
        public static Color ActorTint(BattleModel b,bool player)
        {
            if(player){if(b.PlayerStunned)return Colors[3];if(b.PlayerSkillSealed||b.PlayerAttackSealed)return Colors[6];if(b.PlayerParalyzed)return Colors[1];if(b.PlayerSlowed)return Colors[2];return b.PlayerBurning?Colors[0]:Colors[5];}
            if(b.EnemyStunned)return Colors[3];if(b.EnemyParalyzed)return Colors[1];if(b.EnemySlowed)return Colors[2];return b.EnemyBurning?Colors[0]:Colors[4];
        }
    }
    // Geometry icons avoid relying on emoji glyphs absent from the Korean game font.
    public sealed class BattleStatusIcon : MaskableGraphic
    {
        int kind;public int Kind{set{if(kind==value)return;kind=value;SetVerticesDirty();}}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if(kind==1){Poly(vh,new Vector2(-.05f,1),new Vector2(-.8f,-.1f),new Vector2(-.1f,-.1f));Poly(vh,new Vector2(.1f,.1f),new Vector2(.8f,.1f),new Vector2(-.15f,-1));}
            else if(kind==2){for(int i=0;i<6;i++){float a=i*Mathf.PI/3;Line(vh,Vector2.zero,new Vector2(Mathf.Cos(a),Mathf.Sin(a))*.9f,.12f);}}
            else if(kind==3){for(int i=0;i<5;i++){float a=i*Mathf.PI*2/5+Mathf.PI/2;Poly(vh,Vector2.zero,new Vector2(Mathf.Cos(a),Mathf.Sin(a)),new Vector2(Mathf.Cos(a+.6f),Mathf.Sin(a+.6f))*.4f);Poly(vh,Vector2.zero,new Vector2(Mathf.Cos(a-.6f),Mathf.Sin(a-.6f))*.4f,new Vector2(Mathf.Cos(a),Mathf.Sin(a)));}}
            else if(kind==5){for(int i=-1;i<=1;i++)Line(vh,new Vector2(i*.4f-.2f,-.8f),new Vector2(i*.4f+.2f,.8f),.16f);}
            else if(kind>=6){Quad(vh,-.7f,-.8f,.7f,.2f);Line(vh,new Vector2(-.4f,.2f),new Vector2(-.4f,.75f),.18f);Line(vh,new Vector2(-.4f,.75f),new Vector2(.4f,.75f),.18f);Line(vh,new Vector2(.4f,.75f),new Vector2(.4f,.2f),.18f);}
            else {Poly(vh,new Vector2(0,1),new Vector2(-.75f,-.25f),new Vector2(.7f,-.3f));Poly(vh,new Vector2(-.75f,-.25f),new Vector2(0,-.9f),new Vector2(.7f,-.3f));if(kind==0)Poly(vh,new Vector2(.5f,.75f),new Vector2(0,-.7f),new Vector2(.85f,-.3f));}
        }
        void Quad(VertexHelper vh,float l,float b,float r,float t){Poly(vh,new Vector2(l,b),new Vector2(l,t),new Vector2(r,t));Poly(vh,new Vector2(l,b),new Vector2(r,t),new Vector2(r,b));}
        void Line(VertexHelper vh,Vector2 a,Vector2 b,float width){var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*width*.5f;Poly(vh,a-n,a+n,b+n);Poly(vh,a-n,b+n,b-n);}
        void Poly(VertexHelper vh,params Vector2[] points){int start=vh.currentVertCount;var rect=rectTransform.rect;foreach(var p in points)vh.AddVert(new Vector3(rect.center.x+p.x*rect.width*.45f,rect.center.y+p.y*rect.height*.45f),color,Vector2.zero);vh.AddTriangle(start,start+1,start+2);}
    }
}

