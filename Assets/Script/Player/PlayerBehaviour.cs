using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

public class PlayerBehaviour : MonoBehaviour {

    
                                                           //  private bool canMove = true;                            //disable or enable player movement

    [Header("Player References")]
    public GameObject player2D;                             //holds the 2d depiction of the player
    public MovementController_2D player2DMovementController;           //holds the 2d depiction of the player
    public GameObject player3D;                             //holds the 3d depiction of the player
    public CharacterController playerController;
    public ThirdPersonController thirdPersonController;
    public GameObject interactRadarGameObject;          //holds the game object that has the radar collider on it
    public InteractRadarController interactRadar;          //holds the game object that has the radar collider on it

    public PickupController pickupController;
    public PlayerDimensionController playerDimensionController;

    [Header("Interface")]
    public InterfaceBehaviour interfaceScript;

    [Header("Settings")]
    
    [SerializeField] private bool canResetLocation = true;
    public bool is3D = true;                               //handles checking if the player is in 3d or 2d mode

    private KeyControl resetKey;                             //which key to use for interaction, set in Start()
    [Header("Spawn")]
    [SerializeField] private GameObject spawnPoint;

    public static PlayerBehaviour Instance;

    bool paused = false;

    [Header("Interacting")]
    public float interactDisplayRadius = 20f; //radius of the collider to determine the range at which the player can interact
    [SerializeField] private LayerMask interactableLayerMask; //layer mask for the interactable objects



    private void Start() {
        //set singleton
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(this);
        }
        //set reset key to R key and set the radius of the interact radar
        resetKey = Keyboard.current.rKey;
        interactRadarGameObject.GetComponent<SphereCollider>().radius = interactDisplayRadius;

        //grab the controller scripts from the 3d player object
        if (player3D != null) {
            playerController = player3D.GetComponent<CharacterController>();
            thirdPersonController = player3D.GetComponent<ThirdPersonController>();
        }
        else {
            Debug.LogError("Missing player 3d when assigning controller scripts");
        }
    }
    
    public void SetPaused(bool paused) {
        thirdPersonController.SetPaused(paused);
        this.paused = paused;
    }

    void Update() {
        
        if (!paused) {
            
            if (canResetLocation) {
                HandleResetInput();
            }
        }
    }
    
   private void HandleResetInput() {
        if (is3D) {
            if (resetKey.wasPressedThisFrame) {
                ResetPlayerPosition();
            }
        }
    }
    private void ResetPlayerPosition() {
        Debug.Log("resetting player to: " + spawnPoint.transform.position);
        Spawn();


    }
    //attempts to spawn the player at the spawn point in the secne
    //will try and find a spawn point if one is not set
    //throws an error if no spawn point was set and none was found
    public void Spawn() {
        if (spawnPoint != null) {
            Move3DPlayerToLocation(spawnPoint.transform.position);
        }
        else {
            Debug.LogWarning("Missing spawn point");
            var spawn = GameObject.FindWithTag("PlayerSpawnPoint");
            if (spawn == null) {
                throw new System.Exception("Missing spawn point in level " + SceneManager.GetActiveScene().name);
            }
            Move3DPlayerToLocation(spawn.transform.position);

        }
    }
    //moves the 3d player to the given location
    public void Move3DPlayerToLocation(Vector3 location) {
        Debug.Log(thirdPersonController == null);
        player3D.SetActive(false);
        player3D.transform.position = location;
        player3D.SetActive(true);
        StartCoroutine(EnablePlayerMovementOnNextFrame());
    }
    //enables player movement after 2 frames
    private IEnumerator EnablePlayerMovementOnNextFrame() {
        for (int x = 0; x < 2; x++) {
            yield return new WaitForEndOfFrame();
        }
        thirdPersonController.ToggleMovement(true);
        yield return null;
    }

    //swap between dimensions
    public void ChangeDimension() {
        is3D = !is3D;
        //handle changing the held object dimension
        pickupController.ChangeDimension();

    }
    //returns true if the game is in 3d mode
    public bool IsIn3D() { return is3D; }

    public GameObject GetClosestInteractable3D() {
        // Perform the overlap sphere and get the colliders within the specified radius.
        Collider[] interactableColliders = Physics.OverlapSphere(player3D.transform.position, interactDisplayRadius, interactableLayerMask);

        // Initialize variables to keep track of the closest object.
        GameObject closestObject = null;
        float smallestOrthogonalDistance = float.MaxValue;
        Ray cameraRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        foreach (var collider in interactableColliders) {
            // Get the closest point on the camera ray to the object's position.
            Vector3 closestPointOnRay = cameraRay.GetPoint(Vector3.Dot(collider.transform.position - cameraRay.origin, cameraRay.direction));
            // Calculate the orthogonal distance from the object to the ray.
            float orthogonalDistance = Vector3.Distance(collider.transform.position, closestPointOnRay);

            // Check if this collider is closer to the camera's forward direction than the previous ones.
            if (orthogonalDistance < smallestOrthogonalDistance) {
                smallestOrthogonalDistance = orthogonalDistance;
                closestObject = collider.gameObject;
            }
        }

        if (closestObject == null)
            Debug.Log("no object found");
        else {
            Debug.Log(closestObject.name);
        }
        
        // Return the closest interactable object or null if none was found.
        return closestObject;
    }

}

