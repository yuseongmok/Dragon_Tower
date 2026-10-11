using System;using System.Runtime.InteropServices;using System.Text.RegularExpressions;using UnityEngine;
namespace DragonTower {
 [Serializable] public sealed class TrainerProfile {
  public int schemaVersion=1,revision,bodyType,skinTone,hairStyle;
  public string displayName="",equippedOutfit="starter",equippedHeadAccessory="",equippedFaceAccessory="",selectedDragonId="";
  public static bool Valid(TrainerProfile p)=>p!=null&&p.schemaVersion==1&&p.revision>=0&&p.bodyType>=0&&p.bodyType<2&&p.skinTone>=0&&p.skinTone<3&&p.hairStyle>=0&&p.hairStyle<3&&ValidName(p.displayName)&&p.equippedOutfit=="starter"&&p.equippedHeadAccessory==""&&p.equippedFaceAccessory==""&&Regex.IsMatch(p.selectedDragonId??"",@"\A[A-Za-z0-9_-]{0,80}\z");
  public static bool ValidName(string value)=>value!=null&&value==value.Trim()&&Regex.IsMatch(value,@"\A[A-Za-z0-9가-힣 _-]{1,12}\z")&&Regex.IsMatch(value,"[A-Za-z0-9가-힣]");
  public TrainerProfile Copy()=>JsonUtility.FromJson<TrainerProfile>(JsonUtility.ToJson(this));
 }
 [Serializable] public sealed class AccountRequest {public string id,op,uid,email,password,loginId,code;public int revision;public TrainerProfile profile;public RunProgressRecord record;}
 [Serializable] public sealed class AccountResponse {public string id,uid,scope,code;public bool ok,anonymous,emailVerified,exists,deleted,accepted,improved;public TrainerProfile profile;public LeaderboardEntry[] entries;public LeaderboardEntry mine;public int myRank;public long readAtMs;}
 public interface IAccountService {bool Busy{get;}void Request(AccountRequest request,Action<AccountResponse> complete);}
 public interface ITrainerSaveService {TrainerCache Load(string owner);void Save(string owner,TrainerCache cache);}
 [Serializable] public sealed class TrainerCache {public TrainerProfile profile;public bool dirty;}
 public sealed class LocalTrainerSave:ITrainerSaveService {
  readonly string prefix;public LocalTrainerSave(string prefix){this.prefix=prefix;}
  public static string Key(string owner){using(var sha=System.Security.Cryptography.SHA256.Create())return BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(owner))).Replace("-","").ToLowerInvariant();}
  string KeyFor(string owner)=>prefix+".Trainer."+Key(owner);
  public void Delete(string owner){PlayerPrefs.DeleteKey(KeyFor(owner));PlayerPrefs.Save();}
  public TrainerCache Load(string owner){string s=PlayerPrefs.GetString(KeyFor(owner),"");if(s=="")return null;var c=JsonUtility.FromJson<TrainerCache>(s);if(c==null||!TrainerProfile.Valid(c.profile))throw new InvalidOperationException("플레이어 정보를 읽지 못했습니다. 기존 데이터는 보존됩니다.");return c;}
  public void Save(string owner,TrainerCache cache){if(!TrainerProfile.Valid(cache?.profile))throw new ArgumentException("플레이어 닉네임과 저장 정보를 확인해주세요.");string key=KeyFor(owner);bool exists=PlayerPrefs.HasKey(key);string old=PlayerPrefs.GetString(key,"");try{PlayerPrefs.SetString(key,JsonUtility.ToJson(cache));PlayerPrefs.Save();}catch{if(exists)PlayerPrefs.SetString(key,old);else PlayerPrefs.DeleteKey(key);throw;}}
 }
 public sealed class WebAccountService:MonoBehaviour,IAccountService {
  Action<AccountResponse> pending;string requestId;float deadline;public bool Busy=>pending!=null;
#if UNITY_WEBGL && !UNITY_EDITOR
  [DllImport("__Internal")]static extern void DT_AccountRequest(string receiver,string payload);
#endif
  public void Request(AccountRequest request,Action<AccountResponse> complete){
   if(!OnlineFeatures.Enabled){request.password=null;complete(new AccountResponse{code="online-disabled"});return;}
   if(Busy){complete(new AccountResponse{code="busy"});return;}request.id=Guid.NewGuid().ToString("N");requestId=request.id;pending=complete;deadline=Time.realtimeSinceStartup+35;
#if UNITY_WEBGL && !UNITY_EDITOR
   try{DT_AccountRequest(gameObject.name,JsonUtility.ToJson(request));}catch{Finish(new AccountResponse{code="bridge-unavailable"});}
#else
   Finish(new AccountResponse{code="editor-local"});
#endif
   request.password=null;
  }
  [UnityEngine.Scripting.Preserve] public void OnAccountResponse(string json){try{var r=JsonUtility.FromJson<AccountResponse>(json);if(r!=null&&r.id==requestId)Finish(r);}catch{Finish(new AccountResponse{code="invalid-response"});}}
  void Finish(AccountResponse r){var callback=pending;pending=null;requestId=null;callback?.Invoke(r);}
  void Update(){if(Busy&&Time.realtimeSinceStartup>=deadline)Finish(new AccountResponse{code="timeout"});}
 }
 public sealed partial class CollectionSession {
  public void SetTrainerName(string name){if(!TrainerProfile.ValidName(name))throw new ArgumentException("Invalid trainer name");var next=ProfileStore.Clone(Profile);next.playerName=name;store.Save(next);Profile=next;}
 }
}
