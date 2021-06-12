using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;

public class AnchorAdder : MonoBehaviour
{
    /// <summary>
    /// Gets the <see cref="SpatialAnchorManager"/> instance used by this demo.
    /// </summary>
    public SpatialAnchorManager CloudManager;
    protected AnchorLocateCriteria anchorLocateCriteria = null;

    void Start()
    {
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
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
