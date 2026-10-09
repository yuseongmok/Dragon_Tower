using System;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor {
 public static class DragonNameValidation {
 const string Key="DragonTower.Test.CustomNames";
 static void Check(bool ok,string text){if(!ok)throw new Exception(text);}
 public static void Write(){try{
 PlayerPrefs.DeleteKey(Key+".A");PlayerPrefs.DeleteKey(Key+".B");var db=ContentDatabase.Load();var s=new CollectionSession(new ProfileStore(Key),db.dragons);var d=s.Hatch(0);string original=d.displayName,id=s.Profile.selectedInstanceId;
 Check(s.SelectedName()==original,"default name");Check(s.RenameDragon(id,"별빛 친구",out _),"rename");
 var second=s.RegisterHatchedDragon(d);Check(DragonNames.Display(second,d)==original,"independent instance");
 Check(!s.RenameDragon(id,"   ",out _)&&!s.RenameDragon(id,"<b>이름</b>",out _)&&!s.RenameDragon(id,"가나다라마바사아자차카타파",out _)&&!s.RenameDragon(id,"이름\n줄",out _),"invalid names");
 Check(s.RenameDragon(id,"가나다라마바사아자차카타",out _),"12 characters");Check(s.RenameDragon(id,"별빛 친구",out _),"unlimited rename");
 for(int stage=0;stage<3;stage++)Check(s.SelectedName(stage)=="별빛 친구","evolution name");Check(d.displayName==original&&d.StableId==s.SelectedInstance.speciesId,"source unchanged");Check(DragonNames.Ranking(s.SelectedInstance,d)=="별빛 친구 ("+original+")","ranking formatter");
 var old=JsonUtility.FromJson<OwnedDragon>("{\"instanceId\":\"old\",\"speciesId\":\"old\"}");Check(DragonNames.Display(old,d)==original,"old missing field fallback");
 s.Select(second.instanceId);Check(s.SelectedName()==original,"selection independence");s.Select(id);Check(s.SelectedName()=="별빛 친구","selected again");Debug.Log("DRAGON_NAMES_WRITE_OK");EditorApplication.Exit(0);
 }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
 public static void Reload(){try{var s=new CollectionSession(new ProfileStore(Key),ContentDatabase.Load().dragons);Check(s.SelectedName()=="별빛 친구"&&s.SelectedName(2)=="별빛 친구","new process load");Debug.Log("DRAGON_NAMES_RESTART_OK");EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
 }
}
