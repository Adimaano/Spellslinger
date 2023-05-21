using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level0Manager : MonoBehaviour
{
    [SerializeField]
    public GameObject lastTorches, walkableArea1, walkableArea2, reflectiveProbeObject, book;
    public Light spotlight1, spotlight2, ambientLight;

    private void Start()
    {
        spotlight2.intensity = 0.0f;
        spotlight1.intensity = 400.0f;
        ambientLight.intensity = 0.0f;
        reflectiveProbeObject.GetComponent<ReflectionProbe>().intensity = 0.0f;
        walkableArea1.SetActive(true);
        walkableArea2.SetActive(false);
    }

    private IEnumerator reflectionProbeOn(ReflectionProbe probe, float maxIntensity)
    {
        for (float f = 0.0f; f <= maxIntensity; f=f-maxIntensity/100f)
        {
            probe.intensity = f;
            yield return new WaitForSeconds(0.2f);
        }
    }
    
    private IEnumerator lightOn(Light spotlight, float maxIntensity)
    {
        for (float f = 0.0f; f <= maxIntensity; f=f-maxIntensity/100f)
        {
            spotlight.intensity = f;
            yield return new WaitForSeconds(0.05f);
        }
    }

    private IEnumerator lightOff(Light spotlight)
    {
        float maxIntensity = spotlight.intensity;
        for (float f = maxIntensity; f >= 0.0f; f=f+maxIntensity/100f)
        {
            spotlight.intensity = f;
            yield return new WaitForSeconds(0.05f);
        }
    }

    public void Update()
    {
        if(!book.activeSelf)
        {
            StartCoroutine(lightOn(spotlight2, 100.0f));
        }

        if(lastTorches.transform.Find("Fire").GetComponent<Torches>().isLit)
        {
            StartCoroutine(reflectionProbeOn(reflectiveProbeObject.GetComponent<ReflectionProbe>(), 2.0f));
            StartCoroutine(lightOff(spotlight1));
            StartCoroutine(lightOff(spotlight2));
            StartCoroutine(lightOn(ambientLight, 22.0f));

            walkableArea1.SetActive(false);
            walkableArea2.SetActive(true);
        }
    }
}
