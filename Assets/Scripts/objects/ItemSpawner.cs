using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemSpawner : ActivatablePuzzlePiece {
    [SerializeField] protected GameObject itemToSpawn;
    [SerializeField] protected GameObject spawnedObject;
    public override void Activate() {
        SpawnItem();
    }

    public override void Deactivate(GameObject caller) {
        //do nothing
    }

    protected virtual void SpawnItem() {
        if (spawnedObject != null) {
            Destroy(spawnedObject);
        }
        spawnedObject = Instantiate(itemToSpawn, transform.position, Quaternion.identity);
    }

    private void OnTriggerExit(Collider other) {
        if (spawnedObject && other.gameObject == spawnedObject) {
            SpawnItem();
        }
    }
}
