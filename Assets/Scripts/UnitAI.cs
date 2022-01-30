using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Transform = UnityEngine.Transform;

public class UnitAI : MonoBehaviour
{
    private UnitBase _thisUnit;
    public Transform targetPosition;
    [HideInInspector] public bool isTargetInAttackRange;

    public UnitBase target;
    public LayerMask targetLayer;
    public UnityEvent<GameObject> onOpponentDetected;

    [Range(.1f, 10)] public float attRange = 1f;
    public string targetTag;

    [Header("Gizmo Parameter")]
    public Color gizmoColor = Color.green;
    public bool gizmoShow = true;

    [Header("Direction")]
    [HideInInspector] public float unitDir;

    public bool opponentDetected { get; internal set; }

    private void Awake()
    {
        _thisUnit = GetComponent<UnitBase>();
    }

    private void Start()
    {
        StartCoroutine(GameStart());
    }

    private void Update()
    {
        if (GameManager.Instance.isBattleStarted)
        {
            AttackRangeDetection();
        }

        if(targetPosition != null)
        {
            unitDir = transform.position.x - targetPosition.position.x;
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.isBattleStarted)
        {
            MoveToTarget();
        }
    }

    IEnumerator GameStart()
    {
        yield return new WaitUntil(() => GameManager.Instance.isBattleStarted == true);
        yield return new WaitUntil(() => GameManager.Instance.isEnemyPresent == true);
        DetectTarget();
    }

    private void OnDrawGizmos()
    {
        if (gizmoShow)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, attRange);
        }
    }

    public void DetectTarget()
    {
        try
        {
            targetPosition = GameObject.FindGameObjectWithTag(targetTag).GetComponent<Transform>();
            target = GameObject.FindGameObjectWithTag(targetTag).GetComponent<UnitBase>();
        }
        catch (System.Exception)
        {
            print("Pong");
        }
        
    }

    void MoveToTarget()
    {
        if (targetPosition && 
            !opponentDetected && 
            !target.isUnitDead && 
            !_thisUnit.isUnitDead &&
            (_thisUnit.unitState == UnitAnimState.idle || _thisUnit.unitState == UnitAnimState.moving)
            )
        {
            _thisUnit.unitState = UnitAnimState.moving;
            transform.position = Vector2.MoveTowards(transform.position, targetPosition.position, _thisUnit.movSpeed * Time.deltaTime);
        }  
    }

    void AttackRangeDetection()
    {
        var collider = Physics2D.OverlapCircle(transform.position, attRange, targetLayer);
        opponentDetected = collider != null;
        if (target == null) return;
        if (opponentDetected && !target.isUnitDead && !_thisUnit.isUnitDead &&
            (_thisUnit.unitState == UnitAnimState.idle || _thisUnit.unitState == UnitAnimState.moving))
        {
            onOpponentDetected?.Invoke(collider.gameObject);
            _thisUnit.unitState = UnitAnimState.attacking;
            isTargetInAttackRange = true;
            _thisUnit.Attack();
        }
        else
        {
            isTargetInAttackRange = false;
        }
    }
}
