using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Booster : MonoBehaviour {

    public BoosterEntity ThisBooster;
    private Coroutine _boosterActionRoutine = null;
    private Transform _player;

    public BoosterType ThisBoosterType = BoosterType.Magnet;
    public float ThisBoosterRange = 0.5f;
    public float ThisBoosterDuration = 10f;
    public int ThisBoosterLevel = 1;

    void Start() {
        InitializeBooster(ThisBoosterType, ThisBoosterDuration, ThisBoosterLevel, ThisBoosterRange);
        ActivateBooster();
    }

    public void InitializeBooster(BoosterEntity boosterEntity) {
        if (ThisBooster == null) {
            Debug.Log("Initializing new " + boosterEntity.BoosterLevel + "-level booster " 
            + boosterEntity.BoosterType + " for " + boosterEntity.BoosterDuration + " seconds with " + boosterEntity.BoosterRange + " range");
            CreateCorrectBoosterType(boosterEntity);
        }
        else
            Debug.Log("This booster already initialized!");
    }

    public void InitializeBooster(BoosterType boosterType, float boosterDuration, int boosterLevel, float boosterRange) {
        if (ThisBooster == null){
            Debug.Log("Initializing new " + boosterLevel + "-level booster " + boosterType + " for " 
            + boosterDuration + " seconds with " + boosterRange + " range");
            var boosterEntity = new BoosterEntity(boosterType, boosterDuration, boosterLevel, boosterRange);
            CreateCorrectBoosterType(boosterEntity);
        }
        else
            Debug.Log("This booster already initialized!");
    }

    private void CreateCorrectBoosterType(BoosterEntity boosterEntity) {
        switch (boosterEntity.BoosterType) {
            case BoosterType.Magnet:
                ThisBooster = new MagnetBooster(boosterEntity);
                break;

            case BoosterType.Drone:
                ThisBooster = new DroneBooster(boosterEntity);
                break;

            case BoosterType.Shield:
                ThisBooster = new ShieldBooster(boosterEntity);
                break;

            default:
                ThisBooster = new BoosterEntity(boosterEntity);
                break;
        }
    }

    public void ActivateBooster(){
        if (_player != null)
        {
            Debug.Log("Activating booster...");
            if (ThisBooster != null) {
                BoosterAction boosterAction = ThisBooster.PreformBoosterAction();
                if (_boosterActionRoutine == null) {
                    _boosterActionRoutine = StartCoroutine(boosterAction(_player));
                    Debug.Log("Booster "+ gameObject.name +" activated!");
                }
            }
            else
                Debug.Log("No booster found here!");
        }
        else {
            Debug.Log("Player not found for booster " + gameObject.name);
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "player")
        {
            Debug.Log("Booster " + gameObject.name + " collided with player");
            _player = other.transform;
        }    
    }

}
