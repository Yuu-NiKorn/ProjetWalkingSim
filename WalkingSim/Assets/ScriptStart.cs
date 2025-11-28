using UnityEngine;
using UnityEngine.SceneManagement;

namespace Yuu
{
    public class ButtonStart : MonoBehaviour
    {
        public void OnStartClick()
        {
            Debug.Log("Bouton Start Appuyé");
            SceneManager.LoadScene("SceneDemoScript");
        }

        public void OnExitClick()
        {
#if UNITY_EDITOR
            Debug.Log("all good");
            // quitte playmode quand tu test dans editor
            UnityEditor.EditorApplication.isPlaying = false;

#else
        Application.Quit();
#endif
        }
    }
    
}