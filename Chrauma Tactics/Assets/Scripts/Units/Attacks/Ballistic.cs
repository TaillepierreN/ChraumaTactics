using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace CT.Units.Attacks
{
    public class Ballistic : Attack
    {
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private GameObject _impactVFXPrefab;
        [SerializeField] private float _projectileSpeed;
        [SerializeField] private float _aoeRadius = 3f;
        [SerializeField] private float _impactLifeTime = 0.5f;
        [SerializeField] private int _nbrOfPooledProjectile = 5;
        [SerializeField] private AudioClip _audioClip;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private Transform _poolContainer;
        private int _damage;

        private ObjectPool<GameObject> _projectilePool;
        private ObjectPool<GameObject> _impactPool;


        /// <summary>
        /// Initialize the pools of projectile and impact
        /// </summary>
        void Awake()
        {
            if (_projectilePrefab != null)
            {
                _projectilePool = new ObjectPool<GameObject>(
                    createFunc: () =>
                    {
                        GameObject go = Instantiate(_projectilePrefab, _poolContainer);
                        go.SetActive(false);
                        return go;
                    },
                    actionOnGet: go => go.SetActive(true),
                    actionOnRelease: go => go.SetActive(false),
                    actionOnDestroy: go => Destroy(go),
                    defaultCapacity: 32, maxSize: 256
                );
            }
            if (_impactVFXPrefab != null)
            {
                _impactPool = new ObjectPool<GameObject>(
                    createFunc: () =>
                    {
                        GameObject go = Instantiate(_impactVFXPrefab);
                        go.SetActive(false);
                        return go;
                    },
                    actionOnGet: go => go.SetActive(true),
                    actionOnRelease: go => go.SetActive(false),
                    actionOnDestroy: go => Destroy(go),
                    defaultCapacity: 32, maxSize: 256
                );
            }
            for (int i = 0; i < _nbrOfPooledProjectile; i++)
            {
                _projectilePool.Release(_projectilePool.Get());
                _impactPool.Release(_impactPool.Get());
            }
        }

        void OnDestroy()
        {
            _projectilePool?.Clear();
            _impactPool?.Clear();
        }

        /// <summary>
        /// initialize the owner and exit transform and damage of the projectile
        /// </summary>
        /// <param name="owner"></param>
        public override void Initialize(Unit owner)
        {
            base.Initialize(owner);
            _damage = owner.CurrentAtk;
        }

        /// <summary>
        /// Triggered by animation event, it handles the firing of a projectile and sound effect
        /// </summary>
        /// <param name="target"></param>
        public override void OnFire(Unit target)
        {
            if (_owner == null || CanonTransform == null)
                return;
            GameObject projectile = _projectilePool.Get();
            projectile.transform.SetPositionAndRotation(CanonTransform.position, CanonTransform.rotation);
            Projectile proj = projectile.GetComponent<Projectile>();

            if (proj == null)
            {
                Debug.Log("Prefab doesn't have a projectile script");
                _projectilePool.Release(projectile);
                return;
            }
            if (_audioSource && _audioClip)
                _audioSource.PlayOneShot(_audioClip);

            proj.Launch(
                new Projectile.PayLoad
                {
                    Source = _owner,
                    Target = target,
                    Speed = _projectileSpeed,
                    Damage = _damage,
                    IsAoe = _isAoe,
                    Radius = _aoeRadius
                },
                /*add a vfx at impact, like an explosion or something like that*/
                /*** TODO ***/
                /*Replace gameobject impact with VFX Graph if possiblel*/
                impactVFX: (pos, normal) =>
                {
                    GameObject impactVFX = _impactPool.Get();
                    impactVFX.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(normal));
                    StartCoroutine(ReturnAfter(impactVFX, _impactPool, _impactLifeTime));
                },
                onDone: () => _projectilePool.Release(projectile)
            );
        }

        /// <summary>
        /// Return the impact to the pool
        /// </summary>
        /// <param name="go">impact to return</param>
        /// <param name="pool">pool to return the object too</param>
        /// <param name="t">how long before returning</param>
        /// <returns></returns>
        private IEnumerator ReturnAfter(GameObject go, ObjectPool<GameObject> pool, float t)
        {
            yield return new WaitForSeconds(t);
            if (go != null)
                pool.Release(go);
        }
    }

}