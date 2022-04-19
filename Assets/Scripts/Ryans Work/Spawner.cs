using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    public GameObject Enemy;
    public float SpawnTime;
    public float NextWaveSpawnTime;
    public int WaveNumber;
    public int EnemyCount;
    public GameObject FalseFloor;
    public GameObject BossDoor;
    public GameObject BossBlock;

    private void Awake()
    {
        NextWaveSpawnTime = SpawnTime;
        EnemyCount = 2;
    }
    private void Update()
    {
        if (NextWaveSpawnTime > 0)
        {
            NextWaveSpawnTime -= Time.deltaTime;
        }
        else if (NextWaveSpawnTime <= 0)
        {
            NextWaveSpawnTime = int.MaxValue;
            SpawnEnemy(EnemyCount);
            
        }

        if (EnemyCount == 0)
        {
            WaveNumber = WaveNumber + 1;
            EnemyCount = Random.Range(2, 6);
            NextWaveSpawnTime = SpawnTime;
        }
        if (WaveNumber == 4)
        {
            FalseFloor.SetActive(true);
            BossBlock.SetActive(false);
            BossDoor.SetActive(true);
            Destroy(gameObject);
            
        }
    }

    void SpawnEnemy(int Amount)
    {
        for (int i = 0; i < EnemyCount; i++)
        {
            Vector3 SpawnPoint = GetSpawnPoint();
            Instantiate(Enemy, SpawnPoint, transform.rotation);
        }
        
    }
    private Vector3 GetSpawnPoint()
    {
        int X = Random.Range(-10, -4);
        int Z = Random.Range(41, 35);
        Vector3 SpawnPosition = new Vector3(X, 0, Z);
        return SpawnPosition;
    }
}
