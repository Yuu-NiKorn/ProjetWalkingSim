using UnityEngine;

public class Script_Bouton_Brandon : MonoBehaviour
{
    
    public Script_ObstacleSalle2_Brandon door;

   
    private bool isPressed = false;
    public AudioClip pressSound;    // Ton son à jouer
    private Renderer rend;
    private AudioSource audioSource;

    void Start()
    {
        rend = GetComponent<Renderer>();

        // Ajoute un AudioSource automatiquement si absent
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Interacting()
    {
        if (isPressed) return;

        isPressed = true;
        Debug.Log("Bouton appuyé !");

        // Change la couleur en rouge
        if (rend != null)
            rend.material.color = Color.red;

        // Joue le son
        if (pressSound != null)
            audioSource.PlayOneShot(pressSound);

        // Incrémente le compteur de la porte
        if (door != null)
            door.AddPress();
    }

    public void Hovering(Vector3 point)
    {
        if (!isPressed)
            rend.material.color = Color.yellow;
    }

    public void UnHover()
    {
        if (!isPressed)
            rend.material.color = Color.white;
    }
}
