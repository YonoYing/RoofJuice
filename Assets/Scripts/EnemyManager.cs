using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public List<GameObject> enemies = new List<GameObject>();
    public GameObject policemanPrefab;
    public GameObject raincloudPrefab;
    public GameObject windPrefab;
    public GameObject[] furniturePrefabs;
    public float policemanSpawnPeriod = 5f;
    public float furnitureSpawnPeriod = 3f;
    public float windSpawnPeriod = 3f;
    public GameObject map;

    public float raincloudSpawnPeriod = 8f;
    public float raincloudSpawnPeriodDecreaseStep = 1f;
    public float policemanSpawnPeriodDecreaseStep = 1f;
    public bool raincloudsEnabled;

    private float spawnTimer = 0f;
    public float furnitureTimer = 0f;
    private float windTimer = 0f;
    public float raincloudTimer = 0f;
    public float policeFastSpeed = 4f;
    private GameObject currentPoliceman = null;
    private GameObject secondPoliceman = null;
    private GameObject currentRaincloud = null;
    private GameObject currentWind = null;

    void Start()
    {
        map = GameObject.Find("Map");
    }

    void Update()
    {
        int stage = GetComponent<LevelHandler>().stage;
        if ((currentPoliceman == null) || (stage >= 6 && secondPoliceman == null))
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= policemanSpawnPeriod)
            {
                spawnTimer = 0f;
                SpawnPoliceman();
            }
        }

        if (currentWind == null)
        {
            windTimer += Time.deltaTime;
            if (windTimer >= windSpawnPeriod)
            {
                windTimer = 0f;
                SpawnWind();
            }
        }

        furnitureTimer += Time.deltaTime;
        if (furnitureTimer >= furnitureSpawnPeriod)
        {
            furnitureTimer = 0f;
            SpawnFurniture();
        }

        if(raincloudsEnabled && currentRaincloud==null)
        {
            raincloudTimer += Time.deltaTime;
            if (raincloudTimer >= raincloudSpawnPeriod)
            {
                raincloudTimer = 0f;
                SpawnRaincloud();
            }
        }
    }

    public void SpawnPoliceman()
    {
        var mapHandler = map.GetComponent<MapHandler>();
        GameObject spawnBuilding = Random.value < 0.5f ? mapHandler.bottomLeft : mapHandler.bottomRight;
        if(!currentPoliceman)
            currentPoliceman = SpawnEnemy(policemanPrefab, spawnBuilding);
        else if(!secondPoliceman)
            secondPoliceman = SpawnEnemy(policemanPrefab, spawnBuilding);

        if(GetComponent<LevelHandler>().stage > 4)
        {
            if(currentPoliceman)
                currentPoliceman.GetComponent<PolicemanController>().movementPeriod = policeFastSpeed;
            if(secondPoliceman)
                secondPoliceman.GetComponent<PolicemanController>().movementPeriod = policeFastSpeed;
        }
    }

    public void SpawnFurniture()
    {
        var mapHandler = map.GetComponent<MapHandler>();
        var topBuildingHandler = mapHandler.topBuilding.GetComponent<BuildingHandler>();

        int spawnIndex = Random.value < 0.5f ? 0 : 1;
        GameObject spawnBuilding = topBuildingHandler.buildings[spawnIndex];
        
        GameObject furniturePrefab = furniturePrefabs[Random.Range(0, furniturePrefabs.Length)];
        GameObject newEnemy = SpawnEnemy(furniturePrefab, spawnBuilding);
        newEnemy.GetComponent<Animator>().SetTrigger(spawnIndex == 0 ? "Right" : "Left");
        newEnemy.GetComponent<EnemyController>().moveBias = spawnIndex == 0 ? 2 : 1;
        newEnemy.GetComponent<JumpController>().bounce = true;
    }

    public GameObject SpawnEnemy(GameObject prefab, GameObject spawnBuilding)
    {
        Vector3 spawnPosition = spawnBuilding.transform.position;
        GameObject newEnemy = Instantiate(prefab, spawnPosition, Quaternion.identity);

        newEnemy.GetComponent<JumpController>().currentBuilding = spawnBuilding;
        newEnemy.GetComponent<JumpController>().map = map;
        enemies.Add(newEnemy);
        return newEnemy;
    }

    public void SpawnWind()
    {
        var mapHandler = map.GetComponent<MapHandler>();
        // Bottom left indexes upwards by finding buildings[2], right by buildings[3]
        int cornerDirection = Random.value < 0.5f ? 2 : 3;
        GameObject bottomBuilding = cornerDirection == 2 ? mapHandler.bottomLeft : mapHandler.bottomRight;
        GameObject targetBuilding = bottomBuilding;
        int offset = Random.Range(0, 3);
        for (int i = 0; i < offset; i++)
        {
            targetBuilding = targetBuilding.GetComponent<BuildingHandler>().buildings[cornerDirection];
        }

        //Spawn new wind and set links accordingly 
        Vector3 spawnPosition = targetBuilding.transform.position;
        spawnPosition += cornerDirection == 2 ? new Vector3(0,0,1) : new Vector3(1,0,0);
        GameObject newWind = Instantiate(windPrefab, spawnPosition, Quaternion.Euler(0, 50, -90));
        enemies.Add(newWind);
        // Add links to building and new wind
        if(cornerDirection == 2)
            targetBuilding.GetComponent<BuildingHandler>().buildings[3] = newWind;
        else 
            targetBuilding.GetComponent<BuildingHandler>().buildings[2] = newWind;
        newWind.GetComponent<BuildingHandler>().buildings[0] = map.GetComponent<MapHandler>().topBuilding;
        // currentWind = newWind;
    }

    public void SpawnRaincloud()
    {
        var mapHandler = map.GetComponent<MapHandler>();
        var buildings = mapHandler.buildings;
        // Exclude top, bottomLeft, bottomRight
        List<GameObject> candidates = new List<GameObject>();
        foreach (var b in buildings)
        {
            if (b != mapHandler.topBuilding && b != mapHandler.bottomLeft && b != mapHandler.bottomRight)
                candidates.Add(b);
        }
        if (candidates.Count == 0) return;
        GameObject targetBuilding = candidates[Random.Range(0, candidates.Count)];
        // Pick offset and spawn position
        Vector3[] offsets = { new Vector3(10,0,-10), new Vector3(-10,0,10) };
        int offsetIndex = Random.Range(0, 2);
        Vector3 spawnOffset = offsets[offsetIndex];
        Vector3 moveOffset = offsets[1 - offsetIndex];
        Vector3 spawnPosition = targetBuilding.transform.position + spawnOffset;
        GameObject raincloud = Instantiate(raincloudPrefab, spawnPosition, Quaternion.identity);
        currentRaincloud = raincloud; 
        Raincloud rc = raincloud.GetComponent<Raincloud>();
        rc.targetBuilding = targetBuilding;
        StartCoroutine(rc.MoveCloud(moveOffset));
    }

    public void ResetEnemies()
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                if (enemy.GetComponent<EnemyController>())
                    enemy.GetComponent<EnemyController>().PlayDestroyAnim();
                else 
                    Destroy(enemy);
            }
        }
        enemies.Clear();
        currentPoliceman = null;
    }

    public void UpdateRainclouds(int stage)
    {
        if(stage <= 1)
        {
            raincloudsEnabled = true;
        }
        else
        {
            raincloudSpawnPeriod -= raincloudSpawnPeriodDecreaseStep;
            policemanSpawnPeriod -= policemanSpawnPeriodDecreaseStep;
            if(raincloudSpawnPeriod <= 1)
            {
                raincloudSpawnPeriod = 1;
            }
            if(policemanSpawnPeriod <= 1)
            {
                policemanSpawnPeriod = 1;
            }
        }
    }
}
