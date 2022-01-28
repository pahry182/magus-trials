using UnityEngine;
using TMPro;
using System.Collections;

public enum UnitType
{
    Human,
    Beast,
    Fiend,
    COUNT
}

public enum UnitElement
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
    public float currentMp = 100f;
    public float currentXp;
    public float maxHp = 100f;
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
    public UnitElement unitElement = UnitElement.Neutral;
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

    public void DealDamage(float amount, bool isSpellDamage = false, UnitElement _spellElementType = UnitElement.Neutral)
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
        
        if (target.currentHp - amount < 1)
        {
            DeclareDeath(target);
        }
        else
        {
            target.currentHp -= amount;
        }

        if (isManastriking) isManastriking = false;

        print(gameObject.tag + " " + amount);
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

    private float CalculateElementalRelation(float _amount, UnitElement _spellElementType, UnitBase _target)
    {
        switch (_spellElementType)
        {
            case UnitElement.Fire:
                _amount += _amount * (fireAtt - _target.fireRes);
                break;
            case UnitElement.Water:
                _amount += _amount * (waterAtt - _target.waterRes);
                break;
            case UnitElement.Lightning:
                _amount += _amount * (lightningAtt - _target.lightningRes);
                break;
            case UnitElement.Earth:
                _amount += _amount * (earthAtt - _target.earthRes);
                break;
            case UnitElement.Wind:
                _amount += _amount * (windAtt - _target.windRes);
                break;
            case UnitElement.Ice:
                _amount += _amount * (iceAtt - _target.iceRes);
                break;
            case UnitElement.Light:
                _amount += _amount * (lightAtt - _target.lightRes);
                break;
            case UnitElement.Dark:
                _amount += _amount * (darkAtt - _target.darkRes);
                break;
        }
        return _amount;
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
