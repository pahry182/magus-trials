using System.Collections;
using TMPro;
using UnityEngine;

public enum UnitType
{
    Human,
    Beast,
    Fiend,
    COUNT
}

public enum Element
{
    Neutral,
    Fire,
    Water,
    Lightning,
    Earth,
    Wind,
    Ice,
    Light,
    Dark,
    COUNT
}

public enum UnitAnimState
{
    idle,
    moving,
    attacking,
    special,
    stunned,
    frozen
}

public class UnitBase : MonoBehaviour
{
    private Rigidbody2D _rb;
    [HideInInspector] public UnitAI _UnitAI;
    [HideInInspector] public SpriteRenderer _sr;
    [HideInInspector] public TextMeshPro _levelText;

    [Header("Basic Stat")]
    public string unitName = "Amia";
    public int unitLevel = 1;
    public float currentHp = 100f;
    public float currentShd = 0f;
    public float currentMp = 100f;
    public float currentXp;
    public float maxHp = 100f;
    public float maxShd = 100f;
    public float maxMp = 100f;
    public float maxXp = 100f;
    public float manaRegen = 0;
    public float att = 10f;
    public float critChance = 10;
    public float critMultiplier = 0.5f;
    public float def = 2f;
    public float spellRes = 0.15f;
    public float movSpeed = 1f;
    public float attSpeed = 1f;

    [Header("Growth Stat")]
    public float growthHp;
    public float growthMp;
    public float growthAtt;
    public float growthDef;

    [Header("Advanced Stat")]
    public UnitType unitType = UnitType.Human;
    public Element unitElement = Element.Neutral;
    public bool isBoss;
    public float humanKiller = 0f;
    public float beastKiller = 0f;
    public float fiendKiller = 0f;

    [Header("Elemental Affinity")]
    public float fireAtt;
    public float waterAtt;
    public float lightningAtt;
    public float earthAtt;
    public float windAtt;
    public float iceAtt;
    public float lightAtt;
    public float darkAtt;
    [Header("Elemental Resistances")]
    public float fireRes;
    public float waterRes;
    public float lightningRes;
    public float earthRes;
    public float windRes;
    public float iceRes;
    public float lightRes;
    public float darkRes;

    [Header("Functionality Stat")]
    public UnitAnimState unitState = UnitAnimState.idle;
    public bool isUnitDead;
    public bool isManastriking;
    public bool isMysticFielding;
    public float attCooldown;
    public float stunDuration;
    public float frozenDuration;
    public Element affectedByElement;
    public Element affectedBySecondElement;
    [HideInInspector] public bool isAttack = false;

