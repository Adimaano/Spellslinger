namespace Spellslinger.Game.Environment
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ShootingPillar : MonoBehaviour {
        [SerializeField] private float shootInterval = 2.0f;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform projectileOrigin;

        private bool isShooting = false;

        private void OnEnable() {
            // Start shooting projectiles in intervals
            this.isShooting = true;
            this.StartCoroutine(this.ShootProjectiles());
        }

        private void OnDisable() {
            // Stop shooting projectiles
            this.isShooting = false;
        }

        private IEnumerator ShootProjectiles() {
            while (true && this.isShooting) {
                yield return new WaitForSeconds(this.shootInterval);

                // Instantiate a projectile and shoot it
                GameObject projectile = Instantiate(this.projectilePrefab, this.projectileOrigin.position, Quaternion.identity);
                projectile.GetComponent<Rigidbody>().AddForce(this.projectileOrigin.forward * 1000.0f);
            }
        }
    }
}