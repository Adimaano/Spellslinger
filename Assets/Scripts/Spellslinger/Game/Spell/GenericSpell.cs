namespace Spellslinger.Game.Spell
{
    using UnityEngine;

    public class GenericSpell : MonoBehaviour {
        private float speed = 8.0f;
        private float lifeTime = 5.0f;
        private float lifeTimer = 0.0f;

        public Vector3 SpellDirection { get; set; }

        private void Awake() {
            this.SpellDirection = Vector3.forward;
        }

        private void Update() {
            this.transform.position += this.SpellDirection * this.speed * Time.deltaTime;

            this.lifeTimer += Time.deltaTime;
            if (this.lifeTimer >= this.lifeTime) {
                Destroy(this.gameObject);
            }
        }

        protected virtual void OnCollisionEnter(Collision collision) {
            Destroy(this.gameObject);
        }
    }
}
