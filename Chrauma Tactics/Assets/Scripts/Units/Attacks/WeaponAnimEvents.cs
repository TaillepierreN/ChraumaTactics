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
        /// <summary>
        /// Forward the animation event to the unit attack script for secondary weapon
        /// </summary>
        public void Anim_Fire2()
        {
            if (attack != null && ownerUnit != null && ownerUnit.IsAttacking)
            {
                attack.OnFire2(ownerUnit.CurrentTarget);
            }
        }
        /// <summary>
        /// Forward the animation event to the unit attack script for tertiary weapon
        /// </summary>
        public void Anim_Fire3()
        {
            if (attack != null && ownerUnit != null && ownerUnit.IsAttacking)
            {
                attack.OnFire3(ownerUnit.CurrentTarget);
            }
        }
        /// <summary>
        /// Forward the animation event to the unit attack script for tertiary weapon
        /// </summary>
        public void Anim_Fire4()
        {
            if (attack != null && ownerUnit != null && ownerUnit.IsAttacking)
            {
                attack.OnFire4(ownerUnit.CurrentTarget);
            }
        }
    }

}