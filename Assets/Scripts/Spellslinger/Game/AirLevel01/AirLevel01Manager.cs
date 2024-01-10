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
        
        [SerializeField] private Portal portal;

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
            GameObject.Find("-- XR --").GetComponent<Player>().LearnNewSpell(SpellCasting.Spell.Air);

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

            portal.IsActive = true;
            // enable first child of portal
            portal.transform.GetChild(0).gameObject.SetActive(true);
            // play solved sound
            this.PlayWizardVoice(this.wizSolved);
        }
        

        private void CheckBelow(GameObject obj, Transform startTransform)
        {
            // Check if crystal ball is below 30 units from the start position y
            if (obj.transform.position.y < startTransform.position.y - 30)
            {
                // Reset crystal ball position
                obj.transform.position = startTransform.position;
                // Reset crystal ball velocity
                obj.GetComponent<Rigidbody>().velocity = Vector3.zero;
                // Reset crystal ball angular velocity
                obj.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            }
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