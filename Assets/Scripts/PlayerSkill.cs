using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkill : MonoBehaviour
{
    private UnitBase _ub;
    private Animator _an;

    [Header("Hunter's Instinct")]
    public GameObject specialEffect;
    public Image hunter_image;
    public float additionalBeastKiller = 0.4f;
    public float hunter_manaCost = 36;
    public float hunter_duration = 26;
    public float hunter_cd = 80;
    public float hunter_currentCd;

    [Header("Manastrike")]
    public Image manastrike_image;
    public float additionalFiendKiller = 0.75f;
    public float manastrike_manaCost = 14;
    public float manastrike_cd = 8;
    public float manastrike_currentCd;

    [Header("Mystic Field")]
    public Image mystic_image;
    public float mystic_manaCost = 42;
    public float mystic_duration = 10;
    public float mystic_cd = 30;
    public float mystic_currentCd;

    private void Awake()
    {
        _ub = GetComponent<UnitBase>();
    }

    private void Update()
    {
        if (hunter_currentCd > 0)
        {
            hunter_currentCd -= Time.deltaTime;
            hunter_image.fillAmount -= Time.deltaTime * 1 / hunter_cd;
        }
        if (manastrike_currentCd > 0)
        {
            manastrike_currentCd -= Time.deltaTime;
            manastrike_image.fillAmount -= Time.deltaTime * 1 / manastrike_cd;
        }
        if (mystic_currentCd > 0)
        {
            mystic_currentCd -= Time.deltaTime;
            mystic_image.fillAmount -= Time.deltaTime * 1 / mystic_cd;
        }
    }

    public void HuntersInstinctButton()
    {
        if (hunter_currentCd > 0 || _ub.currentMp < hunter_manaCost) return;
        hunter_image.fillAmount = 1f;
        _ub.currentMp -= hunter_manaCost;
        _ub.Cast();
        StartCoroutine(HuntersInstinct());
    }

    public void ManastrikeButton()
    {
        if (manastrike_currentCd > 0 || _ub.currentMp < manastrike_manaCost) return;
        manastrike_image.fillAmount = 1f;
        _ub.currentMp -= manastrike_manaCost;
        _ub.Cast();
        StartCoroutine(Manastrike());
    }

    public void MysticFieldButton()
    {
        if (mystic_currentCd > 0 || _ub.currentMp < mystic_manaCost) return;
        mystic_image.fillAmount = 1f;
        _ub.currentMp -= mystic_manaCost;
        _ub.Cast();
        StartCoroutine(MysticField());
    }

    IEnumerator HuntersInstinct()
    {
        GameObject _temp = Instantiate(specialEffect, transform.position, Quaternion.identity);
        _temp.GetComponent<TimeLife>().life = 1f;
        _ub.beastKiller += additionalBeastKiller;
        hunter_currentCd = hunter_cd;
        yield return new WaitForSeconds(hunter_duration);

        _ub.beastKiller -= additionalBeastKiller;
    }

    IEnumerator Manastrike()
    {
        GameObject _temp = Instantiate(specialEffect, transform.position, Quaternion.identity);
        _temp.GetComponent<TimeLife>().life = 1f;
        _temp.GetComponent<SpriteRenderer>().material.color = Color.cyan;
        manastrike_currentCd = manastrike_cd;
        _ub.isManastriking = true;
        _ub.fiendKiller += additionalFiendKiller;
        yield return new WaitUntil(() => _ub.isManastriking == false);

        _ub.fiendKiller -= additionalFiendKiller;
    }

    IEnumerator MysticField()
    {
        GameObject _temp = Instantiate(specialEffect, transform.position, Quaternion.identity);
        _temp.GetComponent<TimeLife>().life = 1f;
        _temp.GetComponent<SpriteRenderer>().material.color = Color.white;
        mystic_currentCd = mystic_cd;
        yield return new WaitForSeconds(mystic_duration);

        
    }
}
