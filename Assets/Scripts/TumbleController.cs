using UnityEngine;
using System.Collections;

public class TumbleController : JumpController
{
    public Transform mesh;
    public float bounceDistance = 0.2f;
    public float bounceSpeed = 8f;
    public float bounceXZAmount = 0.1f;

    new void Start()
    {
        base.Start();
        if (mesh == null)
            mesh = transform.Find("mesh");
    }

    public override void Jump(Vector3 targetPosition)
    {
        if (!isJumping)
        {
            StartCoroutine(JumpCoroutine(targetPosition));
        }
    }

    public override IEnumerator JumpCoroutine(Vector3 targetPosition)
    {
        isJumping = true;
        // if (animator)
        //     animator.SetBool("jumping", true);

        Vector3 startPosition = transform.position;
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;

        while (true)
        {
            float distCovered = (Time.time - startTime) * jumpSpeed;
            float fractionOfJourney = journeyLength > 0.001f ? distCovered / journeyLength : 1f;

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
        // if (animator)
        //     animator.SetBool("jumping", false);
        if(bounce)
            yield return StartCoroutine(Bounce());
    }

    private IEnumerator Bounce()
    {
        Vector3 originalPos = transform.position;
        Vector3 upPos = originalPos + Vector3.up * bounceDistance;

        // Random xz offset
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Vector3 xzOffset = new Vector3(randomDir.x, 0, randomDir.y) * bounceXZAmount;
        Vector3 upOffsetPos = upPos + xzOffset;
        Vector3 downOffsetPos = originalPos + xzOffset;

        float t = 0f;
        // Bounce up
        while (t < 1f)
        {
            t += Time.deltaTime * bounceSpeed;
            transform.position = Vector3.Lerp(originalPos, upOffsetPos, t);
            yield return null;
        }
        t = 0f;
        // Bounce down
        while (t < 1f)
        {
            t += Time.deltaTime * bounceSpeed;
            transform.position = Vector3.Lerp(upOffsetPos, downOffsetPos, t);
            yield return null;
        }
        t = 0f;
        // Return to original
        while (t < 1f)
        {
            t += Time.deltaTime * bounceSpeed;
            transform.position = Vector3.Lerp(downOffsetPos, originalPos, t);
            yield return null;
        }
        transform.position = originalPos;
    }

}
