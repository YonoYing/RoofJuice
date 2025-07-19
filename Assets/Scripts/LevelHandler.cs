using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelHandler : MonoBehaviour
{
    public GameObject mapTemplate;
    public GameObject currentMap;
    public GameObject player;
    public float nextLevelDelay = 2f;
    public bool tutorial;
    public int stage = 0;
    public TMP_Text stageText;

    public void NextLevel()
    {
        if(tutorial)
            SceneManager.LoadScene("Level1");

        if (mapTemplate == null || currentMap == null)
            return;

        // 1. Instantiate new map at (10,0,-10) from current map
        Vector3 newMapPos = currentMap.transform.position + new Vector3(20, 0, -20);
        GameObject newMap = Instantiate(mapTemplate, newMapPos, Quaternion.identity);

        // 2. Randomly switch some of the children's transforms in the new map and update their building lists
        SwitchAndUpdateBuildings(newMap);

        // 3. Move the camera to the new map using CameraController
        Camera mainCam = Camera.main;
        CameraController camController = mainCam.GetComponent<CameraController>();
        if (camController != null)
        {
            Vector3 camStart = mainCam.transform.position;
            Vector3 camMoveBy = newMapPos - currentMap.transform.position;
            camController.SmoothMoveCamera(camStart, camMoveBy, camController.moveDuration, () => {
                // 4. Once camera stops, move player to new map's topBuilding
                if (player != null)
                {
                    MapHandler newMapHandler = newMap.GetComponent<MapHandler>();
                    if (newMapHandler != null && newMapHandler.topBuilding != null)
                    {
                        player.transform.position = newMapHandler.topBuilding.transform.position;
                        PlayerController pc = player.GetComponent<PlayerController>();
                        if (pc != null)
                        {
                            pc.map = newMap;
                            pc.currentBuilding = newMapHandler.topBuilding;
                        }
                    }
                }
                // 5. Delete old map and set new map as currentMap
                Destroy(currentMap);
                currentMap = newMap;
                currentMap.GetComponent<MapFeel>().player = player;
                GetComponent<EnemyManager>().map = currentMap;
                GetComponent<DeathManager>().map = currentMap;

                stage += 1;
                stageText.text = stage.ToString(); 
                GetComponent<EnemyManager>().UpdateRainclouds(stage);
            });
        }
        else
        {
            // Fallback: move everything instantly
            if (player != null)
            {
                MapHandler newMapHandler = newMap.GetComponent<MapHandler>();
                if (newMapHandler != null && newMapHandler.topBuilding != null)
                {
                    player.transform.position = newMapHandler.topBuilding.transform.position;
                    PlayerController pc = player.GetComponent<PlayerController>();
                    if (pc != null)
                    {
                        pc.map = newMap;
                        pc.currentBuilding = newMapHandler.topBuilding;
                    }
                }
            }
            Destroy(currentMap);
            currentMap = newMap;
        }
    }

    private void SwitchAndUpdateBuildings(GameObject map)
    {
        // Get all building GameObjects
        GameObject[] buildingObjs = map.GetComponent<MapHandler>().buildings;
        // Randomly swap some buildings
        for (int i = 0; i < 10; i++)
        {
            int j = Random.Range(0, buildingObjs.Length);
            if (i != j)
            {
                GameObject buildingA = buildingObjs[i];
                GameObject buildingB = buildingObjs[j];
                // Swap positions in the array
                buildingObjs[i] = buildingB;
                buildingObjs[j] = buildingA;
                Debug.Log(buildingA);
                Debug.Log(buildingB);
                // Swap their transforms in the hierarchy
                Vector3 tempPos = buildingA.transform.position;
                buildingA.transform.position = buildingB.transform.position;
                buildingB.transform.position = tempPos;

                // 1. Create a new list with all unique buildings from both buildingA and buildingB
                System.Collections.Generic.List<GameObject> allBuildings = new System.Collections.Generic.List<GameObject>();
                
                // Add buildings from buildingA's list
                foreach (GameObject b in buildingA.GetComponent<BuildingHandler>().buildings)
                {
                    if (b != null && !allBuildings.Contains(b))
                        allBuildings.Add(b);
                }
                
                // Add buildings from buildingB's list
                foreach (GameObject b in buildingB.GetComponent<BuildingHandler>().buildings)
                {
                    if (b != null && !allBuildings.Contains(b))
                        allBuildings.Add(b);
                }
                
                // 2. Loop through the new list and swap buildingA/buildingB references
                foreach (GameObject b in allBuildings)
                {
                    if (b != null)
                    {
                        BuildingHandler bh = b.GetComponent<BuildingHandler>();
                        if (bh != null && bh.buildings != null)
                        {
                            for (int m = 0; m < bh.buildings.Length; m++)
                            {
                                if (bh.buildings[m] == buildingA)
                                    bh.buildings[m] = buildingB;
                                else if (bh.buildings[m] == buildingB)
                                    bh.buildings[m] = buildingA;
                            }
                        }
                    }
                }
                
                // Swap their building lists
                GameObject[] tempBuildings = buildingA.GetComponent<BuildingHandler>().buildings;
                buildingA.GetComponent<BuildingHandler>().buildings = buildingB.GetComponent<BuildingHandler>().buildings;
                buildingB.GetComponent<BuildingHandler>().buildings = tempBuildings;
                // Check if we swapped the TopBuilding 
                if(map.GetComponent<MapHandler>().topBuilding == buildingA)
                    map.GetComponent<MapHandler>().topBuilding = buildingB;
                else if(map.GetComponent<MapHandler>().topBuilding == buildingB)
                    map.GetComponent<MapHandler>().topBuilding = buildingA;
                // Check if we swapped the bottomLeft 
                if(map.GetComponent<MapHandler>().bottomLeft == buildingA)
                    map.GetComponent<MapHandler>().bottomLeft = buildingB;
                else if(map.GetComponent<MapHandler>().bottomLeft == buildingB)
                    map.GetComponent<MapHandler>().bottomLeft = buildingA;
                // Check if we swapped the bottomRight 
                if(map.GetComponent<MapHandler>().bottomRight == buildingA)
                    map.GetComponent<MapHandler>().bottomRight = buildingB;
                else if(map.GetComponent<MapHandler>().bottomRight == buildingB)
                    map.GetComponent<MapHandler>().bottomRight = buildingA;
            }
        }
    }

    public void CheckLevel()
    {
        foreach (Transform child in currentMap.transform)
        {
            TagHandler tagHandler = child.GetComponent<TagHandler>();
            if (tagHandler == null || !tagHandler.tagged)
                return;
        }
        
        LevelCompleted();
    }

    public void LevelCompleted()
    {
        StartCoroutine(LevelCompletedCoroutine());
    }

    private IEnumerator LevelCompletedCoroutine()
    {
        yield return new WaitForSeconds(nextLevelDelay/2);
        currentMap.GetComponent<MapFeel>().MapCompleted();
        if(!tutorial)
            GetComponent<EnemyManager>().ResetEnemies();
        yield return new WaitForSeconds(nextLevelDelay/2);
        NextLevel();
    }
}
