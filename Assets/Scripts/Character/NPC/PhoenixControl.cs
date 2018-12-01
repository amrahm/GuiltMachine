using System.Collections;
using Pathfinding;
using UnityEngine;

public class PhoenixControl : CharacterControlAbstract {
    // What to chase?
    public Transform target;
    public string searchTarget = "Player";

    // How many times each second we will update our path
    public float pathUpdateRate = 2f;

    // Caching
    private Seeker seeker;

    // The calculated path
    public Path path;

    [HideInInspector] public bool pathIsEnded = false;

    // The max distance from the AI to the waypoint for it to continue to the next waypoint
    public float nextWaypointDistance = 3;

    // The waypoint we are currently moving towards
    private int currentWaypoint = 0;

    private bool searchingForPlayer = false;

    void Start() {
        seeker = GetComponent<Seeker>();

        if(target == null) {
            if(!searchingForPlayer) {
                searchingForPlayer = true;
                StartCoroutine(SearchForPlayer());
            }
            return;
        }

        // Start a new path to the target position, return the result to the OnComplete method
        seeker.StartPath(transform.position, target.position, OnPathComplete);

        StartCoroutine(UpdatePath());
    }

    IEnumerator SearchForPlayer() {
        // Searches through GameObjects to find the player object
        GameObject sResult = GameObject.FindGameObjectWithTag(searchTarget);
        if(sResult == null) {
            // Search for player every 0.5 seconds if player is still not found due to player death or other circumstance
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(SearchForPlayer());
        } else {
            // If player is found, stop searching, begin pathfinding
            target = sResult.transform;
            searchingForPlayer = false;
            StartCoroutine(UpdatePath());
            yield break;
        }
    }

    IEnumerator UpdatePath() {
        if(target == null) {
            if(!searchingForPlayer) {
                searchingForPlayer = true;
                StartCoroutine(SearchForPlayer());
            }
            yield break;
        }

        // Start a new path to the target position, return the result to the OnComplete method
        seeker.StartPath(transform.position, target.position, OnPathComplete);

        yield return new WaitForSeconds(1f / pathUpdateRate);
        StartCoroutine(UpdatePath());
    }

    // Path calculated by A* seeker script gets passed to this method for processing in case of errors
    public void OnPathComplete(Path p) {
//        Debug.Log("We got a path. Did it have an error? " + p.error);
        if(!p.error) {
            path = p;
            currentWaypoint = 0;
        }
    }

    void Update() {
        //TODO Move to coroutine and update less often
        if(target == null) {
            if(!searchingForPlayer) {
                searchingForPlayer = true;
                StartCoroutine(SearchForPlayer());
            }
            return;
        }

        if(path == null) {
            return;
        }

        if(currentWaypoint >= path.vectorPath.Count) {
            if(pathIsEnded) {
                return;
            }
//            Debug.Log("End of path reached.");
            pathIsEnded = true;
            return;
        }
        pathIsEnded = false;

        // Direction to the next waypoint, scaled by magnitude and mass
        Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
        moveHorizontal = dir.x;
        moveVertical = dir.y;

        float dist = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);
        if(dist < nextWaypointDistance) {
            currentWaypoint++;
            return;
        }
    }
}