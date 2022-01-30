using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Linq;

public class MainSceneController : UIController
{
    public CanvasGroup backgroundPanel;
    public CanvasGroup startMenuPanel;
    public CanvasGroup ingamePanel;
    public CanvasGroup losePanel;
    public CanvasGroup resultTab;
    public TextMeshProUGUI defeatText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI lifeText;
    public TextMeshProUGUI checkpointText;
    public TextMeshProUGUI slotText;
    public TextMeshProUGUI resultWaveText;
    public TextMeshProUGUI resultLifeText;
    public TextMeshProUGUI resultCheckpointText;
    public GameObject bossAlertGO;
    public GameObject[] characterSelection;
    public Button RespawnButton;
    public PlayerSpell activeSpellSet;
    public Image[] cdFillings;
    public Image[] manaFillings;
    public Image switchCdFill;

    public float cdSwitch;
    public float currentCdSwitch;

    [SerializeField] private Transform DamagePopUp;
    [SerializeField] private CanvasGroup SettingPanel;

    private void Awake()
    {
        backgroundPanel.gameObject.SetActive(true);
        startMenuPanel.gameObject.SetActive(true);
        ingamePanel.gameObject.SetActive(false);
        losePanel.gameObject.SetActive(false);
    }

    private void Start()
    {
        foreach (var item in GameManager.Instance._characterPool)
        {
            item.gameObject.SetActive(false);
        }
        ChangeSlot(0);
        ChangeSlot(1);
    }

    private void Update()
    {
        if (GameManager.Instance.currentWave % 4 == 0 && GameManager.Instance.currentWave != 0)
        {
            waveText.text = GameManager.Instance.currentWave.ToString();
        }
        else
        {
            waveText.text = GameManager.Instance.currentWave.ToString();
        }
        lifeText.text = GameManager.Instance.currentLife.ToString();
        checkpointText.text = GameManager.Instance.currentCheckpoint.ToString();
        UpdateBatchCdFill();
        if (currentCdSwitch > 0)
        {
            currentCdSwitch -= Time.deltaTime;
            switchCdFill.fillAmount = currentCdSwitch / cdSwitch;
        }
        
    }

    private IEnumerator StartGame()
    {
        StartCoroutine(FadeOut(backgroundPanel, 1f));
        StartCoroutine(FadeOut(startMenuPanel, 0.4f));
        StartCoroutine(FadeIn(ingamePanel, 1f));
        //GameManager.Instance.PlayBgm("Battle_Normal");
        StartCoroutine(GameManager.Instance._enemySpawnManager.SpawnEnemy());
        GameManager.Instance.InitiateCharacter();
        SetFillings();
        //GameManager.Instance.currentLife = GameManager.Instance.setLife;

        yield return new WaitForSeconds(1f);
        
        
        GameManager.Instance.isBattleStarted = true;
    }

    public void SwitchCharacterButton()
    {
        if (currentCdSwitch <= 0)
        {
            currentCdSwitch = cdSwitch;
            switchCdFill.fillAmount = 1f;
            GameManager.Instance.SwitchActiveCharacter();
            SetFillings();
        }
    }

    private void SetFillings()
    {
        activeSpellSet = GameManager.Instance._activeCharacter.GetComponent<PlayerSpell>();
        for (int i = 0; i < activeSpellSet.spellList.Length; i++)
        {
            activeSpellSet.spellList[i].SetImageFilling(manaFillings[i], cdFillings[i]);
        }
        SetSpellButton();
    }

    private void SetSpellButton()
    {
        foreach (var item in manaFillings)
        {
            item.gameObject.SetActive(false);
        }

        foreach (var item in activeSpellSet.ownedSpell)
        {
            switch (item)
            {
                case "Fire Burst":
                    manaFillings[0].gameObject.SetActive(true);
                    break;
                case "Water Jet-Shot":
                    manaFillings[1].gameObject.SetActive(true);
                    break;
                case "Lightning Bolt":
                    manaFillings[2].gameObject.SetActive(true);
                    break;
                case "Stone Solidify":
                    manaFillings[3].gameObject.SetActive(true);
                    break;
                case "Wind Slash":
                    manaFillings[4].gameObject.SetActive(true);
                    break;
                case "Frost Nova":
                    manaFillings[5].gameObject.SetActive(true);
                    break;
                case "Illuminate":
                    manaFillings[6].gameObject.SetActive(true);
                    break;
                case "Unholy Judgement":
                    manaFillings[7].gameObject.SetActive(true);
                    break;
                default:
                    break;
            }
        }
    }

    private void UpdateBatchCdFill()
    {
        if (activeSpellSet == null) return;
        for (int i = 0; i < activeSpellSet.spellList.Length; i++)
        {
            activeSpellSet.spellList[i].UpdateCdFill(activeSpellSet.sharedCurrentCd, activeSpellSet.sharedCd);
        }
    }

    public void UseSpell(string _name)
    {
        switch (_name)
        {
            case "Fire Burst":
                activeSpellSet.FireBurstButton();
                break;
            case "Water Jet-Shot":
                activeSpellSet.WaterJetShotButton();
                break;
            case "Lightning Bolt":
                activeSpellSet.LightningBoltButton();
                break;
            case "Stone Solidify":
                activeSpellSet.StoneSolidifyButton();
                break;
            case "Wind Slash":
                activeSpellSet.WindSlashButton();
                break;
            case "Frost Nova":
                activeSpellSet.FrostNovaButton();
                break;
            case "Illuminate":
                activeSpellSet.IlluminateButton();
                break;
            case "Unholy Judgement":
                activeSpellSet.UnholyJudgementButton();
                break;
            default:
                break;
        }
    }

