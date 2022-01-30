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
    [HideInInspector] public ElementAffectedController _eac;

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

    [Header("Attack Animation")]
    public float attSwing = 1f;
    public float attBackSwing = 0.4f;

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
        _eac = GetComponentInChildren<ElementAffectedController>();
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
        else if (attCooldown <= 0 && unitState != UnitAnimState.special && unitState != UnitAnimState.moving)
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
            attCooldown = attSwing + attBackSwing;
            //Jump();
            StartCoroutine(AttackAnimation());
        }
    }

    private IEnumerator AttackAnimation()
    {
        yield return new WaitForSeconds(attSwing);
        DealDamage(att);
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

    public void DealDamage(float amount, bool isSpellDamage = false, Element _spellElementType = Element.Neutral, bool isCrit = false)
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

        int select = Random.Range(0, 100);
        if (select < critChance)
        {
            amount += amount * critMultiplier;
            isCrit = true;
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

        Vector3 pos = target.transform.position;
        pos.y += Random.Range(-0.2f, 0.2f);
        GameObject temp = Instantiate(GameManager.Instance._specialEffects[4], pos, Quaternion.identity);
        temp.GetComponentInChildren<TextMeshPro>().color = ElementalDamageColor(_spellElementType);
        if (isCrit)
        {
            temp.GetComponentInChildren<TextMeshPro>().text = "Crit " + Mathf.Round(amount).ToString();
        }
        else
        {
            temp.GetComponentInChildren<TextMeshPro>().text = Mathf.Round(amount).ToString();
        }
        Destroy(temp, 2f);
        GameManager.Instance.StatisticTrackDamageDealt(amount, gameObject);
    }

    private Color32 ElementalDamageColor(Element _element)
    {
        switch (_element)
        {
            case Element.Fire:
                return new Color32(255, 48, 4, 255);
            case Element.Water:
                return new Color32(50, 110, 255, 255);
            case Element.Lightning:
                return new Color32(237, 20, 241, 255);
            case Element.Earth:
                return new Color32(203, 177, 41, 255);
            case Element.Wind:
                return new Color32(32, 254, 2, 255);
            case Element.Ice:
                return new Color32(0, 255, 232, 255);
            case Element.Light:
                return new Color32(255, 253, 139, 255);
            case Element.Dark:
                return new Color32(69, 69, 69, 255);
            default:
                return new Color32(255, 255, 255, 255);
        }
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
            ResetAffectedElement();
            StartCoroutine(Electrocuted());
        }
        if ((affectedByElement == Element.Fire && affectedBySecondElement == Element.Wind) ||
            (affectedByElement == Element.Wind && affectedBySecondElement == Element.Fire))
        {
            ResetAffectedElement();
            StartCoroutine(Wildfire());
        }
        if ((affectedByElement == Element.Ice && affectedBySecondElement == Element.Earth) ||
            (affectedByElement == Element.Earth && affectedBySecondElement == Element.Ice))
        {
            ResetAffectedElement();
            StartCoroutine(Geocrust());
        }
    }

    private void ResetAffectedElement()
    {
        affectedByElement = Element.Neutral;
        affectedBySecondElement = Element.Neutral;
        foreach (var item in _eac.elementIcon)
        {
            item.gameObject.SetActive(false);
        }
    }
    private IEnumerator Electrocuted()
    {
        int instance = 5;
        Vector3 pos = transform.position;
        pos.y++;
        GameObject temp = Instantiate(GameManager.Instance._specialEffects[4], pos, Quaternion.identity);
        temp.GetComponentInChildren<TextMeshPro>().text = "Electrocuted";
        Destroy(temp, 1f);
        for (int i = 0; i < instance; i++)
        {
            yield return new WaitForSeconds(0.6f);
            GameManager.Instance.PlaySfx("Electrocuted");
            Destroy(Instantiate(GameManager.Instance._specialEffects[1], transform.position, Quaternion.identity), 1f);
            _UnitAI.target.DealDamage(15 + (_UnitAI.target.unitLevel), true, Element.Lightning);
        }
    }
    private IEnumerator Wildfire()
    {
        yield return new WaitForSeconds(0f);
        GameManager.Instance.PlaySfx("Wildfire");
        Vector3 pos = transform.position;
        pos.y++;
        GameObject temp = Instantiate(GameManager.Instance._specialEffects[4], pos, Quaternion.identity);
        temp.GetComponentInChildren<TextMeshPro>().text = "Wildfire";
        Destroy(temp, 2f);
        Destroy(Instantiate(GameManager.Instance._specialEffects[2], transform.position, Quaternion.identity), 1f);
        fireRes -= 0.1f;
        waterRes -= 0.1f;
        lightningRes -= 0.1f;
        earthRes -= 0.1f;
        windRes -= 0.1f;
        iceRes -= 0.1f;
        darkRes -= 0.1f;
        lightRes -= 0.1f;
        _UnitAI.target.DealDamage(_UnitAI.target.unitLevel * 1.2f, true, Element.Fire);
    }

    private IEnumerator Geocrust()
    {
        yield return new WaitForSeconds(0f);
        GameManager.Instance.PlaySfx("Geocrust");
        Vector3 pos = _UnitAI.target.transform.position;
        pos.y++;
        GameObject temp = Instantiate(GameManager.Instance._specialEffects[4], pos, Quaternion.identity);
        temp.GetComponentInChildren<TextMeshPro>().text = "Geocrust";
        Destroy(Instantiate(GameManager.Instance._specialEffects[2], _UnitAI.target.transform.position, Quaternion.identity), 1f);
        _UnitAI.target.currentShd += _UnitAI.target.currentHp * 0.1f;
    }

    private IEnumerator ApplyElement(Element _element)
    {
        string savedElement = "";
        
        if (affectedByElement == Element.Neutral)
        {
            affectedByElement = _element;
            foreach (var item in _eac.elementIcon)
            {
                if(item.gameObject.name == _element.ToString())
                {
                    item.gameObject.SetActive(true);
                    savedElement = item.gameObject.name;
                    break;
                }
            }
        }
        else
        {
            StartCoroutine(ApplytoSecond(_element));
        }

        //print(savedElement + " " + savedSecondElement);

        yield return new WaitForSeconds(5f);

        affectedByElement = Element.Neutral;
        foreach (var item in _eac.elementIcon)
        {
            if (item.gameObject.name == savedElement)
            {
                item.gameObject.SetActive(false);
                savedElement = "";
                break;
            }
        }
    }

    private IEnumerator ApplytoSecond(Element _element)
    {
        string savedSecondElement = "";
        affectedBySecondElement = _element;
        foreach (var item in _eac.elementIcon)
        {
            if (item.gameObject.name == _element.ToString())
            {
                item.gameObject.SetActive(true);
                savedSecondElement = item.gameObject.name;
                break;
            }
        }

        yield return new WaitForSeconds(5f);

        affectedBySecondElement = Element.Neutral;
        foreach (var item in _eac.elementIcon)
        {
            if (item.gameObject.name == savedSecondElement)
            {
                item.gameObject.SetActive(false);
                savedSecondElement = "";
                break;
            }
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
