using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
//using UnityEngine.CoreModule;

public class Guide : Guidance
{

    float movementSpeed = 1.0f;
    // Start is called before the first frame update
    public float followDistance;
    public float waitDistance;
    public UnityEvent startGuidance;
    public bool startedGuidance = false;
    public bool guiding = false;
    public Material red;
    public Material blue;
    // Used to show the user where they need to look to start the guidance
    public RawImage image;
    public Poster poster;



    private Animator _animator;
    void Start()
    {
        rotationSpeed = 14.0f;
        _animator = GetComponent<Animator>();
        _animator.SetBool("PowerOn", true);
        InitializeGuidance();
       
        StartCoroutine(GreetUser(5.0f, "Hello friend, please find the scene depickted on my left hand side"));
    }
    public IEnumerator GreetUser(float waitTime, string message)
    {
        SpeechManager.Instance.Speak(message);
        yield return new WaitForSeconds(waitTime);
        StartGuidance();


    }
    void Update()
    {
        if(!guiding)
        {
            image.material = red;
            return;
        }
        else
        {
            image.material = blue;

        }
        if (wayPointIndex < wayPoints.Count)
        {
            if (!WaitForPlayer())
            {
                PersueWaypoint();
            }

        }
        else
        {
            IdleUpdate();
        }
        LookAtDir(lookDir);
    }
    public void IdleUpdate()
    {
        Vector3 dir = (gameObject.transform.position - Camera.main.transform.position).normalized;
        lookDir = dir;
        //LookAtDir(dir);
        Vector2 player = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z);
        Vector2 guide = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);
        if (Vector2.Distance(player, guide) < followDistance)
        {
            state = GuideState.Idle;
        }
        else
        {
            MoveTowards(Camera.main.transform.position);
        }
    }
    //If the User builds up to much distance eve is supposed to wait for them
    public bool WaitForPlayer()
    {
        //if distance to guide is too big wait for the player and look at him / Maby wave at the player
        Vector3 dir = (gameObject.transform.position - Camera.main.transform.position);
        if (state==GuideState.Waiting) {
            if ((dir).magnitude > waitDistance-0.5f)
            {
                lookDir = dir;
                //LookAtDir(dir);
                return true;
            }
            state = GuideState.Guiding;
            return false;
        }
        else
        {
            if ((dir).magnitude > waitDistance)
            {
                state = GuideState.Waiting;
                lookDir = dir;
                //LookAtDir(dir);
                return true;
            }
            state = GuideState.Guiding;
            return false;

        }


    }
    // Iterates to the nex wayypoint in the list of thecurrent path
    public override void PersueWaypoint()
    {
        state = GuideState.Guiding;
        Vector3 wayPointPos = wayPoints[wayPointIndex].transform.position;
        wayPointPos.y += 0.4f;
        MoveTowards(wayPointPos);
        Vector3 dir = (gameObject.transform.position - wayPointPos).normalized;

        lookDir = dir;


        if (wayPointPos.Equals(gameObject.transform.position))
        {
            wayPointIndex++;
            // StartCoroutine(WaypointRached());


        }
    }
    public override  IEnumerator WaypointRached() {
        Debug.Log("ReachedWaypoint");
        if (lastAnchor != null&&lastAnchor?.text!=null)
        {
            SpeechManager.Instance.Speak(lastAnchor.text);
            yield return new WaitUntil(() => SpeechManager.Instance._audioSource.isPlaying == false);
        }
        yield return new WaitForSeconds(0.5f);
        wayPointIndex++;
        if (wayPointIndex >= wayPoints.Count)
        {
            StartCoroutine(DestinationReached());
        }

        Debug.Log("waypoint counter "+wayPointIndex);
    }
    // Slowly move the paarent game object towards a position provided (Called on Update)
    public void MoveTowards(Vector3 target)
    {
        Vector3 pos = Vector3.MoveTowards(gameObject.transform.position, target, movementSpeed * Time.deltaTime);
        //TODO temp for plane intiation
        //pos.y = -0.1f;
        gameObject.transform.position = pos;
    }

    // TODO not used
    public override void GuidanceComplete()
    {
        //guiding = false;
        //lookDir = (Camera.main.transform.position - gameObject.transform.position).normalized;
    }

    //Called by parent class after user was greeted greeted
    public void StartGuidance()
    {
        guiding = true;
        poster.SetCanvasActive(false);
       // startGuidance.Invoke();
    }

    // Called by parent class when the startpoin of the current path was found
    public override void FirstAnchorFound()
    {
        // Wait till introduction is finished first
        if (guiding)
        {
            AnnounceGuidanceStart();
        }
        else 
        {
            //startGuidance.AddListener(AnnounceGuidanceStart);
        }
       

    }

    //Supposed to be called when the last way point of the current path was found
    public IEnumerator DestinationReached() {
        SpeechManager.Instance.Speak("Destination Reached. Shutting down.");
        yield return new WaitForSeconds(2.0f);
        _animator.SetBool("PowerOn", false);
    }

    // Tells the user to follow them once eve starts moving towards a waypoint
    public void AnnounceGuidanceStart() {
        Debug.Log("Annonce Start"); 
        //SpeechManager.Instance.Speak("Let´s go follow me");
    }
}