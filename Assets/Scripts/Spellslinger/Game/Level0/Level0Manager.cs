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
        maxIntensity = spotlight.intensity;
        for (float f = maxIntensity; f >= 0.0f; f=f+maxIntensity/100f)
        {
            spotlight.intensity = f;
            yield return new WaitForSeconds(0.05f);
        }
    }

    public void Update()
    {
        if(!book.enabled)
        {
            StartCoroutine(lightOn(spotlight2, 100.0f));
        }

        if(lastTorches.GetComponent<Fire>().isLit)
        {
            StartCoroutine(lightOn(reflectiveProbeObject.GetComponent<ReflectionProbe>(), 2.0f));
            StartCoroutine(lightOff(spotlight1));
            StartCoroutine(lightOff(spotlight2));

            //ToDo ambientLight needs a fade in too but much slower
            ambientLight.intensity = 21.0f;

            walkableArea1.SetActive(false);
            walkableArea2.SetActive(true);
        }
    }
}
