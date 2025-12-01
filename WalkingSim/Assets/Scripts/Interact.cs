using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* 
    First Person Interaction Toolkit by Steven Harmon stevenharmongames.com
    Adapté pour gestion directe de la touche E + blocage pendant dialogues.
*/

public class Interact : MonoBehaviour
{
    private Vector3 fwd;
    [HideInInspector]
    public bool hover = false;
    private bool alreadyHovered = false;
    private bool alreadyHovered2 = false;

    [Header("General Interaction Variables")]
    public GameObject InteractionUI;
    public GameObject CrosshairUI;
    private Animation anim;
    private Text dispText;
    private float dist = 1000;

    [System.NonSerialized]
    public string message = "";

    [System.NonSerialized]
    public GameObject currentObj = null;
    private GameObject storedIntObj;

    void Start()
    {
        anim = InteractionUI.GetComponent<Animation>();
        dispText = InteractionUI.GetComponent<Text>();
        dispText.text = "";
    }

    void Update()
    {
        // ✅ Si un dialogue est en cours : on bloque TOUTE interaction
        if (DialogueUI.AnyDialoguePlaying)
        {
            hover = false;
            dispText.text = "";
            CrosshairUI.SetActive(false);
            storedIntObj = null;
            currentObj = null;
            alreadyHovered = false;
            alreadyHovered2 = true; // pour éviter de rejouer le popup
            return;
        }

        fwd = transform.TransformDirection(Vector3.forward);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, fwd, out hit, 100))
        {
            currentObj = hit.collider.gameObject;

            if (currentObj.CompareTag("Interactable"))
            {
                storedIntObj = currentObj;
                dist = Vector3.Distance(hit.transform.position, transform.position);

                if (dist < 3)
                {
                    storedIntObj.transform.SendMessage("Hovering", hit.point, SendMessageOptions.DontRequireReceiver);
                    dispText.text = message;

                    if (!alreadyHovered)
                    {
                        anim.Play("An_InteractTextPopup");
                        CrosshairUI.SetActive(true);
                        alreadyHovered2 = false;
                        alreadyHovered = true;
                    }

                    hover = true;

                    // touche E pour interagir
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        hit.transform.SendMessage("Interacting", SendMessageOptions.DontRequireReceiver);
                    }

                    // clic droit pour "Squint" (optionnel)
                    if (Input.GetMouseButtonDown(1))
                    {
                        hit.transform.SendMessage("Looking", SendMessageOptions.DontRequireReceiver);
                    }
                }
                else
                {
                    ResetHoverState();
                }
            }
            else
            {
                ResetHoverState();
            }
        }
        else
        {
            hover = false;
            dispText.text = "";
            CrosshairUI.SetActive(false);
            storedIntObj = null;
        }
    }

    void ResetHoverState()
    {
        CrosshairUI.SetActive(false);
        hover = false;
        alreadyHovered = false;

        if (!alreadyHovered2)
        {
            anim.Play("An_InteractTextPopout");
            alreadyHovered2 = true;
        }

        if (storedIntObj != null)
        {
            storedIntObj.transform.SendMessage("UnHover", SendMessageOptions.DontRequireReceiver);
            storedIntObj = null;
        }
    }
}


