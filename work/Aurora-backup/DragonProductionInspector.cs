using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public static class DragonProductionChecks
    {
        public static string ResourcePath(UnityEngine.Object asset)
        {
            string path=AssetDatabase.GetAssetPath(asset);int start=path.IndexOf("/Resources/",StringComparison.Ordinal);
            return start<0?null:path.Substring(start+11,path.Length-start-11-System.IO.Path.GetExtension(path).Length);
        }
        public static List<string> Validate(DragonData dragon)
        {
            var errors=new List<string>();if(dragon==null){errors.Add("Dragon Data 없음");return errors;}
            if(string.IsNullOrWhiteSpace(dragon.speciesId))errors.Add("고유 Dragon ID 없음");
            foreach(string guid in AssetDatabase.FindAssets("t:DragonData"))
            {var other=AssetDatabase.LoadAssetAtPath<DragonData>(AssetDatabase.GUIDToAssetPath(guid));if(other!=dragon&&other.StableId==dragon.StableId)errors.Add("중복 Dragon ID: "+dragon.StableId);}
            if(dragon.elementType==ElementType.Neutral)errors.Add("Element 미지정(Neutral): 제작용 속성을 지정하세요.");
            if(dragon.battleSprite==null)errors.Add("Battle Sprite 없음");
            if(dragon.skill==null)errors.Add("기본 Skill 없음");
            else if(!dragon.skill.CanEquip(dragon.StableId)||!dragon.CanLearnSkill(dragon.skill.elementType))errors.Add("기본 Skill 소유자/속성 불일치");
            CheckSet(dragon.animationSetPath,"기본",errors);
            if(dragon.elementSkillPool==null)errors.Add("Element Skill Pool 없음");
            else if(dragon.elementSkillPool.element!=dragon.elementType)errors.Add("기본 Skill Pool 속성 불일치");
            var pools=new[]{dragon.elementSkillPool}.Concat(dragon.additionalSkillPools??Array.Empty<ElementSkillPool>());
            foreach(var pool in pools.Where(p=>p!=null))
            {
                if(!dragon.CanLearnSkill(pool.element))errors.Add(pool.name+": 허용되지 않은 추가 속성 Pool");
                if(pool.skills==null||pool.skills.Length==0)errors.Add(pool.name+": Skill Pool 비어 있음");
                else {var seen=new HashSet<SkillData>();foreach(var skill in pool.skills)if(!pool.Contains(skill)||!seen.Add(skill))errors.Add(pool.name+": 비어 있거나 중복/전용/Legendary/다른 속성 Skill");}
            }
            var signature=dragon.signatureSkill;
            if(signature==null)errors.Add("Signature Skill 없음(완성 전 연결 필요)");
            else if(signature.rarity!=ContentRarity.Legendary||signature.exclusiveDragonId!=dragon.StableId||!dragon.CanLearnSkill(signature.elementType))errors.Add("Signature 등급/전용 소유자/속성 불일치");
            bool theme=AssetDatabase.FindAssets("t:ElementHudTheme").Select(g=>AssetDatabase.LoadAssetAtPath<ElementHudTheme>(AssetDatabase.GUIDToAssetPath(g))).Any(t=>t.element==dragon.elementType&&ResourcePath(t)?.StartsWith("HudThemes/")==true);
            if(!theme)errors.Add("Resources/HudThemes에 해당 Element Theme 없음");
            var stages=new HashSet<int>();foreach(var form in dragon.evolutionForms??Array.Empty<DragonEvolutionForm>())
            {
                if(form==null||form.stage<1||form.stage>2||!stages.Add(form.stage)){errors.Add("진화 단계는 중복 없이 1 또는 2");continue;}
                if(form.battleSprite==null)errors.Add("진화 "+form.stage+": Battle Sprite 없음");
                CheckSet(form.animationSetPath,"진화 "+form.stage,errors);
                if(form.defaultSkill!=null&&(!form.defaultSkill.CanEquip(dragon.StableId)||!dragon.CanLearnSkill(form.defaultSkill.elementType)))errors.Add("진화 기본 Skill 소유자/속성 불일치");
            }
            return errors;
        }
        static void CheckSet(string path,string label,List<string> errors)
        {
            var set=string.IsNullOrWhiteSpace(path)?null:Resources.Load<DragonAnimationSet>(path);
            if(set==null){errors.Add(label+": Animation Set 없음/경로 오류");return;}
            foreach(DragonAnimationState state in Enum.GetValues(typeof(DragonAnimationState)))
            {
                var frames=set.Get(state);
                if(frames==null||frames.frames==null||frames.frames.Length==0||frames.frames.Any(s=>s==null)){errors.Add(label+" "+state+": 프레임 없음");continue;}
                if(frames.displayScale<=0||frames.durations==null||frames.durations.Length!=frames.frames.Length||frames.durations.Any(t=>t<=0))errors.Add(label+" "+state+": 배율/프레임 시간 확인 필요");
                if(frames.offsets!=null&&frames.offsets.Length!=0&&frames.offsets.Length!=frames.frames.Length)errors.Add(label+" "+state+": Offset 개수 불일치");
            }
        }
        public static void Register(DragonData dragon)
        {
            var issues=Validate(dragon);if(issues.Count>0)throw new InvalidOperationException(string.Join("\n",issues));
            var db=AssetDatabase.LoadAssetAtPath<ContentDatabase>("Assets/Resources/ContentDatabase.asset");if(db==null)throw new InvalidOperationException("ContentDatabase 없음");
            Undo.RecordObject(db,"Register Dragon");
            db.dragons=(db.dragons??Array.Empty<DragonData>()).Concat(new[]{dragon}).Distinct().ToArray();
            var skills=new List<SkillData>{dragon.skill,dragon.signatureSkill};
            foreach(var pool in new[]{dragon.elementSkillPool}.Concat(dragon.additionalSkillPools??Array.Empty<ElementSkillPool>()).Where(p=>p!=null))skills.AddRange(pool.skills);
            foreach(var form in dragon.evolutionForms??Array.Empty<DragonEvolutionForm>())if(form?.defaultSkill!=null)skills.Add(form.defaultSkill);
            db.skills=(db.skills??Array.Empty<SkillData>()).Concat(skills.Where(s=>s!=null)).Distinct().ToArray();
            EditorUtility.SetDirty(db);AssetDatabase.SaveAssets();
        }
    }
    [CustomEditor(typeof(DragonData))]
    public sealed class DragonProductionInspector : UnityEditor.Editor
    {
        List<string> issues;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();var dragon=(DragonData)target;
            EditorGUILayout.Space();EditorGUILayout.LabelField("Production Tools",EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("기존 6상태 재생기와 속성 HUD를 공유합니다. Animation Set은 Resources 폴더 안에 저장하세요. 진화 배율 0은 기존 공용 배율을 유지합니다.",MessageType.Info);
            DrawSetPicker(dragon,0,dragon.animationSetPath);
            foreach(var form in dragon.evolutionForms??Array.Empty<DragonEvolutionForm>())if(form!=null)DrawSetPicker(dragon,form.stage,form.animationSetPath);
            if(GUILayout.Button("설정 검사"))issues=DragonProductionChecks.Validate(dragon);
            if(issues!=null){if(issues.Count==0)EditorGUILayout.HelpBox("제작 설정 정상",MessageType.Info);foreach(string issue in issues)EditorGUILayout.HelpBox(issue,MessageType.Warning);}
            if(GUILayout.Button("검사 후 게임 목록에 등록 / 업데이트"))
            {issues=DragonProductionChecks.Validate(dragon);if(issues.Count==0){DragonProductionChecks.Register(dragon);Debug.Log("Dragon production registered: "+dragon.StableId);}}
        }
        void DrawSetPicker(DragonData dragon,int stage,string path)
        {
            var current=string.IsNullOrWhiteSpace(path)?null:Resources.Load<DragonAnimationSet>(path);
            var selected=(DragonAnimationSet)EditorGUILayout.ObjectField(stage==0?"기본 Animation Set":"진화 "+stage+" Animation Set",current,typeof(DragonAnimationSet),false);
            if(selected==current)return;string resource=selected==null?null:DragonProductionChecks.ResourcePath(selected);
            if(selected!=null&&resource==null){Debug.LogWarning("Animation Set을 Resources 하위에 저장하세요.");return;}
            Undo.RecordObject(dragon,"Assign Dragon Animation Set");if(stage==0)dragon.animationSetPath=resource;else dragon.Form(stage).animationSetPath=resource;EditorUtility.SetDirty(dragon);
        }
        [MenuItem("Dragon Tower/Production/Create Dragon Template")]
        static void CreateTemplate(){ProjectWindowUtil.CreateAsset(ScriptableObject.CreateInstance<DragonData>(),"NewDragon.asset");}
    }
}
