using System.Collections;
using UnityEngine;

public class CubeInteractable : MonoBehaviour
{
    [TextArea] public string hoverPrompt = "Press E";
    [TextArea] public string interactedMessage = "Tu as interagi avec le cube !";
    public float feedbackDuration = 1.5f;

    Interact inter; // référence au script Interact (HUD + raycast)

    void Awake()
    {
        // On récupère le premier Interact de la scène (ou référence manuelle possible)
        inter = FindObjectOfType<Interact>();
    }

    // Appelé chaque frame où le raycast du joueur touche ce cube (depuis Interact.SendMessage)
    void Hovering(Vector3 hitPoint)
    {
        if (inter != null)
            inter.message = hoverPrompt; // "Press E"
    }

    // Appelé quand le joueur appuie sur E (Input "Interact")
    void Interacting()
    {
        if (inter != null)
        {
            inter.message = interactedMessage;
            StopAllCoroutines();
            StartCoroutine(ClearAfterDelay());
        }
        // Ici tu peux déclencher ce que tu veux (ouvrir porte, ramasser, etc.)
    }

    // Appelé quand le rayon ne regarde plus l’objet
    void UnHover()
    {
        if (inter != null)
            inter.message = "";
    }

    IEnumerator ClearAfterDelay()
    {
        yield return new WaitForSeconds(feedbackDuration);
        if (inter != null) inter.message = "";
    }

    // Optionnel : si tu veux utiliser l’entrée "Squint"
    void Looking()
    {
        if (inter != null)
            inter.message = "Tu plisses les yeux sur le cube…";
    }
}
