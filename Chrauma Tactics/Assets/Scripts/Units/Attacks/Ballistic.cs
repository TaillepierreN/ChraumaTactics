using UnityEngine;
using UnityEngine.Pool;
using Unity.Netcode;
using CT.Tools;
using Unity.Netcode.Components;
using System.Collections;

namespace CT.Units.Attacks
{
    public class Ballistic : Attack
    {
        [SerializeField] private Rd_Gameplay _radioGameplay;
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
        private Transform _vfxRoot;


        [Header("Pools")]
        private ObjectPool<GameObject> _projectilePool;
        private ObjectPool<GameObject> _impactPool;
        private ObjectPool<GameObject> _impactAoEPool;
        private bool Online => NetX.NM && NetX.IsListening;



        #region Unity Callbacks

        void Start()
        {
            ResolveVfxRoot();
            EnsurePoolContainerParented();
            InitPools();
            PrewarmPools(_nbrOfPooledProjectile);
        }

        void OnDestroy()
        {
            _projectilePool?.Clear();
            _impactPool?.Clear();
            _impactAoEPool?.Clear();

        }

        #endregion
        #region Init

        /// <summary>
        /// initialize the owner and exit transform and damage of the projectile
        /// </summary>
        /// <param name="owner"></param>
        public override void Initialize(Unit owner)
        {
            base.Initialize(owner);
            _damage = owner.CurrentAtk;
        }

        private void ResolveVfxRoot()
        {
            if (!_vfxRoot && _radioGameplay && _radioGameplay.Pool)
                _vfxRoot = _radioGameplay.Pool;

            if (!_vfxRoot)
            {
                Debug.Log("can't find a general pool,so i'm making it myself");
                GameObject go = GameObject.Find("--- Pools ---") ?? new GameObject("--- Pools ---");
                _vfxRoot = go.transform;
            }
        }

        private void EnsurePoolContainerParented()
        {
            if (_poolContainer && _poolContainer.parent != _vfxRoot)
                _poolContainer.SetParent(_vfxRoot, worldPositionStays: true);
        }

        /// <summary>
        /// Initialize the pools of projectile and impact
        /// </summary>
        private void InitPools()
        {
            if (!Online && _projectilePrefab != null)
            {
                _projectilePool = new ObjectPool<GameObject>(
                    createFunc: () =>
                    {
                        GameObject go = Instantiate(_projectilePrefab);
                        NetRemover.StripNetcodeComponents(go);
                        StartCoroutine(StripThenParent(go));
                        go.SetActive(false);
                        return go;
                    },
                    actionOnGet: go =>
                    {
                        //go.transform.SetParent(_vfxRoot, true);
                        go.SetActive(true);
                    },
                    actionOnRelease: go =>
                    {
                        //go.transform.SetParent(_poolContainer, true);
                        go.SetActive(false);
                    },
                    actionOnDestroy: go => Destroy(go),
                    defaultCapacity: 32, maxSize: 256
                );
            }
            if (_impactVFXPrefab != null && _impactVFXPrefab.Length >= 1)
            {
                _impactPool = new ObjectPool<GameObject>(
                    createFunc: () =>
                    {
                        GameObject go = Instantiate(_impactVFXPrefab[0]);
                        if (!Online)
                            NetRemover.StripNetcodeComponents(go);

                        go.transform.SetParent(_poolContainer, true);
                        go.SetActive(false);
                        return go;
                    },
                    actionOnGet: go => { go.transform.SetParent(_vfxRoot, true); go.SetActive(true); },
                    actionOnRelease: go => { go.transform.SetParent(_poolContainer, true); go.SetActive(false); },
                    actionOnDestroy: go => Destroy(go),
                    defaultCapacity: 32, maxSize: 256
                );
            }
            if (_impactVFXPrefab != null && _impactVFXPrefab.Length > 1)
            {
                _impactAoEPool = new ObjectPool<GameObject>(
                    createFunc: () =>
                    {
                        GameObject go = Instantiate(_impactVFXPrefab[1]);
                        if (!Online)
                            NetRemover.StripNetcodeComponents(go);

                        go.transform.SetParent(_poolContainer, true);
                        go.SetActive(false);
                        return go;
                    },
                    actionOnGet: go => { go.transform.SetParent(_vfxRoot, true); go.SetActive(true); },
                    actionOnRelease: go => { go.transform.SetParent(_poolContainer, true); go.SetActive(false); },
                    actionOnDestroy: go => Destroy(go),
                    defaultCapacity: 32, maxSize: 256
                );
            }
        }

