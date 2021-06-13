using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using System;

public class AnchorWatcher : MonoBehaviour
{
    public SpatialAnchorManager CloudManager;
    public GameObject AnchoredObjectPrefab;
    AnchorLocateCriteria anchorLocateCriteria = null;
    CloudSpatialAnchorWatcher currentWatcher;
    CloudSpatialAnchor currentCloudAnchor;
    // string id = "76ddffe9-b2b5-45cd-b5c8-e2bd4195e8e6";
    string id = "e52a21b8-3ca4-4c84-b34d-39ca94b56a88";
    readonly List<string> anchorIdsToLocate = new List<string>();
    GameObject spawnedObject = null;

    void Start()
    {
        CloudManager.SessionUpdated += CloudManager_SessionUpdated;
        CloudManager.AnchorLocated += CloudManager_AnchorLocated;
        CloudManager.LocateAnchorsCompleted += CloudManager_LocateAnchorsCompleted;
        CloudManager.LogDebug += CloudManager_LogDebug;
        CloudManager.Error += CloudManager_Error;
        anchorLocateCriteria = new AnchorLocateCriteria();
        ConfigureSession();
        Setup();
    }

    async void Setup()
    {
        if (CloudManager.Session == null)
        {
            await CloudManager.CreateSessionAsync();
        }
        await CloudManager.StartSessionAsync();
        currentWatcher = CreateWatcher();
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
        currentCloudAnchor = args.Anchor;

        if (args.Status == LocateAnchorStatus.Located)
        {
            UnityDispatcher.InvokeOnAppThread(() =>
            {
                Pose anchorPose = Pose.identity;

                anchorPose = args.Anchor.GetPose();

                SpawnOrMoveCurrentAnchoredObject(anchorPose.position, anchorPose.rotation);
            });
        }
    }

    /// <summary>
    /// Called when the current cloud session is updated.
    /// </summary>
    void OnCloudSessionUpdated()
    {
        // To be overridden.
    }

    CloudSpatialAnchorWatcher CreateWatcher()
    {
        if ((CloudManager != null) && (CloudManager.Session != null))
        {
            return CloudManager.Session.CreateWatcher(anchorLocateCriteria);
        }
        else
        {
            return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
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

    GameObject SpawnNewAnchoredObject(Vector3 worldPos, Quaternion worldRot)
    {
        // Create the prefab
        GameObject newGameObject = GameObject.Instantiate(AnchoredObjectPrefab, worldPos, worldRot);

        // Attach a cloud-native anchor behavior to help keep cloud
        // and native anchors in sync.
        var cna = newGameObject.AddComponent<CloudNativeAnchor>();

        // Return created object
        return newGameObject;
    }

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

    private void ConfigureSession()
    {
        List<string> anchorsToFind = new List<string>();
        anchorsToFind.Add(id);
        SetAnchorIdsToLocate(anchorsToFind);
    }

    protected void SetAnchorIdsToLocate(IEnumerable<string> anchorIds)
    {
        if (anchorIds == null)
        {
            throw new ArgumentNullException(nameof(anchorIds));
        }

        anchorLocateCriteria.NearAnchor = new NearAnchorCriteria();

        anchorIdsToLocate.Clear();
        anchorIdsToLocate.AddRange(anchorIds);

        anchorLocateCriteria.Identifiers = anchorIdsToLocate.ToArray();
    }
}
