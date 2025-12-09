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
    public bool spawnAtInteractTime = true;   // ⬅️ NOUVEAU : apparition au moment de l'interaction
    public GameObject prefabToSpawn;       
    public Transform spawnPoint;           
    public GameObject objectToReveal;      // ex : parent "Sang" avec toutes les taches
    public bool fadeInOnReveal = true;
    public float fadeInDuration = 0.6f;

    [System.Serializable]
    public class HideTarget
    {
        [Tooltip("L'objet à faire disparaître")]
        public GameObject objectToHide;

        [Tooltip("Si coché, l'objet disparaît dès l'interaction, sinon après le dialogue")]
        public bool hideOnInteract = false;

        [Header("Fondu")]
        [Tooltip("Utiliser un fondu pour cet objet ?")]
        public bool useFade = true;

        [Tooltip("Durée du fondu pour CET objet")]
        public float fadeDuration = 0.6f;
    }

    [Header("DISPARITION (plusieurs objets)")]
    public bool disableAfterFadeOut = true;
    public List<HideTarget> hideTargets = new List<HideTarget>();

    [Header("Sons")]
    public AudioSource sfxSource;
    public AudioClip appearSfx;
    public AudioClip disappearSfx;

    bool hasTriggered = false;

    void Interacting()
    {
        if (triggerOnce && hasTriggered) return;
        hasTriggered = true;

        // 1) Cacher tout de suite ceux qui doivent disparaître à l'interaction
        if (hideTargets != null)
        {
            foreach (var ht in hideTargets)
            {
                if (ht == null || ht.objectToHide == null) continue;
                if (!ht.hideOnInteract) continue;

                StartCoroutine(HideOneTarget(ht));
            }
        }

        // 2) Apparition dès l'interaction si demandé
        if (spawnObject && spawnAtInteractTime)
        {
            StartCoroutine(SpawnOrRevealObject());
        }

        // 3) Lancer la séquence dialogue + disparitions "après dialogue"
        StartCoroutine(RunSequence());
    }

    IEnumerator RunSequence()
    {
        //  Dialogue
        if (dialogueUI != null && dialogue != null)
            yield return dialogueUI.PlayDialogue(dialogue);

        //  Apparition APRÈS le dialogue (si on n'a pas déjà spawn avant)
        if (spawnObject && !spawnAtInteractTime)
        {
            yield return SpawnOrRevealObject();
        }

        //  Disparition APRÈS dialogue (ceux qui ne sont PAS hideOnInteract)
        if (hideTargets != null)
        {
            foreach (var ht in hideTargets)
            {
                if (ht == null || ht.objectToHide == null) continue;
                if (ht.hideOnInteract) continue; // déjà traités à l'interaction

                yield return StartCoroutine(HideOneTarget(ht));
            }
        }
    }

    // --------- APPARITION / RÉVÉLATION ----------
    IEnumerator SpawnOrRevealObject()
    {
        GameObject revealed = null;

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
            if (fadeInOnReveal)
            {
                yield return StartCoroutine(FadeObject(
                    revealed,
                    fromAlpha: 0f,
                    toAlpha: 1f,
                    duration: fadeInDuration,
                    setTransparentBefore: true,
                    restoreOpaqueAfter: true
                ));
            }

            if (sfxSource && appearSfx)
                sfxSource.PlayOneShot(appearSfx);
        }
    }

    // --------- GÈRE UN SEUL OBJET À CACHER ----------
    IEnumerator HideOneTarget(HideTarget ht)
    {
        if (ht == null || ht.objectToHide == null) yield break;

        GameObject go = ht.objectToHide;

        // Fondu si demandé
        if (ht.useFade)
        {
            yield return StartCoroutine(FadeObject(
                go,
                fromAlpha: 1f,
                toAlpha: 0f,
                duration: ht.fadeDuration,
                setTransparentBefore: true,
                restoreOpaqueAfter: false
            ));
        }

        // Après le fade (ou direct), on désactive ce qu'il faut
        if (disableAfterFadeOut)
        {
            // ⚠️ CAS SPÉCIAL : si c'est l'objet qui porte CE script,
            // on ne désactive pas le GameObject complet sinon toutes les coroutines s'arrêtent.
            if (go == this.gameObject)
            {
                foreach (var r in go.GetComponentsInChildren<Renderer>(true))
                    r.enabled = false;

                foreach (var c in go.GetComponentsInChildren<Collider>(true))
                    c.enabled = false;

                gameObject.tag = "Untagged"; // plus interactable
            }
            else
            {
                go.SetActive(false);
            }
        }

        if (sfxSource && disappearSfx)
            sfxSource.PlayOneShot(disappearSfx);
    }

    // --------- FONDU UTILITAIRE ----------
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
