using UnityEngine;

namespace CT.Units.Animations
{
    public class TurretAim : MonoBehaviour
    {
        /// <summary>
        /// Speed at which the turret rotates to face the target
        /// </summary>
        public float rotationSpeed = 5f;
        private Transform target;
        private Unit targetUnit;
        private bool lookAtTarget = false;

        private bool isResetting = false;
        /// <summary>
        /// Transform of the body of the vehicle from which the turret belongs
        /// </summary>
        [SerializeField] private Transform baseTransform;

        /// <summary>
        /// Update the turret's rotation towards the target or reset its direction.
        /// </summary>
        void Update()
        {
            if (isResetting && baseTransform != null)
            {
                float baseYRotation = baseTransform.eulerAngles.y;
                Quaternion targetRotation = Quaternion.Euler(0f, baseYRotation, 0f);

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
                {
                    transform.rotation = targetRotation;
                    isResetting = false;
                    lookAtTarget = false;
                }
            }
            if (lookAtTarget && target != null)
            {
                if (target == null) return;

                Vector3 direction = target.position - transform.position;
                float targetYRotation = Quaternion.LookRotation(direction).eulerAngles.y;

                Quaternion newRotation = Quaternion.Euler(0f, targetYRotation, 0f);

                transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * rotationSpeed);
            }
        }

        /// <summary>
        /// Sets the target for the turret to look at.
        /// </summary>
        /// <param name="newTarget">Unit to look at</param>
        public void SetLookAtTarget(Unit newTarget)
        {
            if (targetUnit != null)
                targetUnit.OnUnitDeath -= ResetLookDirection;

            target = newTarget.transform;
            targetUnit = newTarget;
            lookAtTarget = true;
            isResetting = false;
            if (targetUnit != null)
                targetUnit.OnUnitDeath += ResetLookDirection;
        }

        /// <summary>
        /// Resets the turret's look direction when the target unit dies.
        /// </summary>
        /// <param name="unit">Unit that died</param>
        public void ResetLookDirection(Unit unit)
        {
            if (unit != null)
                unit.OnUnitDeath -= ResetLookDirection;
            isResetting = true;
            lookAtTarget = false;
            target = null;
            targetUnit = null;
        }

    }
}
