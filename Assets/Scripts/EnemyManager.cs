using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public List<GameObject> enemies = new List<GameObject>();
    public GameObject policemanPrefab;
    public GameObject[] furniturePrefabs;
    public float policemanSpawnPeriod = 5f;
    public float furnitureSpawnPeriod = 3f;
    public GameObject map;

    private float spawnTimer = 0f;
    private float furnitureTimer = 0f;
    private GameObject currentPoliceman = null;

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

        furnitureTimer += Time.deltaTime;
        if (furnitureTimer >= furnitureSpawnPeriod)
        {
            furnitureTimer = 0f;
            SpawnFurniture();
        }
    }

    public void SpawnPoliceman()
    {
        if (currentPoliceman != null)
            return;

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
        SpawnEnemy(furniturePrefab, spawnBuilding);
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
