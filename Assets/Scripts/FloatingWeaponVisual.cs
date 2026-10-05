using System;
using UnityEngine;
using UnityEngine.UI;
namespace DragonTower
{
    [Serializable] public sealed class FloatingWeaponPose
    {
        public Sprite characterFrame;
        public Vector2 position;
        public float angle;
        public bool hover=true;
    }
    [Serializable] public sealed class FloatingWeaponData
    {
        public Sprite sprite;
        public Vector2 size=new Vector2(44,150);
        public Vector2 tip=new Vector2(.5f,.92f);
        public bool attackFromTip=true,skillFromTip=true;
        public FloatingWeaponPose[] poses;
    }
    // Presentation only. Stepped by the existing battle animation clock; no combat or character ID rules.
    public sealed class FloatingWeaponVisual
    {
        FloatingWeaponData data;
        Image image,actor;
        RectTransform parent;
        float clock;
        Vector2 position;
        float angle,castingWeight,castingAngle;Vector2 castingPosition;
        public void SetCastingPose(float weight,Vector2 target,float rotation){castingWeight=Mathf.Clamp01(weight);castingPosition=target;castingAngle=rotation;if(Active)Apply(FindPose());}
        public bool Active=>data!=null&&image!=null&&image.gameObject.activeInHierarchy;
        public void Bind(FloatingWeaponData value,Image character,RectTransform root)
        {
            data=value;actor=character;parent=root;clock=0;castingWeight=0;
            if(data==null||data.sprite==null){data=null;if(image!=null)image.gameObject.SetActive(false);return;}
            if(image==null){var go=new GameObject("Floating weapon",typeof(RectTransform),typeof(Image));image=go.GetComponent<Image>();image.raycastTarget=false;image.preserveAspect=true;}
            var r=image.rectTransform;r.SetParent(root,false);r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;
            r.localScale=Vector3.one;r.sizeDelta=data.size;image.sprite=data.sprite;image.color=Color.white;image.gameObject.SetActive(true);
            var pose=FindPose();position=pose!=null?pose.position:new Vector2(112,-5);angle=pose!=null?pose.angle:0;Apply(pose);
        }
        FloatingWeaponPose FindPose()
        {
            if(data?.poses!=null&&actor!=null)foreach(var pose in data.poses)if(pose!=null&&pose.characterFrame==actor.sprite)return pose;
            return null;
        }
        public void Step(float delta)
        {
            if(data==null||image==null||actor==null)return;
            image.gameObject.SetActive(actor.gameObject.activeSelf);
            if(delta>0){clock+=delta;var p=FindPose();if(p!=null){float blend=1-Mathf.Exp(-32*delta);position=Vector2.Lerp(position,p.position,blend);angle=Mathf.LerpAngle(angle,p.angle,blend);}Apply(p);}
        }
        void Apply(FloatingWeaponPose pose)
        {
            bool hover=pose==null||pose.hover;
            image.rectTransform.anchoredPosition=Vector2.Scale(Vector2.Lerp(position,castingPosition,castingWeight)+new Vector2(0,hover?Mathf.Sin(clock*2.4f)*2.3f:0),parent.rect.size/256f);
            image.rectTransform.localRotation=Quaternion.Euler(0,0,Mathf.LerpAngle(angle,castingAngle,castingWeight)+(hover?Mathf.Sin(clock*1.7f)*1.6f:0));
            image.color=actor.color;
        }
        public bool TryOrigin(DragonVisualAnchor kind,RectTransform relative,out Vector2 point)
        {
            point=default;if(data==null)return false;
            return ((kind==DragonVisualAnchor.AttackOrigin&&data.attackFromTip)||(kind==DragonVisualAnchor.SkillOrigin&&data.skillFromTip))&&TryAnchor("StaffTip",relative,out point);
        }
        public bool TryAnchor(string name,RectTransform relative,out Vector2 point)
        {
            point=default;if(!Active||relative==null)return false;
            Vector2 normalized;
            if(name=="StaffCenter"||name=="WeaponCenter")normalized=Vector2.one*.5f;
            else if(name=="StaffTip"||name=="CastingPoint"||name=="WeaponTip")normalized=data.tip;
            else return false;
            var r=image.rectTransform;point=relative.InverseTransformPoint(r.TransformPoint(Vector2.Scale(normalized-r.pivot,r.rect.size)));return true;
        }
    }
}
