using Unity.IO.LowLevel.Unsafe;
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
        [SerializeField] protected private float _aoeRadius = 3f;
        [SerializeField] protected private AudioClip _audioClip;
        [SerializeField] protected private AudioSource _audioSource;
        
        [HideInInspector]
        public virtual bool IsContinuous => false;

        #region Initialisation
        /// <summary>
        /// Initialize the owner and the muzzle of the unit
        /// </summary>
        /// <param name="owner"></param>
        public virtual void Initialize(Unit owner)
        {
            _owner = owner != null ? owner : GetComponent<Unit>();

            if (_audioSource == null)
                _audioSource = GetComponent<AudioSource>();

            if (BarrelEnd == null || BarrelEnd.Length == 0)
                BarrelEnd = new[] { transform };
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
        /// handle the attack logic of a third weapon
        /// </summary>
        /// <param name="target"></param>
        public abstract void OnFire4(Unit target);

        /// <summary>
        /// Handle the end of attack logic if needed
        /// </summary>
        public virtual void OnStop() { }

        /// <summary>
        /// Continuous specific fire method
        /// </summary>
        /// <param name="target"></param>
        public virtual void StartAutoFire(Unit target) { }
        /// <summary>
        /// Continuous specific fire method
        /// </summary>
        /// <param name="target"></param>
        public virtual void StopAutoFire() { }

        #endregion
    }
}
