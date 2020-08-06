using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class ScriptTrashCloudsController : ScriptObjectController
{
    [NonSerialized]
    public float speed;

    void Start()
    {
        /** Random size **/
        float scaleCoefficient = Random.Range(-.1f, .1f);
        transform.localScale = new Vector3(
            transform.localScale.x + scaleCoefficient, 
            transform.localScale.y + scaleCoefficient, 
            transform.localScale.z + scaleCoefficient
            );
        StartCoroutine(MovementRoutine(speed));
        StartCoroutine(RotateByTorque());
        StartCoroutine(CheckDestruction());
    }

    /** Rotate the gameObject **/
    IEnumerator RotateByTorque()
    {
        while (true)
        {
            transform.Rotate(new Vector3(0, 0, 1), Time.deltaTime * (50 + torque), Space.World);
            yield return new WaitForFixedUpdate();
        }
    }

    /** Check whether the object gets out of the screen if so then destroy **/
    IEnumerator CheckDestruction()
    {
        while (true)
        {
            if(transform.position.x < -ScriptStaticVars.horizontalSizePlusOne)
            {
                Destroy(gameObject);
            }
            yield return new WaitForSeconds(1);
        }
    }
}
