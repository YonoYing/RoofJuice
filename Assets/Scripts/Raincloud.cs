using UnityEngine;
using System.Collections;

public class Raincloud : MonoBehaviour
{
    public GameObject targetBuilding;
    public float moveSpeed = 2f;
    public float rainDelay = 1f;
    public float rainDuration = 1f;
    public float umbrellaCheckDelay = 1f;
    public bool pendingDestroy;
    public Animator animator;
    public ParticleSystem rainParticleSystem;
    public Vector3 nextMoveVector;

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (rainParticleSystem == null)
            rainParticleSystem = transform.Find("rain").GetComponent<ParticleSystem>();
    }

    public IEnumerator MoveCloud(Vector3 moveBy)
    {
        Vector3 start = transform.position;
        Vector3 end = start + moveBy;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
        transform.position = end;
        if (pendingDestroy)
            Destroy(gameObject);
        else
            StartCoroutine(RainCoroutine(moveBy));
    }

    public IEnumerator RainCoroutine(Vector3 nextMove)
    {
        // 1. Set animation bool
        if (animator) animator.SetBool("Raining", true);
        // 2. Wait for rainDelay, then play particle system
        yield return new WaitForSeconds(rainDelay);
        if (rainParticleSystem) rainParticleSystem.Play();
        // 3. Start umbrella check
        StartCoroutine(UmbrellaCheckCoroutine());
        // 4. Wait for rain to finish (or a set time), then move again
        yield return new WaitForSeconds(rainDuration);
        if (animator) animator.SetBool("Raining", false);
        pendingDestroy = true;
        yield return StartCoroutine(MoveCloud(nextMove));
    }

    public IEnumerator UmbrellaCheckCoroutine()
    {
        yield return new WaitForSeconds(umbrellaCheckDelay);
        // Find player
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.currentBuilding == targetBuilding && player.umbrellaOpen)
        {
            // Player is safe, do nothing
            rainParticleSystem.Stop();
        }
        else
        {
            // Player not safe, clear tag
            TagHandler tagHandler = targetBuilding.GetComponent<TagHandler>();
            if (tagHandler != null)
                tagHandler.PuddleTag();
        }
    }
}
