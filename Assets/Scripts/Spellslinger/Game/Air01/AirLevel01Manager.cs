using System;
using System.Collections;
using Spellslinger.Game.Control;
using Spellslinger.Game.Environment;
using Spellslinger.Game.Manager;
using UnityEngine;

namespace Spellslinger.Game.AirLevel01
{
    public class AirLevel01Manager : MonoBehaviour
    {
        [SerializeField] private GameObject crystalBall;
        [SerializeField] private Transform crystalBallStartTransform;
        [SerializeField] private Transform currentCheckpoint;
        
        [SerializeField] private GameObject portal;
        [SerializeField] private Material portalMaterial;
        [SerializeField] private Material portalMaterialDefault;
        private Color portalMaterialColor;
        private float portalActivationDuration = 1.0f;

        [SerializeField] private PedestalController[] pedestals = new PedestalController[3];
        [SerializeField] private PedestalController finalPedestal;

        [SerializeField] private Animator plankAnimator;
        [SerializeField] private PedestalController startIslandPedestal;
        [SerializeField] private GameObject startIslandBall;
        [SerializeField] private Transform startIslandBallStartTransform;
        
        [SerializeField] private AudioClip puzzleSolvedSound;
        
        [SerializeField] private AudioSource wizardVoice;
        [SerializeField] private AudioClip wizIntro;
        [SerializeField] private AudioClip wizSolved;

        private void Start()
        {
            // GameObject.Find("-- XR --").GetComponent<Player>().LearnNewSpell(SpellCasting.Spell.Air);

            foreach (var ped in pedestals)
            {
                ped.IsActivated.AddListener(Activated);
                // set child collider to false
                ped.GetComponentsInChildren<Collider>()[1].enabled = false;
            }

            finalPedestal.IsActivated.AddListener(Activated);
            // set child collider to false
            finalPedestal.GetComponentsInChildren<Collider>()[1].enabled = false;
            
            startIslandPedestal.IsActivated.AddListener(IslandActivated);
            
            this.StartCoroutine(this.PlayWizardVoiceDelayed(this.wizIntro, 3.5f));
            currentCheckpoint = crystalBallStartTransform;

            Color baseEmissionColor = this.portalMaterialDefault.GetColor("_EmissionColor");
            this.portalMaterial.SetColor("_EmissionColor", baseEmissionColor);
        }
        
        /// <summary>
        /// Coroutine that plays an audioclip for the wizard after a delay.
        /// </summary>
        /// <param name="clip">The audioclip to play.</param>
        private IEnumerator PlayWizardVoiceDelayed(AudioClip clip, float delay) {
            yield return new WaitForSeconds(delay);
            this.PlayWizardVoice(clip);
        }
        
        /// <summary>
        /// Plays an audioclip for the wizard.
        /// </summary>
        /// <param name="clip">The audioclip to play.</param>
        private void PlayWizardVoice(AudioClip clip) {
            this.wizardVoice.Stop();
            this.wizardVoice.PlayOneShot(clip);
        }
        
        private void IslandActivated(PedestalController contr)
        {
            // activate colliders of normal pedestals
            foreach (var ped in pedestals)
            {
                ped.GetComponentsInChildren<Collider>()[1].enabled = true;
            }
            
            // activate plank animation
            plankAnimator.SetTrigger("FloatToCenter");
            GameManager.Instance.PlayAudioClip(this.puzzleSolvedSound, 0.15f);
        }

        private void Activated(PedestalController contr)
        {
            if (contr.Checkpoint != null)
            {
                currentCheckpoint = contr.Checkpoint;
            }
            
            // Check if all pedestals are activated
            foreach (var ped in pedestals)
            {
                if (!ped.Active)
                {
                    return;
                }
            }

            // now we can activate the final pedestal collider (in child "Catcher")
            finalPedestal.GetComponentsInChildren<Collider>()[1].enabled = true;

            if (!finalPedestal.Active)
            {
                return;
            }

            this.StartCoroutine(this.ActivatePortal());

            // portal.IsActive = true;
            // enable first child of portal
            // portal.transform.GetChild(0).gameObject.SetActive(true);
            // play solved sound
            this.PlayWizardVoice(this.wizSolved);
        }
        

        private void CheckBelow(GameObject obj, Transform startTransform)
        {
            // Check if crystal ball is below 30 units from the start position y
            if (obj.transform.position.y < startTransform.position.y - 3)
            {
                // Reset crystal ball position
                obj.transform.position = startTransform.position;
                // Reset crystal ball velocity
                obj.GetComponent<Rigidbody>().velocity = Vector3.zero;
                // Reset crystal ball angular velocity
                obj.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// Coroutine that activates the portal. Enables the portal GameObject, fades the emission
        /// intensity of the portal material to 7 and lights all torches.
        /// </summary>
        private IEnumerator ActivatePortal() {
            yield return new WaitForSeconds(1.5f);

            this.portal.SetActive(true);
            this.portal.transform.parent.gameObject.GetComponent<Portal>().IsActive = true;

            float elapsedTime = 0.0f;
            Color baseEmissionColor = this.portalMaterial.GetColor("_EmissionColor");

            while (elapsedTime < this.portalActivationDuration) {
                elapsedTime += Time.deltaTime;
                float currentIntensity = Mathf.Lerp(0.0f, 60.0f, elapsedTime / this.portalActivationDuration);

                this.portalMaterialColor = baseEmissionColor * currentIntensity;
                this.portalMaterial.SetColor("_EmissionColor", this.portalMaterialColor);

                yield return null;
            }

            // Set the final intensity value
            Color finalEmissionColor = baseEmissionColor * 60.0f;
            this.portalMaterial.SetColor("_EmissionColor", finalEmissionColor);
        }

        private void FixedUpdate()
        {
            if (finalPedestal.Active)
            {
                return;
            }

            CheckBelow(crystalBall, currentCheckpoint);
            if (!startIslandPedestal.Active)
                CheckBelow(startIslandBall, startIslandBallStartTransform);
        }
    }
}