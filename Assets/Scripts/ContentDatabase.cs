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
        public static ContentDatabase Load()=>Resources.Load<ContentDatabase>("ContentDatabase");
        public MonsterData PickMonster(int floor,bool boss,int roll)
        {
            var candidates=new List<MonsterData>();int total=0;
            foreach(var monster in monsters??Array.Empty<MonsterData>())
            {
                if(monster==null||monster.boss!=boss||floor<monster.minimumFloor||floor>monster.maximumFloor||monster.spawnWeight<=0)continue;
                candidates.Add(monster);total+=monster.spawnWeight;
            }
            if(candidates.Count==0)return null;
            int point=Math.Abs(roll==int.MinValue?0:roll)%total;
            foreach(var monster in candidates){if(point<monster.spawnWeight)return monster;point-=monster.spawnWeight;}
            return candidates[candidates.Count-1];
        }
        public ItemData[] PickItems(int count,int seed)
        {
            var pool=new List<ItemData>();foreach(var value in items??Array.Empty<ItemData>())if(value!=null)pool.Add(value);
            var result=new List<ItemData>();var random=new System.Random(seed);
            while(result.Count<count&&pool.Count>0)
            {
                var picked=PickWeightedItem(pool,random);if(picked==null)break;
                result.Add(picked);pool.Remove(picked);
            }
            return result.ToArray();
        }
        public static int ItemGradeWeight(ItemGrade grade)
        {switch(grade){case ItemGrade.Rare:return 30;case ItemGrade.Epic:return 12;case ItemGrade.Unique:return 3;default:return 55;}}
        public static ItemData PickWeightedItem(IReadOnlyList<ItemData> candidates,System.Random random)
        {
            if(candidates==null||candidates.Count==0)return null;if(random==null)throw new ArgumentNullException(nameof(random));
            var grades=new List<ItemGrade>();foreach(var value in candidates)if(value!=null&&!grades.Contains(value.grade))grades.Add(value.grade);
            int total=0;foreach(var grade in grades)total+=ItemGradeWeight(grade);
            if(total<=0)return null;int point=random.Next(total);ItemGrade selected=grades[0];
            foreach(var grade in grades){int weight=ItemGradeWeight(grade);if(point<weight){selected=grade;break;}point-=weight;}
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
                result.Add(picked);pool.Remove(picked);
            }
            return result.ToArray();
        }
        public static int AugmentGradeWeight(AugmentGrade grade)
        {
            switch(grade){case AugmentGrade.Rare:return 28;case AugmentGrade.Epic:return 10;case AugmentGrade.Unique:return 2;default:return 60;}
        }
        public static AugmentData PickWeightedAugment(IReadOnlyList<AugmentData> candidates,System.Random random)
        {
            if(candidates==null||candidates.Count==0)return null;if(random==null)throw new ArgumentNullException(nameof(random));
            var grades=new List<AugmentGrade>();
            foreach(var value in candidates)if(value!=null&&!grades.Contains(value.grade))grades.Add(value.grade);
            int total=0;foreach(var grade in grades)total+=AugmentGradeWeight(grade);
            if(total<=0)return null;int point=random.Next(total);AugmentGrade selected=grades[0];
            foreach(var grade in grades){int weight=AugmentGradeWeight(grade);if(point<weight){selected=grade;break;}point-=weight;}
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
