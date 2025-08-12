using CT.Units.Attacks;
using UnityEngine;

namespace CT.Units.Attacks
{

    public class WeaponAnimEvents : MonoBehaviour
    {
        public Attack attack;
        public Unit ownerUnit;

        /// <summary>
        /// Forward the animation event to the unit attack script
        /// </summary>
        public void Anim_Fire()
        {
            if (attack != null && ownerUnit != null && ownerUnit.IsAttacking)
            {
                attack.OnFire(ownerUnit.CurrentTarget);
            }
        }
    }

}