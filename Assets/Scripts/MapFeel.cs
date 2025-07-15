using UnityEngine;
using System.Collections;

public class MapFeel : MonoBehaviour
{
    public GameObject topBuilding;
    public float landingBounceDistance = 0.2f;
    public float landingBounceSpeed = 8f;

    public void PlayerLanding()
    {
        StopCoroutine("LandingBounceCoroutine");
        StartCoroutine(LandingBounceCoroutine());
    }

    private IEnumerator LandingBounceCoroutine()
    {
        Transform mapTransform = transform;
        Vector3 originalPos = mapTransform.localPosition;
        Vector3 downPos = originalPos + Vector3.down * landingBounceDistance;
        float t = 0f;
        
        while (t < 1f)
        {
            t += Time.deltaTime * landingBounceSpeed;
            mapTransform.localPosition = Vector3.Lerp(originalPos, downPos, t);
            yield return null;
        }
        
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * landingBounceSpeed;
            mapTransform.localPosition = Vector3.Lerp(downPos, originalPos, t);
            yield return null;
        }
        mapTransform.localPosition = originalPos;
    }
}
