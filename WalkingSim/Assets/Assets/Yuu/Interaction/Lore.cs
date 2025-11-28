
using UnityEngine;


public class Lore : MonoBehaviour
{
    public GameObject TextLore;
    private bool isLore;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isLore = true;
            Debug.Log("c'est bon t'es dans la zone pelo");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isLore = false;
            TextLore.SetActive(false);
            Debug.Log("ah tie plus la");
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (isLore && Input.GetKeyDown(KeyCode.E))
        {
            TextLore.SetActive(!TextLore.activeSelf);
            if (TextLore.activeSelf)
            {
                Debug.Log("Texte affiché");
            }
            else
                Debug.Log("Texte masqué");
        }
    }
}
