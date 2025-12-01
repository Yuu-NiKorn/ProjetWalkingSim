using UnityEngine;
using TMPro;
using System.Collections;
using System;

public class DialogueUI : MonoBehaviour
{
    public TextMeshProUGUI textUI;
    public AudioSource audioSource;
    public CanvasGroup canvasGroup;
    
    public Action<int, DialogueData.DialogueLine> onLineStarted;
    public int CurrentLineIndex { get; private set; } = -1;
    public bool IsPlaying { get; private set; } = false;

    // ✅ Flag global : n'importe quel script peut vérifier ça
    public static bool AnyDialoguePlaying { get; private set; } = false;

    public IEnumerator PlayDialogue(DialogueData dialogue)
    {
        IsPlaying = true;
        AnyDialoguePlaying = true;   // ✅ on démarre un dialogue

        CurrentLineIndex = -1;
        if (canvasGroup != null)
            canvasGroup.alpha = 1;

        for (int i = 0; i < dialogue.lines.Length; i++)
        {
            CurrentLineIndex = i;
            var line = dialogue.lines[i];
            
            onLineStarted?.Invoke(i, line);

            if (textUI != null) textUI.text = line.text ?? "";

            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = line.audio;
                if (audioSource.clip != null) audioSource.Play();
            }

            float wait = Mathf.Max(0.01f, line.duration);
            yield return new WaitForSeconds(wait);
        }

        // Fin
        if (textUI != null) textUI.text = "";
        if (canvasGroup != null) canvasGroup.alpha = 0;

        IsPlaying = false;
        AnyDialoguePlaying = false;  // ✅ plus de dialogue en cours
        CurrentLineIndex = -1;
    }
}