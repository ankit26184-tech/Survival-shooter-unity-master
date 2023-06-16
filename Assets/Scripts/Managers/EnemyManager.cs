using UnityEngine;
using System;

public class EnemyManager : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public GameObject enemy;
    public float spawnTime = 3f;
    public Transform[] spawnPoints;
    public Action<GameObject> enemyCreatedCallback;

    void Start ()
    {
        InvokeRepeating ("Spawn", spawnTime, spawnTime);
    }


    void Spawn ()
    {
        if(playerHealth.CurrentHealth <= 0f)
        {
            return;
        }

        int spawnPointIndex = UnityEngine.Random.Range (0, spawnPoints.Length);

        Spawn(spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);
    }

    public void Spawn(Vector3 position, Quaternion rotation)
    {
        GameObject go =Instantiate(enemy, position, rotation);
        enemyCreatedCallback?.Invoke(go);
    }
}
