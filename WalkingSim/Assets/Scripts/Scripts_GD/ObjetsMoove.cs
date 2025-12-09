using UnityEngine;

public class VerticalMove : MonoBehaviour
{
    public float amplitude = 1f;   
    public float speed = 2f;       

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float y = Mathf.Sin(Time.time * speed) * amplitude;
        transform.position = startPos + new Vector3(0, y, 0);
    }
}