using UnityEngine;using UnityEngine.UI;
namespace DragonTower {
 // Small hand-authored pixel symbols. Exact action captions avoid decorating reward descriptions.
 public static class CozySymbols {
  public static void Add(Button button){
   if(button.transform.Find("Action symbol")!=null)return;
   var label=button.GetComponentsInChildren<Text>();if(label.Length!=1)return;var text=label[0];string s=text.text;string pattern=null;
   if(s=="드래곤 도감")pattern="11111011111/10001110001/10101110101/10001110001/10101110101/10001110001/11111011111";
   else if(s=="드래곤 상태")pattern="001111100/011111110/110101011/111111111/011111110/001101100/000111000";
   else if(s=="업적 · 도전 기록")pattern="111111111/101111101/101111101/011111110/000111000/000111000/001111100";
   else if(s.Contains("닉네임")||s=="이름 변경")pattern="000000011/000000111/000001110/000011100/000111000/001110000/011100000/111000000/110000000";
   else if(s=="타워 오르기"||s.StartsWith("이어하기")||s=="전투 시작")pattern="001010100/001111100/000111000/000111000/000111000/001111100/001101100/001101100/111111111";
   else if(s.Contains("돌아가기")||s=="돌아가기"||s=="로비로"||s=="취소")pattern="000100000/001100000/011111111/111111111/011111111/001100000/000100000";
   else if(s=="저장"||s=="이름 저장"||s=="확인"||s=="계속"||s=="계속하기"||s=="다음 층으로"||s=="다음 층")pattern="000000001/000000011/000000110/100001100/110011000/011110000/001100000";
   else if(s=="게임 데이터 초기화")pattern="001111100/011000110/110000011/110000000/111100000/110000011/011000110/001111100";
   if(pattern==null)return;
   var rect=(RectTransform)button.transform;if(rect.rect.width<140)return;
   var go=new GameObject("Action symbol",typeof(RectTransform),typeof(CanvasRenderer),typeof(CozyActionSymbol));var r=go.GetComponent<RectTransform>();r.SetParent(button.transform,false);r.anchorMin=r.anchorMax=new Vector2(0,.5f);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(23,0);r.sizeDelta=new Vector2(24,24);var icon=go.GetComponent<CozyActionSymbol>();icon.pattern=pattern;icon.raycastTarget=false;
   if(CozyMenuSurface.InScope(button.transform)){string asset=s=="드래곤 도감"?"book":s=="드래곤 상태"?"dragon":s=="업적 · 도전 기록"?"trophy":s.Contains("닉네임")||s=="이름 변경"?"pencil":s.Contains("타워")||s.Contains("층")||s.StartsWith("이어하기")?"tower":s.Contains("돌아가기")||s=="로비로"||s=="취소"?"arrow":null;
    if(asset!=null){icon.enabled=false;var child=new GameObject("Golden symbol",typeof(RectTransform),typeof(Image));var cr=(RectTransform)child.transform;cr.SetParent(r,false);cr.anchorMin=Vector2.zero;cr.anchorMax=Vector2.one;cr.offsetMin=cr.offsetMax=Vector2.zero;var im=child.GetComponent<Image>();im.sprite=Resources.Load<Sprite>("UI/LobbyGuide/"+asset);im.preserveAspect=true;im.raycastTarget=false;if(asset=="arrow")r.localScale=new Vector3(-1,1,1);}
   }
   text.rectTransform.sizeDelta+=new Vector2(-32,0);text.rectTransform.anchoredPosition+=new Vector2(13,0);
  }
 }
 [RequireComponent(typeof(CanvasRenderer))] public sealed class CozyActionSymbol:MaskableGraphic {
  public string pattern;
  protected override void OnPopulateMesh(VertexHelper v){v.Clear();if(string.IsNullOrEmpty(pattern))return;var rows=pattern.Split('/');int width=rows[0].Length;float px=2,x=-width*px*.5f,y=rows.Length*px*.5f;
   for(int pass=0;pass<2;pass++)for(int row=0;row<rows.Length;row++)for(int col=0;col<rows[row].Length;col++)if(rows[row][col]=='1'){float left=x+col*px,bottom=y-(row+1)*px-(pass==0?2:0);var c=pass==0?new Color(.08f,.12f,.11f,.9f):row<2?CozyUi.Ivory:CozyUi.Gold;int n=v.currentVertCount;v.AddVert(new Vector3(left,bottom),c,Vector2.zero);v.AddVert(new Vector3(left,bottom+px),c,Vector2.zero);v.AddVert(new Vector3(left+px,bottom+px),c,Vector2.zero);v.AddVert(new Vector3(left+px,bottom),c,Vector2.zero);v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);}
  }
 }
}
