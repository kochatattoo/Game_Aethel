using UnityEngine;
using UnityEngine.Events;

namespace RealStep
{
    [AddComponentMenu("RealStep/Foot IK")]
    [RequireComponent(typeof(Animator))]
    public class FootIK : MonoBehaviour
    {
        [Tooltip("Global IK weight. Set to 0 to completely disable foot IK.")]
        [Range(0, 1)] public float Weight = 1f;

        [Tooltip("Maximum step height the foot can adapt to.")]
        public float MaxStep = 0.75f;

        [Tooltip("Automatically calculate MaxStep from character height.")]
        public bool AutoMaxStep;

        [Tooltip("Radius for ground detection.")]
        public float FootRadius = 0.15f;

        [Tooltip("Layer mask used for ground detection raycasting.")]
        public LayerMask Ground = 1;

        [Tooltip("Vertical offset applied to foot placement.")]
        public float Offset = 0.04f;

        [Tooltip("Speed of pelvis height adjustment.")]
        public float HipsPositionSpeed = 1f;

        [Tooltip("Speed of foot vertical position interpolation.")]
        public float FeetPositionSpeed = 1f;

        [Tooltip("Speed of foot rotation adjustment in degrees per second.")]
        public float FeetRotationSpeed = 90f;

        [Tooltip("Weight of pelvis height adjustment.")]
        [Range(0, 1)] public float HipsWeight = 0.8f;

        [Tooltip("Weight of left foot IK position and rotation.")]
        [Range(0, 1)] public float LeftFootWeight = 1f;

        [Tooltip("Weight of right foot IK position and rotation.")]
        [Range(0, 1)] public float RightFootWeight = 1f;

        [Tooltip("Enable body tilt to match ground slope.")]
        public bool EnableBodyTilt = true;

        [Tooltip("Amount of body tilt applied on slopes.")]
        [Range(0, 1)] public float TiltAmount = 0.5f;

        [Tooltip("Speed of body tilt interpolation.")]
        public float TiltSpeed = 10f;

        [Tooltip("Invert the body tilt direction.")]
        public bool InvertTilt = true;

        [Tooltip("Toggle debug gizmos in Scene view.")]
        public bool ShowDebug;

        public UnityEvent OnLeftFootGrounded;
        public UnityEvent OnLeftFootLifted;
        public UnityEvent OnRightFootGrounded;
        public UnityEvent OnRightFootLifted;

        const float NormalUpThreshold = 0.7f;
        const float GroundedHysteresis = 0.05f;

        Vector3 leftIKPosition, rightIKPosition;
        Vector3 leftNormal = Vector3.up, rightNormal = Vector3.up;
        Vector3 smoothLeftNormal = Vector3.up, smoothRightNormal = Vector3.up;
        Quaternion leftIKRotation, rightIKRotation, lastLeftRotation, lastRightRotation;
        float lastRFootHeight, lastLFootHeight;

        Animator anim;
        float velocity;
        float falloffWeight;
        float lastHeight;
        Vector3 lastPosition;
        bool leftGrounded, rightGrounded, isGrounded;
        bool wasLeftGrounded, wasRightGrounded;
        bool isDisabling;
        Quaternion currentTiltRotation = Quaternion.identity;
        float characterHeight = 1f;

        void Start()
        {
            anim = GetComponent<Animator>();
            if (anim && anim.isHuman)
                characterHeight = anim.humanScale;
            ApplyAutoMaxStep();
        }

        void OnValidate()
        {
            if (anim == null) anim = GetComponent<Animator>();
            if (anim && anim.isHuman)
                characterHeight = anim.humanScale;
            ApplyAutoMaxStep();
        }

        void ApplyAutoMaxStep()
        {
            if (AutoMaxStep)
                MaxStep = Mathf.Max(characterHeight * 0.35f, 0.2f);
        }

        void OnEnable()
        {
            isDisabling = false;
        }

        void OnDisable()
        {
            falloffWeight = 0f;
            lastHeight = 0f;
            lastLFootHeight = 0f;
            lastRFootHeight = 0f;
            lastLFootHeight = 0f;
            lastRFootHeight = 0f;
            lastLeftRotation = Quaternion.identity;
            lastRightRotation = Quaternion.identity;
            currentTiltRotation = Quaternion.identity;
        }

        public void DisableSmooth()
        {
            isDisabling = true;
        }

        void FixedUpdate()
        {
            if (Weight == 0 || !anim) return;

            Vector3 speed = (lastPosition - anim.transform.position) / Time.fixedDeltaTime;
            velocity = Mathf.Max(speed.magnitude, 0.01f);
            lastPosition = anim.transform.position;

            Transform leftBone = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
            Transform rightBone = anim.GetBoneTransform(HumanBodyBones.RightFoot);

            if (leftBone == null || rightBone == null)
            {
                Debug.LogWarning("[RealStep] Humanoid foot bones not found.", this);
                enabled = false;
                return;
            }

            FeetSolver(leftBone, ref leftIKPosition, ref leftNormal, ref smoothLeftNormal,
                ref leftIKRotation, ref leftGrounded);
            FeetSolver(rightBone, ref rightIKPosition, ref rightNormal, ref smoothRightNormal,
                ref rightIKRotation, ref rightGrounded);

            ProcessGroundedState();
        }