    //def/att /2 *10 = total taken damage
    

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _UnitAI = GetComponent<UnitAI>();
        _sr = GetComponentInChildren<SpriteRenderer>();
        _levelText = GetComponentInChildren<TextMeshPro>();
    }

    private void Update()
    {
        if (attCooldown > 0)
        {
            attCooldown -= Time.deltaTime;
        }
        if (stunDuration > 0)
        {
            stunDuration -= Time.deltaTime;
            unitState = UnitAnimState.stunned;
        }
        else if (frozenDuration > 0)
        {
            frozenDuration -= Time.deltaTime;
            unitState = UnitAnimState.frozen;
        }
        else if (attCooldown <= 0 && unitState != UnitAnimState.special)
        {
            unitState = UnitAnimState.idle;
        }
        
        _levelText.text = "Lv. " + unitLevel;
        if (currentMp < maxMp && manaRegen != 0 && !isUnitDead)
        {
            currentMp += Time.deltaTime * manaRegen;
        }
    }

    public void Attack()
    {
        if (attCooldown <= 0 && !isUnitDead && unitState == UnitAnimState.attacking)
        {
            attCooldown = attSpeed;
            Jump();
            DealDamage(att);
        }
    }

    public void Cast()
    {
        StartCoroutine(Casting());
    }

    private IEnumerator Casting()
    {
        unitState = UnitAnimState.special;
        GetComponentInChildren<Animator>().SetBool("isUnitSpecial", true);
        yield return new WaitForSeconds(0.4f);
        GetComponentInChildren<Animator>().SetBool("isUnitSpecial", false);
        unitState = UnitAnimState.idle;
    }

    public void Jump()
    {
        _rb.AddForce(transform.up * 4, ForceMode2D.Impulse);
    }

    public void DealDamage(float amount, bool isSpellDamage = false, Element _spellElementType = Element.Neutral)
    {
        UnitBase target = _UnitAI.target;
        amount = CalculateKillers(amount, target);
        if (isSpellDamage)
        {
            amount = CalculateElementalRelation(amount, _spellElementType, target);
            amount -= Mathf.Round(amount * target.spellRes);
        }
        else
        {
            if (target.def < 0)
            {
                float targetDamageIncrease = 2 - Mathf.Pow(0.94f, -target.def);
                amount += Mathf.Round(amount * (targetDamageIncrease - 1));
            }
            else
            {
                const float CReduction = 0.06f;
                float targetDamageReduction = (CReduction * target.def) / (1 + (CReduction * target.def));
                amount -= Mathf.Round(amount * targetDamageReduction);
            }
        }
        
        if (target.currentShd >= amount)
        {
            target.currentShd -= amount;
        }
        else if (target.currentShd < amount && target.currentShd > 0)
        {
            target.currentHp -= (amount - target.currentShd);
            target.currentShd = 0;
        }
        else
        {
            target.currentHp -= amount;
        }
        if (target.currentHp < 1)
        {
            DeclareDeath(target);
        }
        if (isManastriking) isManastriking = false;

        GameObject temp = Instantiate(GameManager.Instance._specialEffects[4], target.transform.position, Quaternion.identity);
        temp.GetComponentInChildren<TextMeshPro>().text = Mathf.Round(amount).ToString();
        Destroy(temp, 2f);
        GameManager.Instance.StatisticTrackDamageDealt(amount, gameObject);
    }

    private float CalculateKillers(float _amount, UnitBase _target)
    {
        switch (_target.unitType)
        {
            case UnitType.Human:
                _amount += _amount * humanKiller;
                break;
            case UnitType.Beast:
                _amount += _amount * beastKiller;
                break;
            case UnitType.Fiend:
                _amount += _amount * fiendKiller;
                break;
            case UnitType.COUNT:
                break;
            default:
                break;
        }
        return _amount;
    }

    private float CalculateElementalRelation(float _amount, Element _spellElementType, UnitBase _target)
    {
        switch (_spellElementType)
        {
            case Element.Fire:
                _amount += _amount * (fireAtt - _target.fireRes);
                break;
            case Element.Water:
                _amount += _amount * (waterAtt - _target.waterRes);
                break;
            case Element.Lightning:
                _amount += _amount * (lightningAtt - _target.lightningRes);
                break;
            case Element.Earth:
                _amount += _amount * (earthAtt - _target.earthRes);
                break;
            case Element.Wind:
                _amount += _amount * (windAtt - _target.windRes);
                break;
            case Element.Ice:
                _amount += _amount * (iceAtt - _target.iceRes);
                break;
            case Element.Light:
                _amount += _amount * (lightAtt - _target.lightRes);
                break;
            case Element.Dark:
                _amount += _amount * (darkAtt - _target.darkRes);
                break;
        }
        return _amount;
    }

    public void InitiateApplyElement(Element _element)
    {
        StartCoroutine(_UnitAI.target.ApplyElement(_element));
        _UnitAI.target.CheckElementalReaction();
    }

    public void CheckElementalReaction()
    {
        if ((affectedByElement == Element.Lightning && affectedBySecondElement == Element.Water) ||
            (affectedByElement == Element.Water && affectedBySecondElement == Element.Lightning))
        {
            affectedByElement = Element.Neutral;
            affectedBySecondElement = Element.Neutral;
            StartCoroutine(Electrocuted());
        }
        if ((affectedByElement == Element.Fire && affectedBySecondElement == Element.Wind) ||
            (affectedByElement == Element.Wind && affectedBySecondElement == Element.Fire))
        {
            affectedByElement = Element.Neutral;
            affectedBySecondElement = Element.Neutral;
            StartCoroutine(Wildfire());
        }
        if ((affectedByElement == Element.Ice && affectedBySecondElement == Element.Earth) ||
            (affectedByElement == Element.Earth && affectedBySecondElement == Element.Ice))
        {
            affectedByElement = Element.Neutral;
            affectedBySecondElement = Element.Neutral;
            StartCoroutine(Geocrust());
        }
    }

    private IEnumerator Electrocuted()
    {
        int instance = 5;
        for (int i = 0; i < instance; i++)
        {
            yield return new WaitForSeconds(1f);
            GameManager.Instance.PlaySfx("Lightning Bolt");
            Destroy(Instantiate(GameManager.Instance._specialEffects[1], transform.position, Quaternion.identity), 1f);
            _UnitAI.target.DealDamage(25, true, Element.Lightning);
        }
    }
    private IEnumerator Wildfire()
    {
        yield return new WaitForSeconds(0f);
        GameManager.Instance.PlaySfx("Fire Burst");
        Destroy(Instantiate(GameManager.Instance._specialEffects[2], transform.position, Quaternion.identity), 1f);
        _UnitAI.target.DealDamage(125, true, Element.Fire);
    }

    private IEnumerator Geocrust()
    {
        yield return new WaitForSeconds(0f);
        GameManager.Instance.PlaySfx("Stone Solidify");
        Destroy(Instantiate(GameManager.Instance._specialEffects[2], _UnitAI.target.transform.position, Quaternion.identity), 1f);
        _UnitAI.target.currentShd += 100;
    }

    private IEnumerator ApplyElement(Element _element)
    {
        bool isSecondSlot = false;
        if (affectedByElement == Element.Neutral)
        {
            affectedByElement = _element;
        }
        else
        {
            affectedBySecondElement = _element;
            isSecondSlot = true;
        }
        yield return new WaitForSeconds(5f);
        if (isSecondSlot)
        {
            affectedBySecondElement = Element.Neutral;
        }
        else
        {
            affectedByElement = Element.Neutral;
        }
    }

    public void DeclareDeath(UnitBase target)
    {
        target.currentHp = 0;
        target.isUnitDead = true;
        target.GetComponentInChildren<Animator>().SetBool("isUnitDead", true);
        int exp;
        if (target.isBoss)
        {
            exp = (int)((Random.Range(45, 75) * (0.65 * target.unitLevel)) + target.unitLevel) * 2;
        } 
        else
        {
            exp = (int)((Random.Range(25, 55) * (0.6 * target.unitLevel)) + target.unitLevel) * 2;
        }
        foreach (var character in GameManager.Instance._characterSlot)
        {
            character.GainExp(exp / 2);
        }
        GameManager.Instance.StatisticTrackKill(target, gameObject);
        GameManager.Instance.CheckDeath(target.gameObject);

    }

    public void GainExp(int amount)
    {
        currentXp += amount;
        while (currentXp >= maxXp)
        {
            currentXp -= maxXp;
            LevelUp();
        }
    }

    public void LevelUp()
    {
        maxXp = (int)((30 + maxXp) * 1.02);
        maxHp += growthHp;
        maxMp += growthMp;
        att += growthAtt;
        if (unitLevel % 4 == 0)
        {
            def += growthDef;
            att += growthAtt - 1;
        }
        unitLevel += 1;
    }

    public void ReviveUnit()
    {
        currentHp = maxHp;
        currentMp = maxMp;
        isUnitDead = false;
        GetComponentInChildren<Animator>().SetBool("isUnitDead", false);
    }
}
