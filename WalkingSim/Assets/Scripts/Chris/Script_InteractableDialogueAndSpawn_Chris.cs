using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InteractableDialogueSpawnFade : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueData dialogue;
    public DialogueUI dialogueUI;
    public bool triggerOnce = true;

    [Header("APPARITION")]
    public bool spawnObject = false;
    public bool usePrefab = false;
    public GameObject prefabToSpawn;       
    public Transform spawnPoint;           
    public GameObject objectToReveal;      
    public bool fadeInOnReveal = true;
    public float fadeInDuration = 0.6f;

    [Header("DISPARITION")]
    public bool hideObject = false;
    public GameObject objectToHide;        
    public bool fadeOutOnHide = true;
    public float fadeOutDuration = 0.6f;
    public bool disableAfterFadeOut = true;

    [Header("Sons")]
    public AudioSource sfxSource;
    public AudioClip appearSfx;
    public AudioClip disappearSfx;

    bool hasTriggered = false;
    
    void Interacting()
    {
        if (triggerOnce && hasTriggered) return;
        hasTriggered = true;
        StartCoroutine(RunSequence());
    }

    IEnumerator RunSequence()
    {
        //  Dialogue
        if (dialogueUI != null && dialogue != null)
            yield return dialogueUI.PlayDialogue(dialogue);

        //  Apparition
        GameObject revealed = null;
        if (spawnObject)
        {
            if (usePrefab && prefabToSpawn != null)
            {
                Vector3 pos = spawnPoint ? spawnPoint.position : transform.position;
                Quaternion rot = spawnPoint ? spawnPoint.rotation : Quaternion.identity;
                revealed = Instantiate(prefabToSpawn, pos, rot);
            }
            else if (objectToReveal != null)
            {
                revealed = objectToReveal;
                revealed.SetActive(true);
            }

            if (revealed != null)
            {
                if (fadeInOnReveal) yield return StartCoroutine(FadeObject(revealed, 0f, 1f, fadeInDuration, setTransparentBefore:true, restoreOpaqueAfter:true));
                if (sfxSource && appearSfx) sfxSource.PlayOneShot(appearSfx);
            }
        }

        //  Disparition
        if (hideObject && objectToHide != null)
        {
            if (fadeOutOnHide)
                yield return StartCoroutine(FadeObject(objectToHide, 1f, 0f, fadeOutDuration, setTransparentBefore:true, restoreOpaqueAfter:false));

            if (sfxSource && disappearSfx) sfxSource.PlayOneShot(disappearSfx);

            if (disableAfterFadeOut)
                objectToHide.SetActive(false);
        }
    }

    // --------- FONDU UTILITAIRE (Standard + URP) ----------
    IEnumerator FadeObject(GameObject go, float fromAlpha, float toAlpha, float duration, bool setTransparentBefore, bool restoreOpaqueAfter)
    {
        if (go == null) yield break;


        var renderers = go.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) yield break;


        List<Material> mats = new List<Material>();
        foreach (var r in renderers)
        {

            mats.AddRange(r.materials);
        }


        if (setTransparentBefore)
            foreach (var m in mats) SetMaterialTransparent(m);


        foreach (var m in mats) SetMaterialAlpha(m, fromAlpha);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(fromAlpha, toAlpha, Mathf.Clamp01(t / duration));
            foreach (var m in mats) SetMaterialAlpha(m, a);
            yield return null;
        }
        foreach (var m in mats) SetMaterialAlpha(m, toAlpha);


        if (restoreOpaqueAfter && toAlpha >= 0.999f)
            foreach (var m in mats) SetMaterialOpaque(m);
    }


    void SetMaterialTransparent(Material m)
    {
        if (m == null) return;
        
        m.SetOverrideTag("RenderType", "Transparent");
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        m.DisableKeyword("_ALPHATEST_ON");
        m.EnableKeyword("_ALPHABLEND_ON");
        m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        m.renderQueue = 3000;
    }

    void SetMaterialOpaque(Material m)
    {
        if (m == null) return;

        m.SetOverrideTag("RenderType", "");
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        m.SetInt("_ZWrite", 1);
        m.DisableKeyword("_ALPHATEST_ON");
        m.DisableKeyword("_ALPHABLEND_ON");
        m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        m.renderQueue = -1; 
    }

    void SetMaterialAlpha(Material m, float a)
    {
        if (m == null) return;
        
        if (m.HasProperty("_Color"))
        {
            Color c = m.color; c.a = a; m.color = c;
        }
        else if (m.HasProperty("_BaseColor"))
        {
            Color c = m.GetColor("_BaseColor"); c.a = a; m.SetColor("_BaseColor", c);
        }
    }
}

