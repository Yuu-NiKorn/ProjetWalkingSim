using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

public class EndInteractable : MonoBehaviour
{
    [TextArea] public string hoverPrompt = "Press E";
    public UnityEvent onInteractUnity;
    public event Action<EndInteractable> onInteracted;
    public GameObject canvasChoice;
    public GameObject canvasEnd1;
    public GameObject canvasEnd2;
    
    [Header("Bloquer contrôles pendant l'UI")]
    public GameObject controlsRoot;
    public List<string> scriptTypeNamesToDisable = new List<string>();
    
    [Header("Bloquer aussi ces scripts pendant l'UI (ex: menu pause)")]
    [Tooltip("Dépose ici ton script de menu pause ou tout autre script à désactiver quand le digicode est ouvert.")]
    public MonoBehaviour[] extraScriptsToDisable;
    
    private List<MonoBehaviour> disabledDuringUI = new List<MonoBehaviour>();

    
    private Interact inter;
    private bool _used = false;

    private void Start()
    {
        canvasChoice.SetActive(false);
    }

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
        // lance le canva du menu de choix et bloque le joueur
        canvasChoice.SetActive(true);
        Time.timeScale = 0f;
        DisableControlsByName();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        
    }

    void UnHover()
    {
        if (!_used && inter != null)
            inter.message = "";
    }

    public void End1()
    {
        canvasChoice.SetActive(false);
        canvasEnd1.SetActive(true);
    }

    public void End2()
    {
        canvasChoice.SetActive(false);
        canvasEnd2.SetActive(true);
    }
    
    void DisableControlsByName()
    {
        disabledDuringUI.Clear();

        // 1) Scripts du player (controlsRoot) trouvés par nom
        if (controlsRoot != null && scriptTypeNamesToDisable != null && scriptTypeNamesToDisable.Count > 0)
        {
            var all = controlsRoot.GetComponentsInChildren<MonoBehaviour>(true);

            foreach (var mb in all)
            {
                if (mb == null) continue;
                string typeName = mb.GetType().Name;

                for (int i = 0; i < scriptTypeNamesToDisable.Count; i++)
                {
                    if (typeName == scriptTypeNamesToDisable[i] && mb.enabled)
                    {
                        mb.enabled = false;
                        disabledDuringUI.Add(mb);
                    }
                }
            }
        }

        // 2) Scripts explicitement référencés (ex: script de menu pause)
        if (extraScriptsToDisable != null)
        {
            foreach (var mb in extraScriptsToDisable)
            {
                if (mb != null && mb.enabled)
                {
                    mb.enabled = false;
                    disabledDuringUI.Add(mb);
                }
            }
        }
    }
    
}