using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Minecraft-like Door")]
    [Tooltip("Rotation de la porte fermée (relative à la position d'origine)")]
    public Vector3 closedRot = new Vector3(0, 0, 0);
    [Tooltip("Rotation de la porte ouverte (en degrés à partir de la rotation actuelle)")]
    public Vector3 openRot = new Vector3(0, 90, 0);
    [Tooltip("Vitesse d'ouverture / fermeture (plus haut = plus instantané)")]
    public float openSpeed = 8f;

    private bool isOpen = false;
    private bool over = false;

    [Tooltip("Door Renderer")]
    public Renderer doorRend;
    private Color originColor;
    [Tooltip("Color when hovered over (looked at)")]
    public Color targetColor = Color.yellow;

    private GameObject MainCam;
    private Interact InteractionScript;

    [Tooltip("Hover prompt - 0 Open Door, 1 Close Door")]
    public string[] prompts = { "Open Door", "Close Door" };

    [Header("Door Audio")]
    [Tooltip("0 = ouverture, 1 = fermeture")]
    public AudioClip[] clips;
    public AudioSource Source;

    private Quaternion startRotation;   // Rotation actuelle au lancement
    private Quaternion targetRotation;  // Rotation visée

    void Start()
    {
        // Récupère la rotation initiale depuis la scène
        startRotation = transform.localRotation;
        targetRotation = startRotation;

        // Sécurise la physique
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Récupération de la caméra principale
        MainCam = GameObject.FindWithTag("MainCamera");
        if (MainCam == null)
            MainCam = GameObject.FindObjectOfType<Camera>().gameObject;

        InteractionScript = MainCam.GetComponent<Interact>();
        originColor = doorRend.material.color;
    }

    void Update()
    {
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * openSpeed);

        // Gestion du surlignage
        if (over)
            doorRend.material.color = Color.Lerp(doorRend.material.color, targetColor, Time.deltaTime * 4);
        else
            doorRend.material.color = Color.Lerp(doorRend.material.color, originColor, Time.deltaTime * 2);
    }

    public void Hovering(Vector3 rayHitPoint)
    {
        over = true;
        StartCoroutine(Fadeout());
        InteractionScript.message = isOpen ? prompts[1] : prompts[0];
    }

    public void Interacting()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            targetRotation = startRotation * Quaternion.Euler(openRot);
            PlaySound(0);
        }
        else
        {
            targetRotation = startRotation * Quaternion.Euler(closedRot);
            PlaySound(1);
        }
    }

    private void PlaySound(int index)
    {
        if (Source && clips.Length > index && clips[index])
        {
            Source.Stop();
            Source.pitch = Random.Range(0.9f, 1.1f);
            Source.clip = clips[index];
            Source.Play();
        }
    }

    private IEnumerator Fadeout()
    {
        yield return new WaitForSeconds(1);
        over = false;
    }
}
