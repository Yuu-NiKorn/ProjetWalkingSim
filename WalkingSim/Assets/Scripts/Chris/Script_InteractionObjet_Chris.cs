using UnityEngine;
using UnityEngine.Events;
using System;

public class SimpleInteractable : MonoBehaviour
{
    [TextArea] public string hoverPrompt = "Press E";
    public UnityEvent onInteractUnity;
    public event Action<SimpleInteractable> onInteracted;
    [SerializeField] public bool exit = false;
    
    private Interact inter;
    private bool _used = false;

    void Awake() => inter = FindObjectOfType<Interact>();

    void Hovering(Vector3 hitPoint)
    {
        // ✅ on ne montre même pas le prompt pendant un dialogue
        if (DialogueUI.AnyDialoguePlaying) return;

        if (!_used && inter != null)
            inter.message = hoverPrompt;
    }

    void Interacting()
    {
        // ✅ bloque totalement l'interaction pendant un dialogue
        if (DialogueUI.AnyDialoguePlaying) return;

        if (exit == true)
            Application.Quit();
        
        if (_used) return; 
        _used = true;

        onInteractUnity?.Invoke();
        onInteracted?.Invoke(this);

        // rend l'objet non-interactable pour la suite
        gameObject.tag = "Untagged";

        if (inter != null) inter.message = "";
    }

    void UnHover()
    {
        if (!_used && inter != null)
            inter.message = "";
    }
}