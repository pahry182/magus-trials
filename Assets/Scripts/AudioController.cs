using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] private GameObject SfxOnButton, SfxOffButton, BgmOnButton, BgmOffButton;
    
    public void SFXOnButton()
    {
        GameManager.Instance.ToggleEffects(true);

        SfxOnButton.SetActive(false);
        SfxOffButton.SetActive(true);
    }

    public void SFXOffButton()
    {
        GameManager.Instance.ToggleEffects(false);

        SfxOnButton.SetActive(true);
        SfxOffButton.SetActive(false);
    }

    public void BGMOnButton()
    {
        GameManager.Instance.ToggleMusic(true);

        BgmOnButton.SetActive(false);
        BgmOffButton.SetActive(true);
    }

    public void BGMOffButton()
    {
        GameManager.Instance.ToggleMusic(false);

        BgmOnButton.SetActive(true);
        BgmOffButton.SetActive(false);
    }
}
