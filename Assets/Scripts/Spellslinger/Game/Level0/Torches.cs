using System.Collections;
using System.Collections.Generic;
using UnityEngine.VFX;
using UnityEngine;

public class Torches : MonoBehaviour
{
    private VisualEffect fire;
    private bool firehit=false;
    private Light light;

    [SerializeField]
    public bool isTarget;
    public bool isLit;
    public GameObject refFire;

    void Start(){
        if(isTarget){
            this.fire = this.transform.Find("Fire").GetComponent<VisualEffect>();
            this.light = this.transform.Find("Fire").transform.Find("Point Light").GetComponent<Light>();
            Debug.Log("Target Awake");
        }
        else{
            this.fire = this.transform.GetComponent<VisualEffect>();
            this.light = this.transform.Find("Point Light").GetComponent<Light>();
            Debug.Log("Not Target Awake");
        }
        Debug.Log("Awake");
        this.light.intensity = 0f;
        this.isLit = false;
        ExtinguishTorch();
    }

    private IEnumerator SlowLightTorch() {
        yield return new WaitForSeconds(1.0f);
        this.fire.Play();
        this.isLit = true;
        yield return new WaitForSeconds(1.0f);
        for (int i = 0; i < 10; i++)
        {
            this.light.intensity += 0.0007f;
            yield return new WaitForSeconds(0.3f);
        }
    }

    private IEnumerator LightTorch() {
        this.fire.Play();
        this.isLit = true;
        yield return new WaitForSeconds(1.0f);
        for (int i = 0; i < 10; i++)
        {
            this.light.intensity += 0.0008f;
            yield return new WaitForSeconds(0.1f);
        }
        firehit=false;
    }

    private void ExtinguishTorch() {
        this.light.intensity = 0f;
        this.isLit = false;
        this.fire.Stop();
    }

    private void OnTriggerEnter(Collider other) {
        if(this.isTarget){
            Debug.Log("Target func OnTriggerEnter");
            if (other.gameObject.tag == "Fire" && !this.isLit) {
                firehit=true;
            }
        }
    }

    void Update(){
        if(!this.isTarget){
            if (refFire.GetComponent<Torches>().isLit == true && this.isLit == false){
                StartCoroutine(SlowLightTorch());
            }

            if (refFire.GetComponent<Torches>().isLit == false && this.isLit == true){
                StartCoroutine(SlowLightTorch());
            }
        }
        if (firehit){
            StartCoroutine(LightTorch());
            Debug.Log("LightTorch0");
        }
    }
}