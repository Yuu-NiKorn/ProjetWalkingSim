using UnityEngine;

[DisallowMultipleComponent]
public class FollowerOrb : MonoBehaviour
{
    [Header("Cible")]
    public Transform target;                 // le joueur à suivre (drag & drop)
    public Vector3 centerOffset = new Vector3(0f, 1.4f, 0f); // hauteur autour du joueur

    [Header("Orbite")]
    public float radius = 1.6f;              // distance au joueur
    public float orbitSpeedDeg = 60f;        // degrés/s autour du joueur
    public bool clockwise = true;

    [Header("Flottement (bob)")]
    public float bobAmplitude = 0.25f;       // hauteur du flottement
    public float bobFrequency = 1.5f;        // Hz

    [Header("Suivi doux")]
    public float followSmooth = 10f;         // 5–15 = lissage confortable

    [Header("Look")]
    public bool faceTarget = true;           // l'orbe regarde le joueur

    [Header("Lumière (optionnel)")]
    public Light pointLight;
    public float baseIntensity = 2f;
    public float pulseAmplitude = 0.35f;     // 0.0–0.6
    public float pulseFrequency = 2.0f;      // Hz

    float angle;                             // angle courant de l'orbite (en degrés)

    void Reset()
    {
        // auto-assign du Light si présent
        pointLight = GetComponent<Light>();
    }

    void Start()
    {
        if (pointLight == null) pointLight = GetComponentInChildren<Light>();
        // si pas de cible renseignée, on tente Player puis MainCamera
        if (target == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null) target = player.transform;
            else if (Camera.main != null) target = Camera.main.transform;
        }
        if (pointLight != null) pointLight.intensity = baseIntensity;
    }

    void Update()
    {
        if (target == null) return;

        // avance l'angle d'orbite
        float dir = clockwise ? -1f : 1f; // pour un sens visuel "droite" autour du joueur
        angle += dir * orbitSpeedDeg * Time.deltaTime;
        if (angle > 360f || angle < -360f) angle = 0f;

        // position désirée : orbite horizontale + flottement sinusoïdal
        Vector3 center = target.position + centerOffset;
        Quaternion rot = Quaternion.Euler(0f, angle, 0f);
        Vector3 orbitPos = center + rot * (Vector3.forward * radius);
        float bob = Mathf.Sin(Time.time * Mathf.PI * 2f * bobFrequency) * bobAmplitude;
        Vector3 desired = new Vector3(orbitPos.x, orbitPos.y + bob, orbitPos.z);

        // suivi lissé (exponentiel) pour éviter les saccades
        float k = 1f - Mathf.Exp(-followSmooth * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desired, k);

        // regarde le joueur (optionnel)
        if (faceTarget)
        {
            Vector3 lookAt = target.position + centerOffset * 0.6f;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookAt - transform.position), k);
        }

        // pulsation de la lumière (optionnel)
        if (pointLight != null)
        {
            float pulse = 1f + Mathf.Sin(Time.time * Mathf.PI * 2f * pulseFrequency) * pulseAmplitude;
            pointLight.intensity = baseIntensity * pulse;
        }
    }
}
