using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudsGenerator : MonoBehaviour {
    private System.Random rand = new System.Random();

    public int quantity;
    public int MIN_QUAN = 30;
    public int MAX_QUAN = 60;

    public int MIN_ANGLE = 0;
    public int MAX_ANGLE = 360;

    public int MIN_UPDOWN = 780;
    public int MAX_UPDOWN = 865;

    public GameObject cloudPrefab;
    public Sprite[] cloudTypes;

    void Start() {
        SetValuesForGeneration();
        GenerateClouds();
    }

    void SetValuesForGeneration() {
        quantity = rand.Next(MIN_QUAN, MAX_QUAN);
        Debug.Log("Quantity of clouds in sky: " + quantity);
    }

    void GenerateClouds() {
        for(int i = 0; i < quantity; i++) {
            var cloudRot = Instantiate(cloudPrefab, transform);
            cloudRot.transform.SetParent(transform);
            cloudRot.transform.rotation = Quaternion.Euler(0, 0, rand.Next(MIN_ANGLE, MAX_ANGLE));

            GameObject upDownCloud = cloudRot.gameObject.transform.GetChild(0).gameObject;
            upDownCloud.transform.localPosition =  new Vector3(
                upDownCloud.transform.localPosition.x, 
                rand.Next(MIN_UPDOWN, MAX_UPDOWN)/100.0f, 
                upDownCloud.transform.localPosition.z);

            upDownCloud.GetComponent<SpriteRenderer>().sprite = cloudTypes[rand.Next(0, cloudTypes.Length)];
        }
    }
}
