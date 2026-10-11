using UnityEngine;using UnityEngine.UI;using UnityEngine.Events;
namespace DragonTower {
 public sealed partial class CollectionFlow {
  void ShowPedestalRoom(string title,string prompt,TowerRoomKind kind,UnityAction action){
   Screen(title);RoomScenery("UI/treasure-chamber",EnvironmentScreen.Item);Label(prompt,0,180,420,42,24,Gold);
   var target=Rect("Pedestal reward touch",body,0,340,310,280);var hit=target.gameObject.AddComponent<Image>();hit.color=Color.clear;
   RoomButton=target.gameObject.AddComponent<Button>();RoomButton.targetGraphic=hit;RoomButton.transition=Selectable.Transition.None;RoomButton.onClick.AddListener(action);
   bool nest=kind==TowerRoomKind.Nest;
   roomIcon=Rect("Pedestal object",target,0,nest?180:140,nest?160:120,nest?180:120);
   if(nest){roomIcon.gameObject.AddComponent<CanvasRenderer>();egg=roomIcon.gameObject.AddComponent<EggGraphic>();egg.raycastTarget=false;}
   else {Graphic symbol=kind==TowerRoomKind.Gold?(Graphic)roomIcon.gameObject.AddComponent<TowerCoinEmblem>():roomIcon.gameObject.AddComponent<RecoveryCross>();symbol.raycastTarget=false;roomIcon.gameObject.AddComponent<PedestalFloat>();}
   var dust=Rect("Pedestal ambient dust",body,0,425,480,850).gameObject.AddComponent<RewardRoomParticles>();dust.raycastTarget=false;dust.ambient=true;
  }
 }
 public sealed class PedestalFloat:MonoBehaviour {
  RectTransform rect;Vector2 home;void Start(){rect=(RectTransform)transform;home=rect.anchoredPosition;}
  void Update(){rect.anchoredPosition=home+Vector2.up*Mathf.Sin(Time.unscaledTime*2.2f)*7;}
 }
 [RequireComponent(typeof(CanvasRenderer))]public sealed class RecoveryCross:MaskableGraphic {
  protected override void OnPopulateMesh(VertexHelper v){v.Clear();float s=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)/24;
   Cross(v,s,0,-2,10,3,new Color(.06f,.19f,.13f));Cross(v,s,0,0,9,3,new Color(.12f,.44f,.25f));Cross(v,s,0,1,8,2.5f,new Color(.29f,.85f,.45f));Quad(v,s,-2,4,1,4,new Color(.7f,1,.76f));Quad(v,s,-7,2,5,1,new Color(.7f,1,.76f));
  }
  static void Cross(VertexHelper v,float s,float x,float y,float a,float b,Color c){Quad(v,s,x-b,y-a,b*2,a*2,c);Quad(v,s,x-a,y-b,a*2,b*2,c);}
  static void Quad(VertexHelper v,float s,float x,float y,float w,float h,Color c){int n=v.currentVertCount;v.AddVert(new Vector3(x*s,y*s),c,Vector2.zero);v.AddVert(new Vector3(x*s,(y+h)*s),c,Vector2.zero);v.AddVert(new Vector3((x+w)*s,(y+h)*s),c,Vector2.zero);v.AddVert(new Vector3((x+w)*s,y*s),c,Vector2.zero);v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);}
 }
}
