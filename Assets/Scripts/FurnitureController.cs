using UnityEngine;

public class FurnitureController : EnemyController
{
    [SerializeField]public float pauseOnSpawn = 2f;
    float spawnTimer = 0f;
    
    override protected void Start()
    {
        player = GameObject.Find("Player");
        jumpController = GetComponent<TumbleController>();
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
        }
    }
    
    protected virtual void Update()
    {
        if(spawnTimer < pauseOnSpawn)
            spawnTimer += Time.deltaTime;
        else
        {
            if(!jumpController.isJumping)
            {
                movementTimer += Time.deltaTime;
                if (movementTimer >= movementPeriod)
                {
                    movementTimer = 0f;
                    TryMove();
                }
            }
        }
    }

}
