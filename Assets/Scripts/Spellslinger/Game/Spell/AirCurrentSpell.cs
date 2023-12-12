using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Spellslinger.Game.Spell
{
    public class AirCurrentSpell : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 10f;
        /// <summary>
        /// Multiplier for the speed of the current in physics calculations
        /// </summary>
        [SerializeField] private float speedMultiplier = 75f;
        [SerializeField] private Transform currentTransform;
        [SerializeField] private GameObject[] enableOnStart = new GameObject[2];

        // bool to check if the spell has started
        private bool _started = false;
        private float _startScale;
        private float _lifeTimer;

        // direction of the current
        private Vector3 _direction;

        // speed / magnitude of the current
        private float _startSpeed;
        private float _currentSpeed;

        public void StartCurrent()
        {
            _started = true;
            _startScale = currentTransform.localScale.x;
            _currentSpeed = _startSpeed;
            foreach (var obj in enableOnStart)
            {
                obj.SetActive(true);
            }
        }

        // Update the visual of the current
        // direction is a normalized vector
        public void UpdateCurrent(Vector3 direction, float speed, bool local = false)
        {
            _direction = direction;
            _startSpeed = speed;
            // rotate current to face direction
            if (local)
            {
                currentTransform.localRotation = Quaternion.LookRotation(direction);
            }
            else
            {
                currentTransform.LookAt(currentTransform.position + _direction);
            }
            // scale air current in all directions based on speed with clamp
            float scale = Mathf.Clamp(_startSpeed, 0.1f, 0.5f);
            currentTransform.localScale = new Vector3(scale, scale, scale);
        }


        private void OnTriggerStay(Collider other)
        {
            // Apply wind force to objects in the trigger
            if (!other.gameObject.CompareTag("PhysicsObject")) return;
            // Get rigidbody of object
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            var dir = _direction;
            // rotate direction by current world rotation as well
            dir = transform.rotation * dir;
            // Apply force to object
            // rb.AddForce(dir * _currentSpeed * speedMultiplier, ForceMode.Force);
            // Set velocity of object
            rb.velocity = dir * _currentSpeed * speedMultiplier;
        }

        private void Update()
        {
            if (!_started || lifeTime == 0) return;
            _lifeTimer += Time.deltaTime;
            
            // scale current based on life time
            var lifePercent = _lifeTimer / lifeTime;
            // fade out faster at end, slower at start
            var scale = Mathf.Lerp(_startScale, 0, lifePercent * lifePercent);
            currentTransform.localScale = new Vector3(scale, scale, scale);
            _currentSpeed = Mathf.Lerp(_startSpeed, 0, lifePercent * lifePercent);
            
            if (_lifeTimer >= lifeTime)
            {
                Destroy(this.gameObject);
            }
        }
    }
}