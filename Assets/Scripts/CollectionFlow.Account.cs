using System;using UnityEngine;using UnityEngine.UI;
namespace DragonTower {
 public sealed partial class CollectionFlow {
  Text accountFeedback;IAccountService account;ITrainerSaveService trainerSaves;TrainerCache trainerCache;AccountResponse identity;
  string accountNotice="",localProfilePrefix="DragonTower.Profile.v1",activeOwner="offline";bool accountRestored;TrainerProfile trainer=>trainerCache?.profile;
  void EnsureAccounts(){if(account!=null)return;
#if UNITY_EDITOR
   var test=Environment.GetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE");if(!string.IsNullOrEmpty(test))localProfilePrefix=test;
#endif
   var bridge=new GameObject("DragonTowerAccountBridge");bridge.transform.SetParent(transform,false);account=bridge.AddComponent<WebAccountService>();trainerSaves=new LocalTrainerSave(localProfilePrefix);
  }
  string AccountOwner(AccountResponse r)=>r.scope+":"+r.uid;
  string AccountStore(string owner)=>localProfilePrefix+".Account."+LocalTrainerSave.Key(owner);
  string AccountStatus=>identity!=null&&!string.IsNullOrEmpty(identity.uid)?(identity.anonymous?"Firebase 게스트":"아이디 계정"):("로컬 플레이 · 클라우드 미연결");
  string ErrorText(string code){switch(code){case "delete-auth-incomplete":case "account-deletion-pending":return "계정 삭제가 진행 중입니다. 삭제를 다시 눌러 마무리해주세요.";case "auth/requires-recent-login":return "다시 로그인한 후 삭제해주세요.";case "invalid-login-id":return "아이디는 영문으로 시작하는 영문·숫자·_ 4~20자입니다.";case "recovery-not-configured":return "복구 메일 서버 설정 전입니다. 아이디 가입과 로그인은 사용할 수 있습니다.";case "functions/resource-exhausted":return "요청이 많습니다. 잠시 후 다시 시도해주세요.";case "functions/invalid-argument":return "이메일 또는 인증번호를 확인해주세요.";case "not-configured":return "Firebase 연결 설정이 아직 없습니다. 로컬 플레이는 가능합니다.";case "editor-local":return "에디터에서는 로컬로 플레이합니다. 실제 로그인은 WebGL에서 확인합니다.";case "auth/credential-already-in-use":case "auth/email-already-in-use":return "이미 사용 중인 계정입니다. 게스트 데이터는 그대로 보존됩니다. 다른 아이디로 연결하거나 별도로 로그인하세요.";case "profile-conflict":return "다른 곳에서 프로필이 변경됐습니다. 자동으로 덮어쓰지 않았습니다.";case "auth/invalid-credential":case "auth/wrong-password":case "auth/user-not-found":return "로그인 정보를 확인해주세요.";case "auth/weak-password":return "비밀번호가 서버의 보안 조건을 만족하지 않습니다.";case "auth/invalid-email":return "이메일 주소를 확인해주세요.";case "permission-denied":return "프로필 접근이 거부되었습니다. Firebase 보안 규칙을 확인해주세요.";case "busy":return "요청을 처리 중입니다. 잠시 후 다시 시도해주세요.";case "timeout":return "응답이 지연됩니다. 로컬 저장은 보존됐습니다. 다시 연결해 저장 여부를 확인하세요.";default:return "연결하지 못했습니다 ("+code+"). 로컬 데이터는 보존됐습니다. 다시 시도해주세요.";}}
  void SendAccount(AccountRequest request,Action<AccountResponse> done){EnsureAccounts();accountNotice="연결 중…";if(accountFeedback!=null)accountFeedback.text=accountNotice;var owner=body;foreach(var button in body.GetComponentsInChildren<Button>())button.interactable=false;account.Request(request,r=>{accountNotice=r.ok?"":ErrorText(r.code);if(body!=owner)return;foreach(var button in body.GetComponentsInChildren<Button>())button.interactable=true;done(r);});}
  // The initial route intentionally stops at login. Tutorial content will be added later.
  public void ShowAccountEntry(){if(!OnlineFeatures.Enabled){BeginOffline();return;}EnsureAccounts();ShowLoginForm();if(!accountRestored){accountRestored=true;SendAccount(new AccountRequest{op="init"},r=>{if(r.ok)identity=string.IsNullOrEmpty(r.uid)?null:r;ShowLoginForm();});}}
  void AccountScreen(string title){Screen(title);var frame=body.Find("Ornate content frame");if(frame!=null){var image=frame.GetComponent<Image>();image.color=new Color(.16f,.17f,.19f,1);}notice.text="";}
  Text AccountMessage(float y=488){var t=Label(accountNotice,0,y,386,78,21,Color.white);t.supportRichText=false;accountFeedback=t;return t;}
  Button AccountButton(string text,float y,UnityEngine.Events.UnityAction action,bool primary=false){var b=Button(text,0,y,380,58,action);b.GetComponentInChildren<Text>().fontSize=24;if(primary)AncientUi.Button(b,true);return b;}
  void ShowLoginForm(){AccountScreen("로그인");Label("다시 만나 반가워요",0,179,400,44,28,Gold);
   var id=AccountInput("아이디",279,"");id.characterLimit=20;var pw=AccountInput("비밀번호",384,"",true);var feedback=AccountMessage();
   AccountButton("로그인",575,()=>{string secret=pw.text;pw.text="";SignInExisting(id.text.Trim(),secret);},true);
   AccountButton("계정 만들기",646,ShowCreateAccount);
   if(identity!=null&&!string.IsNullOrEmpty(identity.uid))AccountButton(identity.anonymous?"이전 게스트로 계속하기":"저장된 계정으로 계속하기",717,()=>Connected(identity));
   else AccountButton("게스트로 체험하기",717,()=>SendAccount(new AccountRequest{op="guest"},r=>{if(r.ok)Connected(r);else ShowLoginForm();}));
   Button("비밀번호 찾기",0,769,240,36,ShowPasswordHelp);
  }
  void ShowCreateAccount(){AccountScreen("계정 만들기");Label("나만의 모험을 시작하세요",0,170,400,40,27,Gold);
   var id=AccountInput("아이디 · 영문으로 시작, 4~20자",268,"");id.characterLimit=20;
   var pw=AccountInput("비밀번호 · 8자 이상",371,"",true);var confirm=AccountInput("비밀번호 확인",474,"",true);var feedback=AccountMessage(555);
   AccountButton("계정 만들기",650,()=>{string name=id.text.Trim();if(!System.Text.RegularExpressions.Regex.IsMatch(name,"^[A-Za-z][A-Za-z0-9_]{3,19}$")){feedback.text="아이디는 영문·숫자·_로 4~20자 입력해주세요.";return;}if(pw.text.Length<8){feedback.text="비밀번호는 8자 이상 입력해주세요.";return;}if(pw.text!=confirm.text){feedback.text="두 비밀번호가 일치하지 않습니다.";return;}string secret=pw.text;pw.text=confirm.text="";
    Action register=()=>SendAccount(new AccountRequest{op="link",uid=identity?.anonymous==true?identity.uid:null,loginId=name,password=secret},r=>{secret=null;if(r.ok)Connected(r);else ShowCreateAccount();});
    if(identity!=null&&!identity.anonymous){SendAccount(new AccountRequest{op="logout"},r=>{if(r.ok){identity=null;register();}else ShowCreateAccount();});}else register();
   },true);AccountButton("로그인으로 돌아가기",724,ShowLoginForm);notice.text="이메일 없이 가입 · 아이디는 대소문자를 구분하지 않습니다.";
  }
  void ShowPasswordHelp(){AccountScreen("비밀번호 찾기");Label("등록한 복구 이메일로\n비밀번호 변경 안내를 보내드립니다.",0,240,394,90,23,Color.white);var id=AccountInput("아이디",405,"");AccountMessage(510);AccountButton("안내 메일 받기",620,()=>SendAccount(new AccountRequest{op="reset",loginId=id.text.Trim()},r=>{if(r.ok)accountNotice="복구 이메일이 등록되어 있으면 안내 메일을 보냅니다.";ShowPasswordHelp();}),true);AccountButton("로그인으로 돌아가기",710,ShowLoginForm);}
  void BindAccountSession(string owner){if(activeOwner==owner)return;CheckpointBattle();controller.EndBattle();towerRun=null;Session=new CollectionSession(new ProfileStore(owner=="offline"?localProfilePrefix:AccountStore(owner)),catalog);Session.AchievementUnlocked+=achievementToast.Show;activeOwner=owner;}
  void UseOfflineTrainer(){EnsureAccounts();try{identity=null;BindAccountSession("offline");trainerCache=trainerSaves.Load("offline");if(trainer==null)ShowTrainerCreator();else{Session.SetTrainerName(trainer.displayName);ShowLobby();}}catch(Exception e){accountNotice=e.Message;ShowAccountEntry();}}
  void Connected(AccountResponse r){identity=r;try{BindAccountSession(AccountOwner(r));trainerCache=trainerSaves.Load(activeOwner);}catch(Exception e){accountNotice=e.Message;ShowAccountEntry();return;}LoadTrainer(false,true);}
  void LoadTrainer(bool acceptCloud,bool enterLobby=false){
   SendAccount(new AccountRequest{op="load",uid=identity.uid},r=>{
    if(!r.ok){if(r.code=="account-deletion-pending")ShowAccountDeletion();else ShowTrainerProfile();return;}
    if(r.exists&&!TrainerProfile.Valid(r.profile)){accountNotice="새 버전이거나 읽을 수 없는 프로필입니다. 덮어쓰지 않습니다.";ShowTrainerProfile();return;}
    if(trainerCache?.dirty==true&&!acceptCloud){accountNotice="이 기기에 저장 대기 중인 닉네임이 있습니다. 재시도하거나 서버 프로필을 명시적으로 선택해주세요.";ShowTrainerProfile();return;}
    if(r.exists){trainerCache=new TrainerCache{profile=r.profile};trainerSaves.Save(activeOwner,trainerCache);Session.SetTrainerName(trainer.displayName);if(enterLobby)EnterAfterTrainerSetup();else ShowTrainerProfile();}
    else if(trainer==null)ShowTrainerCreator();else if(enterLobby)EnterAfterTrainerSetup();else ShowTrainerProfile();
   });
  }
  InputField AccountInput(string label,float y,string value,bool password=false){Label(label,0,y-43,390,32,22,Color.white);var box=Panel(label,body,0,y,390,60,AncientUi.Background,Gold);var input=box.gameObject.AddComponent<InputField>();box.raycastTarget=true;input.targetGraphic=box;input.lineType=InputField.LineType.SingleLine;var text=CardText(box.transform,"Input text","",0,27,350,48,26,Color.white,TextAnchor.MiddleLeft);text.supportRichText=false;input.textComponent=text;input.contentType=password?InputField.ContentType.Password:InputField.ContentType.Standard;input.characterLimit=password?128:254;input.text=value;return input;}
  void ShowEmailAccount(){ShowAccountOptions();}
  void ShowAccountOptions(){AccountScreen("계정 관리");Label(identity?.anonymous==true?"게스트로 플레이 중입니다":"내 계정과 저장 정보",0,202,398,54,26,Gold);AccountMessage(320);
   if(identity==null||identity.anonymous)AccountButton("게스트 기록을 계정에 연결",430,ShowCreateAccount,true);
   else AccountButton("복구 이메일 등록 (선택)",430,ShowRecoveryEmail);
   AccountButton("저장 문제 해결",502,ShowSaveOptions);AccountButton("다른 계정으로 로그인",574,ShowLoginForm);AccountButton("프로필로 돌아가기",714,ShowTrainerProfile);
  }
  void ShowSaveOptions(){AccountScreen("저장 문제 해결");Label("저장 충돌이 생겼을 때만 사용하세요.\n현재 기록은 자동으로 합쳐지지 않습니다.",0,210,390,100,22,Color.white);AccountMessage(345);
   AccountButton("내 닉네임 다시 저장",451,SaveTrainer);AccountButton("서버의 닉네임 불러오기",521,ConfirmCloudProfile);AccountButton("이 기기의 이전 기록 가져오기",591,OfferLegacyImport);AccountButton("돌아가기",720,ShowAccountOptions);
  }
  void ShowRecoveryEmail(){Screen("복구 이메일 · 선택");Label("로그인에는 이메일이 필요하지 않습니다.\n메일 인증을 완료하면 비밀번호를 찾을 수 있습니다.\n미등록 시 비밀번호 분실 복구가 어렵습니다.",0,215,420,100,18,AncientUi.Ivory);var email=AccountInput("복구 이메일",340,"");var code=AccountInput("메일로 받은 6자리 인증번호",445,"");code.characterLimit=6;Label(accountNotice,0,530,410,70,16,Muted);
   Button("인증번호 보내기",-101,614,196,48,()=>SendAccount(new AccountRequest{op="recovery-send",uid=identity.uid,email=email.text.Trim()},r=>{if(r.ok)accountNotice="인증번호를 보냈습니다. 10분 안에 입력해주세요.";ShowRecoveryEmail();}));
   Button("인증 완료",101,614,196,48,()=>SendAccount(new AccountRequest{op="recovery-confirm",uid=identity.uid,code=code.text.Trim()},r=>{if(r.ok)accountNotice="복구 이메일 인증이 완료됐습니다.";ShowRecoveryEmail();}));Button("돌아가기",0,720,380,48,ShowEmailAccount);
  }
  void SignInExisting(string loginId,string password){
   Action login=()=>SendAccount(new AccountRequest{op="login",loginId=loginId,password=password},r=>{password=null;if(r.ok)Connected(r);else ShowLoginForm();});
   if(identity==null||!identity.anonymous){login();return;}
   Screen("게스트 계정 전환 확인");Label("현재 게스트를 먼저 아이디에 연결하는 것을 권장합니다.\n다른 계정으로 로그인하면 현재 게스트 인증으로\n다시 접근하기 어려울 수 있습니다.\n기기 저장은 삭제하거나 자동 병합하지 않습니다.",0,352,420,170,20,AncientUi.Ivory);
   Button("확인 · 다른 계정으로 로그인",0,552,398,58,()=>login());Button("취소 · 게스트 유지",0,632,398,52,()=>{password=null;ShowEmailAccount();});
  }
  // Keep the existing profile schema and cache keys so published rules and old accounts remain compatible.
  // Appearance fields are legacy data only; no trainer visual or customization is instantiated.
  void ShowTrainerCreator(){EnsureAccounts();AccountScreen("닉네임 설정");var draft=trainer?.Copy()??new TrainerProfile();
   Label("모험에서 사용할 이름을 정해주세요",0,225,400,72,27,Gold);
   var input=AccountInput("플레이어 닉네임",382,draft.displayName);input.characterLimit=12;
   Label("한글·영문·숫자 1~12자\n공백과 - _도 사용할 수 있습니다",0,469,386,75,22,Color.white);
   var error=AccountMessage(553);AccountButton("닉네임 저장",653,()=>{draft.displayName=input.text.Trim();draft.selectedDragonId=Session.Selected?.StableId??"";if(!TrainerProfile.Valid(draft)){error.text="닉네임을 확인해주세요. 공백만 사용할 수 없습니다.";return;}try{trainerCache=new TrainerCache{profile=draft.Copy(),dirty=identity!=null};trainerSaves.Save(activeOwner,trainerCache);Session.SetTrainerName(draft.displayName);if(identity!=null)SaveTrainer(true);else EnterAfterTrainerSetup();}catch(Exception e){error.text=e.Message;}},true);
   AccountButton("돌아가기",729,()=>{if(trainer!=null)ShowTrainerProfile();else ShowAccountEntry();});
  }
  void EnterAfterTrainerSetup(){Session.SetTrainerName(trainer.displayName);ShowLobby();}
  void SaveTrainer(){SaveTrainer(false);}
  void SaveTrainer(bool enterLobby){if(trainer==null||identity==null)return;SendAccount(new AccountRequest{op="save",uid=identity.uid,profile=trainer.Copy(),revision=trainer.revision},r=>{if(r.ok){trainerCache=new TrainerCache{profile=r.profile};trainerSaves.Save(activeOwner,trainerCache);accountNotice="닉네임을 저장했습니다.";if(enterLobby){EnterAfterTrainerSetup();return;}}ShowTrainerProfile();});}
  public void ShowTrainerProfile(){if(!OnlineFeatures.Enabled){ShowLocalPlayer();return;}EnsureAccounts();AccountScreen("플레이어 정보");
   Label("내 닉네임",0,230,390,42,23,Color.white);Label(trainer?.displayName??"아직 설정하지 않았습니다",0,294,390,66,32,Gold);
   AccountMessage(443);AccountButton("닉네임 변경",567,ShowTrainerCreator);AccountButton("계정 관리",642,ShowAccountOptions);AccountButton("로비로 돌아가기",717,()=>{if(trainer==null)ShowTrainerCreator();else ShowLobby();},true);
  }
  void ConfirmCloudProfile(){Screen("프로필 선택 확인");Label("서버 프로필을 선택하면 이 기기의\n저장 대기 중인 닉네임을 대체합니다.\n드래곤과 도전 기록은 변경하지 않습니다.",0,360,420,150,22,AncientUi.Ivory);Button("서버 프로필 선택",0,545,380,58,()=>LoadTrainer(true));Button("취소 · 로컬 보존",0,624,380,50,ShowTrainerProfile);}
  void OfferLegacyImport(){Screen("기기 진행 가져오기");Label("로그인 전에 사용한 이 기기의 진행을\n현재 계정의 로컬 저장에 복사합니다.\n원본은 유지하며 서버에는 업로드하지 않습니다.\n현재 계정에 도전/드래곤이 있으면 덮어쓰지 않습니다.",0,355,420,160,20,AncientUi.Ivory);Button("기기 진행 복사",0,555,380,54,()=>{try{if(Session.Profile.hasActiveRun||Session.Profile.dragons.Count>0||Session.Profile.lastRun!=null)throw new InvalidOperationException("현재 계정에 진행이 있어 복사하지 않았습니다.");var legacy=new ProfileStore(localProfilePrefix).Load();var destination=new ProfileStore(AccountStore(activeOwner));destination.Load();destination.Save(ProfileStore.Clone(legacy));Session=new CollectionSession(new ProfileStore(AccountStore(activeOwner)),catalog);Session.AchievementUnlocked+=achievementToast.Show;if(trainer!=null)Session.SetTrainerName(trainer.displayName);accountNotice="복사 완료 · 원래 기기 저장도 유지됩니다.";}catch(Exception e){accountNotice=e.Message;}ShowTrainerProfile();});Button("취소",0,632,380,48,ShowTrainerProfile);}
  public void ShowAccountDeletion(){if(!OnlineFeatures.Enabled){ShowLocalPlayer();return;}EnsureAccounts();CheckpointBattle();controller.EndBattle();towerRun=null;AccountScreen("계정 삭제");
   if(identity==null||string.IsNullOrEmpty(identity.uid)){Label("로그인한 계정이 없습니다.\n로그인 후 계정을 삭제할 수 있습니다.",0,320,390,120,25,Color.white);AccountButton("로그인 화면",620,ShowAccountEntry);return;}
   Label("이 계정을 영구 삭제합니다",0,190,396,50,27,new Color(1,.65f,.53f));
   Label("닉네임과 로그인 계정, 복구 이메일,\n이 기기에 저장된 해당 계정의 드래곤·도전 기록이\n삭제됩니다. 삭제 후에는 복구할 수 없습니다.",0,283,390,112,21,Color.white);
   InputField password=null;if(!identity.anonymous)password=AccountInput("현재 비밀번호",420,"",true);
   var confirm=AccountInput("확인을 위해 ‘삭제’를 입력해주세요",identity.anonymous?440:527,"");confirm.characterLimit=2;var feedback=AccountMessage(604);
   AccountButton("계정 영구 삭제",686,()=>{if(confirm.text!="삭제"){feedback.text="확인란에 ‘삭제’를 입력해주세요.";return;}if(password!=null&&password.text.Length==0){feedback.text="현재 비밀번호를 입력해주세요.";return;}string owner=AccountOwner(identity),secret=password?.text;if(password!=null)password.text="";
    SendAccount(new AccountRequest{op="delete-account",uid=identity.uid,password=secret,code="삭제"},r=>{if(!r.ok||!r.deleted){if(r.code=="permission-denied")accountNotice="삭제 권한이 아직 없습니다. 운영자가 보안 규칙을 업데이트해야 합니다.";ShowAccountDeletion();return;}
     new LocalTrainerSave(localProfilePrefix).Delete(owner);string prefix=AccountStore(owner);PlayerPrefs.DeleteKey(prefix+".A");PlayerPrefs.DeleteKey(prefix+".B");PlayerPrefs.Save();trainerCache=null;identity=null;activeOwner="offline";Session=new CollectionSession(new ProfileStore(localProfilePrefix),catalog);Session.AchievementUnlocked+=achievementToast.Show;accountNotice="계정 삭제가 완료됐습니다.";ShowLoginForm();});
   });AccountButton("취소",756,()=>{if(trainer!=null)ShowLobby();else ShowAccountEntry();});
  }
  void AddLobbyTrainer(){EnsureAccounts();var b=Button(trainer==null?"닉네임 설정":"플레이어 · "+trainer.displayName,0,205,300,42,()=>{if(trainer==null)ShowTrainerCreator();else ShowTrainerProfile();});b.GetComponentInChildren<Text>().supportRichText=false;}
 }
}
