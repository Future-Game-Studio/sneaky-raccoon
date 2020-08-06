using System;
using System.Collections;
using UnityEngine;

public class ScriptObjectController : MonoBehaviour
{
    [NonSerialized]
    public Vector3 aimPoint;
    [NonSerialized]
    public float torque;
    protected IEnumerator MovementRoutine(float speed)
    {
        while (true)
        {
            transform.position = Vector3.MoveTowards(transform.position, aimPoint, speed * Time.deltaTime);
            yield return new WaitForFixedUpdate();
        }
    }
}
