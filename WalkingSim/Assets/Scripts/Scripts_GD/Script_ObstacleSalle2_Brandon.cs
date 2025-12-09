using UnityEngine;
public class Script_ObstacleSalle2_Brandon : MonoBehaviour
{
    public int requiredPresses = 2;
    private int currentPresses = 0;

    public void AddPress()
    {
        currentPresses++;
        Debug.Log("Bouton activé. Total = " + currentPresses);

        if (currentPresses >= requiredPresses)
        {
            OpenDoor();
        }
    }
    private void OpenDoor()
    {
        Debug.Log("Porte ouverte !");
        gameObject.SetActive(false);
    }
}
