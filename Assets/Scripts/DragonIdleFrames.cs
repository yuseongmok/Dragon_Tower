using UnityEngine;
namespace DragonTower
{
    [CreateAssetMenu(menuName="Dragon Tower/Idle Frames")]
    public sealed class DragonIdleFrames : ScriptableObject
    {
        public Sprite[] frames;
        public float[] durations;
        public Vector2[] offsets;
        [Min(.1f)] public float displayScale=1;
        public float Length
        {
            get { float length=0;if(frames!=null)for(int i=0;i<frames.Length;i++)length+=Duration(i);return length; }
        }
        public Sprite Once(float time)
        {
            if(frames==null || frames.Length==0)return null;
            for(int i=0;i<frames.Length;i++){time-=Duration(i);if(time<0)return frames[i];}
            return frames[frames.Length-1];
        }
        public Vector2 OffsetFor(Sprite sprite)
        {
            int i=System.Array.IndexOf(frames,sprite);
            return offsets!=null && i>=0 && i<offsets.Length ? offsets[i] : Vector2.zero;
        }
        public Sprite At(float time)
        {
            if(frames==null || frames.Length==0) return null;
            float length=0;
            for(int i=0;i<frames.Length;i++) length+=Duration(i);
            time=Mathf.Repeat(time,length);
            for(int i=0;i<frames.Length;i++) { time-=Duration(i); if(time<0)return frames[i]; }
            return frames[0];
        }
        float Duration(int i) => durations!=null && i<durations.Length ? Mathf.Max(.02f,durations[i]) : .2f;
    }
}
