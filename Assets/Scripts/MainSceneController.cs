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
    public TextMeshProUGUI defeatText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI lifeText;
    public TextMeshProUGUI checkpointText;
    public TextMeshProUGUI slotText;
    public GameObject resultText;
    public Button RespawnButton;
    public PlayerSpell activeSpellSet;
    public Image[] cdFillings;
    public Image[] manaFillings;
    public Image switchCdFill;

    public float cdSwitch;
    public float currentCdSwitch;

    private void Awake()
    {
        backgroundPanel.gameObject.SetActive(true);
        startMenuPanel.gameObject.SetActive(true);
        ingamePanel.gameObject.SetActive(false);
        losePanel.gameObject.SetActive(false);
    }

    private void Start()
    {
        ChangeSlot(0);
        ChangeSlot(1);
    }

    private void Update()
    {
        if (GameManager.Instance.currentWave % 4 == 0 && GameManager.Instance.currentWave != 0)
        {
            waveText.text = "Wave: " + GameManager.Instance.currentWave + " (Boss)";
        }
        else
        {
            waveText.text = "Wave: " + GameManager.Instance.currentWave;
        }
        lifeText.text = "Life: " + GameManager.Instance.currentLife;
        checkpointText.text = "Checkpoint: " + GameManager.Instance.currentCheckpoint;
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
        resultText.SetActive(false);
        CalculateResult();
        StartCoroutine(FadeIn(losePanel, 0.4f));

        yield return new WaitForSeconds(0.2f);

        StartCoroutine(FadeIn(resultText.GetComponent<CanvasGroup>(), 0.4f));
    }

    public void StartGameButton()
    {
        if (GameManager.Instance._characterSlot[0] == null)
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
        resultText.GetComponent<TextMeshProUGUI>().text = "Current Wave: " + GameManager.Instance.currentWave +
            " Current Life: " + GameManager.Instance.currentLife +
            " Current Checkpoint: " + GameManager.Instance.currentCheckpoint;
    }

    public void ChangeSlot(int slot)
    {
        UnitBase assignee = null;
        UnitBase[] activeSlot = GameManager.Instance._characterSlot;
        UnitBase[] pool = GameManager.Instance._characterPool;
        assignee = pool[slot];
        for (int i = 0; i < activeSlot.Length; i++)
        {
            if (assignee == activeSlot[i])
            {
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
                break;
            }
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
}
