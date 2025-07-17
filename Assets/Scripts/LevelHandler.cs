using UnityEngine;
using System.Collections;

public class LevelHandler : MonoBehaviour
{
    public GameObject mapTemplate;
    public GameObject currentMap;
    public GameObject player;
    public float nextLevelDelay = 2f;

    public void NextLevel()
    {
        if (mapTemplate == null || currentMap == null)
            return;

        // 1. Instantiate new map at (10,0,-10) from current map
        Vector3 newMapPos = currentMap.transform.position + new Vector3(10, 0, -10);
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
            Debug.Log("move cam");
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
        for (int i = 0; i < 16; i++)
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
                // Update all references in all buildings' BuildingHandler.buildings arrays
                for (int k = 0; k < buildingObjs.Length; k++)
                {
                    BuildingHandler bh = buildingObjs[k].GetComponent<BuildingHandler>();
                    if (bh != null && bh.buildings != null)
                    {
                        int aIndex = -1, bIndex = -1;
                        for (int m = 0; m < bh.buildings.Length; m++)
                        {
                            if (bh.buildings[m] == buildingA) aIndex = m;
                            else if (bh.buildings[m] == buildingB) bIndex = m;
                        }
                        if (aIndex != -1) bh.buildings[aIndex] = buildingB;
                        if (bIndex != -1) bh.buildings[bIndex] = buildingA;
                    }
                }
                // Swap their transforms in the hierarchy
                GameObject[] tempBuildings = buildingA.GetComponent<BuildingHandler>().buildings;
                buildingA.GetComponent<BuildingHandler>().buildings = buildingB.GetComponent<BuildingHandler>().buildings;
                buildingB.GetComponent<BuildingHandler>().buildings = tempBuildings;
                // Check if we swapped the TopBuilding 
                if(map.GetComponent<MapHandler>().topBuilding == buildingA)
                    map.GetComponent<MapHandler>().topBuilding = buildingB;
                else if(map.GetComponent<MapHandler>().topBuilding == buildingB)
                    map.GetComponent<MapHandler>().topBuilding = buildingA;
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
        GetComponent<EnemyManager>().ResetEnemies();
        yield return new WaitForSeconds(nextLevelDelay/2);
        NextLevel();
    }
}
