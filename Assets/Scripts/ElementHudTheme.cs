using UnityEngine;
namespace DragonTower
{
    [CreateAssetMenu(menuName="Dragon Tower/UI/Element HUD Theme",fileName="ElementTheme")]
    public sealed class ElementHudTheme : ScriptableObject
    {
        public ElementType element;
        public Color primaryColor = new Color(.55f,.63f,.72f);
        public Color secondaryColor = new Color(.30f,.38f,.47f);
        public Color glowColor = new Color(.80f,.88f,1f);
        [Tooltip("Optional override. Empty uses the existing element icon library.")]
        public Sprite elementIcon;
    }
}
