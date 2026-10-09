using System;
using System.Globalization;
using System.Text;
namespace DragonTower {
 public static class DragonNames {
  public const int MaxLength=12;
  public static bool Validate(string input,out string name,out string error){
   name=(input??"").Trim(' ');error=null;
   try{name=name.Normalize(NormalizationForm.FormC);}catch(ArgumentException){error="사용할 수 없는 문자가 있습니다.";return false;}
   if(string.IsNullOrWhiteSpace(name)){error="이름을 입력해주세요.";return false;}
   if(new StringInfo(name).LengthInTextElements>MaxLength){error="이름은 최대 12자까지 입력할 수 있습니다.";return false;}
   foreach(char c in name)if(!char.IsLetterOrDigit(c)&&c!=' '&&c!='-'&&c!='_'){error="한글·영문 등 글자, 숫자, 공백, -와 _만 사용할 수 있습니다.";return false;}
   return true;
  }
  public static string Display(OwnedDragon owned,DragonData data,int stage=0)=>owned!=null&&Validate(owned.customName,out var name,out _)?name:data.NameForStage(stage);
  public static string Ranking(OwnedDragon owned,DragonData data)=>owned!=null&&Validate(owned.customName,out var name,out _)?name+" ("+data.displayName+")":data.displayName;
 }
 public sealed partial class CollectionSession {
  public OwnedDragon SelectedInstance=>Profile.dragons.Find(d=>d.instanceId==Profile.selectedInstanceId);
  public string SelectedName(int stage=0)=>Selected==null?"":DragonNames.Display(SelectedInstance,Selected,stage);
  public bool RenameDragon(string instanceId,string input,out string error){
   if(!DragonNames.Validate(input,out var name,out error))return false;
   var next=ProfileStore.Clone(Profile);var owned=next.dragons.Find(d=>d.instanceId==instanceId);
   if(owned==null){error="보유한 드래곤을 찾을 수 없습니다.";return false;}
   owned.customName=name;store.Save(next);Profile=next;return true;
  }
  public void ResetDragonName(string instanceId){var next=ProfileStore.Clone(Profile);var owned=next.dragons.Find(d=>d.instanceId==instanceId);if(owned==null)throw new ArgumentException("Unknown owned dragon");owned.customName=null;store.Save(next);Profile=next;}
 }
}

