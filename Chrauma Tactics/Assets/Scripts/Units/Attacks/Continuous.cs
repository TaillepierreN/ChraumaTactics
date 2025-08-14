using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace CT.Units.Attacks
{
    public class Continuous : Attack
    {
        /// <summary>
        /// May be changed by atkspeed?
        /// </summary>
        [SerializeField] private float tickInterval = 0.2f;
        [SerializeField] private LineRenderer[] beams;
        [SerializeField] private Transform[] impactMarkers;

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


        /// <summary>
        /// Addon to attack initialisation for the number of weapons,their beams and attack loops
        /// </summary>
        /// <param name="owner"></param>
        public override void Initialize(Unit owner)
        {
            base.Initialize(owner);

            /*match the arrays to the number of weapons firing*/
            int numberOfBarrel = BarrelEnd?.Length ?? 1;
            if (beams == null || beams.Length != numberOfBarrel)
                System.Array.Resize(ref beams, numberOfBarrel);
            if (impactMarkers == null || impactMarkers.Length != numberOfBarrel)
                System.Array.Resize(ref impactMarkers, numberOfBarrel);
            if (_loops == null || _loops.Length != numberOfBarrel)
                _loops = new Coroutine[numberOfBarrel];

            for (int i = 0; i < beams.Length; i++)
                if (beams[i] != null)
                    beams[i].enabled = false;
            for (int i = 0; i < impactMarkers.Length; i++)
                if (impactMarkers[i] != null)
                    impactMarkers[i].gameObject.SetActive(false);
            _damage = owner.CurrentAtk;
        }

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

            if (beams != null && barrelIndex < beams.Length && beams[barrelIndex] != null)
                beams[barrelIndex].enabled = false;

            if (impactMarkers != null && barrelIndex < impactMarkers.Length && impactMarkers[barrelIndex] != null)
                impactMarkers[barrelIndex].gameObject.SetActive(false);

            StopAudio();
        }

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
            LineRenderer beam = (beams != null && index < beams.Length) ? beams[index] : null;
            Transform shootPosition = (BarrelEnd != null && index < BarrelEnd.Length && BarrelEnd[index] != null) ? BarrelEnd[index] : null;
            Transform marker = (impactMarkers != null && index < impactMarkers.Length) ? impactMarkers[index] : null;

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

                while (timeAccumulation >= tickInterval)
                {
                    timeAccumulation -= tickInterval;
                    int dmg = Mathf.RoundToInt(_damage * tickInterval);

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
}
