using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using Microsoft.Azure.SpatialAnchors.Unity.Examples;

public class AnchorAdder : InputInteractionBase
{
    /// <summary>
    /// Gets the <see cref="SpatialAnchorManager"/> instance used by this demo.
    /// </summary>
    public SpatialAnchorManager CloudManager;
    public GameObject AnchoredObjectPrefab;
    protected AnchorLocateCriteria anchorLocateCriteria = null;
    bool isPlacingObject = false;
    GameObject spawnedObject = null;
    CloudSpatialAnchor currentCloudAnchor;

    public override void Start()
    {
        base.Start();
        CloudManager.SessionUpdated += CloudManager_SessionUpdated;
        CloudManager.AnchorLocated += CloudManager_AnchorLocated;
        CloudManager.LocateAnchorsCompleted += CloudManager_LocateAnchorsCompleted;
        CloudManager.LogDebug += CloudManager_LogDebug;
        CloudManager.Error += CloudManager_Error;
        anchorLocateCriteria = new AnchorLocateCriteria();
        Setup();
    }

    async void Setup()
    {
        if (CloudManager.Session == null)
        {
            await CloudManager.CreateSessionAsync();
        }
        await CloudManager.StartSessionAsync();
        isPlacingObject = true;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    private void CloudManager_AnchorLocated(object sender, AnchorLocatedEventArgs args)
    {
        Debug.LogFormat("Anchor recognized as a possible anchor {0} {1}", args.Identifier, args.Status);
        if (args.Status == LocateAnchorStatus.Located)
        {
            OnCloudAnchorLocated(args);
        }
    }

    private void CloudManager_LocateAnchorsCompleted(object sender, LocateAnchorsCompletedEventArgs args)
    {
        OnCloudLocateAnchorsCompleted(args);
    }

    private void CloudManager_SessionUpdated(object sender, SessionUpdatedEventArgs args)
    {
        OnCloudSessionUpdated();
    }

    private void CloudManager_Error(object sender, SessionErrorEventArgs args)
    {
        Debug.Log(args.ErrorMessage);
    }

    private void CloudManager_LogDebug(object sender, OnLogDebugEventArgs args)
    {
        Debug.Log(args.Message);
    }

    /// <summary>
    /// Called when cloud anchor location has completed.
    /// </summary>
    /// <param name="args">The <see cref="LocateAnchorsCompletedEventArgs"/> instance containing the event data.</param>
    void OnCloudLocateAnchorsCompleted(LocateAnchorsCompletedEventArgs args)
    {
        Debug.Log("Locate pass complete");
    }

    /// <summary>
    /// Called when a cloud anchor is located.
    /// </summary>
    /// <param name="args">The <see cref="AnchorLocatedEventArgs"/> instance containing the event data.</param>
    void OnCloudAnchorLocated(AnchorLocatedEventArgs args)
    {
        // To be overridden.
    }

    /// <summary>
    /// Called when the current cloud session is updated.
    /// </summary>
    void OnCloudSessionUpdated()
    {
        // To be overridden.
    }

    /// <summary>
    /// Called when a touch interaction occurs.
    /// </summary>
    /// <param name="touch">The touch.</param>
    protected override void OnTouchInteraction(Touch touch)
    {
        if (touch.phase == TouchPhase.Ended)
        {
            OnTouchInteractionEnded(touch);
        }
    }

    /// <summary>
    /// Called when a touch object interaction occurs.
    /// </summary>
    /// <param name="hitPoint">The position.</param>
    /// <param name="target">The target.</param>
    protected override void OnSelectObjectInteraction(Vector3 hitPoint, object target)
    {
        if (isPlacingObject)
        {
            Quaternion rotation = Quaternion.AngleAxis(0, Vector3.up);

            SpawnOrMoveCurrentAnchoredObject(hitPoint, rotation);
        }
    }

    /// <summary>
    /// Spawns a new anchored object and makes it the current object or moves the
    /// current anchored object if one exists.
    /// </summary>
    /// <param name="worldPos">The world position.</param>
    /// <param name="worldRot">The world rotation.</param>
    void SpawnOrMoveCurrentAnchoredObject(Vector3 worldPos, Quaternion worldRot)
    {
        // Create the object if we need to, and attach the platform appropriate
        // Anchor behavior to the spawned object
        if (spawnedObject == null)
        {
            // Use factory method to create
            spawnedObject = SpawnNewAnchoredObject(worldPos, worldRot, currentCloudAnchor);
        }
        else
        {
            // Use factory method to move
            MoveAnchoredObject(spawnedObject, worldPos, worldRot, currentCloudAnchor);
        }
    }

    /// <summary>
    /// Spawns a new object.
    /// </summary>
    /// <param name="worldPos">The world position.</param>
    /// <param name="worldRot">The world rotation.</param>
    /// <param name="cloudSpatialAnchor">The cloud spatial anchor.</param>
    /// <returns><see cref="GameObject"/>.</returns>
    GameObject SpawnNewAnchoredObject(Vector3 worldPos, Quaternion worldRot, CloudSpatialAnchor cloudSpatialAnchor)
    {
        // Create the object like usual
        GameObject newGameObject = SpawnNewAnchoredObject(worldPos, worldRot);

        // If a cloud anchor is passed, apply it to the native anchor
        if (cloudSpatialAnchor != null)
        {
            CloudNativeAnchor cloudNativeAnchor = newGameObject.GetComponent<CloudNativeAnchor>();
            cloudNativeAnchor.CloudToNative(cloudSpatialAnchor);
        }

        // Return newly created object
        return newGameObject;
    }

    /// <summary>
    /// Moves the specified anchored object.
    /// </summary>
    /// <param name="objectToMove">The anchored object to move.</param>
    /// <param name="worldPos">The world position.</param>
    /// <param name="worldRot">The world rotation.</param>
    /// <param name="cloudSpatialAnchor">The cloud spatial anchor.</param>
    void MoveAnchoredObject(GameObject objectToMove, Vector3 worldPos, Quaternion worldRot, CloudSpatialAnchor cloudSpatialAnchor = null)
    {
        // Get the cloud-native anchor behavior
        CloudNativeAnchor cna = objectToMove.GetComponent<CloudNativeAnchor>();

        // Warn and exit if the behavior is missing
        if (cna == null)
        {
            Debug.LogWarning($"The object {objectToMove.name} is missing the {nameof(CloudNativeAnchor)} behavior.");
            return;
        }

        // Is there a cloud anchor to apply
        if (cloudSpatialAnchor != null)
        {
            // Yes. Apply the cloud anchor, which also sets the pose.
            cna.CloudToNative(cloudSpatialAnchor);
        }
        else
        {
            // No. Just set the pose.
            cna.SetPose(worldPos, worldRot);
        }
    }

    /// <summary>
    /// Spawns a new anchored object.
    /// </summary>
    /// <param name="worldPos">The world position.</param>
    /// <param name="worldRot">The world rotation.</param>
    /// <returns><see cref="GameObject"/>.</returns>
    GameObject SpawnNewAnchoredObject(Vector3 worldPos, Quaternion worldRot)
    {
        // Create the prefab
        GameObject newGameObject = GameObject.Instantiate(AnchoredObjectPrefab, worldPos, worldRot);

        // Attach a cloud-native anchor behavior to help keep cloud
        // and native anchors in sync.
        newGameObject.AddComponent<CloudNativeAnchor>();

        // Return created object
        return newGameObject;
    }

}
