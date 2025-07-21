using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : JumpController
{
    private PlayerInput playerInput;
    private InputAction moveAction;
    private bool pendingDeath = false;
    private Vector3 offMapJumpTarget;
    public bool edgeBlocking;
    
    public GameObject umbrella, umbrellaSplash;
    public bool umbrellaOpen, pendingWind, executingWind, umbrellaCooldown;
    public float umbrellaAutoCloseTime = 1f; 
    public float umbrellaCooldownTime = 1f; 
    public float deathTimer = 2f;
    GameObject gm;

    new void Start()
    {
        if (currentBuilding == null && map != null)
            currentBuilding = map.GetComponent<MapHandler>().topBuilding;
        transform.position = currentBuilding.transform.position;
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        animator = GetComponent<Animator>();
        gm = GameObject.Find("GameManager");
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(!umbrellaOpen && !umbrellaCooldown)
        {
            umbrellaOpen = true;
            umbrella.GetComponent<Animator>().SetBool("Open", true);
            StartCoroutine(UmbrellaTimerCoroutine());
        }
        if(GameObject.Find("SpaceIndicator"))
        {
            GameObject.Find("SpaceIndicator").GetComponent<Animator>().SetTrigger("Disable");
        }
    }

    private void CloseUmbrella()
    {
        if (!pendingWind)
        {
            umbrellaOpen = false;
            umbrellaCooldown = true;
            umbrella.GetComponent<Animator>().SetBool("Open", false);
            StartCoroutine(UmbrellaCooldownCoroutine());
        }
    }

    private IEnumerator UmbrellaTimerCoroutine()
    {
        yield return new WaitForSeconds(umbrellaAutoCloseTime);
        CloseUmbrella();
    }

    private IEnumerator UmbrellaCooldownCoroutine()
    {
        yield return new WaitForSeconds(umbrellaCooldownTime);
        umbrellaCooldown = false;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!context.performed || isJumping) return;
        Vector2 moveInput = context.ReadValue<Vector2>();
        int dir = -1;
        
        // Use the largest axis for cardinal directions, but allow diagonals for touch
        float absX = Mathf.Abs(moveInput.x);
        float absY = Mathf.Abs(moveInput.y);

        // Cardinal directions (keyboard/gamepad or strong swipe)
        if (absX > absY)
        {
            if (moveInput.x > 0.5f) dir = 0; // right
            else if (moveInput.x < -0.5f) dir = 3; // left
        }
        else if (absY > absX)
        {
            if (moveInput.y > 0.5f) dir = 2; // up
            else if (moveInput.y < -0.5f) dir = 1; // down
        }

        // For touchscreen, allow diagonals to override
        if (Input.touchSupported && Input.touchCount > 0)
        {
            if (moveInput.x > 0 && moveInput.y > 0) dir = 2; // up-right
            else if (moveInput.x < 0 && moveInput.y > 0) dir = 3; // up-left
            else if (moveInput.x > 0 && moveInput.y < 0) dir = 0; // down-right
            else if (moveInput.x < 0 && moveInput.y < 0) dir = 1; // down-left
        }

        if(!pendingDeath)
        {
            var bh = currentBuilding.GetComponent<BuildingHandler>();
            if (bh != null && bh.buildings != null && dir < bh.buildings.Length)
            {
                GameObject neighbor = bh.buildings[dir];
                if (neighbor != null)
                {
                    if(neighbor.CompareTag("Wind"))
                    {
                        if(umbrellaOpen)
                            pendingWind = true;
                        else 
                            DeathJump(dir);
                    }
                    targetBuilding = neighbor;
                    Jump(neighbor.transform.position);
                    return;
                }
            }
            // No neighbor in that direction, jump off the map
            DeathJump(dir);
        }
    }

    void DeathJump(int dir) 
    {
        if(!edgeBlocking)
        {
            Vector3 jumpDir = Vector3.zero;
            if (dir == 0) jumpDir = Vector3.back;
            else if (dir == 1) jumpDir = Vector3.left;
            else if (dir == 2) jumpDir = Vector3.right;
            else if (dir == 3) jumpDir = Vector3.forward;
            offMapJumpTarget = currentBuilding.transform.position + jumpDir + Vector3.up;
            pendingDeath = true;
            Jump(offMapJumpTarget);
        }
    }

    public override IEnumerator JumpCoroutine(Vector3 targetPosition)
    {
        var baseEnum = base.JumpCoroutine(targetPosition);
        while (baseEnum.MoveNext())
            yield return baseEnum.Current;
        if (pendingDeath && (targetPosition == offMapJumpTarget))
        {
            pendingDeath = false;
            // Move object downward for 3 seconds at the same speed as the end of the jump
            float duration = 1f;
            float elapsed = 0f;
            float speed = jumpSpeed*2; // Use jumpSpeed as the downward speed
            while (elapsed < duration)
            {
                float moveAmount = speed * Time.deltaTime;
                transform.position += Vector3.down * moveAmount;
                elapsed += Time.deltaTime;
                yield return null;
            }
            gm.GetComponent<DeathManager>().OnPlayerDeath();
        }
        else if (pendingWind)
        {
            pendingWind = false;
            executingWind = true;
            GameObject neighbor = currentBuilding.GetComponent<BuildingHandler>().buildings[0];
            targetBuilding = neighbor;
            GetComponent<CapsuleCollider>().enabled = false;
            jumpSpeed = 2*jumpSpeed;
            Jump(neighbor.transform.position);
            gm.GetComponent<EnemyManager>().ResetEnemies();
        } 
        else if (executingWind)
        {
            executingWind = false;
            jumpSpeed = jumpSpeed/2;
            GetComponent<CapsuleCollider>().enabled = true;
            CloseUmbrella();
        }
        else 
        {
            map.GetComponent<MapFeel>().PlayerLanding();
        }
    }

    public void OnDeath()
    {
        CloseUmbrella();
        GetComponent<Animator>().SetTrigger("Destroy");
    }

    public void DeathReset()
    {
        GameObject.Find("GameManager").GetComponent<DeathManager>().StartPlayerDeathCoroutine();
    }

    void OnDestroy()
    {
        if (moveAction != null)
        {
            moveAction.Disable();
        }
    }
}
