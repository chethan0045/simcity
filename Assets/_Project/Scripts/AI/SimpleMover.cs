using UnityEngine;

namespace Simcity.AI
{
    /// <summary>
    /// Dead-simple ground movement toward a target (no pathfinding). Fine for the
    /// greybox town; a NavMesh agent replaces this once the world has real geometry
    /// and obstacles to route around.
    /// </summary>
    public class SimpleMover : MonoBehaviour
    {
        public float speed = 2.2f;
        public float arriveThreshold = 0.7f;
        public float turnSpeed = 8f;

        public bool AtPosition(Vector3 p)
        {
            var d = p - transform.position; d.y = 0f;
            return d.sqrMagnitude <= arriveThreshold * arriveThreshold;
        }

        public void MoveTowards(Vector3 p)
        {
            var d = p - transform.position; d.y = 0f;
            if (d.sqrMagnitude < 1e-4f) return;

            var dir = d.normalized;
            transform.position += dir * speed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir), turnSpeed * Time.deltaTime);
        }
    }
}
