using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiItemSpawner : ItemSpawner
{
    [SerializeField] private List<GameObject> spawnedObjects;

    protected override void SpawnItem()
    {
        spawnedObject = Instantiate(itemToSpawn, transform.position, Quaternion.identity);
        spawnedObjects.Add(spawnedObject);
    }
}
