using UnityEngine;

public class PolicemanController : EnemyController
{
    protected override void TryMove()
    {
        if (jumpController == null || playerController == null)
            return;

        GameObject enemyCurrentBuilding = jumpController.currentBuilding;
        GameObject playerCurrentBuilding = playerController.currentBuilding;
        if (enemyCurrentBuilding == null || playerCurrentBuilding == null)
            return;

        // Get adjacent buildings
        BuildingHandler bh = enemyCurrentBuilding.GetComponent<BuildingHandler>();
        if (bh == null || bh.buildings == null || bh.buildings.Length == 0)
            return;

        GameObject bestBuilding = null;
        float bestDist = float.MaxValue;
        foreach (GameObject adj in bh.buildings)
        {
            if (adj == null) continue;
            float dist = Vector3.Distance(adj.transform.position, playerCurrentBuilding.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestBuilding = adj;
            }
        }

        if (bestBuilding != null)
        {
            jumpController.targetBuilding = bestBuilding;
            jumpController.Jump(bestBuilding.transform.position);
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "Player")
        {
            GameObject.Find("GameManager").GetComponent<DeathManager>().OnPlayerDeath();
            GetComponent<Animator>().SetTrigger("Destroy");
        }
    }
}
