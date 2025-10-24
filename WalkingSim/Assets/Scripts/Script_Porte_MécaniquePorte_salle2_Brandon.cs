using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 

    First Person Interaction Toolkit by Steven Harmon stevenharmongames.com
    Licensed under the MPL 2.0. https://www.mozilla.org/en-US/MPL/2.0/FAQ/
    Please use in your walking sims/horror/adventure/puzzle games! Drop me a line and share what make with it! :)    

 */
public class Door : MonoBehaviour
{
    [Header("Minecraft-like Door")]
    [Tooltip("Rotation de la porte fermée")]
    public Vector3 closedRot = new Vector3(0, 0, 0);
    [Tooltip("Rotation de la porte ouverte")]
    public Vector3 openRot = new Vector3(0, 90, 0);
    [Tooltip("Vitesse d'ouverture / fermeture (plus haute = plus instantané)")]
    public float openSpeed = 8f;

    private bool isOpen = false;      // État de la porte
    private bool over = false;        // Le joueur regarde la porte

    [Tooltip("Door Renderer")]
    public Renderer doorRend;         // Pour changer la couleur de la porte
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

    private Quaternion targetRotation;  // Rotation visée

    // ==========================
    // ==== Initialisation ======
    // ==========================
    void Start()
    {
        // Récupération de la caméra principale
        MainCam = GameObject.FindWithTag("MainCamera");
        if (MainCam == null)
        {
            MainCam = GameObject.FindObjectOfType<Camera>().gameObject;
        }

        // Récupère le script d'interaction
        InteractionScript = MainCam.GetComponent<Interact>();

        // Couleur d'origine du matériau
        originColor = doorRend.material.color;

        // Porte fermée par défaut
        targetRotation = Quaternion.Euler(closedRot);
        transform.localRotation = targetRotation;
    }

    // ==========================
    // ==== Boucle d'update =====
    // ==========================
    void Update()
    {
        // Transition douce de la rotation actuelle vers la rotation cible
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * openSpeed);

        // Gestion du surlignage couleur
        if (over)
        {
            doorRend.material.color = Color.Lerp(doorRend.material.color, targetColor, Time.deltaTime * 4);
        }
        else
        {
            doorRend.material.color = Color.Lerp(doorRend.material.color, originColor, Time.deltaTime * 2);
        }
    }

    // ==========================
    // ==== Quand le joueur regarde la porte ====
    // ==========================
    public void Hovering(Vector3 rayHitPoint)
    {
        over = true;
        StartCoroutine(Fadeout());

        // Change le message selon l'état
        if (!isOpen)
        {
            InteractionScript.message = prompts[0]; // "Open Door"
        }
        else
        {
            InteractionScript.message = prompts[1]; // "Close Door"
        }
    }

    // ==========================
    // ==== Quand le joueur interagit ====
    // ==========================
    public void Interacting()
    {
        // Inverse l'état de la porte
        isOpen = !isOpen;

        // Change la rotation cible
        if (isOpen)
        {
            targetRotation = Quaternion.Euler(openRot);

            // Joue le son d'ouverture
            if (Source && clips.Length > 0 && clips[0])
            {
                Source.Stop();
                Source.pitch = Random.Range(0.9f, 1.1f);
                Source.clip = clips[0];
                Source.Play();
            }
        }
        else
        {
            targetRotation = Quaternion.Euler(closedRot);

            // Joue le son de fermeture
            if (Source && clips.Length > 1 && clips[1])
            {
                Source.Stop();
                Source.pitch = Random.Range(0.9f, 1.1f);
                Source.clip = clips[1];
                Source.Play();
            }
        }
    }

    // ==========================
    // ==== Fait disparaître le surlignage ====
    // ==========================
    private IEnumerator Fadeout()
    {
        yield return new WaitForSeconds(1);
        over = false;
    }
}
