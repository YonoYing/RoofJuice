using UnityEngine;
using System;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public bool nextLevel = false;
    public float moveDuration = 1.5f;
    private bool hasMoved = false;

    public Coroutine SmoothMoveCamera(Vector3 startPos, Vector3 moveBy, float duration, Action onComplete)
    {
        return StartCoroutine(SmoothMoveCameraCoroutine(startPos, moveBy, duration, onComplete));
    }

    private IEnumerator SmoothMoveCameraCoroutine(Vector3 startPos, Vector3 moveBy, float duration, Action onComplete)
    {
        Vector3 initialPos = startPos;
        Vector3 targetPos = startPos + moveBy;
        float elapsed = 0f;
        transform.position = initialPos;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(initialPos, targetPos, t);
            yield return null;
        }
        transform.position = targetPos;
        if (onComplete != null)
            onComplete();
    }
}

