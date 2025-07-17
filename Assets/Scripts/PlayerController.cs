using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : JumpController
{
    private PlayerInput playerInput;
    private InputAction moveAction;
    private bool pendingDeath = false;
    private Vector3 offMapJumpTarget;
    
    public GameObject umbrella;
    public bool umbrellaOpen, pendingWind, executingWind;
    public float umbrellaAutoCloseTime = 1f; // Time in seconds before umbrella auto-closes
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
        if(!umbrellaOpen)
        {
            umbrellaOpen = true;
            umbrella.GetComponent<Animator>().SetBool("Open", true);
            StartCoroutine(UmbrellaTimerCoroutine());
        }
    }

    private void CloseUmbrella()
    {
        if (!pendingWind)
        {
            umbrellaOpen = false;
            umbrella.GetComponent<Animator>().SetBool("Open", false);
        }
    }

    private IEnumerator UmbrellaTimerCoroutine()
    {
        yield return new WaitForSeconds(umbrellaAutoCloseTime);
        CloseUmbrella();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!context.performed || isJumping) return;
        Vector2 moveInput = context.ReadValue<Vector2>();
        int dir = -1;
        if (moveInput.x > 0.5f) dir = 0; // right
        else if (moveInput.x < -0.5f) dir = 3; // left
        else if (moveInput.y > 0.5f) dir = 2; // up
        else if (moveInput.y < -0.5f) dir = 1; // down

        if (dir == -1) return;
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

    void DeathJump(int dir) 
    {
        Vector3 jumpDir = Vector3.zero;
        if (dir == 0) jumpDir = Vector3.right;
        else if (dir == 1) jumpDir = Vector3.back;
        else if (dir == 2) jumpDir = Vector3.forward;
        else if (dir == 3) jumpDir = Vector3.left;
        offMapJumpTarget = currentBuilding.transform.position + jumpDir + Vector3.up;
        pendingDeath = true;
        Jump(offMapJumpTarget);
    }

    public override IEnumerator JumpCoroutine(Vector3 targetPosition)
    {
        var baseEnum = base.JumpCoroutine(targetPosition);
        while (baseEnum.MoveNext())
            yield return baseEnum.Current;
        if (pendingDeath && (targetPosition == offMapJumpTarget))
        {
            pendingDeath = false;
            gm.GetComponent<DeathManager>().OnPlayerDeath();
        }
        else if (pendingWind)
        {
            pendingWind = false;
            executingWind = true;
            GameObject neighbor = currentBuilding.GetComponent<BuildingHandler>().buildings[0];
            targetBuilding = neighbor;
            Jump(neighbor.transform.position);
            gm.GetComponent<EnemyManager>().ResetEnemies();
        } 
        else if (executingWind)
        {
            executingWind = false;
            CloseUmbrella();
        }
    }

    public void OnDeath()
    {
        CloseUmbrella();
    }

    void OnDestroy()
    {
        if (moveAction != null)
        {
            moveAction.Disable();
        }
    }
}
