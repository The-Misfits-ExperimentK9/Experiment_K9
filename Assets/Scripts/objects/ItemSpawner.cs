using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemSpawner : ActivatablePuzzlePiece
{
    [SerializeField] private GameObject itemToSpawn;
    [SerializeField] private GameObject spawnedObject;
    public override void Activate() {
        SpawnItem();
    }

    public override void Deactivate(GameObject caller) {
        //do nothing
    }


    void SpawnItem() {
        if (spawnedObject != null) {
            Destroy(spawnedObject);
        }
        spawnedObject = Instantiate(itemToSpawn, transform.position, Quaternion.identity);
    }
    private void OnTriggerExit(Collider other) {
        if (other.gameObject.layer == LayerInfo.INTERACTABLE_OBJECT) {
            if (Vector3.Distance(other.transform.position, transform.position) > 10f) {
                SpawnItem();
            }
        }
    }

}
