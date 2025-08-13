using UnityEngine;

namespace CT.Units.Attacks
{
    public abstract class Attack : MonoBehaviour
    {
        /// <summary>
        /// Where the bullet/laser shoot from
        /// </summary>
        public Transform CanonTransform;
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
            if (!CanonTransform)
                CanonTransform = transform;
        }
        #endregion

        #region Attack logics
        /// <summary>
        /// handle the attack logic
        /// </summary>
        /// <param name="target"></param>
        public abstract void OnFire(Unit target);

        /// <summary>
        /// Handle the end of attack logic if needed
        /// </summary>
        public virtual void OnStop() { }

        #endregion
    }
}
