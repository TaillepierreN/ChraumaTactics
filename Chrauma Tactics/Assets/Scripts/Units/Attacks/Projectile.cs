using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace CT.Units.Attacks
{

    public class Projectile : MonoBehaviour
    {
        public bool DebugMode;
        /// <summary>
        /// The payload has all relevant data for the projectile to do it's job
        /// Source: the unit that fired,
        /// Target: Who you fired on,
        /// Speed: Speed of the projectile,
        /// Damage: Damage of the projectile,
        /// IsAoe: If splash damage,
        /// Radius: how much splash
        /// </summary>
        public struct PayLoad
        {
            public Unit Source;
            public Unit Target;
            public float Speed;
            public int Damage;
            public bool IsAoe;
            public float Radius;
        }

        private PayLoad _payload;
        private Unit _owner;
        private bool _isActive;
        private Action<Vector3, Vector3> _spawnImpact;
        private Action<List<Vector3>, Vector3> _spawnAoEImpact;
        private List<Vector3> _aoeTargetPos = new List<Vector3>();
        private Action _onDone;

        private Vector3 debugPos;

        /// <summary>
        /// Load the payload and set the projectile as launched
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="impactVFX"></param>
        /// <param name="onDone"></param>
        public void Launch(PayLoad payload, Action<Vector3, Vector3> impactVFX, Action<List<Vector3>, Vector3> impactVFXAoE, Action onDone)
        {
            _owner = payload.Source;
            _payload = payload;
            _spawnImpact = impactVFX;
            _spawnAoEImpact = impactVFXAoE;
            _onDone = onDone;
            _isActive = true;
        }

        /// <summary>
        /// Handle the movement of the projectile and what to do once enemy is hit
        /// </summary>
        void Update()
        {
            if (!_isActive) return;

            if (_payload.Target == null || !_payload.Target.gameObject.activeInHierarchy)
            {
                _isActive = false;
                _onDone?.Invoke();
                return;
            }

            /*movement toward the target*/
            Vector3 targetPos = _payload.Target.Hitbox.position;
            Vector3 direction = targetPos - transform.position;
            float step = _payload.Speed * Time.deltaTime;

            if (direction.magnitude <= step)
            {
                Hit(targetPos, direction == Vector3.zero ? Vector3.up : -direction.normalized);
                return;
            }

            transform.position += direction.normalized * step;
            transform.forward = direction.normalized;
        }

        /// <summary>
        /// Handle what to do once the projectile was close enough to hit
        /// </summary>
        /// <param name="pos">position of the hit</param>
        /// <param name="normal">normal to be able to orient the impact</param>
        private void Hit(Vector3 pos, Vector3 normal)
        {
            debugPos = pos;
            if (_payload.IsAoe)
            {
                Collider[] hits = Physics.OverlapSphere(pos, _payload.Radius);
                _aoeTargetPos.Clear();
                int count = 1;
                _aoeTargetPos.Add(_payload.Target.Hitbox.position);
                _payload.Target?.TakeDamage(_payload.Damage);
                foreach (Collider hit in hits)
                {
                    if (hit.TryGetComponent(out Unit aoeTarget))
                    {
                        if (aoeTarget == _owner || aoeTarget.team == _owner.team || aoeTarget == _payload.Target) continue;
                        if (aoeTarget.Hitbox != null)
                        {
                            _aoeTargetPos.Add(aoeTarget.Hitbox.position);
                            count++;
                            aoeTarget.TakeDamage(_payload.Damage);
                        }
                    }
                }
                if (count > 1)
                    _spawnAoEImpact?.Invoke(_aoeTargetPos, normal);
                else
                    Debug.Log("no valid target");


            }
            else
            {
                _payload.Target?.TakeDamage(_payload.Damage);
                _spawnImpact?.Invoke(pos, normal);
            }


            _isActive = false;
            _onDone?.Invoke();
        }
        private void OnDrawGizmosSelected()
        {
            if (DebugMode && debugPos != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, _payload.Radius);
            }
        }
    }

}