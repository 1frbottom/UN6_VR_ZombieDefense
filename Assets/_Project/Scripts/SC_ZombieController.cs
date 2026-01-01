using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;



public class ZombieController : MonoBehaviour
{
    public Transform target; // Camera Rig 드래그
    NavMeshAgent agent;
    Animator anim;

    bool isDead = false;

    public float attackRange = 1.0f;

    AudioSource audioSource;

    [Header("Health UI")]
    public Image hpBarFill;
    public float maxHealth = 100.0f;
    float currentHealth;

    float attackTimer = 0f;
    public float attackInterval = 1.5f;



    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth;

    }

    void Update()
    {
        if (target == null || isDead)
            return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= attackRange)
        {
            // 1. 공격 모드
            agent.isStopped = true;
            anim.SetBool("bIsWalking", false);
            anim.SetBool("bIsAttacking", true);

            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                // 타겟이 기지(Base)라면 데미지 입힘
                if (target.CompareTag("Base"))
                {
                    var gm = FindAnyObjectByType<GameManager>();
                    if (gm != null)
                        gm.OnBaseAttacked(5);
                }

                attackTimer = 0.0f;
            }

            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
        }
        else
        {
            // 2. 추적 모드
            agent.isStopped = false;
            agent.SetDestination(target.position);
            anim.SetBool("bIsWalking", true);
            anim.SetBool("bIsAttacking", false);
            attackTimer = attackInterval; // 붙자마자 바로 때리게 타이머 미리 채움
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        // HpBar UI 업데이트
        if (hpBarFill != null)
            hpBarFill.fillAmount = currentHealth / maxHealth;

        if (currentHealth <= 0)
            OnDie();
    }

    public void OnDie()
    {
        isDead = true;
        agent.enabled = false; // 네비메쉬 에이전트 끄기

        // 1. 물리 충돌 및 밀림 방지
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // 물리 엔진의 영향을 받지 않게 설정
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false; // 콜라이더를 꺼서 플레이어가 통과하게 함
        }

        if (audioSource != null)
            audioSource.Stop();

        int randomDeath = Random.Range(0, 2);
        anim.SetInteger("DeathType", randomDeath);
        anim.SetTrigger("Die");

        FindAnyObjectByType<GameManager>().OnZombieDied();

        Destroy(gameObject, 5.0f);

        this.enabled = false; // 스크립트 로직 중단
    }



}