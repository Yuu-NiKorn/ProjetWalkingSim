using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    public TextMeshProUGUI textUI;
    public AudioSource audioSource;
    public CanvasGroup canvasGroup;

    public IEnumerator PlayDialogue(DialogueData dialogue)
    {
        canvasGroup.alpha = 1;

        foreach (var line in dialogue.lines)
        {
            textUI.text = line.text;

            if (line.audio != null)
            {
                audioSource.clip = line.audio;
                audioSource.Play();
            }

            yield return new WaitForSeconds(line.duration);
        }

        textUI.text = "";
        canvasGroup.alpha = 0;
    }
}