        void OnAnimatorIK(int layerIndex)
        {
            if (Weight == 0 || !anim) return;

            MovePelvisHeight();

            if (EnableBodyTilt)
                ApplyBodyTilt();

            ApplyFootIK(AvatarIKGoal.LeftFoot, leftIKPosition, smoothLeftNormal, leftIKRotation,
                ref lastLFootHeight, ref lastLeftRotation, LeftFootWeight);

            ApplyFootIK(AvatarIKGoal.RightFoot, rightIKPosition, smoothRightNormal, rightIKRotation,
                ref lastRFootHeight, ref lastRightRotation, RightFootWeight);
        }

        void MovePelvisHeight()
        {
            float leftOffset = leftGrounded ? leftIKPosition.y - anim.transform.position.y : 0f;
            float rightOffset = rightGrounded ? rightIKPosition.y - anim.transform.position.y : 0f;
            
            float maxOffset = Mathf.Max(leftOffset, rightOffset);
            float minOffset = Mathf.Min(leftOffset, rightOffset);

            float totalOffset = 0f;
            
            // If the highest potential foot placement is near or below the root, we might be landing/falling.
            // Only pull the pelvis *down* to reach a step if at least one foot is somewhat grounded normally.
            if (maxOffset >= -0.15f)
                totalOffset = minOffset;

            Vector3 newPosition = anim.bodyPosition;
            float newHeight = totalOffset * (HipsWeight * falloffWeight);
            
            // Allow pelvis to drop faster than it rises to quickly accommodate lower steps
            float speed = (newHeight < lastHeight) ? HipsPositionSpeed * 2f : HipsPositionSpeed;
            if (!isGrounded) speed = 6f; // Smooth airborne recovery
            
            lastHeight = Mathf.MoveTowards(lastHeight, newHeight, speed * Time.deltaTime);
            newPosition.y += lastHeight + Offset;

            anim.bodyPosition = newPosition;
        }

        void ApplyBodyTilt()
        {
            Vector3 averageNormal = (smoothLeftNormal + smoothRightNormal).normalized;
            if (averageNormal.sqrMagnitude < 0.01f) averageNormal = Vector3.up;

            float slopeAngle = Vector3.Angle(Vector3.up, averageNormal);
            if (slopeAngle < 1f)
            {
                currentTiltRotation = Quaternion.Slerp(currentTiltRotation, Quaternion.identity, TiltSpeed * Time.deltaTime);
                anim.bodyRotation = currentTiltRotation * anim.bodyRotation;
                return;
            }

            Vector3 axis = Vector3.Cross(Vector3.up, averageNormal).normalized;
            float direction = InvertTilt ? -1f : 1f;
            float angle = slopeAngle * TiltAmount * falloffWeight * direction;

            Quaternion targetTilt = Quaternion.AngleAxis(angle, axis);
            currentTiltRotation = Quaternion.Slerp(currentTiltRotation, targetTilt, TiltSpeed * Time.deltaTime);
            anim.bodyRotation = currentTiltRotation * anim.bodyRotation;
        }

        void ApplyFootIK(AvatarIKGoal foot, Vector3 ikPosition, Vector3 normal, Quaternion ikRotation,
            ref float footLastHeight, ref Quaternion lastRotation, float footWeight)
        {
            float effectiveWeight = footWeight * falloffWeight;

            Vector3 position = anim.GetIKPosition(foot);
            Quaternion rotation = anim.GetIKRotation(foot);

            // Calculate flat rotation to align the sole of the animated foot with the ground normal
            Vector3 footForward = Vector3.ProjectOnPlane(rotation * Vector3.forward, normal).normalized;
            if (footForward == Vector3.zero) footForward = rotation * Vector3.forward;
            Quaternion flatRotation = Quaternion.LookRotation(footForward, normal);

            // How much is the IK lifting this foot upwards? (e.g. stepping up a stair)
            float liftAmount = ikPosition.y - position.y;
            // The more it's lifted, the more we flatten the foot so the toe doesn't stab into the step
            float flattenWeight = Mathf.Clamp01(liftAmount * 5f);
            
            // Base slope adjustment using the normal angle mixed with the animated rotation
            Quaternion slopeRotation = ikRotation * rotation;
            // Smoothly blend to completely flat if vertically compressed
            Quaternion targetRotation = Quaternion.Slerp(slopeRotation, flatRotation, flattenWeight);

            position = anim.transform.InverseTransformPoint(position);
            ikPosition = anim.transform.InverseTransformPoint(ikPosition);
            footLastHeight = Mathf.MoveTowards(footLastHeight, ikPosition.y, FeetPositionSpeed * Time.deltaTime);
            position.y += footLastHeight;
            position = anim.transform.TransformPoint(position);
            position += normal * Offset;

            lastRotation = Quaternion.Slerp(lastRotation, targetRotation, FeetRotationSpeed * Time.deltaTime);

            anim.SetIKPosition(foot, position);
            anim.SetIKPositionWeight(foot, effectiveWeight);
            anim.SetIKRotation(foot, lastRotation);
            anim.SetIKRotationWeight(foot, effectiveWeight);
        }

