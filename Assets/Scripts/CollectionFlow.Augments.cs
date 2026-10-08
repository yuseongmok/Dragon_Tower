using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace DragonTower
{
    public sealed partial class CollectionFlow
    {
        Coroutine rewardReveal;
        GameObject rewardDetail;
        bool rewardsReady,choosingReward;
        const float RevealDelay=.28f,RevealStagger=.20f,RevealDuration=.34f;
        public bool RewardsReady=>rewardsReady;
        void CancelRewardReveal()
        {
            if(rewardReveal!=null)StopCoroutine(rewardReveal);
            rewardReveal=null;rewardsReady=false;choosingReward=false;rewardDetail=null;
        }
        void OnDisable(){CancelRewardReveal();}
        static string EmphasizeNumbers(string text)=>Regex.Replace(text??"",@"(?<![\w])\d+(?:\.\d+)?(?:%p|%|초|회|배)?",m=>"<color=#FFE0A0><b>"+m.Value+"</b></color>");
        Button AugmentRewardCard(string badge,string title,string summary,Sprite icon,float y,Color accent,UnityAction action)
        {
            var card=Button("",0,y,432,164,()=>{if(rewardsReady&&!choosingReward)action();});
            StyleCard(card,new Color(.055f,.075f,.115f,.98f),accent);
            var art=Rect("Reward icon",card.transform,-179,35,42,42).gameObject.AddComponent<Image>();art.sprite=icon;art.preserveAspect=true;art.raycastTarget=false;
            CardText(card.transform,"Reward badge",badge,-8,18,288,19,13,accent,TextAnchor.MiddleLeft);
            CardText(card.transform,"Reward title",title,-8,44,288,30,23,Color.white,TextAnchor.MiddleLeft);
            var desc=CardText(card.transform,"Reward summary",EmphasizeNumbers(summary),0,104,394,74,16,new Color(.91f,.94f,1),TextAnchor.UpperLeft);
            desc.lineSpacing=1.16f;desc.supportRichText=true;
            CardText(card.transform,"Read detail","자세히 보기  ›",131,149,126,18,13,new Color(.72f,.8f,.91f),TextAnchor.MiddleRight);
            var group=card.gameObject.AddComponent<CanvasGroup>();group.alpha=0;group.interactable=false;group.blocksRaycasts=false;
            return card;
        }
        void BeginRewardReveal(){rewardsReady=false;choosingReward=false;rewardReveal=StartCoroutine(RevealRewards());}
        IEnumerator RevealRewards()
        {
            var owner=body;var cards=ChoiceButtons;float elapsed=0;
            int count=0;foreach(var b in cards)if(b!=null)count++;
            float total=RevealDelay+Mathf.Max(0,count-1)*RevealStagger+RevealDuration;
            while(elapsed<total&&body==owner)
            {
                elapsed+=Time.unscaledDeltaTime;
                for(int i=0;i<count;i++)
                {
                    var b=cards[i];if(b==null)continue;
                    float t=Mathf.Clamp01((elapsed-RevealDelay-i*RevealStagger)/RevealDuration),ease=1-Mathf.Pow(1-t,3);
                    var group=b.GetComponent<CanvasGroup>();group.alpha=t;
                    var rect=b.GetComponent<RectTransform>();rect.anchoredPosition=new Vector2(0,-(301+i*178)-18*(1-ease));rect.localScale=Vector3.one*(.96f+.04f*ease);
                }
                yield return null;
            }
            if(body!=owner)yield break;
            foreach(var b in cards)if(b!=null){var group=b.GetComponent<CanvasGroup>();group.alpha=1;group.interactable=group.blocksRaycasts=true;b.transform.localScale=Vector3.one;}
            rewardsReady=true;rewardReveal=null;
            if(SecondRoomButton!=null)SecondRoomButton.interactable=true;
            notice.text="읽는 시간에는 제한이 없습니다 · 1개를 선택하세요";
        }
        void SkipAugmentReward(bool levelReward)
        {
            if(!rewardsReady||choosingReward||rewardDetail!=null)return;
            choosingReward=true;
            if(SecondRoomButton!=null)SecondRoomButton.interactable=false;
            towerRun.ConsumeEmptyAugmentReward(levelReward);
            if(levelReward)ContinueAfterLevelRewards();else AdvanceFloor();
        }
        void ShowRewardDetails(AugmentData augment,SkillData skill,bool levelReward)
        {
            if(!rewardsReady||rewardDetail!=null)return;
            var overlay=Rect("Reward details",body,0,425,480,850);rewardDetail=overlay.gameObject;
            var shade=overlay.gameObject.AddComponent<Image>();shade.color=new Color(.015f,.025f,.05f,.97f);shade.raycastTarget=true;
            string title=augment!=null?augment.displayName:skill.displayName;
            Color accent=augment!=null?DragonTowerTheme.Grade(augment.grade):new Color(.4f,.8f,1);
            CardText(overlay,"Detail grade",augment!=null?GradeName(augment.grade)+" 증강":"스킬 교체",0,128,404,30,17,accent,TextAnchor.MiddleLeft);
            CardText(overlay,"Detail name",title,0,176,404,48,31,Color.white,TextAnchor.MiddleLeft);
            CardText(overlay,"Detail tags",augment!=null?augment.buildTags:ElementLabel(skill.elementType)+" 속성",0,220,404,32,16,new Color(.72f,.8f,.91f),TextAnchor.MiddleLeft);
            // Scroll only the reading area; confirmation never covers the text.
            var viewport=Rect("Reading viewport",overlay,0,445,404,370);viewport.gameObject.AddComponent<RectMask2D>();
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.viewport=viewport;
            var content=Rect("Reading content",viewport,0,0,394,370);content.pivot=new Vector2(.5f,1);content.anchoredPosition=Vector2.zero;scroll.content=content;
            var ray=viewport.gameObject.AddComponent<Image>();ray.color=new Color(0,0,0,.001f);
            string main=augment!=null?augment.description:SkillSummary(skill);
            string details=augment!=null?augment.rulesDescription:skill.description;
            var text=CardText(content,"Full effect",EmphasizeNumbers(main)+"\n\n<color=#B9C8DF>발동 조건과 제한</color>\n"+EmphasizeNumbers(details),0,0,388,370,20,new Color(.94f,.96f,1),TextAnchor.UpperLeft);
            text.lineSpacing=1.25f;text.rectTransform.pivot=new Vector2(.5f,1);text.rectTransform.anchoredPosition=Vector2.zero;
            Canvas.ForceUpdateCanvases();float height=Mathf.Max(370,text.preferredHeight+20);text.rectTransform.sizeDelta=new Vector2(388,height);content.sizeDelta=new Vector2(394,height);
            var confirm=Button("이 선택으로 성장",0,697,404,62,()=>
            {
                if(choosingReward)return;choosingReward=true;
                if(augment!=null)ChooseAugment(augment,levelReward);else ChooseSkill(skill,levelReward);
                choosingReward=false;
            });confirm.transform.SetParent(overlay,false);StyleCard(confirm,new Color(.18f,.13f,.055f,1),accent);
            var back=Button("다른 선택지 읽기",0,770,404,48,()=>{Destroy(rewardDetail);rewardDetail=null;});back.transform.SetParent(overlay,false);
        }
    }
}
