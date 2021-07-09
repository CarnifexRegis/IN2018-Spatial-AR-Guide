using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
//using UnityEngine.CoreModule;

public class Guide : Guidance
{

    float movementSpeed = 0.4f;
    // Start is called before the first frame update
    public float followDistance;
    public float waitDistance;
    public UnityEvent startGuidance;
    public bool startedGuidance = false;
    public bool guiding = false;
    private Animator _animator;
    void Start()
    {
        _animator = GetComponent<Animator>();
        rotationSpeed = 14.0f;
        InitializeGuidance();
        StartCoroutine(GreetUser(10.0f, "Hello friend, please find the scene depickted on my ledt hand side"));
    }
    public IEnumerator GreetUser(float waitTime, string message)
    {
        SpeechManager.Instance.Speak(message);
        yield return new WaitForSeconds(waitTime);
        StartGuidance();


    }
    void Update()
    {
        if (!guiding)
        {
            return;
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
        // @Simon:
        // I put this code snippet for you as an example, basically switch between two states as follows.
        // You can remove the region when you're done.
        #region Test
        if (Input.GetKeyDown(KeyCode.F1))
            _animator.SetBool("PowerOn", true);
        if (Input.GetKeyDown(KeyCode.F2))
            _animator.SetBool("PowerOn", false);
        #endregion
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
            if (!_animator.GetBool("PowerOn")) {
                _animator.SetBool("PowerOn", true);
            }
            MoveTowards(Camera.main.transform.position);
        }
    }

    public bool WaitForPlayer()
    {
        //if distance to guide is too big wait for the player and look at him / Maby wave at the player
        Vector3 dir = (gameObject.transform.position - Camera.main.transform.position);
        if ((dir).magnitude > waitDistance)
        {
            state = GuideState.Waiting;
            lookDir = dir;
            //LookAtDir(dir);
            return true;
        }
        return false;

    }

    public override void PersueWaypoint()
    {
        state = GuideState.Guiding;
        Vector3 wayPointPos = wayPoints[wayPointIndex].transform.position;
        wayPointPos.y += 0.2f;
        MoveTowards(wayPointPos);
        Vector3 dir = (gameObject.transform.position - wayPointPos).normalized;

        lookDir = dir;


        if (wayPointPos.Equals(gameObject.transform.position))
        {
            if (lastAnchor.children.Count == 0) {
                GuidanceComplete(5.0f, "Target Destination Reached");
            }
            WaypointRached();
        }
    }
    public void MoveTowards(Vector3 target)
    {
        Vector3 pos = Vector3.MoveTowards(gameObject.transform.position, target, movementSpeed * Time.deltaTime);
        //TODO temp for plane intiation
        //pos.y = -0.1f;
        gameObject.transform.position = pos;
    }
    public IEnumerator GuidanceComplete(float waitTime, string message)
    {
        SpeechManager.Instance.Speak(message);
        yield return new WaitForSeconds(waitTime);
        _animator.SetBool("PowerOn", false);


    }
    public override void GuidanceComplete() { 
        
    }
    public void StartGuidance()
    {
        guiding = true;
        startGuidance.Invoke();
    }
    public override void FirstAnchorFound()
    {
        _animator.SetBool("PowerOn", false);
    }
}