using UnityEngine;
using UnityEngine.UI;

public class InteractableUI : MonoBehaviour
{
    [Header("UI Buttons Panel")]
    public GameObject panelBoutons;

    [Header("Canvases to open")]
    public GameObject canvasA;
    public GameObject canvasB;

    private bool playerIsNear = false;

    void Start()
    {
        panelBoutons.SetActive(false);
        canvasA.SetActive(false);
        canvasB.SetActive(false);
    }

    void Update()
    {
        if (playerIsNear && Input.GetKeyDown(KeyCode.E))
        {
            panelBoutons.SetActive(!panelBoutons.activeSelf);
        }
        
    }

    public void OpenCanvasA()
    {
        canvasA.SetActive(true);
        canvasB.SetActive(false);
        panelBoutons.SetActive(false);
    }

    public void OpenCanvasB()
    {
        canvasB.SetActive(true);
        canvasA.SetActive(false);
        panelBoutons.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerIsNear = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerIsNear = false;
    }
}