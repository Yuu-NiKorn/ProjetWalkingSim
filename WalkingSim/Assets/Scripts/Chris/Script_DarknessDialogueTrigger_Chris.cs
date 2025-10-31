using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DarknessDialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueData dialogue;
    public DialogueUI dialogueUI;

    [Header("Voile noir plein écran")]
    // CanvasGroup d'une Image noire plein écran (voir étapes setup)
    public CanvasGroup blackout;
    [Range(0f, 1f)] public float targetDarkness = 0.95f; // 0.95 ≈ on ne voit presque rien
    public float fadeSpeed = 3f; // vitesse du fondu (1–5)

    [Header("Options")]
    public bool triggerOnce = true;
    public bool restoreOnDialogueEnd = true; // remet la vision en fin de dialogue

    bool hasTriggered = false;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        if (blackout != null) blackout.alpha = Mathf.Clamp01(blackout.alpha); // sécurité
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggerOnce && hasTriggered) return;
        hasTriggered = true;

        // Assombrir fortement l'écran
        if (blackout != null)
            StartCoroutine(FadeCanvas(blackout, blackout.alpha, targetDarkness, fadeSpeed));

        // Lancer le dialogue
        if (dialogueUI != null && dialogue != null)
            StartCoroutine(RunDialogueThenMaybeRestore());
    }

    IEnumerator RunDialogueThenMaybeRestore()
    {
        yield return dialogueUI.PlayDialogue(dialogue);

        if (restoreOnDialogueEnd && blackout != null)
            StartCoroutine(FadeCanvas(blackout, blackout.alpha, 0f, fadeSpeed));

        if (triggerOnce)
            Destroy(gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        // Si tu veux que sortir de la zone rende la vue, décommente :
        /*
        if (!other.CompareTag("Player")) return;
        if (blackout != null)
            StartCoroutine(FadeCanvas(blackout, blackout.alpha, 0f, fadeSpeed));
        */
    }

    IEnumerator FadeCanvas(CanvasGroup cg, float from, float to, float speed)
    {
        if (cg == null) yield break;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            cg.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
        cg.alpha = to;
    }
}
