using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public List<GameObject> enemies = new List<GameObject>();
    public GameObject policemanPrefab;
    public GameObject windPrefab;
    public GameObject[] furniturePrefabs;
    public float policemanSpawnPeriod = 5f;
    public float furnitureSpawnPeriod = 3f;
    public float windSpawnPeriod = 3f;
    public GameObject map;

    private float spawnTimer = 0f;
    private float furnitureTimer = 0f;
    private float windTimer = 0f;
    private GameObject currentPoliceman = null;
    private GameObject currentWind = null;

    void Start()
    {
        map = GameObject.Find("Map");
    }

    void Update()
    {
        if (currentPoliceman == null)
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
    }

    public void SpawnPoliceman()
    {
        var mapHandler = map.GetComponent<MapHandler>();
        GameObject spawnBuilding = Random.value < 0.5f ? mapHandler.bottomLeft : mapHandler.bottomRight;
        currentPoliceman = SpawnEnemy(policemanPrefab, spawnBuilding);
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
        GameObject newWind = Instantiate(windPrefab, spawnPosition, Quaternion.identity);
        enemies.Add(newWind);
        // Add links to building and new wind
        if(cornerDirection == 2)
            targetBuilding.GetComponent<BuildingHandler>().buildings[3] = newWind;
        else 
            targetBuilding.GetComponent<BuildingHandler>().buildings[2] = newWind;
        newWind.GetComponent<BuildingHandler>().buildings[0] = map.GetComponent<MapHandler>().topBuilding;
    }

    public void ResetEnemies()
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        enemies.Clear();
        currentPoliceman = null;
    }
}
