using UnityEngine;

public class CarryableObject : MonoBehaviour
{
    public bool beingCarried = false;
    public Rigidbody rigid;
    public Transform HoldPos;
    public Camera MainCam;
    public MouseLook[] lookScript;

    void Start()
    {
        if (rigid == null) rigid = GetComponent<Rigidbody>();
        if (MainCam == null) MainCam = Camera.main;
    }

    void Update()
    {
        if (beingCarried)
        {
            rigid.isKinematic = true;
            transform.position = HoldPos.position;
            transform.parent = MainCam.transform;
        }
        else
        {
            rigid.isKinematic = false;
            transform.parent = null;
        }
    }
    
    public void Interacting()
    {
        beingCarried = !beingCarried;
    }
}