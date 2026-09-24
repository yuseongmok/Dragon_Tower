using System.Collections.Generic;
using UnityEngine;
namespace DragonTower
{
    [CreateAssetMenu(menuName="Dragon Tower/Item")]
    public sealed class ItemData : IdentifiedContent
    {
        [Min(0)] public int price;
        public List<ContentEffect> effects=new List<ContentEffect>();
    }
}
