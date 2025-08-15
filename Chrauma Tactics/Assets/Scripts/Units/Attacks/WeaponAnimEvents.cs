using UnityEngine;

namespace CT.Units.Attacks
{

    public class WeaponAnimEvents : MonoBehaviour
    {
        public Attack Attack;
        public Unit OwnerUnit;

        /// <summary>
        /// Forward the animation event to the unit attack script
        /// </summary> 
        public void Anim_Fire()
        {
            if (Attack != null && OwnerUnit != null && OwnerUnit.IsAttacking)
            {
                Attack.OnFire(OwnerUnit.CurrentTarget);
            }
        }
        /// <summary>
        /// Forward the animation event to the unit attack script for secondary weapon
        /// </summary>
        public void Anim_Fire2()
        {
            if (Attack != null && OwnerUnit != null && OwnerUnit.IsAttacking)
            {
                Attack.OnFire2(OwnerUnit.CurrentTarget);
            }
        }
        /// <summary>
        /// Forward the animation event to the unit attack script for tertiary weapon
        /// </summary>
        public void Anim_Fire3()
        {
            if (Attack != null && OwnerUnit != null && OwnerUnit.IsAttacking)
            {
                Attack.OnFire3(OwnerUnit.CurrentTarget);
            }
        }
        /// <summary>
        /// Forward the animation event to the unit attack script for tertiary weapon
        /// </summary>
        public void Anim_Fire4()
        {
            if (Attack != null && OwnerUnit != null && OwnerUnit.IsAttacking)
            {
                Attack.OnFire4(OwnerUnit.CurrentTarget);
            }
        }
    }

}