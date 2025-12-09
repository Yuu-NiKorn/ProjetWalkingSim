using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RoomSelectorInteractable : MonoBehaviour
{
    [Header("Prompt Interact")]
    [TextArea] public string hoverPrompt = "Appuyez sur E pour choisir une pièce";

    [Header("UI")]
    public GameObject roomSelectionCanvasRoot;
    public Button salonButton;
    public Button cuisineButton;
    public Button salleDeBainButton;
    public Button chambreButton;
    public Button backButton;

    public Image salonImage;
    public Image cuisineImage;
    public Image salleDeBainImage;
    public Image chambreImage;

    public Color normalColor = Color.white;
    public Color disabledColor = Color.gray;

    [Header("Points de téléportation")]
    public Transform salonSpawn;
    public Transform cuisineSpawn;
    public Transform salleDeBainSpawn;
    public Transform chambreSpawn;

    [Header("Player")]
    public Transform player;   // ✅ à assigner dans l’inspecteur

    [Header("Bloquer contrôles pendant l'UI")]
    public GameObject controlsRoot;
    public List<string> scriptTypeNamesToDisable = new List<string>();

    private Interact inter;
    private bool uiOpen = false;
    private List<MonoBehaviour> disabledDuringUI = new List<MonoBehaviour>();

    enum RoomId { None, Salon, Cuisine, SalleDeBain, Chambre }

    void Awake()
    {
        inter = FindObjectOfType<Interact>();

        if (roomSelectionCanvasRoot != null)
            roomSelectionCanvasRoot.SetActive(false);

        if (controlsRoot == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) controlsRoot = p;
            else if (Camera.main != null) controlsRoot = Camera.main.gameObject;
        }

        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }

        // On peut garder ces listeners, mais on va surtout les mettre à la main dans l'Inspector
        if (salonButton != null) salonButton.onClick.AddListener(TeleportSalon);
        if (cuisineButton != null) cuisineButton.onClick.AddListener(TeleportCuisine);
        if (salleDeBainButton != null) salleDeBainButton.onClick.AddListener(TeleportSalleDeBain);
        if (chambreButton != null) chambreButton.onClick.AddListener(TeleportChambre);

        if (backButton != null) backButton.onClick.AddListener(CloseUI);
    }

    // ========= INTERACTION VIA TON SCRIPT Interact =========

    void Hovering(Vector3 hitPoint)
    {
        if (uiOpen) return;
        if (DialogueUI.AnyDialoguePlaying) return;

        if (inter != null) inter.message = hoverPrompt;
    }

    void UnHover()
    {
        if (uiOpen) return;
        if (inter != null) inter.message = "";
    }

    void Interacting()
    {
        if (DialogueUI.AnyDialoguePlaying) return;
        OpenUI();
    }

    // ========= GESTION UI =========

    public void OpenUI()
    {
        if (uiOpen) return;
        uiOpen = true;

        if (inter != null) inter.message = "";

        if (roomSelectionCanvasRoot != null)
            roomSelectionCanvasRoot.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        DisableControlsByName();
        UpdateRoomButtons();

        Debug.Log("RoomSelector : UI ouverte.");
    }

    public void CloseUI()
    {
        if (!uiOpen) return;
        uiOpen = false;

        if (roomSelectionCanvasRoot != null)
            roomSelectionCanvasRoot.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        RestoreControls();

        Debug.Log("RoomSelector : UI fermée.");
    }

    void DisableControlsByName()
    {
        disabledDuringUI.Clear();
        if (controlsRoot == null || scriptTypeNamesToDisable == null || scriptTypeNamesToDisable.Count == 0)
            return;

        var all = controlsRoot.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (var mb in all)
        {
            if (mb == null) continue;
            string typeName = mb.GetType().Name;

            for (int i = 0; i < scriptTypeNamesToDisable.Count; i++)
            {
                if (typeName == scriptTypeNamesToDisable[i] && mb.enabled)
                {
                    mb.enabled = false;
                    disabledDuringUI.Add(mb);
                }
            }
        }
    }

    void RestoreControls()
    {
        foreach (var mb in disabledDuringUI)
        {
            if (mb != null) mb.enabled = true;
        }
        disabledDuringUI.Clear();
    }

    // ========= MÉTHODES PUBLIQUES POUR LES BOUTONS =========

    public void TeleportSalon()       => TeleportTo(RoomId.Salon);
    public void TeleportCuisine()     => TeleportTo(RoomId.Cuisine);
    public void TeleportSalleDeBain() => TeleportTo(RoomId.SalleDeBain);
    public void TeleportChambre()     => TeleportTo(RoomId.Chambre);

    // ========= TÉLÉPORTATION =========

    void TeleportTo(RoomId targetRoom)
    {
        // On récupère le player par tag à chaque fois, comme dans ton Teleporter
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO == null)
        {
            Debug.LogWarning("RoomSelectorInteractable : aucun GameObject avec le tag 'Player' trouvé.");
            return;
        }

        // On met aussi à jour la ref 'player' si tu l'utilises ailleurs
        player = playerGO.transform;

        // On chope le CharacterController (même principe que ton script qui marche)
        CharacterController cc = playerGO.GetComponent<CharacterController>();

        Transform target = null;

        switch (targetRoom)
        {
            case RoomId.Salon:       target = salonSpawn; break;
            case RoomId.Cuisine:     target = cuisineSpawn; break;
            case RoomId.SalleDeBain: target = salleDeBainSpawn; break;
            case RoomId.Chambre:     target = chambreSpawn; break;
        }

        if (target == null)
        {
            Debug.LogWarning("RoomSelectorInteractable : pas de Transform assigné pour " + targetRoom);
            return;
        }

        Debug.Log($"RoomSelector : Téléportation vers {targetRoom} à {target.position}");

        // 🔴 Très important : on désactive le CharacterController pendant la TP
        if (cc != null) cc.enabled = false;

        playerGO.transform.position = new Vector3(
            target.position.x,
            target.position.y,
            target.position.z
        );
        playerGO.transform.rotation = target.rotation;

        if (cc != null) cc.enabled = true;

        CloseUI();
    }


    // ========= GESTION DES BOUTONS (GRISER) =========

    RoomId DetectCurrentRoom()
    {
        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (player == null) return RoomId.None;

        float bestDist = float.MaxValue;
        RoomId bestRoom = RoomId.None;

        CheckRoomDistance(salonSpawn, RoomId.Salon, ref bestDist, ref bestRoom);
        CheckRoomDistance(cuisineSpawn, RoomId.Cuisine, ref bestDist, ref bestRoom);
        CheckRoomDistance(salleDeBainSpawn, RoomId.SalleDeBain, ref bestDist, ref bestRoom);
        CheckRoomDistance(chambreSpawn, RoomId.Chambre, ref bestDist, ref bestRoom);

        if (bestDist > 3f)
            return RoomId.None;

        return bestRoom;
    }

    void CheckRoomDistance(Transform spawn, RoomId roomId, ref float bestDist, ref RoomId bestRoom)
    {
        if (spawn == null) return;
        if (player == null) return;

        float d = Vector3.Distance(player.position, spawn.position);
        if (d < bestDist)
        {
            bestDist = d;
            bestRoom = roomId;
        }
    }

    void UpdateRoomButtons()
    {
        RoomId current = DetectCurrentRoom();

        SetButtonState(salonButton, salonImage, true);
        SetButtonState(cuisineButton, cuisineImage, true);
        SetButtonState(salleDeBainButton, salleDeBainImage, true);
        SetButtonState(chambreButton, chambreImage, true);

        switch (current)
        {
            case RoomId.Salon:
                SetButtonState(salonButton, salonImage, false);
                break;
            case RoomId.Cuisine:
                SetButtonState(cuisineButton, cuisineImage, false);
                break;
            case RoomId.SalleDeBain:
                SetButtonState(salleDeBainButton, salleDeBainImage, false);
                break;
            case RoomId.Chambre:
                SetButtonState(chambreButton, chambreImage, false);
                break;
        }

        Debug.Log("RoomSelector : pièce détectée = " + current);
    }

    void SetButtonState(Button btn, Image img, bool interactable)
    {
        if (btn != null) btn.interactable = interactable;
        if (img != null) img.color = interactable ? normalColor : disabledColor;
    }
}

