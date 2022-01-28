using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatRandomizer : MonoBehaviour
{
    private UnitBase _ub;
    public Color changable;

    private void Awake()
    {
        _ub = GetComponent<UnitBase>();
    }

    private void Start()
    {
        RandomizeAdvancedStat();
        RandomizeStat();
    }

    private void RandomizeStat()
    {
        int wave = GameManager.Instance.currentWave;
        int designedLevel;
        float ratio;
        //Check Type, Decide Ratio
        if (_ub.isBoss)
        {
            ratio = Random.Range(1.5f, 1.8f);
        }
        else
        {
            ratio = Random.Range(0.8f, 1.2f);
        }
        //Apply Ratio
        designedLevel = (int)(ratio * wave);
        for (int i = 0; i < designedLevel; i++)
        {
            _ub.LevelUp();
        }
        //Boss Double Stat Ratio
        if (_ub.isBoss)
        {
            _ub.spellRes = 0.25f;
            _ub.maxHp *= ratio;
            _ub.maxMp *= ratio;
            _ub.att *= ratio;
            _ub.def *= ratio;
        }
        _ub.currentHp = _ub.maxHp;
        _ub.currentMp = _ub.maxMp;
    }

    private void RandomizeAdvancedStat()
    {
        //int select = Random.Range(0, (int)UnitType.COUNT);
        //for (int i = 0; i <= (int)UnitType.COUNT; i++)
        //{
        //    print(i);
        //}
        int select = Random.Range(1, (int)Element.COUNT);
        _ub.unitElement = (Element)select;
        switch (_ub.unitElement)
        {
            case Element.Fire:
                _ub.waterRes -= GameManager.Instance.elementalRelationPoint;
                _ub.iceRes += GameManager.Instance.elementalRelationPoint;
                _ub.fireAtt += GameManager.Instance.elementalProwessPoint;
                _ub._sr.material.color = new Color32(255, 0, 0, 255);
                break;
            case Element.Water:
                _ub.lightningRes -= GameManager.Instance.elementalRelationPoint;
                _ub.fireRes += GameManager.Instance.elementalRelationPoint;
                _ub.waterAtt += GameManager.Instance.elementalProwessPoint;
                _ub._sr.material.color = new Color32(0, 0, 255, 255);
                break;
            case Element.Lightning:
                _ub.earthRes -= GameManager.Instance.elementalRelationPoint;
                _ub.waterRes += GameManager.Instance.elementalRelationPoint;
                _ub.lightningAtt += GameManager.Instance.elementalProwessPoint;
                _ub._sr.material.color = new Color32(255, 0, 255, 255);
                break;
            case Element.Earth:
                _ub.windRes -= GameManager.Instance.elementalRelationPoint;
                _ub.lightningRes += GameManager.Instance.elementalRelationPoint;
                _ub.earthAtt += GameManager.Instance.elementalProwessPoint;
                _ub._sr.material.color = new Color32(240, 230, 16, 255);
                break;
            case Element.Wind:
                _ub.iceRes -= GameManager.Instance.elementalRelationPoint;
                _ub.earthRes += GameManager.Instance.elementalRelationPoint;
                _ub.windAtt += GameManager.Instance.elementalProwessPoint;
                _ub._sr.material.color = new Color32(0, 255, 0, 255);
                break;
            case Element.Ice:
                _ub.fireRes -= GameManager.Instance.elementalRelationPoint;
                _ub.windRes += GameManager.Instance.elementalRelationPoint;
                _ub.iceAtt += GameManager.Instance.elementalProwessPoint;
                _ub._sr.material.color = new Color32(0, 255, 255, 255);
                break;
            case Element.Light:
                _ub.darkRes -= GameManager.Instance.elementalRelationPoint;
                _ub.lightAtt += GameManager.Instance.elementalProwessPoint;
                _ub._sr.material.color = new Color32(255, 255, 255, 255);
                break;
            case Element.Dark:
                _ub.lightRes -= GameManager.Instance.elementalRelationPoint;
                _ub.darkAtt += GameManager.Instance.elementalProwessPoint;
                _ub._sr.material.color = new Color32(0, 0, 0, 255);
                break;
            default:
                break;
        }
    }
}
