using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DarknessDialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueData dialogue;
    public DialogueUI dialogueUI;

    [Header("Voile noir plein écran")]
    public CanvasGroup blackout;
    [Range(0f, 1f)] public float targetDarkness = 0.95f; // quasi noir
    public float fadeSpeed = 3f;

    [Header("Comportement")]
    public int keepDarkForFirstLines = 3; // ← rester sombre pendant ces N premières répliques
    public bool triggerOnce = true;
    public bool restoreAtEnd = true;      // remettre la vision à la fin du dialogue (au cas où)

    private bool hasTriggered = false;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        if (blackout != null) blackout.alpha = 0f;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggerOnce && hasTriggered) return;
        hasTriggered = true;

        // 1) On plonge dans le noir
        if (blackout != null)
            StartCoroutine(FadeCanvas(blackout, blackout.alpha, targetDarkness, fadeSpeed));

        // 2) On écoute le moment où on atteint la ligne keepDarkForFirstLines
        if (dialogueUI != null)
        {
            dialogueUI.onLineStarted -= OnLineStarted; // éviter les doublons
            dialogueUI.onLineStarted += OnLineStarted;
        }

        // 3) On lance le dialogue
        StartCoroutine(RunDialogue());
    }

    void OnLineStarted(int index, DialogueData.DialogueLine line)
    {

        if (blackout != null && index >= keepDarkForFirstLines)
        {

            dialogueUI.onLineStarted -= OnLineStarted;
            StartCoroutine(FadeCanvas(blackout, blackout.alpha, 0f, fadeSpeed));
        }
    }

    IEnumerator RunDialogue()
    {
        if (dialogueUI != null && dialogue != null)
            yield return dialogueUI.PlayDialogue(dialogue);

        // Si le dialogue s’est fini avant d’avoir éclairci 
        if (restoreAtEnd && blackout != null && blackout.alpha > 0f)
            StartCoroutine(FadeCanvas(blackout, blackout.alpha, 0f, fadeSpeed));

        if (triggerOnce) Destroy(gameObject);
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
