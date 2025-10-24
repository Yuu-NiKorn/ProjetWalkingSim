using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class CubeInteractable : MonoBehaviour
{
    // 🟢 NOUVEAU : événement envoyé au manager
    public event Action<CubeInteractable> onInteracted;

    [Header("Prompts")]
    [TextArea] public string hoverPrompt = "Press E";
    [TextArea] public string dialogueLine = "Tu as interagi avec le cube !";

    [Header("UI de dialogue")]
    public TextMeshProUGUI dialogueLabel;
    public CanvasGroup dialogueGroup;

    [Header("Durées")]
    public float dialogueFade = 0.2f;
    public float cubeFade = 1.0f;

    Interact inter;
    bool dialogueOpen = false;
    bool isFading = false;

    Renderer[] rends;
    Material[][] matsPerRenderer;

    void Awake()
    {
        inter = FindObjectOfType<Interact>();
        rends = GetComponentsInChildren<Renderer>(true);
        matsPerRenderer = new Material[rends.Length][];
        for (int i = 0; i < rends.Length; i++)
            matsPerRenderer[i] = rends[i].materials;

        if (dialogueGroup != null) dialogueGroup.alpha = 0f;
        if (dialogueLabel != null) dialogueLabel.text = "";
    }

    void Hovering(Vector3 hitPoint)
    {
        if (inter != null && !dialogueOpen) inter.message = hoverPrompt;
    }

    void Interacting()
    {
        if (isFading) return;

        if (!dialogueOpen)
        {
            dialogueOpen = true;
            if (inter != null) inter.message = "";
            ShowDialogue(dialogueLine);
        }
        else
        {
            dialogueOpen = false;
            HideDialogue();
            StartCoroutine(FadeOutCubeAndDisable());

            // 🟢 NOTIFIE le manager
            onInteracted?.Invoke(this);
        }
    }

    void UnHover()
    {
        if (!dialogueOpen && inter != null) inter.message = "";
    }

    void ShowDialogue(string text)
    {
        if (dialogueLabel != null) dialogueLabel.text = text;
        if (dialogueGroup != null) StartCoroutine(FadeCanvasGroup(dialogueGroup, dialogueGroup.alpha, 1f, dialogueFade));
    }

    void HideDialogue()
    {
        if (dialogueGroup != null) StartCoroutine(FadeCanvasGroup(dialogueGroup, dialogueGroup.alpha, 0f, dialogueFade));
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / dur);
            yield return null;
        }
        cg.alpha = to;
        if (to == 0f && dialogueLabel != null) dialogueLabel.text = "";
    }

    IEnumerator FadeOutCubeAndDisable()
    {
        isFading = true;
        SetAllMaterialsTransparent();
        float t = 0f;

        while (t < cubeFade)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / cubeFade);
            for (int i = 0; i < matsPerRenderer.Length; i++)
            {
                foreach (var m in matsPerRenderer[i])
                {
                    if (m.HasProperty("_Color"))
                    {
                        Color c = m.color;
                        c.a = a;
                        m.color = c;
                    }
                }
            }
            yield return null;
        }

        gameObject.SetActive(false);
        isFading = false;
    }

    void SetAllMaterialsTransparent()
    {
        foreach (var mats in matsPerRenderer)
        {
            foreach (var m in mats)
            {
                if (m == null) continue;
                m.SetOverrideTag("RenderType", "Transparent");
                m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.SetInt("_ZWrite", 0);
                m.DisableKeyword("_ALPHATEST_ON");
                m.EnableKeyword("_ALPHABLEND_ON");
                m.renderQueue = 3000;

                if (m.HasProperty("_Color"))
                {
                    Color c = m.color;
                    c.a = 1f;
                    m.color = c;
                }
            }
        }
    }
}
