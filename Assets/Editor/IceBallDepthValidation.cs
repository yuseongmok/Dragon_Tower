using System;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace DragonTower.Editor
{
    public static class IceBallDepthValidation
    {
        public static void Run(){IceBallDepthSetup.Install();Environment.SetEnvironmentVariable("DRAGON_TOWER_TEST_SAVE","DragonTower.IceBallDepth.Validation");EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity");SessionState.SetInt("IceThrough",3);SessionState.SetBool("IceBatch",true);EditorApplication.isPlaying=true;}
    }
}