        private void PrewarmPools(int n)
        {
            for (int i = 0; i < n; i++)
            {
                if (_projectilePool != null)
                    _projectilePool.Release(_projectilePool.Get());
                if (_impactPool != null)
                    _impactPool.Release(_impactPool.Get());
                if (_impactAoEPool != null)
                    _impactAoEPool.Release(_impactAoEPool.Get());
            }
        }
        private IEnumerator StripThenParent(GameObject go)
        {
            NetRemover.StripNetcodeComponents(go);
            yield return null;
            go.transform.SetParent(_poolContainer, true);
        }


        #endregion

        #region TriggerPull

        /// <summary>
        /// Triggered by animation event, it handles preparation of the projectile and set to shoot
        /// </summary>
        /// <param name="target"></param>
        public override void OnFire(Unit target)
        {
            if (NetX.NM && NetX.IsListening && !NetX.IsServer)
                return;
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
            if (NetX.NM && NetX.IsListening && !NetX.IsServer)
                return;
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
            if (NetX.NM && NetX.IsListening && !NetX.IsServer)
                return;
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
            if (NetX.NM && NetX.IsListening && !NetX.IsServer)
                return;
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
                if (Online)
                    Destroy(projectile);
                else
                    _projectilePool?.Release(projectile);
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
                    _projectileShot.Remove(projectile);
                    NetworkObject no = projectile.GetComponent<NetworkObject>();
                    if (no && no.IsSpawned)
                    {
                        no.Despawn(true);
                    }
                    else
                    {
                        _projectilePool?.Release(projectile);
                    }
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
            /*Online*/
            if (Online && NetX.IsServer)
            {
                projectile = Instantiate(_projectilePrefab, BarrelEnd[index].position, BarrelEnd[index].rotation);

                if (!projectile.activeInHierarchy)
                    projectile.SetActive(true);

                NetworkTransform nt = projectile.GetComponent<NetworkTransform>();
                if (nt != null && !nt.enabled)
                    nt.enabled = true;

                NetworkObject no = projectile.GetComponent<NetworkObject>();
                if (no == null)
                {
                    Debug.LogError("[SRV] Projectile prefab missing NetworkObject on ROOT.");
                    Destroy(projectile);
                    proj = null;
                    return;
                }

                if (no && !no.IsSpawned)
                    no.Spawn();

                proj = projectile.GetComponent<Projectile>();
                return;
            }
            /*offline*/
            projectile = _projectilePool.Get();
            projectile.transform.SetPositionAndRotation(BarrelEnd[index].position, BarrelEnd[index].rotation);
            proj = projectile.GetComponent<Projectile>();
        }

        public override void ClearProjectiles()
        {
            if (_projectileShot == null) return;

            for (int i = _projectileShot.Count - 1; i >= 0; i--)
            {
                GameObject go = _projectileShot[i];
                if (!go)
                {
                    _projectileShot.RemoveAt(i);
                    continue;
                }

                Projectile proj = go.GetComponent<Projectile>();
                proj?.Abort();

                NetworkObject no = go.GetComponent<NetworkObject>();
                if (no && no.IsSpawned)
                {
                    no.Despawn(true);
                }
                else
                {
                    _projectilePool?.Release(go);
                }
                _projectileShot.RemoveAt(i);
            }
        }

        #endregion
    }

}