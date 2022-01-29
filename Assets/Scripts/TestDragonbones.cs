using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DragonBones;

public class TestDragonbones : MonoBehaviour
{
    [SerializeField] private UnityArmatureComponent player;
    private UnitBase _thisUnit;
    private UnitAI _thisUnitAI;
    private bool isAttacking;

    private void Awake()
    {
        _thisUnit = GetComponent<UnitBase>();
        _thisUnitAI = GetComponent<UnitAI>();
    }
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
            AttackAnim();
        }

        if (_thisUnit.unitState == UnitAnimState.idle)
        {
            IdleAnim();
        }

        if (_thisUnit.unitState == UnitAnimState.moving)
        {
            
            MoveAnim();
        }
    }

    private void AttackAnim()
    {
        //if (!isAttacking)
        //{
        //    player.animation.Play("amia_attack_1", 1);
        //    isAttacking = true;
        //}
        //if (_thisUnit.attCooldown <= 0)
        //{
        //    isAttacking = false;
        //}
        if (player.animation.lastAnimationName != "amia_attack_1")
        {
            player.animation.Reset();
        }
        if (!player.animation.isPlaying)
        {
            player.animation.Play("amia_attack_1", 1);
            print("pong1");
        }
    }

    private void IdleAnim()
    {
        if (player.animation.lastAnimationName != "amia_idle_1")
        {
            player.animation.Reset();
        }
        if (!player.animation.isPlaying)
        {
            player.animation.Play("amia_idle_1", 1);
            print("pong2");
        }
    }
    
    private void MoveAnim()
    {
        if (player.animation.lastAnimationName != "amia_walk_fast")
        {
            player.animation.Reset();
        }
        //player.animation.Reset()
        if (!player.animation.isPlaying)
        {
            player.animation.Play("amia_walk_fast", 1);
            print("pong3");
        }
    }
}
