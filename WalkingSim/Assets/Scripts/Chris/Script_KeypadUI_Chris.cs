using UnityEngine;
using TMPro;

public class Script_KeypadUI_Chris : MonoBehaviour
{
    [HideInInspector] public KeypadInteractable owner;

    [Header("Affichage")]
    public TextMeshProUGUI screenText;
    public int maxDigits = 4;

    [Header("Sons")]
    public AudioSource sfxSource;
    public AudioClip buttonClickSfx;

    private string current = "";

    void OnEnable()
    {
        ResetInput();
    }

    public void PressDigit(int d)
    {
        PlayClickSound();

        if (current.Length >= maxDigits) return;
        current += d.ToString();
        RefreshScreen();
    }

    public void Validate()
    {
        PlayClickSound();
        owner?.TrySubmit(current);
    }

    public void Back()
    {
        PlayClickSound();
        owner?.CloseUI();
    }

    public void ResetInput()
    {
        current = "";
        RefreshScreen();
    }

    private void RefreshScreen()
    {
        if (screenText != null)
            screenText.text = current;
    }

    private void PlayClickSound()
    {
        if (sfxSource != null && buttonClickSfx != null)
            sfxSource.PlayOneShot(buttonClickSfx);
    }
}