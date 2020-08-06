using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundParallax : MonoBehaviour
{
    /// <summary>
    /// Scrolling speed
    /// </summary>
    public Vector2 speed = new Vector2(1f, 1f);

    Vector3 prevPoint;

    private void Start() {
        prevPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
    
    void Update()
    {
        Vector2 direction = (prevPoint - Camera.main.ScreenToWorldPoint(Input.mousePosition)).normalized;

        Vector3 movement = new Vector3(
        speed.x * direction.x,
        speed.y * direction.y,
        0);

        movement *= Time.deltaTime;
        transform.Translate(movement);

        prevPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
