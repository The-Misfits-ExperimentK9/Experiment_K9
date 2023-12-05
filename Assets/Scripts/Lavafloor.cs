using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Lavafloor : MonoBehaviour
{
    [SerializeField] AudioClip bubbling;
    [SerializeField] AudioClip death;
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource.clip = bubbling;
        audioSource.loop = true;
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == LayerInfo.PLAYER) {
            audioSource.clip = death;
            audioSource.Play();
            Debug.Log("Resetting scene");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
