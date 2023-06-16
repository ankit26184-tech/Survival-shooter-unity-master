using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
class EnemyData
{
    public string name;
    public Vector3 positions;
    public Vector3 rotation;
    public int health;

    public EnemyData(string name, Vector3 position, Vector3 rotation, int health)
    {
        this.name = name;
        this.positions = position;
        this.rotation = rotation;
        this.health = health;
    }
}

[System.Serializable]
class GameData
{
    public List<EnemyData> enemyData;
    public Vector3 playerPosition;
    public Vector3 playerRotation;
    public int playerHealth;
    public int score;
    public Vector3 cameraPosition;
}

public class GameStateManager : MonoBehaviour
{
    private const string GAME_DATA_KEY = "GAME_DATA";
    //Cache enemies in dictionary to fetch data fast while quiting.
    private Dictionary<GameObject, EnemyHealth> enemyDic = new Dictionary<GameObject, EnemyHealth>();
    private GameData gameData;
    private Dictionary<string, EnemyManager> enemyManagerDic = new Dictionary<string, EnemyManager>();

    [SerializeField] private PlayerHealth playerHealth;

    private void Start()
    {
        //Cache EnemyManager and register enemy creation callback.

        EnemyManager[] enemyManagers = FindObjectsOfType<EnemyManager>();
        foreach (EnemyManager manager in enemyManagers)
        {
            manager.enemyCreatedCallback = AddEnemy;
            enemyManagerDic.Add(manager.enemy.name + "(Clone)", manager);
        }

        LoadGameData();
    }

#region Save Game Data
    private void OnApplicationQuit()
    {
        if (playerHealth.CurrentHealth <= 0)
            return;

        SaveGameData();
    }

    private void SaveGameData()
    {
        gameData = new GameData();

        SaveEnemiesData();
        SavePlayerData();
        SaveScore();
        SaveCameraData();

        string json = JsonConvert.SerializeObject(gameData);
        PlayerPrefs.SetString(GAME_DATA_KEY, json);
        Debug.Log(json);
    }

    private void SaveEnemiesData()
    {
        gameData.enemyData = new List<EnemyData>();

        foreach (KeyValuePair<GameObject, EnemyHealth> keyValuePair in enemyDic)
        {
            GameObject go = keyValuePair.Key;
            EnemyHealth healthComponent = keyValuePair.Value;
            EnemyData enemyData = new EnemyData(go.name, go.transform.position, go.transform.localEulerAngles, healthComponent.CurrentHealth);
            gameData.enemyData.Add(enemyData);
        }
    }
    
    private void SavePlayerData()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        gameData.playerPosition = player.transform.position;
        gameData.playerRotation = player.transform.localEulerAngles;
        gameData.playerHealth = player.GetComponent<PlayerHealth>().CurrentHealth;
    }

    private void SaveScore()
    {
        gameData.score = ScoreManager.score;
    }

    private void SaveCameraData()
    {
        gameData.cameraPosition = Camera.main.transform.position;
    }
    #endregion

#region Load Game Data
    private void LoadGameData()
    {
        gameData = JsonConvert.DeserializeObject<GameData>(PlayerPrefs.GetString(GAME_DATA_KEY));
        ClearGameData();

        if (gameData == null)
            return;

        LoadEnemiesData();
        LoadPlayerData();
        LoadScore();
        LoadCameraData();

        gameData = null;
       
    }

    private void LoadEnemiesData()
    {
        /*  Create Dictionary where key is the prefab name and EnemyManager is the Value.
        This way it will be easy to search type of enemy prefab which has to be created.
        Because searching using Dictionary will be O(1) complexity otherwise
        If we use nested loop for each prefab type go through each manager then:
        If total enemies are n and total enemy managers are m then coplexity will be O(n*m) */


        foreach (EnemyData enemyData in gameData.enemyData)
        {
            EnemyManager enemyManager = enemyManagerDic[enemyData.name];
            enemyManager.Spawn(enemyData.positions, Quaternion.Euler(enemyData.rotation.x, enemyData.rotation.y, enemyData.rotation.z));
        }

    }

    private void LoadPlayerData()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.transform.position = gameData.playerPosition;
        player.transform.localEulerAngles = gameData.playerRotation;
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        playerHealth.CurrentHealth = gameData.playerHealth;
        playerHealth.healthSlider.value = playerHealth.CurrentHealth;
    }

    private void LoadScore()
    {
        ScoreManager.score = gameData.score;
    }

    private void LoadCameraData()
    {
        Camera.main.transform.position = gameData.cameraPosition;
    }
#endregion

#region CLEAR_GAME_DATA
    public void ClearGameData()
    {
        PlayerPrefs.SetString(GAME_DATA_KEY, "");
    }
#endregion
    private void AddEnemy(GameObject enemy)
    {
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        enemyHealth.destroyCallback = RemoveEnemy;
        enemyDic.Add(enemy, enemyHealth);
    }

    private void RemoveEnemy(GameObject enemy)
    {
        enemyDic.Remove(enemy);
    }
}
