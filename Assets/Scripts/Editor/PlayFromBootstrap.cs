using UnityEditor;

namespace MyEditor {
    [InitializeOnLoad]
    public class PlayFromBootstrap {
        /*static PlayFromBootstrap() {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state) {
            if (state == PlayModeStateChange.EnteredPlayMode) {
                // Save the current scene to return to after play mode
                var currentScene = SceneManager.GetActiveScene().path;

                // Load the Bootstrap scene if not already loaded
                if (!SceneManager.GetActiveScene().name.Equals("BootstrapScene")) {
                    //EditorSceneManager.OpenScene("Scenes/BootstrapScene");
                    SceneManager.LoadScene("Scenes/BootstrapScene");
                    EditorPrefs.SetString("LastScene", currentScene);
                }
            } else if (state == PlayModeStateChange.ExitingPlayMode) {
                // Return to the original scene after play mode
                var lastScene = EditorPrefs.GetString("LastScene", "");
                if (!string.IsNullOrEmpty(lastScene)) {
                    SceneManager.LoadScene(lastScene);
                }
            }
        }*/
    }
}