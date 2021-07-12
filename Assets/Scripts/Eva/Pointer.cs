using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Pointer : Guidance
{
    //Stays on Idle if no target available and Guiding if they are available
    // Start is called before the first frame update
    public Material active;
    public Material notActive;
    public float detectionDistance = 1.0f;
    bool pauseTracking = false;
    public GameObject poster;
    bool done = false;
    void Start()
    {
        rotationSpeed = 14.0f;
        InitializeGuidance();
        SpeechManager.Instance._audioSource = gameObject.GetComponent<AudioSource>();
        poster.SetActive(true);
        StartCoroutine(GreetUser(7.0f, "Hello friend, please find the scene displayed on top"));
    }

    public IEnumerator GreetUser(float waitTime, string message)
    {
        SpeechManager.Instance.Speak(message);
        yield return new WaitForSeconds(waitTime);
    }

    // Update is called once per frame
    void Update()
    {

        if (wayPointIndex < wayPoints.Count)
        {
            Debug.Log("Guiding");


            PersueWaypoint();


        }
        else
        {
            ArrowIdle();
        }
    }
    // TODO Designwise terrible
    // arrow stands still when no target exist
    public void ArrowIdle()
    {
        gameObject.GetComponent<Renderer>().enabled = false;
        gameObject.GetComponent<Renderer>().material = notActive;
        Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward * 1.5f;
        //TODO temp for plane intiation
        //pos.y = -0.1f;
        gameObject.transform.position = pos;
    }
    public override void GuidanceComplete()
    {
        Debug.Log("Guidance Complete guiding false");
        //guiding = false;
    }
    // points to the currently persued wypoit and procedes to the next one once the user is close enough to the current one
    public override void PersueWaypoint()
    {
        gameObject.GetComponent<Renderer>().enabled = true;
        gameObject.GetComponent<Renderer>().material = active;
        state = GuideState.Guiding;
        Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward * 1.5f;
        //TODO temp for plane intiation
        //pos.y = -0.1f;
        gameObject.transform.position = pos;
        Vector3 wayPointPos = wayPoints[wayPointIndex].transform.position;
        wayPointPos.y = pos.y;
        Vector3 dir = (Camera.main.transform.position - wayPointPos).normalized;
        lookDir = dir;
        LookAtDir(lookDir);

        float distance = Vector2.Distance(new Vector2(wayPointPos.x, wayPointPos.z), new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z));
        if (distance <= detectionDistance && !pauseTracking)
        {
            pauseTracking = true;
            StartCoroutine(WaypointRached());
            // wayPointIndex++;
            //wayPointIndex %= (wayPoints.Count);
        }
    }

    public override  IEnumerator WaypointRached() {
        Debug.Log("ReachedWaypoint");
        if(textsToSpeak[wayPointIndex] != "")
        {
            SpeechManager.Instance.Speak(textsToSpeak[wayPointIndex]);
            yield return new WaitUntil(() => SpeechManager.Instance._audioSource.isPlaying == false);
        }

        yield return new WaitForSeconds(0.2f);
        wayPointIndex++;

        // state = GuideState.Waiting;
        pauseTracking = false;
        if (wayPointIndex >= wayPoints.Count)
        {
            StartCoroutine(DestinationReached());
        }
        Debug.Log("waypoint counter "+wayPointIndex);
    }


    public IEnumerator DestinationReached() {
        SpeechManager.Instance.Speak("We hope you enjoyed the tour and have a good time at our dorm");
        yield return new WaitForSeconds(5.0f);
        // _animator.SetBool("PowerOn", false);
    }

    public override void FirstAnchorFound()
    {
       poster.SetActive(false);
       SpeechManager.Instance.Speak("The Scene has been found. Now you can follow me");
    }
    
}