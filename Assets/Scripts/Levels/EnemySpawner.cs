using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using RPNEvaluator;

public class EnemySpawner : MonoBehaviour
{
    public Image level_selector;
    public GameObject button;
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;

    private List<Enemy> enemies;

    private List<Level> levels;

    private Level currentLevel;
    private int currentWave;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.ResetRun();

        // load enemy data from json
        string json = File.ReadAllText(Application.dataPath + "/Resources/enemies.json");
        enemies = JsonConvert.DeserializeObject<List<Enemy>>(json);

        string level_json = File.ReadAllText(Application.dataPath + "/Resources/levels.json");
        levels = JsonConvert.DeserializeObject<List<Level>>(level_json);

        for (int i = 0; i < levels.Count; i++)
        {
            GameObject selector = Instantiate(button, level_selector.transform);
            selector.transform.localPosition = new Vector3(0, 130 - i * 60);
            selector.GetComponent<MenuSelectorController>().spawner = this;
            selector.GetComponent<MenuSelectorController>().SetLevel(levels[i].name);
        }

    }


    public void StartLevel(string levelname)
    {
        GameManager.Instance.ResetRun();

        currentLevel = levels.FirstOrDefault(level => level.name == levelname);

        if (currentLevel == null)
        {
            Debug.LogError("level not found: " + levelname);
            return;
        }

        currentWave = 1;

        Debug.Log("Starting Level: " + currentLevel.name);

        level_selector.gameObject.SetActive(false);

        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();

        StartCoroutine(SpawnWave());
        // level_selector.gameObject.SetActive(false);
        // // this is not nice: we should not have to be required to tell the player directly that the level is starting
        // GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();
        // StartCoroutine(SpawnWave());
    }

    public void NextWave()
    {
        if (GameManager.Instance.state == GameManager.GameState.GAMEOVER)
        {
            ReturnToStart();
            return;
        }

        currentWave++;
        StartCoroutine(SpawnWave());
    }

    public void ReturnToStart()
    {
        StopAllCoroutines();
        GameManager.Instance.ResetRun();
        level_selector.gameObject.SetActive(true);
    }

    IEnumerator SpawnWave()
    {
        GameManager.Instance.currentWave = currentWave;
        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;
        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            if (GameManager.Instance.state == GameManager.GameState.GAMEOVER)
            {
                yield break;
            }
            GameManager.Instance.countdown--;
        }

        GameManager.Instance.state = GameManager.GameState.INWAVE;
        float waveStartTime = Time.time;

        foreach (Spawn spawn in currentLevel.spawns)
        {
            //Get the correct enemy from the pawn info
            Enemy enemyData = enemies.FirstOrDefault(enemy => enemy.name == spawn.enemy);

            if (enemyData == null)
            {
                Debug.LogError("Could not find enemy: " + spawn.enemy);

                continue;
            }


            Dictionary<string, int> variables = new Dictionary<string, int>();
            variables["wave"] = currentWave;
            variables["base"] = enemyData.hp;


            int count = RPNEvaluator.RPNEvaluator.Evaluate(spawn.count, variables);

            Debug.Log("Spawning " + count + " " + enemyData.name + " enemies");

            for (int i = 0; i < count; i++)
            {
                if (GameManager.Instance.state == GameManager.GameState.GAMEOVER)
                {
                    yield break;
                }

                yield return SpawnEnemy(enemyData);
            }



        }

        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0 && GameManager.Instance.state != GameManager.GameState.GAMEOVER);
        GameManager.Instance.lastWaveTime = Time.time - waveStartTime;

        if (GameManager.Instance.state == GameManager.GameState.GAMEOVER)
        {
            yield break;
        }

        if (currentLevel.waves > 0 && currentWave >= currentLevel.waves)
        {
            GameManager.Instance.gameOverMessage = "You beat " + currentLevel.name + "!";
            GameManager.Instance.state = GameManager.GameState.GAMEOVER;
        }
        else
        {
            GameManager.Instance.state = GameManager.GameState.WAVEEND;
        }
    }

    IEnumerator SpawnEnemy(Enemy enemyData)
    {
        SpawnPoint spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        Vector2 offset = Random.insideUnitCircle * 1.8f;

        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);

        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(enemyData.sprite);

        EnemyController en = new_enemy.GetComponent<EnemyController>();
        en.hp = new Hittable(enemyData.hp, Hittable.Team.MONSTERS, new_enemy);
        en.speed = enemyData.speed;

        GameManager.Instance.AddEnemy(new_enemy);

        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator SpawnZombie()
    {
        SpawnPoint spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        Vector2 offset = Random.insideUnitCircle * 1.8f;

        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);

        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(0);
        EnemyController en = new_enemy.GetComponent<EnemyController>();
        en.hp = new Hittable(50, Hittable.Team.MONSTERS, new_enemy);
        en.speed = 10;
        GameManager.Instance.AddEnemy(new_enemy);
        yield return new WaitForSeconds(0.5f);
    }
}
public class Enemy
{
    public string name;
    public int sprite;
    public int hp;
    public int speed;
    public int damage;
}

public class Level
{
    public string name;
    public int waves;

    public List<Spawn> spawns;
}

public class Spawn
{
    public string enemy;
    public string count;
    public string hp;
    public string damage;
    public string delay;
    public int[] sequence;
    public string location;
}
