//using UnityEngine;

//[RequireComponent(typeof(Character))]
//public class WaypointCharacterMover : MonoBehaviour
//{
//    [SerializeField] int destination = -1;
//    [SerializeField] GameObject[] waypoints;
//    [SerializeField] float reachDistance = 1f;
//    [SerializeField] float reachAngle = 1f;

//    private Character character;

//    private void Awake() => character = GetComponent<Character>();

//    private void Update()
//    {
//        if (destination >= 0)
//        {
//            if (Vector3.Distance(transform.position, waypoints[destination].transform.position) > reachDistance
//             && transform.AngleToTarget(waypoints[destination].transform.forward, false, true)  > reachAngle) character.MoveTo(waypoints[destination].transform.position);
//            else destination = -1;
//        }
//    }

//    public void OrderMoveTo(int point) => destination = point >= 0 && point < waypoints.Length ? point : -1;
//}   