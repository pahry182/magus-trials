using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    private UnitBase _enemy;
    private UnitAI _playerAI;

    private bool isCheckerRunning;

    public float enemyRespawnDelay = 1f;

    private void Awake()
    {
        _playerAI = GameObject.FindGameObjectWithTag("Player").GetComponent<UnitAI>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isCheckerRunning)
            StartCoroutine(CheckIfEnemyDie());
    }

    public IEnumerator SpawnEnemy()
    {
        if (GameManager.Instance.isPlayerRespawning)
        {
            GameManager.Instance.isPlayerRespawning = false;
        }
        else
        {
            GameManager.Instance.currentWave++;

        }

        AudioCheck();

        yield return new WaitUntil(() => GameManager.Instance.isBattleStarted == true);
        yield return new WaitForSeconds(enemyRespawnDelay);

        GameManager.Instance.isEnemyPresent = true;
        GameManager.Instance.SetCheckpoint();
        int select = Random.Range(0, GameManager.Instance._normalEnemyPool.Length);
        if (GameManager.Instance.currentWave % 4 == 0)
        {
            Instantiate(GameManager.Instance._bossEnemyPool[select], new Vector3(Random.Range(-8f, 8f), 0f, 0f), Quaternion.identity);
        }
        else
        {
            Instantiate(GameManager.Instance._normalEnemyPool[select], new Vector3(Random.Range(-8f, 8f), 0f, 0f), Quaternion.identity);
        }
        _enemy = GameObject.FindGameObjectWithTag("Enemy").GetComponent<UnitBase>();
        _playerAI.DetectTarget();
    }

    private void AudioCheck()
    {
        if ((GameManager.Instance.currentWave) % 4 == 0 && GameManager.Instance.currentWave != 0)
        {
            GameManager.Instance.PlayBgm("Battle_Boss");
        }
        else
        {
            GameManager.Instance.PlayBgm("Battle_Normal");
        }
    }

    IEnumerator CheckIfEnemyDie()
    {
        isCheckerRunning = true;
        if (_enemy == null)
        {
            yield return new WaitForSeconds(0);
        }
        else if (_enemy.isUnitDead)
        {
            yield return new WaitForSeconds(enemyRespawnDelay/2);
            Destroy(_enemy.gameObject);
            yield return new WaitForSeconds(enemyRespawnDelay/2);
            StartCoroutine(SpawnEnemy());
        }
        isCheckerRunning = false;
    }

    public void RemoveEnemy()
    {
        if (_enemy == null) return;
        Destroy(_enemy.gameObject);
        GameManager.Instance.isEnemyPresent = false;
    }
}
