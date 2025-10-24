using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [Header("Objets à activer avant la porte")]
    public List<CubeInteractable> requiredObjects = new List<CubeInteractable>();

    [Header("Porte à débloquer")]
    public GameObject door;

    private int interactedCount = 0;
    private bool unlocked = false;

    void Start()
    {
        // Sécurité : vérifie que la porte est bien “fermée” au départ
        if (door != null) door.tag = "Untagged";

        // Initialise les callbacks pour chaque cube
        foreach (var obj in requiredObjects)
        {
            if (obj != null)
            {
                obj.onInteracted += OnObjectInteracted;
            }
        }
    }

    void OnObjectInteracted(CubeInteractable obj)
    {
        if (unlocked) return;

        interactedCount++;
        Debug.Log($"[{obj.name}] interagi ({interactedCount}/{requiredObjects.Count})");

        // Vérifie si tous les objets ont été trouvés
        if (interactedCount >= requiredObjects.Count)
        {
            UnlockDoor();
        }
    }

    void UnlockDoor()
    {
        unlocked = true;
        if (door != null)
        {
            door.tag = "Interactable";
            Debug.Log(" Tous les objets interagis : la porte est maintenant INTERACTABLE !");
        }
    }
}