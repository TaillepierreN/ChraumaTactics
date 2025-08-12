using System;
using UnityEngine;

namespace CT.Units.Attacks
{

    public class Projectile : MonoBehaviour
    {
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
        private bool _isActive;
        private Action<Vector3, Vector3> _spawnImpact;
        private Action _onDone;

        /// <summary>
        /// Load the payload and set the projectile as launched
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="impactVFX"></param>
        /// <param name="onDone"></param>
        public void Launch(PayLoad payload, Action<Vector3, Vector3> impactVFX, Action onDone)
        {
            _payload = payload;
            _spawnImpact = impactVFX;
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
            if (_payload.IsAoe)
            {
                Collider[] hits = Physics.OverlapSphere(pos, _payload.Radius);
                foreach (Collider hit in hits)
                    hit.GetComponent<Unit>()?.TakeDamage(_payload.Damage);
            }
            else
            {
                _payload.Target?.TakeDamage(_payload.Damage);
            }

            _spawnImpact?.Invoke(pos, normal);

            _isActive = false;
            _onDone?.Invoke();
        }
	}

}