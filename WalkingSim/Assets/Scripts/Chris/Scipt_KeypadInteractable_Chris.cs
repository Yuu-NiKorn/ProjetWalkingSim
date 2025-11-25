using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KeypadInteractable : MonoBehaviour
{
    [Header("Prompt Interact")]
    [TextArea] public string hoverPrompt = "Press E";

    [Header("Code")]
    [SerializeField] private string correctCode = "1234";

    [Header("UI")]
    public Script_KeypadUI_Chris keypadUI;
    public GameObject keypadCanvasRoot;

    [Header("Curseur de secours (croix UI)")]
    public RectTransform fakeCursor;
    public bool useFakeCursorIfNeeded = true;

    [Header("Objet à débloquer")]
    public GameObject targetToUnlock;
    public bool makeTargetInteractableByTag = true;
    public string interactableTagName = "Interactable";

    [Header("Feedback visuel digicode")]
    public Renderer keypadRenderer;
    public Color idleColor = Color.white;
    public Color successColor = Color.green;
    public Color failColor = Color.red;

    [Tooltip("Temps pendant lequel le digicode reste vert/rouge")]
    public float colorFeedbackTime = 1.5f;

    [Tooltip("Délai avant fermeture de l'UI après Validé")]
    public float uiCloseDelay = 0.2f;

    [Header("Feedback audio")]
    public AudioSource sfxSource;
    public AudioClip successSfx;
    public AudioClip failSfx;

    [Header("Bloquer contrôles pendant l'UI")]
    public GameObject controlsRoot;
    public List<string> scriptTypeNamesToDisable = new List<string>();

    private Material[] mats;
    private bool uiOpen = false;

    private List<MonoBehaviour> disabledDuringUI = new List<MonoBehaviour>();

    // Référence à ton HUD/raycast Interact
    private Interact inter;

    void Awake()
    {
        inter = FindObjectOfType<Interact>();

        if (keypadRenderer == null) keypadRenderer = GetComponentInChildren<Renderer>();
        if (keypadRenderer != null) mats = keypadRenderer.materials;

        if (keypadUI != null) keypadUI.owner = this;

        if (keypadCanvasRoot != null) keypadCanvasRoot.SetActive(false);
        if (fakeCursor != null) fakeCursor.gameObject.SetActive(false);

        if (targetToUnlock != null && makeTargetInteractableByTag)
            targetToUnlock.tag = "Untagged";

        SetKeypadColor(idleColor);

        if (controlsRoot == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) controlsRoot = p;
            else if (Camera.main != null) controlsRoot = Camera.main.gameObject;
        }
    }

    void Update()
    {
        if (uiOpen && useFakeCursorIfNeeded && fakeCursor != null && fakeCursor.gameObject.activeSelf)
        {
            fakeCursor.position = Input.mousePosition;
        }
    }

    // ========== INTERACTION SYSTEM ==========

    void Hovering(Vector3 hitPoint)
    {
        if (uiOpen) return;
        if (inter != null) inter.message = hoverPrompt;
    }

    void UnHover()
    {
        if (uiOpen) return;
        if (inter != null) inter.message = "";
    }

    void Interacting()
    {
        OpenUI();
    }

    // ========== UI CONTROL ==========

    public void OpenUI()
    {
        if (uiOpen) return;
        uiOpen = true;

        if (inter != null) inter.message = "";

        if (keypadCanvasRoot != null) keypadCanvasRoot.SetActive(true);
        keypadUI?.ResetInput();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        DisableControlsByName();

        if (useFakeCursorIfNeeded && fakeCursor != null)
        {
            fakeCursor.gameObject.SetActive(true);
            fakeCursor.position = Input.mousePosition;
        }
    }

    public void CloseUI()
    {
        if (!uiOpen) return;
        uiOpen = false;

        if (keypadCanvasRoot != null) keypadCanvasRoot.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (fakeCursor != null) fakeCursor.gameObject.SetActive(false);

        RestoreControls();
    }

    void DisableControlsByName()
    {
        disabledDuringUI.Clear();
        if (controlsRoot == null || scriptTypeNamesToDisable == null || scriptTypeNamesToDisable.Count == 0)
            return;

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

    void RestoreControls()
    {
        foreach (var mb in disabledDuringUI)
        {
            if (mb != null) mb.enabled = true;
        }
        disabledDuringUI.Clear();
    }

    // ========== VALIDATION ==========

    public void TrySubmit(string entered)
    {
        if (entered == correctCode)
            StartCoroutine(SuccessRoutine());
        else
            StartCoroutine(FailRoutine());
    }

    IEnumerator SuccessRoutine()
    {
        SetKeypadColor(successColor);
        if (sfxSource && successSfx) sfxSource.PlayOneShot(successSfx);

        if (targetToUnlock != null && makeTargetInteractableByTag)
            targetToUnlock.tag = interactableTagName;

        // Remet la couleur après un temps indépendant
        StartCoroutine(ResetColorAfter(colorFeedbackTime));

        // Ferme l'UI plus vite (ou instant si uiCloseDelay=0)
        yield return new WaitForSeconds(uiCloseDelay);
        CloseUI();
    }

    IEnumerator FailRoutine()
    {
        SetKeypadColor(failColor);
        if (sfxSource && failSfx) sfxSource.PlayOneShot(failSfx);

        StartCoroutine(ResetColorAfter(colorFeedbackTime));

        keypadUI?.ResetInput();

        yield return new WaitForSeconds(uiCloseDelay);
        CloseUI();
    }

    IEnumerator ResetColorAfter(float t)
    {
        yield return new WaitForSeconds(t);
        SetKeypadColor(idleColor);
    }

    void SetKeypadColor(Color c)
    {
        if (mats == null) return;
        foreach (var m in mats)
        {
            if (m.HasProperty("_Color")) m.color = c;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", c);
        }
    }
}



