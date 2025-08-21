using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace CT.Units.Attacks
{
    public class Ballistic : Attack
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private GameObject[] _impactVFXPrefab;

        [Header("Stats")]
        [SerializeField] private float _projectileSpeed = 30f;
        [SerializeField] private float _impactLifeTime = 0.5f;
        [SerializeField] private int _nbrOfPooledProjectile = 5;
        private int _damage;

        [Header("Storage")]
        /// <summary>
        /// Empty gameobject, direct child of this gameobject
        /// used to store pooled projectiles and impacts
        /// </summary>
        [SerializeField] private Transform _poolContainer;


        [Header("Pools")]
        private ObjectPool<GameObject> _projectilePool;
        private ObjectPool<GameObject> _impactPool;
        private ObjectPool<GameObject> _impactAoEPool;

        #region Unity Callbacks

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
                        GameObject go = Instantiate(_impactVFXPrefab[0], _poolContainer);
                        go.SetActive(false);
                        return go;
                    },
                    actionOnGet: go => go.SetActive(true),
                    actionOnRelease: go => go.SetActive(false),
                    actionOnDestroy: go => Destroy(go),
                    defaultCapacity: 32, maxSize: 256
                );
            }
            if (_impactVFXPrefab != null && _impactVFXPrefab.Length > 1)
            {
                _impactAoEPool = new ObjectPool<GameObject>(
                    createFunc: () =>
                    {
                        GameObject go = Instantiate(_impactVFXPrefab[1], _poolContainer);
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
                if (_projectilePool != null)
                    _projectilePool.Release(_projectilePool.Get());
                if (_impactPool != null)
                    _impactPool.Release(_impactPool.Get());
                if (_impactAoEPool != null)
                    _impactAoEPool.Release(_impactAoEPool.Get());

            }
        }

        void OnDestroy()
        {
            _projectilePool?.Clear();
            _impactPool?.Clear();
            _impactAoEPool?.Clear();
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

        #endregion

        #region TriggerPull

        /// <summary>
        /// Triggered by animation event, it handles preparation of the projectile and set to shoot
        /// </summary>
        /// <param name="target"></param>
        public override void OnFire(Unit target)
        {
            if (CheckOwnerAndBarrelEnd(0))
                return;

            GameObject projectile;
            Projectile proj;
            GetAndSetProjectile(0, out projectile, out proj);

            Firing(target, projectile, proj);
        }

        /// <summary>
        /// Triggered by animation event, it handles the  preparation of the projectile and set to shoot
        /// from a secondary cannon
        /// </summary>
        /// <param name="target"></param>
        public override void OnFire2(Unit target)
        {
            if (CheckOwnerAndBarrelEnd(1))
                return;
            GameObject projectile;
            Projectile proj;
            GetAndSetProjectile(1, out projectile, out proj);

            Firing(target, projectile, proj);
        }

        /// <summary>
        /// Triggered by animation event, it handles the preparation of the projectile and set to shoot
        /// from a tertiary cannon
        /// </summary>
        /// <param name="target"></param>
        public override void OnFire3(Unit target)
        {
            if (CheckOwnerAndBarrelEnd(2))
                return;
            GameObject projectile;
            Projectile proj;
            GetAndSetProjectile(2, out projectile, out proj);

            Firing(target, projectile, proj);
        }

        /// <summary>
        /// Triggered by animation event, it handles the preparation of the projectile and set to shoot
        /// from a quaternary cannon
        /// </summary>
        /// <param name="target"></param>
        public override void OnFire4(Unit target)
        {
            if (CheckOwnerAndBarrelEnd(3))
                return;
            GameObject projectile;
            Projectile proj;
            GetAndSetProjectile(3, out projectile, out proj);

            Firing(target, projectile, proj);
        }

        #endregion

        #region Fire handling

        /// <summary>
        /// load the projectile with payload and launch it
        /// handle impact spawning
        /// </summary>
        /// <param name="target">target</param>
        /// <param name="projectile">projectile gameobject</param>
        /// <param name="proj">projectile script</param>
        /// <returns>bool</returns>
        private void Firing(Unit target, GameObject projectile, Projectile proj)
        {
            if (proj == null)
            {
                Debug.Log("Prefab doesn't have a projectile script");
                _projectilePool.Release(projectile);
                return;
            }
            if (_audioSource && _audioClip)
                _audioSource.PlayOneShot(_audioClip);

            _projectileShot.Add(projectile);
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
                    impactVFX.GetComponent<AutoRelease>().Arm(_impactPool, _impactLifeTime);
                },
                impactVFXAoE: (pos, normal) =>
                {
                    int count = 0;
                    foreach (Vector3 p in pos)
                    {
                        if (count == 0)
                        {
                            GameObject impactVFX = _impactPool.Get();
                            impactVFX.transform.SetPositionAndRotation(p, Quaternion.LookRotation(normal));
                            impactVFX.GetComponent<AutoRelease>().Arm(_impactPool, _impactLifeTime);
                        }
                        else
                        {
                            GameObject impactVFXAoE = _impactAoEPool.Get();
                            impactVFXAoE.transform.SetPositionAndRotation(p, Quaternion.LookRotation(normal));
                            impactVFXAoE.GetComponent<AutoRelease>().Arm(_impactAoEPool, _impactLifeTime);
                        }
                        count++;
                    }
                },
                onDone: () =>
                {
                    _projectilePool.Release(projectile);
                    _projectileShot.Remove(projectile);
                }
            );
            return;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// check if owner is set and barrel end exist
        /// </summary>
        /// <param name="barrelIndex"></param>
        /// <returns></returns>
        private bool CheckOwnerAndBarrelEnd(int barrelIndex)
        {
            return _owner == null || BarrelEnd[barrelIndex] == null;
        }

        /// <summary>
        /// get projectile from pool and set it at right position
        /// </summary>
        /// <param name="index"></param>
        /// <param name="projectile"></param>
        /// <param name="proj"></param>
        private void GetAndSetProjectile(int index, out GameObject projectile, out Projectile proj)
        {
            projectile = _projectilePool.Get();
            projectile.transform.SetPositionAndRotation(BarrelEnd[index].position, BarrelEnd[index].rotation);
            proj = projectile.GetComponent<Projectile>();
        }

        public override void ClearProjectiles()
        {
            if (_projectileShot == null) return;

            for (int i = _projectileShot.Count - 1; i >= 0; i--)
            {
                var go = _projectileShot[i];
                if (!go) { _projectileShot.RemoveAt(i); continue; }

                var proj = go.GetComponent<Projectile>();
                proj?.Abort();
                _projectilePool.Release(go);
                _projectileShot.RemoveAt(i);
            }
        }

        #endregion
    }

}