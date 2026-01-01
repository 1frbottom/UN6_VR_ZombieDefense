using UnityEngine;

public class SC_Bullet : MonoBehaviour
{
    public float damage = 35.0f;


    void OnCollisionEnter(Collision collision)
    {
        // 총이나 플레이어에 부딪히면 '아무것도 하지 말고' 그냥 나감 (삭제도 안 함)
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.name.Contains("Gun"))
        {
            return;
        }

        // 좀비 체크
        ZombieController zombie = collision.gameObject.GetComponentInParent<ZombieController>();
        if (zombie != null)
        {
            zombie.TakeDamage(damage);
        }

        // 좀비나 벽 등 '총이 아닌 것'에 부딪혔을 때만 삭제
        Destroy(gameObject);

    }


}