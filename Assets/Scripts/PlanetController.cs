using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetController : MonoBehaviour {
    public string name = "planet";
    public int order;
    public float RotationSpeed = 0.1f;

    /* difference between events mousedown and mouseup */
    private float difMouseUpDown = 0;

    private int garbagePollution = 0; // Varible to value of garbage pollution
    private int maxGarbage = 800;

    void FixedUpdate() {
        transform.Rotate(0f, 0f, RotationSpeed);
    }

    /* get the time when occur the event OnMouseDown  */
    private void OnMouseDown()
    {
        difMouseUpDown = Time.time;
    }

    /* get the time when occur the event OnMouseUp  */
    void OnMouseUp()
    {
        /* Check if this is the mouse click */
        if(Time.time - difMouseUpDown < .1)
        {
            Debug.Log("name = " + name);
            UIController.CurrentUIController.UnityEventPlanetSelected.Invoke(gameObject);
        }
    }

    public void DamageToPlanet(int damage) {
        garbagePollution += damage;

        if (garbagePollution > maxGarbage) {
            StopGameFullOfTrash();
        }
    }

    private void StopGameFullOfTrash() {
        // Here have to be explousion of planet

        GameObject.FindGameObjectWithTag("Respawn").GetComponent<SpawnManager>().StopSpawning();
        Debug.Log("Planet is full of trash!!!");
    }
    
    private void OnTriggerEnter2D(Collider2D collision) {
        // Adding damage

        if (collision.gameObject.tag == "Trash") {
            DamageToPlanet(collision.GetComponent<TrashController>().trashItem.damageToPlanet);
        }
    }
}
