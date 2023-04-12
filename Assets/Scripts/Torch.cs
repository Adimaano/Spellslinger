using UnityEngine;
using UnityEngine.VFX;

public class Torch : MonoBehaviour {
    private VisualEffect fire;
    [SerializeField] private bool isLit = false;
    
    void Start() {
        fire = transform.Find("Fire").GetComponent<VisualEffect>();

        if (isLit) {
            LightTorch();
        } else {
            ExtinguishTorch();
        }
    }

    private void LightTorch() {
        isLit = true;
        fire.Play();
    }

    private void ExtinguishTorch() {
        isLit = false;
        fire.Stop();
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log(other.gameObject.name);
        Debug.Log(other.gameObject.tag);

        if (other.gameObject.tag == "Fire" && !isLit) {
            LightTorch();
        }
    }
}
