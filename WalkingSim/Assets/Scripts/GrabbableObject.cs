using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
    First Person Interaction Toolkit by Steven Harmon stevenharmongames.com
    Licensed under the MPL 2.0. https://www.mozilla.org/en-US/MPL/2.0/FAQ/
    Please use in your walking sims/horror/adventure/puzzle games! Drop me a line and share what make with it! :)    
 */

[RequireComponent(typeof(AudioSource))]
public class GrabbableObject : MonoBehaviour
{
    [HideInInspector]
    public bool beingCarried = false;
    private Rigidbody rigid;

    [Header("For Picking Up / Inspecting Things - ensure collider, rigidbody, and trigger")]
    [Tooltip("MainCamera -> Empty Game Object, a bit in front")]
    public Transform HoldPos;

    [Tooltip("200 Strong, 50 weak")]
    public float throwSpeed = 200;

    [Tooltip("0 pickup, 1 throw, 2 crash soft, 3 crash hard")]
    public AudioClip[] clips;
    private AudioSource source;

    [Tooltip("Primary renderer of this object")]
    public Renderer rend;
    private Color originColor;
    private bool over = false;

    [Tooltip("Color when hovered over (looked at)")]
    public Color targetColor = Color.yellow;

    private GameObject MainCam;
    private Interact InteractionScript;

    [Tooltip("Grab Object, Throw Object OR n/a")]
    public string[] prompts = { "Grab Object", "" };

    private bool touched = false;
    private float lowPitchRange = .97f;
    private float highPitchRange = 1.5f;
    private float velocityClipSplit = 5f;

    [Tooltip("set to 2 default")]
    public MouseLook[] lookScript;

    private Vector3 originPos;
    private Quaternion originRot;

    [Tooltip("if object is returned to original position, snaps into place")]
    public bool canReturn = true;
    private bool objectReset = true;

    private float distToOrigin;
    private bool muteCollSound = true;

    void Start()
    {
        rigid = gameObject.GetComponent<Rigidbody>();
        MainCam = GameObject.FindWithTag("MainCamera");
        if (MainCam == null)
        {
            MainCam = GameObject.FindObjectOfType<Camera>().gameObject;
        }

        InteractionScript = MainCam.GetComponent<Interact>();
        source = gameObject.GetComponent<AudioSource>();
        originColor = rend.material.color;
        StartCoroutine(SetOriginTrans());
    }

    public void Hovering()
    {
        over = true;
        StartCoroutine(Fadeout());
        if (!beingCarried)
            InteractionScript.message = prompts[0];
    }

    public void Interacting()
    {
        beingCarried = !beingCarried;

        if (!beingCarried)
        {
            rigid.isKinematic = false;
            transform.parent = null;
            rigid.AddForce(MainCam.transform.forward * throwSpeed);
            source.pitch = 1;
            source.clip = clips[1]; // Throw
            source.Play();

            for (int i = 0; i < lookScript.Length; i++)
                lookScript[i].working = true;

            touched = false;
        }
        else
        {
            rigid.isKinematic = true;
            transform.position = HoldPos.position;
            transform.parent = MainCam.transform;
            source.pitch = 1;
            source.clip = clips[0]; // Pickup
            source.Play();
            objectReset = false;
        }

        InteractionScript.message = prompts[1];
        InteractionScript.CrosshairUI.SetActive(false);
    }

    void FixedUpdate()
    {
        if (!beingCarried)
        {
            if (over)
                rend.material.color = Color.Lerp(rend.material.color, targetColor, Time.deltaTime * 4);
            else
                rend.material.color = Color.Lerp(rend.material.color, originColor, Time.deltaTime * 2);
        }
        else
        {
            rend.material.color = originColor;
        }

        if (touched)
        {
            rigid.isKinematic = false;
            transform.parent = null;
            beingCarried = false;

            for (int i = 0; i < lookScript.Length; i++)
                lookScript[i].working = true;

            touched = false;
        }
    }

    void Update()
    {
        if (!beingCarried && canReturn && !objectReset)
        {
            distToOrigin = Vector3.Distance(originPos, transform.position);
            if (distToOrigin <= .25f)
            {
                rigid.isKinematic = true;
                transform.position = originPos;
                transform.rotation = originRot;
                rigid.isKinematic = false;
                objectReset = true;
            }
        }
    }

    void OnTriggerEnter(Collider coll)
    {
        if (!beingCarried)
        {
            source.Stop();
            source.pitch = Random.Range(lowPitchRange, highPitchRange);
            float hitVol = Random.Range(.7f, 1);

            if (!muteCollSound)
            {
                if (rigid.linearVelocity.magnitude < velocityClipSplit)
                    source.PlayOneShot(clips[2], hitVol); // Soft Collision
                else
                    source.PlayOneShot(clips[3], hitVol); // Hard Collision
            }

            touched = true;
        }
    }

    private IEnumerator Fadeout()
    {
        yield return new WaitForSeconds(1);
        over = false;
    }

    private IEnumerator SetOriginTrans()
    {
        yield return new WaitForSeconds(.25f);
        originPos = transform.position;
        originRot = transform.rotation;
        yield return new WaitForSeconds(.75f);
        muteCollSound = false;
    }
}
