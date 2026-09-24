using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace DragonTower.Editor
{
    public sealed class ContentStudio : EditorWindow
    {
        enum Tab { Dragons, Monsters, Skills, Items, Augments }
        Tab tab;string search="";Vector2 listScroll,editorScroll;UnityEngine.Object selected;UnityEditor.Editor objectEditor;
        readonly string[] labels={"드래곤","몬스터","스킬","아이템","증강"};
        [MenuItem("Dragon Tower/콘텐츠 관리")]
        public static void Open(){var window=GetWindow<ContentStudio>("콘텐츠 관리");window.minSize=new Vector2(820,520);window.Show();}
        void OnEnable(){ContentDataSetup.Install(false);RefreshSelection();}
        void OnDisable(){if(objectEditor!=null)DestroyImmediate(objectEditor);}
        void OnGUI()
        {
            EditorGUILayout.Space(6);EditorGUILayout.LabelField("DRAGON TOWER · 콘텐츠 관리",EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("새 콘텐츠는 이 창에서 만들고, 많은 숫자는 CSV로 한꺼번에 수정하세요. 고유 ID는 출시 후 바꾸지 마세요.",MessageType.Info);
            var next=(Tab)GUILayout.Toolbar((int)tab,labels);if(next!=tab){tab=next;selected=null;RefreshSelection();}
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            search=GUILayout.TextField(search,GUI.skin.FindStyle("ToolbarSearchTextField"),GUILayout.MinWidth(180));
            if(GUILayout.Button("새로 만들기",EditorStyles.toolbarButton))Create();
            using(new EditorGUI.DisabledScope(selected==null))
            {
                if(GUILayout.Button("복제",EditorStyles.toolbarButton))Duplicate();
                if(GUILayout.Button("삭제",EditorStyles.toolbarButton))Delete();
            }
            if(GUILayout.Button("CSV 내보내기",EditorStyles.toolbarButton))ExportCsv();
            if(GUILayout.Button("CSV 가져오기",EditorStyles.toolbarButton))ImportCsv();
            if(GUILayout.Button("전체 검사",EditorStyles.toolbarButton))ValidateAll(true);
            if(GUILayout.Button("DB 갱신",EditorStyles.toolbarButton)){ContentDataSetup.Rebuild();ShowNotification(new GUIContent("데이터베이스를 갱신했습니다"));}
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();DrawList();DrawEditor();EditorGUILayout.EndHorizontal();
        }
        void DrawList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(260));EditorGUILayout.LabelField(labels[(int)tab]+" 목록",EditorStyles.boldLabel);
            listScroll=EditorGUILayout.BeginScrollView(listScroll,"box");
            foreach(var asset in Assets())
            {
                string name=Display(asset);if(!string.IsNullOrWhiteSpace(search)&&name.IndexOf(search,StringComparison.OrdinalIgnoreCase)<0)continue;
                bool active=asset==selected;if(GUILayout.Toggle(active,name,"Button")&&!active)Select(asset);
            }
            EditorGUILayout.EndScrollView();EditorGUILayout.LabelField("총 "+Assets().Length+"개",EditorStyles.miniLabel);EditorGUILayout.EndVertical();
        }
        void DrawEditor()
        {
            EditorGUILayout.BeginVertical("box");if(selected==null){GUILayout.FlexibleSpace();EditorGUILayout.LabelField("왼쪽에서 데이터를 선택하거나 새로 만드세요.",EditorStyles.centeredGreyMiniLabel);GUILayout.FlexibleSpace();EditorGUILayout.EndVertical();return;}
            EditorGUILayout.BeginHorizontal();EditorGUILayout.LabelField(Display(selected),EditorStyles.boldLabel);if(GUILayout.Button("Project에서 찾기",GUILayout.Width(110))){Selection.activeObject=selected;EditorGUIUtility.PingObject(selected);}EditorGUILayout.EndHorizontal();
            editorScroll=EditorGUILayout.BeginScrollView(editorScroll);
            if(objectEditor==null||objectEditor.target!=selected){if(objectEditor!=null)DestroyImmediate(objectEditor);objectEditor=UnityEditor.Editor.CreateEditor(selected);}
            objectEditor.OnInspectorGUI();EditorGUILayout.EndScrollView();
            if(GUI.changed)EditorUtility.SetDirty(selected);
            EditorGUILayout.EndVertical();
        }
        UnityEngine.Object[] Assets()
        {
            Type type=TypeFor(tab);return AssetDatabase.FindAssets("t:"+type.Name,new[]{FolderFor(tab)})
                .Select(g=>AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(g),type)).Where(x=>x!=null).OrderBy(Display).ToArray();
        }
        static Type TypeFor(Tab value)
        {switch(value){case Tab.Dragons:return typeof(DragonData);case Tab.Monsters:return typeof(MonsterData);case Tab.Skills:return typeof(SkillData);case Tab.Items:return typeof(ItemData);default:return typeof(AugmentData);}}
        static string FolderFor(Tab value)
        {switch(value){case Tab.Monsters:return "Assets/Data/Monsters";case Tab.Items:return "Assets/Data/Items";case Tab.Augments:return "Assets/Data/Augments";default:return "Assets/Data";}}
        static string Display(UnityEngine.Object value)
        {
            if(value is DragonData d)return d.displayName+"  ["+d.StableId+"]";
            if(value is MonsterData m)return m.displayName+"  ["+m.StableId+"]";
            if(value is SkillData s)return s.displayName+"  ["+s.StableId+"]";
            if(value is ItemData i)return i.displayName+"  ["+i.StableId+"]";
            if(value is AugmentData a)return "["+GradeLabel(a.grade)+"] "+a.displayName+"  ["+a.StableId+"]";return value==null?"":value.name;
        }
        static string GradeLabel(AugmentGrade grade)
        {switch(grade){case AugmentGrade.Rare:return "레어";case AugmentGrade.Epic:return "에픽";case AugmentGrade.Unique:return "유니크";default:return "일반";}}
        void Select(UnityEngine.Object value){selected=value;if(objectEditor!=null)DestroyImmediate(objectEditor);objectEditor=null;Repaint();}
        void RefreshSelection(){var values=Assets();if(selected==null&&values.Length>0)Select(values[0]);}
        void Create()
        {
            var asset=ScriptableObject.CreateInstance(TypeFor(tab));string baseName="new_"+tab.ToString().ToLowerInvariant();
            string path=AssetDatabase.GenerateUniqueAssetPath(FolderFor(tab)+"/"+baseName+".asset");asset.name=Path.GetFileNameWithoutExtension(path);
            SetDefaultId(asset,asset.name);AssetDatabase.CreateAsset(asset,path);AssetDatabase.SaveAssets();ContentDataSetup.Rebuild();Select(asset);
        }
        void Duplicate()
        {
            string source=AssetDatabase.GetAssetPath(selected),path=AssetDatabase.GenerateUniqueAssetPath(Path.GetDirectoryName(source).Replace('\\','/')+"/"+selected.name+"_copy.asset");
            if(!AssetDatabase.CopyAsset(source,path))return;var copy=AssetDatabase.LoadAssetAtPath(path,TypeFor(tab));SetDefaultId(copy,Path.GetFileNameWithoutExtension(path));EditorUtility.SetDirty(copy);AssetDatabase.SaveAssets();ContentDataSetup.Rebuild();Select(copy);
        }
        void Delete()
        {
            if(!EditorUtility.DisplayDialog("콘텐츠 삭제",Display(selected)+"을(를) 삭제할까요? 저장 데이터가 사용하는 ID라면 삭제하지 마세요.","삭제","취소"))return;
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(selected));selected=null;ContentDataSetup.Rebuild();RefreshSelection();
        }
        static void SetDefaultId(UnityEngine.Object asset,string id)
        {
            id=id.Replace(' ','_').ToLowerInvariant();if(asset is DragonData d)d.speciesId="dragon_"+id;
            else if(asset is MonsterData m)m.contentId="monster_"+id;else if(asset is SkillData s)s.skillId="skill_"+id;
            else if(asset is ItemData i)i.contentId="item_"+id;else if(asset is AugmentData a)a.contentId="augment_"+id;
        }
        void ExportCsv()
        {
            string path=EditorUtility.SaveFilePanel("CSV 내보내기",Path.GetFullPath("Balance"),tab.ToString()+".csv","csv");if(string.IsNullOrEmpty(path))return;
            File.WriteAllText(path,ContentCsv.Export(tab.ToString(),Assets()),new UTF8Encoding(true));ShowNotification(new GUIContent("CSV를 저장했습니다"));
        }
        void ImportCsv()
        {
            string path=EditorUtility.OpenFilePanel("CSV 가져오기",Path.GetFullPath("Balance"),"csv");if(string.IsNullOrEmpty(path))return;
            try{int changed=ContentCsv.Import(tab.ToString(),path);ContentDataSetup.Rebuild();AssetDatabase.SaveAssets();selected=null;RefreshSelection();EditorUtility.DisplayDialog("CSV 가져오기",changed+"개 데이터를 추가하거나 수정했습니다.\n전체 검사도 실행합니다.","확인");ValidateAll(false);}
            catch(Exception e){EditorUtility.DisplayDialog("CSV 오류",e.Message,"확인");}
        }
        void ValidateAll(bool dialog)
        {
            string report=ContentValidation.Report();if(dialog||report!="문제 없음")EditorUtility.DisplayDialog("콘텐츠 검사",report,"확인");
        }
    }

    static class ContentCsv
    {
        public static string Export(string tab,UnityEngine.Object[] assets)
        {
            var rows=new List<string[]>();
            if(tab=="Dragons")
            {rows.Add(new[]{"id","name","displayElement","elementType","rarity","maxHP","attackDamage","skillId","intermediateName","finalName"});foreach(DragonData d in assets)rows.Add(new[]{d.StableId,d.displayName,d.element,d.elementType.ToString(),d.rarity.ToString(),d.maxHP.ToString(),d.attackDamage.ToString(),d.skill==null?"":d.skill.StableId,d.intermediateName,d.finalName});}
            else if(tab=="Monsters")
            {rows.Add(new[]{"id","name","elementType","rarity","baseHP","attackDamage","attackInterval","hpGrowthPerFloor","attackGrowthPerFloor","hpGrowthPerFloorPercent","attackGrowthPerFloorPercent","minimumFloor","maximumFloor","spawnWeight","boss"});foreach(MonsterData m in assets)rows.Add(new[]{m.StableId,m.displayName,m.elementType.ToString(),m.rarity.ToString(),m.baseHP.ToString(),m.attackDamage.ToString(),F(m.attackInterval),m.hpGrowthPerFloor.ToString(),F(m.attackGrowthPerFloor),F(m.hpGrowthPerFloorPercent),F(m.attackGrowthPerFloorPercent),m.minimumFloor.ToString(),m.maximumFloor.ToString(),m.spawnWeight.ToString(),m.boss.ToString()});}
            else if(tab=="Skills")
            {rows.Add(new[]{"id","name","elementType","damage","cooldown","hitCount","hitInterval","statusEffect","statusChancePercent","statusDuration","statusPower","effectKind","description"});foreach(SkillData s in assets)rows.Add(new[]{s.StableId,s.displayName,s.elementType.ToString(),s.damage.ToString(),F(s.cooldown),s.hitCount.ToString(),F(s.hitInterval),s.statusEffect.ToString(),F(s.statusChancePercent),F(s.statusDuration),F(s.statusPower),s.effectKind.ToString(),Clean(s.description)});}
            else if(tab=="Items")
            {rows.Add(new[]{"id","name","rarity","price","description","effects"});foreach(ItemData i in assets)rows.Add(new[]{i.StableId,i.displayName,i.rarity.ToString(),i.price.ToString(),Clean(i.description),Effects(i.effects)});}
            else
            {rows.Add(new[]{"id","name","grade","maximumStacks","description","mechanic","primaryValue","secondaryValue","chancePercent","duration","triggerCount","effects"});foreach(AugmentData a in assets)rows.Add(new[]{a.StableId,a.displayName,a.grade.ToString(),a.maximumStacks.ToString(),Clean(a.description),a.mechanic.ToString(),F(a.primaryValue),F(a.secondaryValue),F(a.chancePercent),F(a.duration),a.triggerCount.ToString(),Effects(a.effects)});}
            var builder=new StringBuilder();foreach(var row in rows)builder.AppendLine(string.Join(",",row.Select(Escape)));return builder.ToString();
        }
        public static int Import(string tab,string path)
        {
            var lines=File.ReadAllLines(path);if(lines.Length<2)return 0;var header=Parse(lines[0]);int count=0;
            for(int line=1;line<lines.Length;line++){if(string.IsNullOrWhiteSpace(lines[line]))continue;var cells=Parse(lines[line]);var row=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);for(int i=0;i<header.Count&&i<cells.Count;i++)row[header[i]]=cells[i];string id=Get(row,"id");if(string.IsNullOrWhiteSpace(id))throw new Exception((line+1)+"행의 id가 비어 있습니다.");ImportRow(tab,id,row);count++;}return count;
        }
        static void ImportRow(string tab,string id,Dictionary<string,string> row)
        {
            if(tab=="Dragons")
            {
                var d=FindOrCreate<DragonData>(id,"Assets/Data",x=>x.StableId);d.speciesId=id;d.displayName=Get(row,"name");d.element=Get(row,"displayElement");d.elementType=E<ElementType>(row,"elementType");d.rarity=E<ContentRarity>(row,"rarity");d.maxHP=I(row,"maxHP");d.attackDamage=I(row,"attackDamage");string skill=Get(row,"skillId");if(!string.IsNullOrEmpty(skill))d.skill=Find<SkillData>(skill,x=>x.StableId);string middle=Get(row,"intermediateName"),finalName=Get(row,"finalName");if(!string.IsNullOrEmpty(middle))d.intermediateName=middle;if(!string.IsNullOrEmpty(finalName))d.finalName=finalName;EditorUtility.SetDirty(d);
            }
            else if(tab=="Monsters")
            {
                var m=FindOrCreate<MonsterData>(id,"Assets/Data/Monsters",x=>x.StableId);m.contentId=id;m.displayName=Get(row,"name");m.elementType=E<ElementType>(row,"elementType");m.rarity=E<ContentRarity>(row,"rarity");m.baseHP=I(row,"baseHP");m.attackDamage=I(row,"attackDamage");m.attackInterval=R(row,"attackInterval");m.hpGrowthPerFloor=I(row,"hpGrowthPerFloor");m.attackGrowthPerFloor=R(row,"attackGrowthPerFloor");m.hpGrowthPerFloorPercent=R(row,"hpGrowthPerFloorPercent");m.attackGrowthPerFloorPercent=R(row,"attackGrowthPerFloorPercent");m.minimumFloor=I(row,"minimumFloor");m.maximumFloor=I(row,"maximumFloor");m.spawnWeight=I(row,"spawnWeight");m.boss=B(row,"boss");EditorUtility.SetDirty(m);
            }
            else if(tab=="Skills")
            {
                var s=FindOrCreate<SkillData>(id,"Assets/Data/Skills",x=>x.StableId);s.skillId=id;s.displayName=Get(row,"name");s.elementType=E<ElementType>(row,"elementType");s.damage=I(row,"damage");s.cooldown=R(row,"cooldown");
                if(row.ContainsKey("hitCount"))s.hitCount=I(row,"hitCount");if(row.ContainsKey("hitInterval"))s.hitInterval=R(row,"hitInterval");
                if(row.ContainsKey("statusEffect"))s.statusEffect=E<CombatStatusEffect>(row,"statusEffect");if(row.ContainsKey("statusChancePercent"))s.statusChancePercent=R(row,"statusChancePercent");
                if(row.ContainsKey("statusDuration"))s.statusDuration=R(row,"statusDuration");if(row.ContainsKey("statusPower"))s.statusPower=R(row,"statusPower");
                s.effectKind=E<SkillEffectKind>(row,"effectKind");s.description=Get(row,"description");EditorUtility.SetDirty(s);
            }
            else if(tab=="Items")
            {
                var i=FindOrCreate<ItemData>(id,"Assets/Data/Items",x=>x.StableId);i.contentId=id;i.displayName=Get(row,"name");i.rarity=E<ContentRarity>(row,"rarity");i.price=I(row,"price");i.description=Get(row,"description");i.effects=ParseEffects(Get(row,"effects"));EditorUtility.SetDirty(i);
            }
            else
            {
                var a=FindOrCreate<AugmentData>(id,"Assets/Data/Augments",x=>x.StableId);a.contentId=id;a.displayName=Get(row,"name");
                if(row.ContainsKey("grade"))a.grade=E<AugmentGrade>(row,"grade");
                else if(row.ContainsKey("rarity")){a.rarity=E<ContentRarity>(row,"rarity");a.grade=LegacyAugmentGrade(a.rarity);}
                a.maximumStacks=I(row,"maximumStacks");a.description=Get(row,"description");a.mechanic=E<AugmentMechanic>(row,"mechanic");a.primaryValue=R(row,"primaryValue");a.secondaryValue=R(row,"secondaryValue");a.chancePercent=R(row,"chancePercent");a.duration=R(row,"duration");a.triggerCount=I(row,"triggerCount");a.effects=ParseEffects(Get(row,"effects"));EditorUtility.SetDirty(a);
            }
        }
        static AugmentGrade LegacyAugmentGrade(ContentRarity rarity)
        {switch(rarity){case ContentRarity.Legendary:return AugmentGrade.Unique;case ContentRarity.Epic:return AugmentGrade.Epic;case ContentRarity.Uncommon:case ContentRarity.Rare:return AugmentGrade.Rare;default:return AugmentGrade.Common;}}
        static T FindOrCreate<T>(string id,string folder,Func<T,string> getId) where T:ScriptableObject
        {
            var found=Find(id,getId);if(found!=null)return found;var data=ScriptableObject.CreateInstance<T>();
            string safe=new string(id.Select(c=>char.IsLetterOrDigit(c)||c=='_'||c=='-'?c:'_').ToArray());
            AssetDatabase.CreateAsset(data,AssetDatabase.GenerateUniqueAssetPath(folder+"/"+safe+".asset"));return data;
        }
        static T Find<T>(string id,Func<T,string> getId) where T:UnityEngine.Object
        {foreach(string guid in AssetDatabase.FindAssets("t:"+typeof(T).Name,new[]{"Assets/Data"})){var x=AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));if(x!=null&&getId(x)==id)return x;}return null;}
        static string Effects(IReadOnlyList<ContentEffect> effects)=>effects==null?"":string.Join(";",effects.Where(x=>x!=null).Select(x=>x.type+":"+F(x.value)));
        static List<ContentEffect> ParseEffects(string text){var result=new List<ContentEffect>();if(string.IsNullOrWhiteSpace(text))return result;foreach(string token in text.Split(';')){var pair=token.Split(':');if(pair.Length!=2)throw new Exception("효과 형식은 Heal:35;AttackDamage:2 형태여야 합니다.");result.Add(new ContentEffect{type=(ContentEffectType)Enum.Parse(typeof(ContentEffectType),pair[0],true),value=float.Parse(pair[1],CultureInfo.InvariantCulture)});}return result;}
        static string Escape(string value){value=value??"";return value.IndexOfAny(new[]{',','"','\n','\r'})>=0?"\""+value.Replace("\"","\"\"")+"\"":value;}
        static List<string> Parse(string line){var result=new List<string>();var b=new StringBuilder();bool quoted=false;for(int i=0;i<line.Length;i++){char c=line[i];if(c=='"'){if(quoted&&i+1<line.Length&&line[i+1]=='"'){b.Append('"');i++;}else quoted=!quoted;}else if(c==','&&!quoted){result.Add(b.ToString());b.Clear();}else b.Append(c);}result.Add(b.ToString());return result;}
        static string Clean(string value)=>(value??"").Replace("\r"," ").Replace("\n"," ");static string F(float value)=>value.ToString("0.###",CultureInfo.InvariantCulture);
        static string Get(Dictionary<string,string> row,string key){if(!row.TryGetValue(key,out string value))throw new Exception("CSV에 "+key+" 열이 없습니다.");return value.Trim();}
        static int I(Dictionary<string,string> row,string key)=>int.Parse(Get(row,key),CultureInfo.InvariantCulture);static float R(Dictionary<string,string> row,string key)=>float.Parse(Get(row,key),CultureInfo.InvariantCulture);static bool B(Dictionary<string,string> row,string key)=>bool.Parse(Get(row,key));
        static T E<T>(Dictionary<string,string> row,string key) where T:struct=>(T)Enum.Parse(typeof(T),Get(row,key),true);
    }

    static class ContentValidation
    {
        public static string Report()
        {
            var issues=new List<string>();var ids=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            void Id(string id,string label){if(string.IsNullOrWhiteSpace(id))issues.Add(label+": 고유 ID가 비어 있습니다.");else if(id.Any(c=>!(char.IsLetterOrDigit(c)||c=='_'||c=='-')))issues.Add(label+": ID에는 영문·숫자·밑줄·하이픈만 사용하세요.");else if(!ids.Add(id))issues.Add(label+": 중복 ID "+id);}
            foreach(string guid in AssetDatabase.FindAssets("t:DragonData",new[]{"Assets/Data"})){var x=AssetDatabase.LoadAssetAtPath<DragonData>(AssetDatabase.GUIDToAssetPath(guid));Id(x.speciesId,x.name);if(x.skill==null)issues.Add(x.name+": 스킬이 없습니다.");if(x.maxHP<1||x.attackDamage<1)issues.Add(x.name+": 능력치는 1 이상이어야 합니다.");}
            foreach(string guid in AssetDatabase.FindAssets("t:MonsterData",new[]{"Assets/Data"})){var x=AssetDatabase.LoadAssetAtPath<MonsterData>(AssetDatabase.GUIDToAssetPath(guid));Id(x.contentId,x.name);if(x.minimumFloor>x.maximumFloor)issues.Add(x.name+": 시작 층이 마지막 층보다 큽니다.");if(x.spawnWeight<=0)issues.Add(x.name+": 등장 가중치가 0입니다.");if(x.battleSprite==null)issues.Add(x.name+": 전투 그림이 없습니다.");}
            foreach(string guid in AssetDatabase.FindAssets("t:SkillData",new[]{"Assets/Data"})){var x=AssetDatabase.LoadAssetAtPath<SkillData>(AssetDatabase.GUIDToAssetPath(guid));Id(x.skillId,x.name);if(x.damage<1||x.cooldown<=0||x.hitCount<1||x.hitInterval<.03f)issues.Add(x.name+": 피해·쿨타임·타격 설정을 확인하세요.");if(x.statusChancePercent<0||x.statusChancePercent>100)issues.Add(x.name+": 상태이상 확률은 0~100이어야 합니다.");}
            foreach(string guid in AssetDatabase.FindAssets("t:ItemData",new[]{"Assets/Data"})){var x=AssetDatabase.LoadAssetAtPath<ItemData>(AssetDatabase.GUIDToAssetPath(guid));Id(x.contentId,x.name);if(x.effects==null||x.effects.Count==0)issues.Add(x.name+": 아이템 효과가 없습니다.");}
            foreach(string guid in AssetDatabase.FindAssets("t:AugmentData",new[]{"Assets/Data"})){var x=AssetDatabase.LoadAssetAtPath<AugmentData>(AssetDatabase.GUIDToAssetPath(guid));Id(x.contentId,x.name);if(x.maximumStacks<1)issues.Add(x.name+": 최대 중첩은 1 이상이어야 합니다.");}
            return issues.Count==0?"문제 없음":string.Join("\n",issues.Take(40))+(issues.Count>40?"\n... 외 "+(issues.Count-40)+"개":"");
        }
    }

    [CustomEditor(typeof(AugmentData))]
    sealed class AugmentDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("등급 등장 확률: 일반 60% · 레어 28% · 에픽 10% · 유니크 2%",MessageType.Info);
            DrawPropertiesExcluding(serializedObject,"m_Script","rarity");
            serializedObject.ApplyModifiedProperties();
        }
    }
}
