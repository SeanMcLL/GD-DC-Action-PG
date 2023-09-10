using UnityEngine;

namespace GDDC
{
    /*
     * This is a script that handles collision detection during movement
     * Author: Aryan Sinha
     */
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    [DisallowMultipleComponent]
    public class CharacterController2D : MonoBehaviour
    {
        private Rigidbody2D m_Rigidbody;
        private BoxCollider2D m_Collider;

        [SerializeField, Tooltip("The ground layer")] private LayerMask m_GroundLayer;
        [SerializeField, Tooltip("How much this object can penetate another collider"), Min(0.001f)] private float m_SkinWidth = 0.01f;
        [SerializeField, Tooltip("The maximum slope before the player is no longer considered grounded")] private float m_MaxSlopeAngle = 45.0f;

        public RaycastHit2D LastGroundedHit { get; private set; }

        public bool IsGrounded
        {
            get
            {
                Vector2 origin = m_Rigidbody.position + m_Collider.offset;
                Vector2 direction = Vector2.down;
                float distance = 2 * m_SkinWidth;

                if (FindClosestHit(origin, direction, distance, out RaycastHit2D hit))
                {
                    LastGroundedHit = hit;
                    return Vector2.Angle(Vector2.up, hit.normal) <= m_MaxSlopeAngle;
                }

                return false;
            }
        }

        public Vector2 Velocity { get; private set; }

        public float SkinWidth
        {
            get { return m_SkinWidth; }
            set { m_SkinWidth = value; }
        }

        public LayerMask GroundLayer
        {
            get { return m_GroundLayer; }
            set { m_GroundLayer = value; }
        }

        public float MaxSlopeAngle
        {
            get { return m_MaxSlopeAngle; }
            set { m_MaxSlopeAngle = value; }
        }

        private void Awake()
        {
            //Get a reference to all required components
            m_Rigidbody = GetComponent<Rigidbody2D>();
            m_Collider = GetComponent<BoxCollider2D>();
        }

        private void Start()
        {
            //Set all necessary settings for the required components
            //m_Rigidbody.bodyType = RigidbodyType2D.Kinematic;
            m_Rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private bool FindClosestHit(Vector2 origin, Vector2 direction, float distance, out RaycastHit2D closestHit)
        {
            Vector2 size = m_Collider.size - Vector2.one * m_SkinWidth;
            float angle = 0.0f;
            LayerMask layerMask = m_GroundLayer;
            float minDepth = -Mathf.Infinity;
            float maxDepth = Mathf.Infinity;

            RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, angle, direction, distance, layerMask, minDepth, maxDepth);
            closestHit = new RaycastHit2D();

            if (hits.Length > 0)
                closestHit = hits[0];

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.distance < closestHit.distance)
                    closestHit = hit;
            }

            return hits.Length > 0;
        }

        /// <summary>
        /// Move this character and detect and resolve collisions. Only call this method in the FixedUpdate function!
        /// </summary>
        /// <param name="movement">The amount to move by this physics step</param>
        public void Move(Vector2 movement)
        {
            float deltaTime = Time.fixedDeltaTime * Time.timeScale;

            Vector2 origin = m_Rigidbody.position + m_Collider.offset;
            Vector2 direction = movement.normalized;
            float distance = movement.magnitude;

            if (FindClosestHit(origin, direction, distance, out RaycastHit2D closestHit))
            {
                Vector2 newMovement = direction * closestHit.distance + (closestHit.normal * m_SkinWidth);
                Vector2 remainingMovement = movement - newMovement;

                Vector2 remainingProjected = closestHit.normal * Vector2.Dot(remainingMovement, closestHit.normal);
                Vector2 perpendicularMovement = remainingMovement - remainingProjected;

                //Do another round of collision detection
                origin = m_Rigidbody.position + m_Collider.offset + newMovement;
                direction = perpendicularMovement.normalized;
                distance = perpendicularMovement.magnitude;

                if (FindClosestHit(origin, direction, distance, out closestHit))
                    perpendicularMovement = direction * closestHit.distance + (closestHit.normal * m_SkinWidth);

                movement = newMovement + perpendicularMovement;
            }

            if (Mathf.Abs(movement.x) <= m_SkinWidth)
                movement.x = 0.0f;

            if (Mathf.Abs(movement.y) <= m_SkinWidth)
                movement.y = 0.0f;

            Vector2 targetPosition = m_Rigidbody.position + movement;
            m_Rigidbody.MovePosition(targetPosition);

            Velocity = movement / deltaTime;
        }
    }
}
