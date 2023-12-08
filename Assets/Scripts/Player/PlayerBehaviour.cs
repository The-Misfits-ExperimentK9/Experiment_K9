using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
public class PlayerBehaviour : MonoBehaviour
{
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
    [SerializeField] private LayerMask receiverLayers; //layer mask for the ball receivers
    private List<Collider> receivables = new(); //list of potential ball receivers
    private BallReceiver closestBallReceiver;
    [Header("Development")]
    public bool EnableItemSpawning = true;
    public List<GameObject> items = new();
    public bool TutorialEnabled = true;
    private KeyControl spawnKey1, spawnKey2, tutorialSkipKey;
    [Header("Audio/Barking")]
    [SerializeField] List<AudioClip> barks;
    AudioSource audioSource;
    int barkIndex;
    int barkChance;
    int randomBarkNumber;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    private void Start()
    {
        //set singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        //set reset key to R key and set the radius of the interact radar
        resetKey = Keyboard.current.rKey;
        spawnKey1 = Keyboard.current.digit1Key;
        spawnKey2 = Keyboard.current.digit2Key;
        tutorialSkipKey = Keyboard.current.f1Key;
        interactRadarGameObject.GetComponent<SphereCollider>().radius = interactDisplayRadius;
        //grab the controller scripts from the 3d player object
        if (player3D != null)
        {
            playerController = player3D.GetComponent<CharacterController>();
            thirdPersonController = player3D.GetComponent<ThirdPersonController>();
        }
        else
        {
            Debug.LogError("Missing player 3d when assigning controller scripts");
        }
        // At the start, the random number the random barking mechanism needs
        // to hit will be randomly generated.
        randomBarkNumber = Random.Range(1, 20001);
    }
    public void SetPaused(bool paused)
    {
        thirdPersonController.SetPaused(paused);
        this.paused = paused;
    }
    void Update()
    {
        if (tutorialSkipKey.wasPressedThisFrame)
        {
            TutorialEnabled = false;
            var triggers = FindObjectsOfType<TriggerNarration>();
            foreach (var trigger in triggers)
            {
                trigger.StopNarration();
            }
            // Each frame, barkChance will get assigned to a random number between
            // 1 and 2000.
            barkChance = Random.Range(1, 20001);
            // Regardless of player input, if barkChance lands on the random number
            // generated, the dog will bark, and the random number gets re-rolled.
            if (barkChance == randomBarkNumber)
            {
                Bark();
                randomBarkNumber = Random.Range(1, 20001);
            }
            if (!paused)
            {
                if (canResetLocation)
                {
                    HandleResetInput();
                }
                //only check ball receivers while holding object and in 3d
                if (pickupController.IsHoldingObject() && is3D)
                {
                    FindNearestReceiverAndActivateHologram();
                }
                if (EnableItemSpawning)
                {
                    HandleItemSpawning();
                }
            }
        }
    }
        void HandleItemSpawning()
        {
            if (spawnKey1.wasPressedThisFrame)
            {
                Debug.Log("one key pressed");
                var item = items[0];
                if (item != null)
                {
                    Instantiate(item, player3D.transform.position + player3D.transform.forward * 5, Quaternion.identity);
                }
            }
            if (spawnKey2.wasPressedThisFrame)
            {
                var item = items[1];
                if (item != null)
                {
                    Instantiate(item, player3D.transform.position + player3D.transform.forward * 10, Quaternion.identity);
                }
            }
        }
        private void FindNearestReceiverAndActivateHologram()
        {
            //enable the closest one
            var closestReceiver = GetClosest3DObjectInColliderArray(receivables);
            if (closestReceiver != null)
            {
                var receiver = closestReceiver.GetComponent<BallReceiver>();
                if (receiver.HoloIsOn)
                    return;
                receiver.ActivateHolo();
                closestBallReceiver = receiver;
            }
            if (receivables.Count > 1)
            {
                receivables.ForEach(collider => {
                    if (collider.gameObject != closestReceiver)
                    {
                        var receiver = collider.GetComponent<BallReceiver>();
                        receiver.DeactivateHolo();
                    }
                });
            }
        }
        public void AddReceivableToList(Collider receivable)
        {
            if (receivables.Contains(receivable))
            {
                return;
            }
            receivables.Add(receivable);
        }
        public void RemoveReceivableFromList(Collider receivable)
        {
            if (receivable.TryGetComponent(out BallReceiver component))
            {
                if (component == closestBallReceiver)
                    closestBallReceiver = null;
                component.DeactivateHolo();
            }
            receivables.Remove(receivable);
        }
        private void HandleResetInput()
        {
            if (is3D)
            {
                if (resetKey.wasPressedThisFrame)
                {
                    ResetPlayerPosition();
                }
            }
        }
        private void ResetPlayerPosition()
        {
            Debug.Log("resetting player to: " + spawnPoint.transform.position);
            Spawn();
        }
        //attempts to spawn the player at the spawn point in the secne
        //will try and find a spawn point if one is not set
        //throws an error if no spawn point was set and none was found
        public void Spawn()
        {
            if (spawnPoint != null)
            {
                Move3DPlayerToLocation(spawnPoint.transform.position);
            }
            else
            {
                Debug.LogWarning("Missing spawn point");
                var spawn = GameObject.FindWithTag("PlayerSpawnPoint");
                if (spawn == null)
                {
                    throw new System.Exception("Missing spawn point in level " + SceneManager.GetActiveScene().name);
                }
                Move3DPlayerToLocation(spawn.transform.position);
            }
        }
        //moves the 3d player to the given location
        public void Move3DPlayerToLocation(Vector3 location)
        {
            Debug.Log(thirdPersonController == null);
            player3D.SetActive(false);
            player3D.transform.position = location;
            player3D.SetActive(true);
            StartCoroutine(EnablePlayerMovementOnNextFrame());
        }
        //enables player movement after 2 frames
        private IEnumerator EnablePlayerMovementOnNextFrame()
        {
            for (int x = 0; x < 2; x++)
            {
                yield return new WaitForEndOfFrame();
            }
            thirdPersonController.ToggleMovement(true);
            yield return null;
        }
        //swap between dimensions
        public void ChangeDimension()
        {
            is3D = !is3D;
            //handle changing the held object dimension
            pickupController.ChangeDimension();
        }
        //returns true if the game is in 3d mode
        public bool IsIn3D() { return is3D; }
        public GameObject GetClosest3DObjectOnLayers(LayerMask layers)
        {
            // Perform the overlap sphere and get the colliders within the specified radius.
            Collider[] interactableColliders = Physics.OverlapSphere(player3D.transform.position, interactDisplayRadius, layers);
            return GetClosest3DObjectInColliderArray(interactableColliders);
        }
        private GameObject GetClosest3DObjectInColliderArray(List<Collider> interactableColliders)
        {
            return GetClosest3DObjectInColliderArray(interactableColliders.ToArray());
        }
        private GameObject GetClosest3DObjectInColliderArray(Collider[] colliders)
        {
            if (colliders.Length == 0)
                return null;
            if (colliders.Length == 1)
                return colliders[0].gameObject;
            // Initialize variables to keep track of the closest object.
            GameObject closestObject = null;
            float smallestOrthogonalDistance = float.MaxValue;
            Ray cameraRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            foreach (var collider in colliders)
            {
                // Get the closest point on the camera ray to the object's position.
                Vector3 closestPointOnRay = cameraRay.GetPoint(Vector3.Dot(collider.transform.position - cameraRay.origin, cameraRay.direction));
                // Calculate the orthogonal distance from the object to the ray.
                float orthogonalDistance = Vector3.Distance(collider.transform.position, closestPointOnRay);
                // Check if this collider is closer to the camera's forward direction than the previous ones.
                if (orthogonalDistance < smallestOrthogonalDistance)
                {
                    // var vecFromCameraToObject = collider.transform.position- Camera.main.transform.position;
                    //   var cameraForward = Camera.main.transform.forward;
                    Vector3 viewportPos = Camera.main.WorldToViewportPoint(collider.transform.position);
                    // Check if the object is within the viewport bounds
                    bool isOnScreen = viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1;
                    //allow picking up items in 90 degree cone in front of camera
                    if (isOnScreen)
                    {
                        smallestOrthogonalDistance = orthogonalDistance;
                        closestObject = collider.gameObject;
                    }
                }
            }
            // Return the closest interactable object or null if none was found.
            return closestObject;
        }
        public BallReceiver GetClosestReceiver()
        {
            return closestBallReceiver;
        }
        public void Bark()
        {
            barkIndex = Random.Range(0, barks.Count);
            audioSource.clip = barks[barkIndex];
            audioSource.Play();
        }
        //public GameObject GetClosestReciever() {
        //    // Perform the overlap sphere and get the colliders within the specified radius.
        //    Collider[] eventTriggerColliders = Physics.OverlapSphere(player3D.transform.position + player3D.transform.forward, 10, LayerMask.GetMask("EventTrigger"));
        //    // Initialize variables to keep track of the closest object.
        //    GameObject closestObject = null;
        //    float smallestOrthogonalDistance = float.MaxValue;
        //    Ray cameraRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        //    foreach (var collider in eventTriggerColliders) {
        //        if (collider.TryGetComponent(out BallReceiver b)) {
        //            // Get the closest point on the camera ray to the object's position.
        //            Vector3 closestPointOnRay = cameraRay.GetPoint(Vector3.Dot(collider.transform.position - cameraRay.origin, cameraRay.direction));
        //            // Calculate the orthogonal distance from the object to the ray.
        //            float orthogonalDistance = Vector3.Distance(collider.transform.position, closestPointOnRay);
        //            // Check if this collider is closer to the camera's forward direction than the previous ones.
        //            if (orthogonalDistance < smallestOrthogonalDistance) {
        //                smallestOrthogonalDistance = orthogonalDistance;
        //                closestObject = collider.gameObject;
        //            }
        //        }
        //    }
        //    // Return the closest interactable object or null if none was found.
        //    return closestObject;
        //}
    }
