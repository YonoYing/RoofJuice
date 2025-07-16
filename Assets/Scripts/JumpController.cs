using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class JumpController : MonoBehaviour
{
    [SerializeField] public GameObject map;
    [SerializeField] public GameObject currentBuilding;
    [SerializeField] public GameObject targetBuilding;
    [SerializeField] public float jumpSpeed = 1f;
    [SerializeField] public float jumpHeight = 1f;

    public bool isJumping = false;
    [SerializeField] public Animator animator;

    public void Start()
    {
        transform.position = currentBuilding.transform.position;
        animator = GetComponent<Animator>();
    }

    public virtual void Jump(Vector3 targetPosition)
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

    public virtual IEnumerator JumpCoroutine(Vector3 targetPosition)
    {
        isJumping = true;
        if(animator)
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
        if(animator)
            animator.SetBool("jumping", false);
    }
}