    public IEnumerator GameOver()
    {
        GameManager.Instance.isBattleStarted = false;
        GameManager.Instance.PlayBgm("Battle_Defeat");
        StartCoroutine(FadeOut(ingamePanel, 1f));
        StartCoroutine(FadeIn(backgroundPanel, 1f));

        yield return new WaitForSeconds(1f);

        defeatText.text = GenerateDefeatText();
        //resultWaveText.SetActive(false);
        CalculateResult();
        StartCoroutine(FadeIn(losePanel, 0.4f));

        yield return new WaitForSeconds(0.2f);

        //StartCoroutine(FadeIn(resultWaveText.GetComponent<CanvasGroup>(), 0.4f));
    }

    public void StartGameButton()
    {
        if (GameManager.Instance._characterSlot[0] == null || GameManager.Instance._characterSlot[1] == null)
        {
            print("Pasang char di slot 1 tod");
            return;
        }
        StartCoroutine(StartGame());
    }

    private string GenerateDefeatText()
    {
        int chose = Random.Range(0, 3);
        switch (chose)
        {
            case 0:
                return "Bego ah";
            case 1:
                return "Maen yang bener tod";
            default:
                return "Jangan ampe kalah tod";
        }
    }

    public void SurrenderGameButton()
    {
        GameManager.Instance.PlayBgm("Menu_Main");
        StartCoroutine(SurrenderGame());
    }

    IEnumerator SurrenderGame()
    {
        //StartCoroutine(FadeOut(backgroundPanel, 1f));
        StartCoroutine(FadeOut(losePanel, 0.4f));
        StartCoroutine(FadeIn(startMenuPanel, 1f));
        //StartCoroutine(GameManager.Instance._enemySpawnManager.SpawnEnemy());

        yield return new WaitForSeconds(0f);

        //GameManager.Instance.isBattleStarted = true;
        GameManager.Instance.ResetState();
    }

    public void RespawnCheckpointButton()
    {
        
        StartCoroutine(RespawnCheckpoint());
    }


    IEnumerator RespawnCheckpoint()
    {
        StartCoroutine(FadeOut(backgroundPanel, 1f));
        StartCoroutine(FadeOut(losePanel, 0.4f));
        StartCoroutine(FadeIn(ingamePanel, 1f));

        yield return new WaitForSeconds(0f);

        GameManager.Instance.RespawnCheckpoint();
        GameManager.Instance.isBattleStarted = true;
        StartCoroutine(GameManager.Instance._enemySpawnManager.SpawnEnemy());
    }

    public void CalculateResult()
    {
        resultWaveText.text = GameManager.Instance.currentWave.ToString();
        resultLifeText.text = GameManager.Instance.currentLife.ToString();
        resultCheckpointText.text = GameManager.Instance.currentCheckpoint.ToString();
    }

    public void ChangeSlot(int slot)
    {
        UnitBase assignee = null;
        UnitBase[] activeSlot = GameManager.Instance._characterSlot;
        UnitBase[] pool = GameManager.Instance._characterPool;
        bool isAssigned = false;
        assignee = pool[slot];
        for (int i = 0; i < activeSlot.Length; i++)
        {
            if (assignee == activeSlot[i])
            {
                assignee.gameObject.SetActive(false);
                activeSlot[i] = null;
                if (activeSlot[0] == null && activeSlot[1] != null)
                {
                    activeSlot[0] = activeSlot[1];
                    activeSlot[1] = null;
                }
                break;
            }
            if (activeSlot[i] == null && !activeSlot.Contains(assignee))
            {
                activeSlot[i] = assignee;
                isAssigned = true;
                assignee.gameObject.SetActive(true);
                break;
            }
        }

        if (isAssigned)
        {
            characterSelection[slot].SetActive(true);
        }
        else
        {
            characterSelection[slot].SetActive(false);
        }

        UpdateSlot();
    }

    public void UpdateSlot()
    {
        UnitBase[] activeSlot = GameManager.Instance._characterSlot;
        string slot1name = "";
        string slot2name = "";
        if (activeSlot[0] != null) slot1name = activeSlot[0].unitName;
        if (activeSlot[1] != null) slot2name = activeSlot[1].unitName;
        slotText.text = "Slot 1: " + slot1name + " Slot 2: " + slot2name;
    }

    public void testDamagePopUp()
    {
        Transform damagePopUpTransform = Instantiate(DamagePopUp, Vector3.zero, Quaternion.identity);
        DamagePopUp damagePopup = damagePopUpTransform.GetComponent<DamagePopUp>();
        damagePopup.SetupDamagePopUp(999);
    }

    public void CloseSettingPanel()
    {
        StartCoroutine(SettingandClose(SettingPanel, startMenuPanel, 0.3f));
    }

    public void SettingButton()
    {
        StartCoroutine(SettingandClose(startMenuPanel, SettingPanel, 0.3f));
    }

    private IEnumerator SettingandClose(CanvasGroup con1, CanvasGroup con2, float duration)
    {
        StartCoroutine(FadeOut(con1, duration));
        yield return new WaitForSeconds(duration);
        StartCoroutine(FadeIn(con2, duration));
    }
}
