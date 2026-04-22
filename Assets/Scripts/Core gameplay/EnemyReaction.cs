using UnityEngine;

namespace CoreGameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyReaction : MonoBehaviour
    {
        [SerializeField] private float knockbackForce = 25f;
        [SerializeField] private float upwardForce = 2f;
        [SerializeField] private float randomSpread = 0.1f;
        private Rigidbody rb;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true; // выключаем физику до столкновения
            rb.useGravity = false;
        }

        public void ReactToHit(Vector3 hitDirection, float worldSpeed)
        {
            // отключаем автодвижение от спавнера
            rb.isKinematic = false; // включаем физику
            rb.useGravity = true;          

                // немного разброса, чтобы удары не были идентичными
                Vector3 random = new Vector3(0,Random.Range(0f, randomSpread), Random.Range(-randomSpread, randomSpread));

            // направление — строго вперёд по игроку
            Vector3 finalDir = (hitDirection + random).normalized;

            // Масштабируем силу в зависимости от скорости мира
            float scaledKnockback = knockbackForce;
          
            // применяем импульс
            rb.AddForce(finalDir + Vector3.up * upwardForce, ForceMode.Impulse);
            rb.AddForce(finalDir * knockbackForce, ForceMode.Impulse);

            // лёгкое вращение для визуала
            rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);

            //PlayerHealth.TakeDamage();
            // уничтожаем через несколько секунд
            Destroy(gameObject, 3f);
        }
    }
}