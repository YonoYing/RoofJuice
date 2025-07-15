using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public GameObject map;
    public GameObject currentBuilding;
    public GameObject targetBuilding;
    public float jumpSpeed = 5f;
    public float jumpHeight = 1f;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private bool isJumping = false;
    private Animator animator;

    void Start()
    {
        currentBuilding = map.GetComponent<MapHandler>().topBuilding;
        transform.position = currentBuilding.transform.position;
        
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        animator = GetComponent<Animator>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();

        if(context.phase == InputActionPhase.Performed && !isJumping)
        {
            if (moveInput.x > 0.5f) 
            {
                targetBuilding = currentBuilding.GetComponent<BuildingHandler>().buildings[0];
                Jump(targetBuilding.transform.position);
            }
            else if (moveInput.x < -0.5f) 
            {
                targetBuilding = currentBuilding.GetComponent<BuildingHandler>().buildings[3];
                Jump(targetBuilding.transform.position);
            }
            else if (moveInput.y > 0.5f) 
            {
                targetBuilding = currentBuilding.GetComponent<BuildingHandler>().buildings[2];
                Jump(targetBuilding.transform.position);
            }
            else if (moveInput.y < -0.5f) 
            {
                targetBuilding = currentBuilding.GetComponent<BuildingHandler>().buildings[1];
                Jump(targetBuilding.transform.position);
            }
        }
    }

    void Update()
    {
        if (Touchscreen.current == null) return;

        if (Touchscreen.current.primaryTouch.press.isPressed)
        {
            // if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            // {
            //     touchStart = Touchscreen.current.primaryTouch.position.ReadValue();
            // }
            if (Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
            {
                Debug.Log("swipe!");
            }
        }
    }

    void Jump(Vector3 targetPosition)
    {
        if (!isJumping)
        {
            // Rotate to face the direction of movement (ignore y)
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = targetRotation;
            }
            StartCoroutine(JumpCoroutine(targetPosition));
        }
    }

    IEnumerator JumpCoroutine(Vector3 targetPosition)
    {
        isJumping = true;
        animator.SetBool("jumping", true);
        
        Vector3 startPosition = transform.position;
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;
        
        while (true)
        {
            float distCovered = (Time.time - startTime) * jumpSpeed;
            float fractionOfJourney = distCovered / journeyLength;
            
            if (fractionOfJourney >= 1f)
                break;
            
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);
            float height = Mathf.Sin(fractionOfJourney * Mathf.PI) * jumpHeight;
            currentPos.y += height;
            
            transform.position = currentPos;
            yield return null;
        }
        
        transform.position = targetPosition;
        currentBuilding = targetBuilding;
        isJumping = false;
        animator.SetBool("jumping", false);
        map.GetComponent<MapFeel>().PlayerLanding();
    }

    void OnDestroy()
    {
        if (moveAction != null)
        {
            moveAction.Disable();
        }
    }
}
