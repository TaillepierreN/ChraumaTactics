using UnityEngine;

namespace CT.Units.Attacks
{
    [RequireComponent(typeof(AudioSource))]
    public abstract class Attack : MonoBehaviour
    {
        /// <summary>
        /// Where the bullet/laser shoot from
        /// </summary>
        public Transform[] BarrelEnd;
        protected private Unit _owner;
        [SerializeField] protected private bool _isAoe;
        [SerializeField] protected private AudioClip _audioClip;
        [SerializeField] protected private AudioSource _audioSource;


        #region Initialisation
        /// <summary>
        /// Initialize the owner and the muzzle of the unit
        /// </summary>
        /// <param name="owner"></param>
        public virtual void Initialize(Unit owner)
        {
            _owner = owner ?? GetComponent<Unit>();
            if (BarrelEnd == null)
                BarrelEnd[0] = transform;
        }
        #endregion

        #region Attack logics
        /// <summary>
        /// handle the attack logic of main weapon
        /// </summary>
        /// <param name="target"></param>
        public abstract void OnFire(Unit target);

        /// <summary>
        /// handle the attack logic of a second weapon
        /// </summary>
        /// <param name="target"></param>
        public abstract void OnFire2(Unit target);

        /// <summary>
        /// handle the attack logic of a third weapon
        /// </summary>
        /// <param name="target"></param>
        public abstract void OnFire3(Unit target);

        /// <summary>
        /// Handle the end of attack logic if needed
        /// </summary>
        public virtual void OnStop() { }

        #endregion
    }
}
