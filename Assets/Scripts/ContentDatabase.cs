using System;
using System.Collections.Generic;
using UnityEngine;
namespace DragonTower
{
    [CreateAssetMenu(menuName="Dragon Tower/Content Database")]
    public sealed class ContentDatabase : ScriptableObject
    {
        public DragonData[] dragons=Array.Empty<DragonData>();
        public MonsterData[] monsters=Array.Empty<MonsterData>();
        public SkillData[] skills=Array.Empty<SkillData>();
        public ItemData[] items=Array.Empty<ItemData>();
        public AugmentData[] augments=Array.Empty<AugmentData>();
        [NonSerialized] bool enemyResourcesRegistered;
        public static ContentDatabase Load(){
            var db=Resources.Load<ContentDatabase>("ContentDatabase");
            if(db!=null&&!db.enemyResourcesRegistered){
                var registered=new List<MonsterData>(db.monsters??Array.Empty<MonsterData>());
                foreach(var monster in Resources.LoadAll<MonsterData>("Enemies"))
                    if(monster!=null&&!registered.Exists(m=>m!=null&&m.StableId==monster.StableId))registered.Add(monster);
                db.monsters=registered.ToArray();db.enemyResourcesRegistered=true;
            }
            return db;
        }
        public MonsterData PickMonster(int floor,bool boss,int roll)
        {
            bool hasFloorBoss=false;
            if(boss)foreach(var monster in monsters??Array.Empty<MonsterData>())
                if(monster!=null&&monster.boss&&monster.minimumFloor==floor&&monster.maximumFloor==floor&&monster.spawnWeight>0){hasFloorBoss=true;break;}
            var candidates=new List<MonsterData>();int total=0;
            foreach(var monster in monsters??Array.Empty<MonsterData>())
            {
                if(monster==null||monster.boss!=boss||floor<monster.minimumFloor||floor>monster.maximumFloor||monster.spawnWeight<=0)continue;
                if(hasFloorBoss&&(monster.minimumFloor!=floor||monster.maximumFloor!=floor))continue;
                candidates.Add(monster);total+=monster.spawnWeight;
            }
            if(candidates.Count==0)return null;
            int point=Math.Abs(roll==int.MinValue?0:roll)%total;
            foreach(var monster in candidates){if(point<monster.spawnWeight)return monster;point-=monster.spawnWeight;}
            return candidates[candidates.Count-1];
        }
        public static bool CanOfferItem(TowerRun run,ItemData item){if(item==null)return false;if(run==null)return true;foreach(var id in run.Items)if(id==item.StableId)return false;foreach(var slot in run.PendingInventory)if(slot.Item!=null&&slot.Item.StableId==item.StableId)return false;return true;}
        public ItemData[] PickItems(int count,int seed,TowerRun run=null)
        {
            var pool=new List<ItemData>();foreach(var value in items??Array.Empty<ItemData>())if(value!=null&&CanOfferItem(run,value)&&!pool.Exists(x=>x.StableId==value.StableId))pool.Add(value);
            var result=new List<ItemData>();var random=new System.Random(seed);
            while(result.Count<count&&pool.Count>0)
            {
                var picked=PickWeightedItem(pool,random,run?.Floor??1);if(picked==null)break;
                result.Add(picked);pool.RemoveAll(x=>x.StableId==picked.StableId);
            }
            return result.ToArray();
        }
        public ItemData[] PickShopItems(int seed,TowerRun run=null)
        {
            ItemData healing=null;var pool=new List<ItemData>();
            foreach(var item in items??Array.Empty<ItemData>())
            {
                if(item==null)continue;
                if(item.StableId=="item_healing_potion")healing=item;
                else if(CanOfferItem(run,item)&&!pool.Exists(x=>x.StableId==item.StableId))pool.Add(item);
            }
            if(healing==null)throw new InvalidOperationException("상점 고정 상품인 회복약을 찾을 수 없습니다.");
            var other=PickWeightedItem(pool,new System.Random(seed),run?.Floor??1);
            return other==null?new[]{healing}:new[]{healing,other};
        }
        public static int ItemGradeWeight(ItemGrade grade,int floor=1)
        {int zone=Math.Max(0,Math.Min(9,(floor-1)/10));switch(grade){case ItemGrade.Rare:return 30;case ItemGrade.Epic:return 12+zone*2;case ItemGrade.Unique:return 3+zone;case ItemGrade.Legendary:return 1+zone;default:return 55-zone*4;}}
        public static ItemData PickWeightedItem(IReadOnlyList<ItemData> candidates,System.Random random,int floor=1)
        {
            if(candidates==null||candidates.Count==0)return null;if(random==null)throw new ArgumentNullException(nameof(random));
            var grades=new List<ItemGrade>();foreach(var value in candidates)if(value!=null&&!grades.Contains(value.grade))grades.Add(value.grade);
            int total=0;foreach(var grade in grades)total+=ItemGradeWeight(grade,floor);
            if(total<=0)return null;int point=random.Next(total);ItemGrade selected=grades[0];
            foreach(var grade in grades){int weight=ItemGradeWeight(grade,floor);if(point<weight){selected=grade;break;}point-=weight;}
            var matching=new List<ItemData>();foreach(var value in candidates)if(value!=null&&value.grade==selected)matching.Add(value);
            return matching.Count==0?null:matching[random.Next(matching.Count)];
        }
        public AugmentData[] PickAugments(int count,int seed)
        {
            var pool=new List<AugmentData>();foreach(var value in augments??Array.Empty<AugmentData>())if(value!=null)pool.Add(value);
            var result=new List<AugmentData>();var random=new System.Random(seed);
            while(result.Count<count&&pool.Count>0)
            {
                var picked=PickWeightedAugment(pool,random);if(picked==null)break;
                result.Add(picked);pool.RemoveAll(x=>x.StableId==picked.StableId);
            }
            return result.ToArray();
        }
        public static int AugmentGradeWeight(AugmentGrade grade,int floor=1)
        {
            int zone=Math.Max(0,Math.Min(9,(floor-1)/10));switch(grade){case AugmentGrade.Rare:return 30;case AugmentGrade.Epic:return 15+zone*2;case AugmentGrade.Unique:return 4+zone;case AugmentGrade.Legendary:return 1+zone;default:return 50-zone*4;}
        }
        public static AugmentData PickWeightedAugment(IReadOnlyList<AugmentData> candidates,System.Random random,int floor=1)
        {
            if(candidates==null||candidates.Count==0)return null;if(random==null)throw new ArgumentNullException(nameof(random));
            var grades=new List<AugmentGrade>();
            foreach(var value in candidates)if(value!=null&&!grades.Contains(value.grade))grades.Add(value.grade);
            int total=0;foreach(var grade in grades)total+=AugmentGradeWeight(grade,floor);
            if(total<=0)return null;int point=random.Next(total);AugmentGrade selected=grades[0];
            foreach(var grade in grades){int weight=AugmentGradeWeight(grade,floor);if(point<weight){selected=grade;break;}point-=weight;}
            var matching=new List<AugmentData>();foreach(var value in candidates)if(value!=null&&value.grade==selected)matching.Add(value);
            return matching.Count==0?null:matching[random.Next(matching.Count)];
        }
        static T[] PickUnique<T>(T[] source,int count,int seed) where T:UnityEngine.Object
        {
            var pool=new List<T>();foreach(var value in source??Array.Empty<T>())if(value!=null)pool.Add(value);
            var result=new List<T>();var random=new System.Random(seed);
            while(result.Count<count&&pool.Count>0){int index=random.Next(pool.Count);result.Add(pool[index]);pool.RemoveAt(index);}
            return result.ToArray();
        }
    }
}
