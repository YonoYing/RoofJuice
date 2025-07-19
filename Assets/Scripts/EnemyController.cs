using UnityEngine;
using System.Collections;


public class EnemyController : MonoBehaviour
{
    [SerializeField]public GameObject player;
    [SerializeField]public float movementPeriod = 2f;

    public float movementTimer = 0f;
    protected JumpController jumpController;
    protected PlayerController playerController;
    public bool pendingDestroy = false;
    protected Vector3 offMapJumpTarget;
    public int moveBias = 0;


    protected virtual void Start()
    {
        player = GameObject.Find("Player");
        jumpController = GetComponent<JumpController>();
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
        }
    }

    protected virtual void Update()
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

    protected virtual void TryMove()
    {
        if (jumpController == null)
            return;

        GameObject enemyCurrentBuilding = jumpController.currentBuilding;
        if (enemyCurrentBuilding == null)
            return;

        BuildingHandler bh = enemyCurrentBuilding.GetComponent<BuildingHandler>();
        if (bh == null || bh.buildings == null || bh.buildings.Length < 2)
            return;

        GameObject b0 = bh.buildings[0];
        GameObject b1 = bh.buildings[1];
        GameObject chosen = null;
        if (b0 != null && b1 != null)
        {
            if(moveBias > 0)
            {
                chosen = moveBias == 1 ? b0 : b1;
                moveBias = 0;
            }
            else
                chosen = (Random.value < 0.5f) ? b0 : b1;
        }
        else if (b0 != null)
        {
            chosen = b0;
        }
        else if (b1 != null)
        {
            chosen = b1;
        }
        else
        {
            // Both are null, jump off the map in the right direction (use Vector3.right)
            offMapJumpTarget = (Random.value < 0.5f) ? 
                                enemyCurrentBuilding.transform.position + Vector3.back + Vector3.down : 
                                enemyCurrentBuilding.transform.position + Vector3.left + Vector3.down;
            pendingDestroy = true;
            GetComponent<JumpController>().bounce = false;
            jumpController.Jump(offMapJumpTarget);
            jumpController.targetBuilding = null;
            StartCoroutine(CheckDestroyAfterJump());
            return;
        }
        jumpController.targetBuilding = chosen;
        jumpController.Jump(chosen.transform.position);
        StartCoroutine(CheckDestroyAfterJump());
    }

    private IEnumerator CheckDestroyAfterJump()
    {
        // Wait until jump is finished
        while (jumpController.isJumping)
            yield return null;
        if (pendingDestroy)
        {
            pendingDestroy = false;
            // Move object downward for 3 seconds at the same speed as the end of the jump
            float duration = 3f;
            float elapsed = 0f;
            float speed = jumpController.jumpSpeed*2; // Use jumpSpeed as the downward speed
            while (elapsed < duration)
            {
                float moveAmount = speed * Time.deltaTime;
                transform.position += Vector3.down * moveAmount;
                elapsed += Time.deltaTime;
                yield return null;
            }
            Destroy(gameObject);
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "Player")
            GameObject.Find("GameManager").GetComponent<DeathManager>().OnPlayerDeath();
    }

    public void PlayDestroyAnim()
    {
        Debug.Log("Booooby");
        GetComponent<Animator>().SetBool("Destroy", true);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
