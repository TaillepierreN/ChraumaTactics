using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CT.Units.Attacks
{
    public class Continuous : Attack
    {
        /// <summary>
        /// May be changed by atkspeed?
        /// </summary>
        [SerializeField] private float _tickInterval = 0.2f;

        /// <summary>
        /// Laser visual, may change from linerenderer to vfxgraph
        /// </summary>
        [SerializeField] private LineRenderer[] _beams;
        [SerializeField] private Transform[] _impactMarkers;

        private float _damage = 10f;
        private Coroutine[] _loops;
        /// <summary>
        /// Tracks which units have already been hit during this AoE tick to prevent applying damage multiple times.
        /// </summary>
        private readonly HashSet<Unit> _aoeSeen = new();
        /// <summary>
        /// Reusable buffer for storing colliders hit during AoE detection to avoid memory allocations each tick.
        /// </summary>
        private Collider[] _aoeBuffer = new Collider[50];

        /// <summary>
        /// Bool for the unit to start attacking directly without relying on animation
        /// </summary>
        [HideInInspector]
        public override bool IsContinuous => true;


        #region Initialisation

        /// <summary>
        /// Addon to attack initialisation for the number of weapons,their beams and attack loops
        /// </summary>
        /// <param name="owner"></param>
        public override void Initialize(Unit owner)
        {
            base.Initialize(owner);

            /*match the arrays to the number of weapons firing*/
            int numberOfBarrel = BarrelEnd?.Length ?? 1;
            if (_beams == null || _beams.Length != numberOfBarrel)
                System.Array.Resize(ref _beams, numberOfBarrel);
            if (_impactMarkers == null || _impactMarkers.Length != numberOfBarrel)
                System.Array.Resize(ref _impactMarkers, numberOfBarrel);
            if (_loops == null || _loops.Length != numberOfBarrel)
                _loops = new Coroutine[numberOfBarrel];

            for (int i = 0; i < _beams.Length; i++)
                if (_beams[i] != null)
                    _beams[i].enabled = false;
            for (int i = 0; i < _impactMarkers.Length; i++)
                if (_impactMarkers[i] != null)
                    _impactMarkers[i].gameObject.SetActive(false);
            _damage = owner.CurrentAtk;
        }

        #endregion

        #region Action

        /// <summary>
        /// called by Unit to start shooting
        /// </summary>
        /// <param name="target">enemy</param>
        public override void StartAutoFire(Unit target)
        {
            for (int i = 0; i < BarrelEnd.Length; i++)
                StartBeam(i, target);
        }

        /// <summary>
        /// called by Unit to stop shooting
        /// </summary>
        /// <param name="target">enemy</param>
        public override void StopAutoFire()
        {
            OnStop();
        }

        /// <summary>
        /// Start individual beams, unsused at this point
        /// </summary>
        /// <param name="target"></param>
        public override void OnFire(Unit target) => StartBeam(0, target);
        public override void OnFire2(Unit target) => StartBeam(1, target);
        public override void OnFire3(Unit target) => StartBeam(2, target);
        public override void OnFire4(Unit target) => StartBeam(3, target);

        /// <summary>
        /// Stop attacking with all beams
        /// </summary>
        public override void OnStop()
        {
            if (_loops == null)
                return;
            for (int i = 0; i < _loops.Length; i++)
                StopBeam(i);
        }

        /// <summary>
        /// start beam of index weapon
        /// </summary>
        /// <param name="barrelIndex">weapon that is shooting</param>
        /// <param name="target">enemy</param>
        private void StartBeam(int barrelIndex, Unit target)
        {
            if (_owner == null)
                return;
            if (BarrelEnd == null || BarrelEnd.Length <= barrelIndex)
                return;

            if (!CheckLoopsLength(barrelIndex))
                return;

            if (CheckLoopExisting(barrelIndex))
                return;

            if (_audioClip && _audioSource && !AnyBeamRunning())
            {
                _audioSource.clip = _audioClip;
                _audioSource.loop = true;
                _audioSource.Play();
            }
            _damage = _owner.CurrentAtk;

            _loops[barrelIndex] = StartCoroutine(BeamLoop(barrelIndex, target));
        }

        /// <summary>
        /// stop beam of index weapon
        /// </summary>
        /// <param name="barrelIndex">weapon that will stop shooting</param>
        private void StopBeam(int barrelIndex)
        {
            if (!CheckLoopsLength(barrelIndex))
                return;

            if (CheckLoopExisting(barrelIndex))
            {
                StopCoroutine(_loops[barrelIndex]);
                _loops[barrelIndex] = null;
            }

            if (_beams != null && barrelIndex < _beams.Length && _beams[barrelIndex] != null)
                _beams[barrelIndex].enabled = false;

            if (_impactMarkers != null && barrelIndex < _impactMarkers.Length && _impactMarkers[barrelIndex] != null)
                _impactMarkers[barrelIndex].gameObject.SetActive(false);

            StopAudio();
        }

        #endregion

        #region Loop

        /// <summary>
        /// Coroutine of the beam lifecycle
        /// start the beam at shootpositon to target hitbox
        /// accumulate time to accurately trigger damage at tickInterval
        /// if aoe, use overlap sphere non alloc(buffered in aoebuffer) to iterate through enemies and damage them
        /// and finally stop damaging,visuals and audio at the end
        /// </summary>
        /// <param name="index">weapon index</param>
        /// <param name="target">enemy</param>
        /// <returns></returns>
        IEnumerator BeamLoop(int index, Unit target)
        {
            LineRenderer beam = (_beams != null && index < _beams.Length) ? _beams[index] : null;
            Transform shootPosition = (BarrelEnd != null && index < BarrelEnd.Length && BarrelEnd[index] != null) ? BarrelEnd[index] : null;
            Transform marker = (_impactMarkers != null && index < _impactMarkers.Length) ? _impactMarkers[index] : null;

            if (beam)
                beam.enabled = true;
            if (marker)
                marker.gameObject.SetActive(true);

            float timeAccumulation = 0f;

            while (target != null && target.gameObject.activeInHierarchy)
            {
                Vector3 startPos = shootPosition.position;
                Vector3 endPos = target.Hitbox ? target.Hitbox.position : target.transform.position;

                if (beam)
                {
                    beam.SetPosition(0, startPos);
                    beam.SetPosition(1, endPos);
                }
                if (marker)
                    marker.position = endPos;

                timeAccumulation += Time.deltaTime;

                while (timeAccumulation >= _tickInterval)
                {
                    timeAccumulation -= _tickInterval;
                    int dmg = Mathf.RoundToInt(_damage * _tickInterval);

                    if (_isAoe)
                    {
                        _aoeSeen.Clear();
                        int hitCount = Physics.OverlapSphereNonAlloc(endPos, _aoeRadius, _aoeBuffer);
                        if (hitCount == _aoeBuffer.Length)
                            Debug.LogWarning("aoe buffer is full, need to make it bigger");
                        for (int i = 0; i < hitCount; i++)
                        {
                            Collider col = _aoeBuffer[i];
                            if (!col)
                                continue;
                            if (!col.TryGetComponent(out Unit unit))
                                continue;
                            if (unit == _owner || unit.team == _owner.team)
                                continue;
                            if (_aoeSeen.Add(unit))
                                unit.TakeDamage(dmg);
                        }
                    }
                    else
                    {
                        target.TakeDamage(dmg);
                    }
                    if (target == null || !target.gameObject.activeInHierarchy)
                        break;
                }
                yield return null;
            }

            if (beam)
                beam.enabled = false;
            if (marker)
                marker.gameObject.SetActive(false);
            _loops[index] = null;
            StopAudio();

        }

        #endregion

        #region Helper

        /// <summary>
        /// check if loops is null and if barrelindex is within loops length
        /// </summary>
        /// <param name="barrelIndex"></param>
        /// <returns></returns>
        private bool CheckLoopsLength(int barrelIndex)
        {
            return _loops != null && barrelIndex >= 0 && _loops.Length > barrelIndex;
        }

        /// <summary>
        /// check if loops has a loop at barrel index
        /// </summary>
        /// <param name="barrelIndex"></param>
        /// <returns>true if there is a loop</returns>
        private bool CheckLoopExisting(int barrelIndex)
        {
            return _loops[barrelIndex] != null;
        }

        /// <summary>
        /// check for any beam already on
        /// </summary>
        /// <returns></returns>
        private bool AnyBeamRunning()
        {
            if (_loops == null)
                return false;
            for (int i = 0; i < _loops.Length; i++)
                if (_loops[i] != null)
                    return true;
            return false;
        }

        /// <summary>
        /// stop the beam attack audio
        /// </summary>
        private void StopAudio()
        {
            if (_audioSource && !AnyBeamRunning())
            {
                _audioSource.loop = false;
                _audioSource.Stop();
            }
        }

    }
    #endregion
}
