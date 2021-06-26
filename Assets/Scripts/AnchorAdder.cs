using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using Microsoft.Azure.SpatialAnchors.Unity.Examples;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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
    public Text text;
    bool isErrorActive = false;
    AnchorStore anchorStore;
    public Dropdown dropdown;
    string selection;
    List<Dropdown.OptionData> dropdownOption;
    Dictionary<string,string> nameIDDataset = new Dictionary<string, string>();
    Dictionary<string,GameObject> spawnedGameObject = new Dictionary<string, GameObject>();
    readonly List<string> anchorIdsToLocate = new List<string>();
    CloudSpatialAnchorWatcher currentWatcher;

    public override void Start()
    {
        print(Application.persistentDataPath);
        SaveFile();
        text.text = "OOF";
        base.Start();
        CloudManager.SessionUpdated += CloudManager_SessionUpdated;
        CloudManager.AnchorLocated += CloudManager_AnchorLocated;
        CloudManager.LocateAnchorsCompleted += CloudManager_LocateAnchorsCompleted;
        CloudManager.LogDebug += CloudManager_LogDebug;
        CloudManager.Error += CloudManager_Error;
        anchorLocateCriteria = new AnchorLocateCriteria();
        ConfigureSession();
        Setup();
    }

    private void ConfigureSession()
    {
        SetAnchorIdsToLocate(nameIDDataset.Values);
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

    void SaveFile()
    {
        if(!File.Exists(Application.persistentDataPath + "/AnchorStore.json"))
        {
            anchorStore = new AnchorStore();
            anchorStore.name = "dorm";
            anchorStore.anchor = new Anchor();
            anchorStore.anchor.name = "Entry";
            anchorStore.anchor.children = new List<Anchor>();
            anchorStore.anchor.children.Add(new Anchor());
            anchorStore.anchor.children[0].name="Door";
            FileStream file = File.Create(Application.persistentDataPath + "/AnchorStore.json");
            file.Close();
            string json = JsonUtility.ToJson(anchorStore);
            File.WriteAllText(Application.persistentDataPath + "/AnchorStore.json", json);
        }
        else
        {
            string json = File.ReadAllText(Application.persistentDataPath + "/AnchorStore.json");
            anchorStore = JsonUtility.FromJson<AnchorStore>(json);
            print(json);
        }
        GetAllNames();
        MakeDictionary();
        selection = anchorStore.anchor.name;
    }

    void MakeDictionary()
    {
        GetKeyIdPair(anchorStore.anchor);
    }

    void GetKeyIdPair(Anchor anchor)
    {
        print(anchor.id);
        nameIDDataset.Add(anchor.name, anchor.id);
        spawnedGameObject.Add(anchor.name,null);
        foreach(Anchor a in anchor.children)
        {
            GetKeyIdPair(a);
        }
    }

    void GetAllNames()
    {
        dropdownOption = new List<Dropdown.OptionData>();
        //dropdownOption.Add(new Dropdown.OptionData(anchorStore.name));
        RecursiveChildNameCall(dropdownOption, anchorStore.anchor);
        dropdown.AddOptions(dropdownOption);
        dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(dropdown);
        });
    }

    void RecursiveChildNameCall(List<Dropdown.OptionData> dropdownOption, Anchor anchor)
    {
        dropdownOption.Add(new Dropdown.OptionData(anchor.name));
        foreach(Anchor a in anchor.children)
        {
            RecursiveChildNameCall(dropdownOption, a);
        }
    }

    async void Setup()
    {
        if (CloudManager.Session == null)
        {
            await CloudManager.CreateSessionAsync();
        }
        await CloudManager.StartSessionAsync();
        text.text = CloudManager.IsSessionStarted.ToString();
        isPlacingObject = true;
        currentWatcher = CreateWatcher();
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
        if (spawnedGameObject[selection] == null)
        {
            // Use factory method to create
            spawnedGameObject[selection] = SpawnNewAnchoredObject(worldPos, worldRot, currentCloudAnchor);
            Save();
        }
        else
        {
            // Use factory method to move
            MoveAnchoredObject(spawnedGameObject[selection], worldPos, worldRot, currentCloudAnchor);
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

    async void Save()
    {
        await SaveCurrentObjectAnchorToCloudAsync();
        text.text = currentCloudAnchor.Identifier;
        FindAndSetAnchorId(anchorStore.anchor, currentCloudAnchor.Identifier);
        UpdateFile();
        // spawnedObject = null;
    }

    void FindAndSetAnchorId(Anchor anchor,string id)
    {
        if(anchor.name == selection)
        {
            anchor.id = id;
            return;
        }
        foreach(Anchor a in anchor.children)
        {
            FindAndSetAnchorId(a,id);
        }
    }

    void UpdateFile()
    {
        string json = JsonUtility.ToJson(anchorStore);
        File.WriteAllText(Application.persistentDataPath + "/AnchorStore.json", json);
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
        var cna = newGameObject.AddComponent<CloudNativeAnchor>();

        // Return created object
        return newGameObject;
    }

    async Task SaveCurrentObjectAnchorToCloudAsync()
    {
        // Get the cloud-native anchor behavior
        CloudNativeAnchor cna = spawnedObject.GetComponent<CloudNativeAnchor>();

        // If the cloud portion of the anchor hasn't been created yet, create it
        if (cna.CloudAnchor == null)
        {
            await cna.NativeToCloud();
        }

        // Get the cloud portion of the anchor
        CloudSpatialAnchor cloudAnchor = cna.CloudAnchor;

        // In this sample app we delete the cloud anchor explicitly, but here we show how to set an anchor to expire automatically
        cloudAnchor.Expiration = DateTimeOffset.Now.AddDays(7);

        while (!CloudManager.IsReadyForCreate)
        {
            await Task.Delay(330);
            float createProgress = CloudManager.SessionStatus.RecommendedForCreateProgress;
            text.text = $"Move your device to capture more environment data: {createProgress:0%}";
        }

        bool success = false;

        text.text = "Saving...";

        try
        {
            // Actually save
            await CloudManager.CreateAnchorAsync(cloudAnchor);

            // Store
            currentCloudAnchor = cloudAnchor;

            // Success?
            success = currentCloudAnchor != null;

            if (success || !isErrorActive)
            {
                // Await override, which may perform additional tasks
                // such as storing the key in the AnchorExchanger
                await OnSaveCloudAnchorSuccessfulAsync();
            }
            else
            {
                OnSaveCloudAnchorFailed(new Exception("Failed to save, but no exception was thrown."));
            }
        }
        catch (Exception ex)
        {
            OnSaveCloudAnchorFailed(ex);
        }
    }

    /// <summary>
    /// Called when a cloud anchor is saved successfully.
    /// </summary>
    Task OnSaveCloudAnchorSuccessfulAsync()
    {
        // To be overridden.
        return Task.CompletedTask;
    }

    void OnSaveCloudAnchorFailed(Exception exception)
    {
        // we will block the next step to show the exception message in the UI.
        isErrorActive = true;
        Debug.LogException(exception);
        Debug.Log("Failed to save anchor " + exception.ToString());

        UnityDispatcher.InvokeOnAppThread(() => this.text.text = string.Format("Error: {0}", exception.ToString()));
    }


    void DropdownValueChanged(Dropdown change)
    {
        selection = dropdownOption[change.value].text;
    }

}
