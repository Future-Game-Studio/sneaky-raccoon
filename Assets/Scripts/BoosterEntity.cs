using Unity;
using UnityEngine;
using System.Collections;
using System;

public enum BoosterType {
    Empty,
    Drone,
    Shield,
    Magnet,
    Recycling
}

public delegate IEnumerator BoosterAction(Transform player);

public interface IBooster {
    BoosterAction PreformBoosterAction();    
}

public class BoosterEntity: IBooster {
    public float BoosterDuration;
    public int BoosterLevel;
    public float BoosterRange;
    public BoosterType BoosterType;
    public int TrashObjectToFindLayer = 5;
    public bool BoosterActivated = false;

    public BoosterEntity(BoosterType boosterType, float boosterDuration, int boosterLevel, float boosterRange) {
        BoosterType = boosterType;
        BoosterDuration = boosterDuration;
        BoosterLevel = boosterLevel;
        BoosterRange = boosterRange;
        BoosterActivated = false;
        Debug.Log("New booster created from parameters");
    }

    public BoosterEntity(BoosterEntity boosterEntity) {
        BoosterDuration = boosterEntity.BoosterDuration;
        BoosterLevel = boosterEntity.BoosterLevel;
        BoosterType = boosterEntity.BoosterType;
        BoosterRange = boosterEntity.BoosterRange;
        BoosterActivated = false;
        Debug.Log("New booster created from entity");
    }

    public BoosterEntity()
    {
        BoosterType = BoosterType.Empty;
        BoosterDuration = 0f;
        BoosterLevel = 0;
        BoosterRange = 0f;
        BoosterActivated = false;
        Debug.Log("New booster created - empty");
    }

    public BoosterAction PreformBoosterAction() {
        Debug.Log("Booster entity has no action!");
        BoosterActivated = true;
        return null;
    }
}

public class MagnetBooster: BoosterEntity {
    public float TrashSpeed = 10f;

    public MagnetBooster(BoosterEntity boosterEntity):base(boosterEntity) {

    }

    public new BoosterAction PreformBoosterAction(){
        if (!BoosterActivated) {
            BoosterAction boosterAction = MagnetTrash;
            return boosterAction;
        }
        else {
            Debug.Log("This booster was activated before");
            return null;
        }
    }

    private IEnumerator MagnetTrash(Transform player) {
        var durationTime = 0f;
        while (durationTime <= BoosterDuration){
            FindTrashAndMoveToPlayer(player);
            yield return new WaitForSeconds(Time.deltaTime);
            durationTime += Time.deltaTime;
        }
    }

    private void FindTrashAndMoveToPlayer(Transform player) {
        var trashColliders = Physics2D.OverlapCircleAll(player.position, BoosterRange, TrashObjectToFindLayer);

        if (trashColliders != null)
        {
            foreach (var trash in trashColliders) {
                var trashTransform = trash.transform;
                Vector3.MoveTowards(trashTransform.position, player.position, TrashSpeed * Time.deltaTime);
            }
        }
        else
            Debug.Log("No trash found in range " + BoosterRange);
    }

}

public class ShieldBooster: BoosterEntity {

    public ShieldBooster(BoosterEntity boosterEntity):base(boosterEntity) {

    }
    public new BoosterAction PreformBoosterAction(){
        if (!BoosterActivated) {
            BoosterAction boosterAction = DestroyTrashInRadius;
            return boosterAction;
        }
        else {
            Debug.Log("This booster was activated before");
            return null;
        }
    }

    private IEnumerator DestroyTrashInRadius(Transform player) {
        var durationTime = 0f;
        while (durationTime <= BoosterDuration){
            FindTrashAndDestroy(player);
            yield return new WaitForSeconds(Time.deltaTime);
            durationTime += Time.deltaTime;
        }
    }

    private void FindTrashAndDestroy(Transform player) {
        var trashColliders = Physics2D.OverlapCircleAll(player.position, BoosterRange, TrashObjectToFindLayer);

        if (trashColliders != null)
        {
            foreach (var trash in trashColliders)
                trash.transform.GetComponent<TrashController>().SelfDestroy();
        }
        else
            Debug.Log("No trash found in range " + BoosterRange);
    }

}

public class DroneBooster: BoosterEntity {

    public DroneBooster(BoosterEntity boosterEntity):base(boosterEntity) {

    }

    // TODO: implement drone controller 

    public new BoosterAction PreformBoosterAction(){
        if (!BoosterActivated) {
            BoosterAction boosterAction = DestroyTrashInRadius;
            return boosterAction;
        }
        else {
            Debug.Log("This booster was activated before");
            return null;
        }
    }

    private IEnumerator DestroyTrashInRadius(Transform player) {
        var durationTime = 0f;
        var drone = player; // TODO: implement drone search to player
        while (durationTime <= BoosterDuration){
            FindTrashAndDestroy(player);
            yield return new WaitForSeconds(Time.deltaTime); // TODO: make attackspeed to drone
            durationTime += Time.deltaTime;
        }
    }

    private void FindTrashAndDestroy(Transform drone) {
        var trashCollider = Physics2D.OverlapCircle(drone.position, BoosterRange, TrashObjectToFindLayer);

        if (trashCollider != null)
            // TODO: implement drone shooting
            Debug.Log("Shooting in trahs " + trashCollider.gameObject.name);
        else
            Debug.Log("No trash found in range " + BoosterRange);
    }

}
