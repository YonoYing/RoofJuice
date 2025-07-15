using UnityEngine;
using System.Collections;

public class MapFeel : MonoBehaviour
{
    public GameObject player;
    public float landingBounceDistance = 0.2f;
    public float landingBounceSpeed = 8f;
    public float completionBounceDistance = 0.2f;
    public float completionBounceSpeed = 8f;

    public void PlayerLanding()
    {
        StopCoroutine("LandingBounceCoroutine");
        StartCoroutine(LandingBounceCoroutine(Vector3.down, gameObject, landingBounceSpeed, landingBounceDistance));
    }

    public void MapCompleted()
    {
        StopCoroutine("LandingBounceCoroutine");
        StartCoroutine(LandingBounceCoroutine(Vector3.up, gameObject, completionBounceSpeed, completionBounceDistance));
        StartCoroutine(LandingBounceCoroutine(Vector3.up, player, completionBounceSpeed, completionBounceDistance*2));
    }

    private IEnumerator LandingBounceCoroutine(Vector3 direction, GameObject target, float speed, float distance)
    {
        Transform mapTransform = target.transform;
        Vector3 originalPos = mapTransform.localPosition;
        Vector3 downPos = originalPos + direction * distance;
        float t = 0f;
        
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            mapTransform.localPosition = Vector3.Lerp(originalPos, downPos, t);
            yield return null;
        }
        
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            mapTransform.localPosition = Vector3.Lerp(downPos, originalPos, t);
            yield return null;
        }
        mapTransform.localPosition = originalPos;
    }
}
