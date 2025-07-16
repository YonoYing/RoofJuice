using UnityEngine;

public class FurnitureController : EnemyController
{
    
    override protected void Start()
    {
        player = GameObject.Find("Player");
        jumpController = GetComponent<TumbleController>();
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
        }
    }
}
