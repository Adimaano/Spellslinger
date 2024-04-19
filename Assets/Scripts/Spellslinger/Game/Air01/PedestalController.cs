using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Spellslinger.Game.AirLevel01
{
    public class PedestalController : MonoBehaviour
    {
        [SerializeField] private GameObject beam;
        [SerializeField] private bool final;
        [SerializeField] private Transform checkpoint;
        [SerializeField] private UnityEvent<PedestalController> isActivated = new();
        private Coroutine _runningCoroutine;
        private bool _active = false;

        public UnityEvent<PedestalController> IsActivated => isActivated;
        public Transform Checkpoint => checkpoint;

        public bool Active => _active;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("PhysicsObject"))
            {
                // Its the crystal ball
                other.GetComponent<Rigidbody>().velocity = Vector3.zero;
                other.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                if (_runningCoroutine != null)
                {
                    StopCoroutine(_runningCoroutine);
                }

                if (!_active)
                {
                    _runningCoroutine = StartCoroutine(MoveToCenter(other.gameObject));
                }
                else
                {
                    // if active, move instead to the right of pedestal
                    _runningCoroutine = StartCoroutine(MoveToRight(other.gameObject));
                }
            }
        }

        private IEnumerator MoveToRight(GameObject obj)
        {
            // Disable physics
            obj.GetComponent<Rigidbody>().isKinematic = true;
            // Disable gravity
            obj.GetComponent<Rigidbody>().useGravity = false;
            // Move to center of pedestal
            float time = 0;
            while (time < 1.5f)
            {
                obj.transform.position =
                    Vector3.Lerp(obj.transform.position,
                        transform.position + transform.right * 2f + transform.up * 0.5f,
                        time / 1.5f);
                time += Time.deltaTime;
                yield return null;
            }

            // Enable physics
            obj.GetComponent<Rigidbody>().isKinematic = false;
            // Enable gravity
            obj.GetComponent<Rigidbody>().useGravity = true;
        }


        private IEnumerator MoveToCenter(GameObject obj)
        {
            // Disable physics
            obj.GetComponent<Rigidbody>().isKinematic = true;
            // Disable gravity
            obj.GetComponent<Rigidbody>().useGravity = false;
            // Disable beam
            beam.SetActive(false);
            // Move to center of pedestal
            float time = 0;
            while (time < 3)
            {
                obj.transform.position =
                    Vector3.Lerp(obj.transform.position, transform.position + transform.up * 2f, time / 3);
                time += Time.deltaTime;
                yield return null;
            }

            _active = true;
            isActivated.Invoke(this);
            // Enable beam
            beam.SetActive(true);
            // Enable physics
            obj.GetComponent<Rigidbody>().isKinematic = false;
            // Enable gravity
            obj.GetComponent<Rigidbody>().useGravity = true;

            // if final pedestal, destroy object
            if (final)
            {
                Destroy(obj);
            }
        }
    }
}