        void ProcessGroundedState()
        {
            isGrounded = leftGrounded || rightGrounded;

            float target = isDisabling ? 0f : (isGrounded ? 1f : 0f);
            
            // Lightning fast disable when jumping, smooth fade in when landing
            float speed = isGrounded ? 10f : 15f; 
            falloffWeight = Mathf.MoveTowards(falloffWeight, target, speed * Time.fixedDeltaTime) * Weight;

            if (isDisabling && falloffWeight < 0.01f)
            {
                enabled = false;
                return;
            }

            if (leftGrounded && !wasLeftGrounded) OnLeftFootGrounded?.Invoke();
            if (!leftGrounded && wasLeftGrounded) OnLeftFootLifted?.Invoke();
            if (rightGrounded && !wasRightGrounded) OnRightFootGrounded?.Invoke();
            if (!rightGrounded && wasRightGrounded) OnRightFootLifted?.Invoke();

            wasLeftGrounded = leftGrounded;
            wasRightGrounded = rightGrounded;
        }

        void FeetSolver(Transform footBone, ref Vector3 ikPosition, ref Vector3 normal,
            ref Vector3 smoothNormal, ref Quaternion ikRotation, ref bool grounded)
        {
            Vector3 footPos = footBone.position;
            
            // Anchor raycast to the physical foot mesh instead of the root!
            // This naturally breaks the raycast (disabling IK) when the animated foot jumps high into the air.
            float castUp = Mathf.Min(MaxStep, 0.4f);
            float castDist = MaxStep + castUp;
            
            Vector3 castOrigin = footPos;
            castOrigin.y += castUp;

            float feetHeight = float.MaxValue;
            bool validHit = false;

            if (ShowDebug)
                Debug.DrawLine(castOrigin, castOrigin + Vector3.down * castDist, Color.yellow);

            if (Physics.Raycast(castOrigin, Vector3.down, out RaycastHit rayHit,
                castDist, Ground, QueryTriggerInteraction.Ignore))
            {
                // Edge case handling: if the ray hits an almost vertical wall instead of the step surface,
                // do a secondary short cast slightly inward from the normal to find the actual flat surface.
                if (rayHit.normal.y <= NormalUpThreshold)
                {
                    Vector3 adjustedOrigin = castOrigin - (rayHit.normal * FootRadius);
                    if (Physics.Raycast(adjustedOrigin, Vector3.down, out RaycastHit secondaryHit, MaxStep * 2, Ground, QueryTriggerInteraction.Ignore))
                    {
                        if (secondaryHit.normal.y > NormalUpThreshold)
                        {
                            rayHit = secondaryHit;
                        }
                    }
                }

                float hitDistance = Vector2.Distance(new Vector2(rayHit.point.x, rayHit.point.z), new Vector2(footPos.x, footPos.z));
                bool isDirectHit = hitDistance < FootRadius * 1.5f;

                if (rayHit.normal.y > NormalUpThreshold && hitDistance < FootRadius * 4f)
                {
                    validHit = true;
                    feetHeight = anim.transform.position.y - rayHit.point.y;
                    ikPosition = rayHit.point;
                    normal = rayHit.normal;

                    if (ShowDebug)
                        Debug.DrawRay(rayHit.point, rayHit.normal * 0.3f, Color.blue);
                }
                else if (ShowDebug)
                {
                    Debug.DrawRay(rayHit.point, rayHit.normal * 0.3f, Color.red);
                }
            }

            if (validHit)
            {
                float slopeAngle = Vector3.Angle(Vector3.up, normal);
                
                // Keep foot totally flat when stepping vertically, otherwise tilt to slope
                if (slopeAngle > 1f && slopeAngle < 35f)
                {
                    Vector3 axis = Vector3.Cross(Vector3.up, normal);
                    ikRotation = Quaternion.AngleAxis(slopeAngle, axis);
                }
                else
                {
                    ikRotation = Quaternion.identity;
                }
            }
            else
            {
                normal = Vector3.up;
                ikRotation = Quaternion.identity;
            }

            smoothNormal = Vector3.Lerp(smoothNormal, normal, Time.fixedDeltaTime * 8f);

            float groundedThreshold = grounded ? MaxStep + GroundedHysteresis : MaxStep;
            grounded = feetHeight < groundedThreshold;

            if (!grounded)
            {
                ikPosition = anim.transform.position;
                ikRotation = Quaternion.identity;
                normal = Vector3.up;
                smoothNormal = Vector3.Lerp(smoothNormal, Vector3.up, Time.fixedDeltaTime * 12f);
            }
        }
    }
}