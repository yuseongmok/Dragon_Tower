using System.IO;
using UnityEditor;
namespace DragonTower.Editor
{
    [InitializeOnLoad]
    public static class FirstOpen
    {
        static FirstOpen()
        {
            if (!UnityEngine.Application.isBatchMode)
                EditorApplication.delayCall += () =>
                {
                    if (!File.Exists("Assets/Scenes/Battle.unity")) PrototypeSetup.Create();
                };
        }
    }
}
