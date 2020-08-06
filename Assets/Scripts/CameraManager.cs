using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Vector3[] KeyPoints;
    int _currentInputPointNumber = -1;
    public float Smoothing = 5f;
    bool _isInputEnables = true;

    public void MoveCameraToPoint(int pointNumber){
        StartCoroutine(LerpTo(KeyPoints[pointNumber], 1.5f));
        _currentInputPointNumber = -1;
    }

    IEnumerator LerpTo(Vector3 newPoint, float duration) {
        _isInputEnables = false;
        
        for (float t = 0f; t < duration; t += Time.deltaTime) {
            transform.position = Vector3.Lerp(transform.position, newPoint, t / duration);
            yield return 0;
        }
        
        transform.position = newPoint;
        _isInputEnables = true;
    }
}
