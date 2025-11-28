using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public Transform respawnPoint;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("DeadZone"))
        {
            Debug.Log("Je respawn");
            transform.position = respawnPoint.position;
        }
    }
}