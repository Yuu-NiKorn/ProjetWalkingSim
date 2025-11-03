using UnityEngine;
using UnityEngine.Events;
using System;

public class SimpleInteractable : MonoBehaviour
{
    [TextArea] public string hoverPrompt = "Press E";
    public UnityEvent onInteractUnity;
    public event Action<SimpleInteractable> onInteracted;

    private Interact inter;
    private bool _used = false;

    void Awake() => inter = FindObjectOfType<Interact>();

    void Hovering(Vector3 hitPoint)
    {
        if (!_used && inter != null)
            inter.message = hoverPrompt;
    }

    void Interacting()
    {
        if (_used) return; 
        _used = true;

        onInteractUnity?.Invoke();
        onInteracted?.Invoke(this);


        gameObject.tag = "Untagged";

        if (inter != null) inter.message = "";
    }

    void UnHover()
    {
        if (!_used && inter != null)
            inter.message = "";
    }
}