using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    public GameObject pauseUI;      // Canvas du menu pause
    public GameObject controlsRoot; // ton Player (pour désactiver les scripts)
    public string[] scriptsToDisable;

    private bool paused = false;
    private readonly System.Collections.Generic.List<MonoBehaviour> disabledScripts =
        new System.Collections.Generic.List<MonoBehaviour>();

    void Start()
    {
        if (pauseUI != null) pauseUI.SetActive(false);

        if (controlsRoot == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) controlsRoot = p;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !UIBlocker_Chris.blockEscape)
        {
            if (!paused) Pause();
            else Resume();
        }
    }

    public void Pause()
    {
        paused = true;

        if (pauseUI != null) pauseUI.SetActive(true);

        // curseur visible
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        DisableControlScripts();
        Time.timeScale = 0f; // Freeze gameplay
    }

    public void Resume()
    {
        paused = false;

        if (pauseUI != null) pauseUI.SetActive(false);

        // Curseur FPS
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        RestoreControlScripts();
        Time.timeScale = 1f;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void DisableControlScripts()
    {
        disabledScripts.Clear();
        if (controlsRoot == null || scriptsToDisable.Length == 0) return;

        var all = controlsRoot.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (var mb in all)
        {
            foreach (var name in scriptsToDisable)
            {
                if (mb.GetType().Name == name && mb.enabled)
                {
                    mb.enabled = false;
                    disabledScripts.Add(mb);
                }
            }
        }
    }

    private void RestoreControlScripts()
    {
        foreach (var mb in disabledScripts)
        {
            if (mb != null) mb.enabled = true;
        }
        disabledScripts.Clear();
    }
}
