using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    GameObject player3D;
    // Start is called before the first frame update
    void Start()
    {
        player3D = GameObject.Find("Player3D");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerInfo.DEATH)
        {
            gameObject.transform.root.position = player3D.transform.position + (-player3D.transform.forward * 15f);
            Debug.Log("died");
        }
    }
}
