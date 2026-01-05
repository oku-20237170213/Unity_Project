using System.Collections;
using TMPro;
using UnityEngine;

public class BossEnemyAI : MonoBehaviour
{
    [Header("Hedef ve Referanslar")]
    public Transform player;
    private Rigidbody2D rb;
    private Collider2D myCollider;
    private Animator anim;

    [Header("Boss İstatistikleri")]
    public float maxHealth = 500f;
    public float currentHealth;
    public float moveSpeed = 2.5f;

    [Header("Mesafe Ayarları")]
    public float chaseDist = 8f;
    public float spellDist = 5f;
    public float attackDist = 1.5f;
    public float stopDist = 1.0f;

    [Header("Bekleme Süreleri")]
    public float attackCooldown = 2f;
    public float spellCooldown = 4f;
    public float healCooldown = 15f;
    public int meleeDamage = 15;
    public int spellDamage = 10;
    public int healAmount = 100;

    [Header("Görsel Ayarlar")]
    public Transform visualChild;
    public bool spriteYonuTers = false;

    // Durumlar
    private bool isDead = false;
    private bool isBusy = false;
    private float nextAttackTime = 0f;
    private float nextSpellTime = 0f;
    private float nextHealTime = 0f;

    public TMP_Text deathTex;

    private float flipDeadzone = 0.5f; 

    public Player_Script playerScript;
    public GameObject attackHitbox;

    private AudioSource audioSource;
    public AudioClip YouDied_Sound;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
        anim = GetComponentInChildren<Animator>();

        currentHealth = maxHealth;

        if (visualChild == null && transform.childCount > 0)
        {
            visualChild = transform.GetChild(0);
        }

        if (anim == null) Debug.LogError("HATA: Boss_Sprite üzerinde Animator bulunamadı!");

        audioSource = GetComponent<AudioSource>();

        rb.gravityScale = 1;
        rb.freezeRotation = true;
    }

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                player = p.transform;
                playerScript = p.GetComponent<Player_Script>();
            }
        }

        if (attackHitbox != null) attackHitbox.SetActive(false);
    }

    void Update()
    {
        if (isDead || player == null) return;

        if (isBusy)
        {
            StopMovement();
            return;
        }

        float dist = Vector2.Distance(myCollider.bounds.center, player.position);
        float xDiff = player.position.x - transform.position.x;

        int direction = 0;
        if (Mathf.Abs(xDiff) > flipDeadzone)
        {
            direction = (xDiff > 0) ? 1 : -1;
            FacePlayer(direction);
        }

        if (currentHealth < (maxHealth * 0.4f) && Time.time >= nextHealTime)
        {
            StartCoroutine(ActionRoutine("cast", 1.5f, () =>
            {
                currentHealth += healAmount;
                if (currentHealth > maxHealth) currentHealth = maxHealth;
                nextHealTime = Time.time + healCooldown;
            }));
        }

        else if (dist <= attackDist)
        {
            StopMovement();

            if (Time.time >= nextAttackTime)
            {
                StartCoroutine(ActionRoutine("attack", 0.6f, () => {
                    if (playerScript != null)
                    {
                        playerScript.TriggerHurt(meleeDamage);
                    }
                }));
                nextAttackTime = Time.time + attackCooldown;
            }
        }

        else if (dist <= spellDist)
        {
            if (Time.time >= nextSpellTime)
            {
                StopMovement();
                StartCoroutine(ActionRoutine("spell", 0.8f, () => {
                    if (playerScript != null) playerScript.TriggerHurt(spellDamage);
                }));
                nextSpellTime = Time.time + spellCooldown;
            }

            else if (dist > stopDist)
            {
                MoveToPlayer(direction);
            }
            else
            {
                StopMovement();
            }
        }

        else if (dist <= chaseDist)
        {
            if (dist > stopDist)
                MoveToPlayer(direction);
            else
                StopMovement();
        }
        else
        {
            StopMovement();
        }
    }

    void MoveToPlayer(int dir)
    {

        if (dir == 0) return;

        anim.SetBool("walk", true);
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
    }

    void StopMovement()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        anim.SetBool("walk", false);
    }

    void FacePlayer(int direction)
    {
        if (visualChild != null && direction != 0)
        {
            Vector3 scale = visualChild.localScale;

            if (direction > 0)
                scale.x = Mathf.Abs(scale.x) * (spriteYonuTers ? -1 : 1);
            else
                scale.x = -Mathf.Abs(scale.x) * (spriteYonuTers ? -1 : 1);

            visualChild.localScale = scale;
        }
    }

    IEnumerator ActionRoutine(string triggerName, float delay, System.Action onActionExecute)
    {
        isBusy = true;
        StopMovement();

        anim.SetTrigger(triggerName);

        if (triggerName == "attack")
        {
            yield return new WaitForSeconds(delay);
            if (attackHitbox != null) attackHitbox.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            if (attackHitbox != null) attackHitbox.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(delay);
        }

        onActionExecute?.Invoke();

        yield return new WaitForSeconds(0.5f);

        isBusy = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (!isBusy)
        {
            anim.SetTrigger("hurt");
            StopMovement();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        isBusy = true;

        if (myCollider != null) myCollider.enabled = false;
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;

        if (audioSource != null && YouDied_Sound != null)
            audioSource.PlayOneShot(YouDied_Sound);

        if (deathTex != null) deathTex.gameObject.SetActive(true);
        anim.SetTrigger("death");

        StartCoroutine(StopGame());
    }

    IEnumerator StopGame()
    {
        yield return new WaitForSecondsRealtime(2f);
        Time.timeScale = 0f;
    }

    void OnDrawGizmos()
    {
        if (GetComponent<Collider2D>() != null)
        {
            Vector3 center = GetComponent<Collider2D>().bounds.center;
            Gizmos.color = Color.green; Gizmos.DrawWireSphere(center, chaseDist);
            Gizmos.color = Color.blue; Gizmos.DrawWireSphere(center, spellDist);
            Gizmos.color = Color.red; Gizmos.DrawWireSphere(center, attackDist);
        }
    }
}