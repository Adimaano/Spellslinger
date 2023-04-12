using UnityEngine.VFX;
using UnityEngine;

public class FireBallSpell : MonoBehaviour {
    [SerializeField] private ParticleSystem onCollisionParticleSystem;

    private float speed = 8.0f;
    private Vector3 spellDirection = Vector3.forward;

    public Vector3 SpellDirection { get => spellDirection; set => spellDirection = value; }

    private void Update() {
        transform.position += SpellDirection * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Inflammable")) {
            // Check if the object has a Visual Effect component and enable it if not already enabled
            VisualEffect visualEffect = other.gameObject.GetComponent<VisualEffect>();
            if (visualEffect != null) {
                if (!visualEffect.enabled) {
                    visualEffect.enabled = true;
                    other.gameObject.transform.GetChild(0).gameObject.SetActive(true);
                }
            }
        }
        
    }

    private void OnCollisionEnter(Collision collision) {
        Instantiate(onCollisionParticleSystem, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
