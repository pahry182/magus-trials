using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonBones;

public class TestDragonbones : MonoBehaviour
{
    [SerializeField] private UnityArmatureComponent player;
    [SerializeField] private UnitAI _thisUnitAI;
    [SerializeField] private UnitBase _thisUnit;
    void Start()
    {
        
    }

    void Update()
    {
        if(_thisUnitAI.unitDir < 0)
        {
            player._armature.flipX = true;
        }
        if (_thisUnitAI.unitDir > 0)
        {
            player._armature.flipX = false;
        }

        if(_thisUnit.unitState == UnitAnimState.attacking)
        {
            player.animation.Play("amia_attack_1");
        }
    }
}
