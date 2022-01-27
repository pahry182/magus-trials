using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerSpell : MonoBehaviour
{
    [Serializable]
    public class Spell
    {
        public string spellName;
        public Image cdFillImage;
        public Image manaFillImage;
        public UnitElement spellElement;
        public float cooldown;
        public float manaCost;
        public float baseManaCost;
        public float manaIncrement;
        public float damage;
        public float baseDamage;
        public float damageIncrement;

        public void UpdateCooldown(float _sharedCurrentCd, float _sharedCd)
        {
            if (cdFillImage == null) return;
            cdFillImage.fillAmount -= Time.deltaTime * 1 / _sharedCd;
        }

        public void StartCdFill()
        {
            if (cdFillImage == null) return;
            cdFillImage.fillAmount = 1f;
        }

        public void UpdateIncrement(int _level)
        {
            manaCost = baseManaCost + manaIncrement * _level;
            damage = baseDamage + damageIncrement * _level;
        }

        public void UpdateManaCost(float _currentMp)
        {
            if (cdFillImage == null) return;
            if (_currentMp < manaCost)
            {
                manaFillImage.color = new Color32(115, 128, 212, 255);
            }
            else
            {
                manaFillImage.color = new Color32(255, 255, 255, 255);
            }
        }
    }
    private UnitBase _ub;
    private int lastLevel;
    public float sharedCd = 0;
    public float sharedCurrentCd = 0;
    public Spell currentSpell = null;

    public Spell[] spellList;
    public GameObject specialEffect;

    [Header("Fire Burst")]
    public float fireBurst_fireResDown = 0.16f;

    [Header("Water Jet-Shot")]
    public float jet_critChance = 25f;
    public float jet_critDamage = 2.25f;

    [Header("Lightning Bolt")]
    public float bolt_stunChance = 10;
    public float bolt_stunDuration = 1.5f;

    [Header("Stone Solidify")]
    public float solidify_petrifyChance = 8;

    [Header("Wind Slash")]
    public float windSlash_resDown = 0.08f;

    [Header("Frost Nova")]
    public float nova_freezeChance = 25;
    public float nova_freezeDuration = 5f;

    [Header("Iluminate")]
    public float illuminate_defDown = 7;
    public float illuminate_baseDefDown = 7;
    public float illuminate_defDownIncrement = 0.8f;

    [Header("Unholy Judgement")]
    public float judge_hpPercentage = 0.3f;


    private void Awake()
    {
        _ub = GetComponent<UnitBase>();
    }

    private void Update()
    {
        UpdateBatchCooldown();
        UpdateBatchSpellIncrement();
        UpdateBatchManaCost();
    }

    private void UpdateBatchSpellIncrement()
    {
        int level = _ub.unitLevel;
        if(lastLevel == level)
        {
            return;
        }
        else
        {
            lastLevel = _ub.unitLevel;
            for (int i = 0; i < spellList.Length; i++)
            {
                spellList[i].UpdateIncrement(level);
            }
            illuminate_defDown = illuminate_baseDefDown + illuminate_defDownIncrement * level;
        }
    }

    private void UpdateBatchCooldown()
    {
        if (sharedCurrentCd > 0)
        {
            sharedCurrentCd -= Time.deltaTime;
            for (int i = 0; i < spellList.Length; i++)
            {
                spellList[i].UpdateCooldown(sharedCurrentCd, sharedCd);
            }
        }
    }

    private void UpdateBatchManaCost()
    {
        for (int i = 0; i < spellList.Length; i++)
        {
            spellList[i].UpdateManaCost(_ub.currentMp);
        }
    }

    private float StartCooldown()
    {
        for (int i = 0; i < spellList.Length; i++)
        {
            spellList[i].StartCdFill();
        }
        return currentSpell.cooldown;
    }

    private bool CheckSpellCondition(string name)
    {
        for (int i = 0; i < spellList.Length; i++)
        {
            if (name == spellList[i].spellName)
            {
                currentSpell = spellList[i];
                break;
            }
        }
        
        if (_ub.currentMp <= currentSpell.manaCost)
        {
            print(currentSpell.spellName + " No Mana");
        }
        else if (sharedCurrentCd > 0)
        {
            print("Spell Under Cooldown");
        }
        else if (_ub._UnitAI.target == null)
        {
            print("No Target");
        }
        else
        {
            return true;
        }
        return false;
    }

    private void DeliverSpellDamage(float _spellDamageAmount)
    {
        Destroy(Instantiate(specialEffect, _ub._UnitAI.targetPosition.position, Quaternion.identity), 2f);
        _ub.Cast();
        _ub.currentMp -= currentSpell.manaCost;
        _ub.DealDamage(_spellDamageAmount, true, currentSpell.spellElement);
        sharedCd = StartCooldown();
        sharedCurrentCd = sharedCd;
        GameManager.Instance.PlaySfx(currentSpell.spellName);
    }

    public void FireBurstButton()
    {
        if (CheckSpellCondition("Fire Burst"))
        {
            DeliverSpellDamage(currentSpell.damage);
            _ub._UnitAI.target.fireRes -= fireBurst_fireResDown;
        }
    }

    public void WaterJetShotButton()
    {
        if (CheckSpellCondition("Water Jet-Shot"))
        {
            float amount = currentSpell.damage;
            int select = Random.Range(0, 100);
            if (select < jet_critChance)
            {
                amount *= jet_critDamage;
                print(currentSpell.spellName + " Critical");
            }
            DeliverSpellDamage(amount);
        }
    }

    public void LightningBoltButton()
    {
        if (CheckSpellCondition("Lightning Bolt"))
        {
            DeliverSpellDamage(currentSpell.damage);
            int select = Random.Range(0, 100);
            if (select < bolt_stunChance)
            {
                _ub._UnitAI.target.stunDuration += bolt_stunDuration;
                print(currentSpell.spellName + " Stun");
            }
        }
    }

    public void StoneSolidifyButton()
    {
        if (CheckSpellCondition("Stone Solidify"))
        {
            DeliverSpellDamage(currentSpell.damage);
            int select = Random.Range(0, 100);
            if (select < solidify_petrifyChance)
            {
                _ub._UnitAI.target.currentHp = 0f;
                print(currentSpell.spellName + " Petrified");
            }
        }
    }

    public void WindSlashButton()
    {
        if (CheckSpellCondition("Wind Slash"))
        {
            DeliverSpellDamage(currentSpell.damage);
            _ub._UnitAI.target.spellRes -= windSlash_resDown;
        }
    }

    public void FrostNovaButton()
    {
        if (CheckSpellCondition("Frost Nova"))
        {
            DeliverSpellDamage(currentSpell.damage);
            int select = Random.Range(0, 100);
            if (select < nova_freezeChance)
            {
                _ub._UnitAI.target.frozenDuration += nova_freezeDuration;
                print(currentSpell.spellName + " Frozen");
            }
        }
    }

    public void IlluminateButton()
    {
        if (CheckSpellCondition("Illuminate"))
        {
            DeliverSpellDamage(currentSpell.damage);
            _ub._UnitAI.target.def -= illuminate_defDown;
        }
    }

    public void UnholyJudgementButton()
    {
        if (CheckSpellCondition("Unholy Judgement"))
        {
            float amount = currentSpell.damage;
            amount += _ub._UnitAI.target.currentHp * judge_hpPercentage;
            DeliverSpellDamage(amount);
        }
    }
}
