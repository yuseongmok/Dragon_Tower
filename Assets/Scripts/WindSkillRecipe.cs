using UnityEngine;
namespace DragonTower
{
    public enum WindModule { WindGather,WindTrail,WindSlash,TornadoCore,TornadoOuter,WindHitSpark,WindShockwave,WindPixelParticles,WindAfterimage }
    [System.Serializable] public sealed class WindModuleClip
    {
        public WindModule module;
        public float start,duration=.3f;
        public Vector2 size=new Vector2(160,160),offset;
        [Range(0,1)] public float anchor=1;
        public float endAnchor=1;
        public float startScale=1,endScale=1;
        public bool loop;
        [Range(0,1)] public float opacity=1;
    }
    [CreateAssetMenu(menuName="Dragon Tower/VFX/Wind Skill Recipe")]
    public sealed class WindSkillRecipe : ScriptableObject
    {
        public string skillId="skill_gale_strike";
        public float duration=1.35f,finalImpact=.86f;
        public Sprite[] frames;
        public WindModuleClip[] clips;
        public Sprite Frame(WindModule module,float phase)
        {
            int index=(int)module*8+Mathf.Clamp((int)(phase*8),0,7);
            return frames!=null&&index<frames.Length?frames[index]:null;
        }
    }
}
