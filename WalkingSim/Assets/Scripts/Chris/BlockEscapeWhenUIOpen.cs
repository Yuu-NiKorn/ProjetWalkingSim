using UnityEngine;

public class BlockEscapeWhenUIOpen : MonoBehaviour
{
    [Header("Canvas du Digicode")]
    public GameObject keypadCanvas; // le Canvas du digicode

    void Update()
    {
        if (keypadCanvas == null) return;

        // Si le digicode est ouvert ET qu'on appuie sur Échap
        if (keypadCanvas.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            // On bloque totalement l'action
            Debug.Log("Échap bloqué pendant le digicode !");
        }
    }
}