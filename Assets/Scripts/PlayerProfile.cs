using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
namespace DragonTower
{
    [Serializable] public sealed class OwnedDragon
    {
        public string instanceId;
        public string speciesId;
    }
    [Serializable] public sealed class PlayerProfile
    {
        public int version=1;
        public bool starterGiftClaimed;
        public int eggs;
        public List<OwnedDragon> dragons=new List<OwnedDragon>();
        public string selectedInstanceId;
    }
    // The two slots preserve the preceding valid save if one write is damaged.
    public sealed class ProfileStore
    {
        [Serializable] sealed class Envelope { public int version=1; public long sequence; public string payload; public string checksum; }
        readonly string prefix;
        long sequence;
        public bool RecoveredBackup { get; private set; }
        public ProfileStore(string prefix="DragonTower.Profile.v1") { this.prefix=prefix; }
        static string Hash(string text)
        {
            using(var sha=SHA256.Create())return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(text)));
        }
        public PlayerProfile Load()
        {
            Envelope best=null;bool invalid=false;bool exists=false;
            foreach(var suffix in new[]{".A",".B"})
            {
                if(!PlayerPrefs.HasKey(prefix+suffix))continue;exists=true;
                Envelope envelope=null;
                try { envelope=JsonUtility.FromJson<Envelope>(PlayerPrefs.GetString(prefix+suffix)); }
                catch { invalid=true; }
                if(envelope!=null&&envelope.version>1)throw new InvalidOperationException("새 버전의 저장 데이터입니다. 이전 버전으로 덮어쓰지 않습니다.");
                try
                {
                    if(envelope==null||envelope.sequence<1||envelope.checksum!=Hash(envelope.sequence+":"+envelope.payload)) {invalid=true;continue;}
                    var profile=JsonUtility.FromJson<PlayerProfile>(envelope.payload);
                    if(profile!=null&&profile.version>1)throw new NotSupportedException("새 버전의 저장 데이터입니다.");
                    Validate(profile);
                    if(best==null||envelope.sequence>best.sequence)best=envelope;
                }
                catch(NotSupportedException) { throw; }
                catch { invalid=true; }
            }
            if(best!=null){sequence=best.sequence;RecoveredBackup=invalid;return JsonUtility.FromJson<PlayerProfile>(best.payload);}
            if(exists)throw new InvalidOperationException("저장 데이터를 읽지 못했습니다. 기존 데이터를 보존했습니다. 임의로 초기화하지 않습니다.");
            var fresh=new PlayerProfile{starterGiftClaimed=true,eggs=1};Save(fresh);return fresh;
        }
        public void Save(PlayerProfile profile)
        {
            Validate(profile);
            string payload=JsonUtility.ToJson(profile);long next=sequence+1;
            var envelope=new Envelope{sequence=next,payload=payload,checksum=Hash(next+":"+payload)};
            string key=prefix+(next%2==1?".A":".B");
            bool existed=PlayerPrefs.HasKey(key);string previous=PlayerPrefs.GetString(key,"");
            try
            {
                PlayerPrefs.SetString(key,JsonUtility.ToJson(envelope));
                PlayerPrefs.Save();sequence=next;
            }
            catch
            {
                // Avoid accidentally committing the failed candidate on a later Save/quit.
                if(existed)PlayerPrefs.SetString(key,previous);else PlayerPrefs.DeleteKey(key);
                throw;
            }
        }
        public static PlayerProfile Clone(PlayerProfile profile) => JsonUtility.FromJson<PlayerProfile>(JsonUtility.ToJson(profile));
        static void Validate(PlayerProfile p)
        {
            if(p==null||p.version!=1||!p.starterGiftClaimed||p.eggs<0||p.dragons==null)throw new InvalidOperationException("Invalid profile");
            if(p.eggs==0&&p.dragons.Count==0)throw new InvalidOperationException("Empty starter profile");
            var ids=new HashSet<string>();bool selected=string.IsNullOrEmpty(p.selectedInstanceId)&&p.dragons.Count==0;
            foreach(var d in p.dragons)
            {
                if(d==null||string.IsNullOrEmpty(d.instanceId)||string.IsNullOrEmpty(d.speciesId)||!ids.Add(d.instanceId))throw new InvalidOperationException("Invalid dragon");
                if(d.instanceId==p.selectedInstanceId)selected=true;
            }
            if(!selected)throw new InvalidOperationException("Invalid selection");
        }
    }
    public sealed class CollectionSession
    {
        readonly ProfileStore store;
        readonly DragonData[] catalog;
        public PlayerProfile Profile { get; private set; }
        public bool RecoveredBackup => store.RecoveredBackup;
        public CollectionSession(ProfileStore store,DragonData[] catalog)
        {
            this.store=store;this.catalog=catalog;
            if(catalog==null||catalog.Length==0)throw new InvalidOperationException("드래곤 데이터가 없습니다.");
            var ids=new HashSet<string>();foreach(var d in catalog)
                if(d==null||d.skill==null||!ids.Add(d.StableId))throw new InvalidOperationException("드래곤 데이터 또는 식별자를 확인하세요.");
            Profile=store.Load();
            foreach(var d in Profile.dragons)if(Find(d.speciesId)==null)throw new InvalidOperationException("보유 드래곤 데이터를 찾을 수 없습니다. 저장은 유지됩니다.");
        }
        public DragonData Find(string id){foreach(var d in catalog)if(d.StableId==id)return d;return null;}
        public DragonData Selected
        {
            get {foreach(var d in Profile.dragons)if(d.instanceId==Profile.selectedInstanceId)return Find(d.speciesId);return null;}
        }
        public bool Owns(string speciesId){foreach(var d in Profile.dragons)if(d.speciesId==speciesId)return true;return false;}
        public DragonData Hatch(int randomIndex)
        {
            if(Profile.eggs<=0)return null;
            if(randomIndex<0||randomIndex>=catalog.Length)throw new ArgumentOutOfRangeException(nameof(randomIndex));
            var next=ProfileStore.Clone(Profile);var dragon=catalog[randomIndex];
            var owned=new OwnedDragon{instanceId=Guid.NewGuid().ToString("N"),speciesId=dragon.StableId};
            next.eggs--;next.dragons.Add(owned);next.selectedInstanceId=owned.instanceId;
            store.Save(next);Profile=next;return dragon;
        }
        public bool Select(string instanceId)
        {
            if(!Profile.dragons.Exists(d=>d.instanceId==instanceId))return false;
            var next=ProfileStore.Clone(Profile);next.selectedInstanceId=instanceId;store.Save(next);Profile=next;return true;
        }
        public OwnedDragon RegisterHatchedDragon(DragonData dragon)
        {
            if(dragon==null||Find(dragon.StableId)==null)throw new ArgumentException("Unknown dragon data.");
            var next=ProfileStore.Clone(Profile);
            var owned=new OwnedDragon{instanceId=Guid.NewGuid().ToString("N"),speciesId=dragon.StableId};
            next.dragons.Add(owned);store.Save(next);Profile=next;return owned;
        }
    }
}
