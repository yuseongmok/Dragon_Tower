using System;
using UnityEngine;
namespace DragonTower
{
    // Shared Wind palette, sampled from the approved claw. Normal UI alpha blending;
    // no additive material, bloom or runtime texture generation. Claw remains independent.
    public static class WindVFXStyle
    {
        public static readonly Color Deep = new Color(.025f,.20f,.32f);
        public static readonly Color Teal = new Color(.035f,.39f,.43f);
        public static readonly Color Cyan = new Color(.14f,.90f,.91f);
        public static readonly Color Mint = new Color(.35f,.98f,.72f);
        public static readonly Color Pale = new Color(.94f,1,.93f);
        public static Color Alpha(Color color,float alpha){color.a=Mathf.Clamp01(alpha);return color;}
        // Resolve at initialization only, never per frame. All slices share one texture.
        public static Sprite[] Load()=>Resources.LoadAll<Sprite>("VFX/WindRemaster");
        public static Sprite Find(Sprite[] atlas,string name)=>Array.Find(atlas,s=>s.name==name);
        // Common: 2-3 restrained layers; Rare: 3-4 layers with a thin bright edge.
        // Existing lifetimes, fragment budgets and confirmed damage cues stay authoritative.
    }
}
