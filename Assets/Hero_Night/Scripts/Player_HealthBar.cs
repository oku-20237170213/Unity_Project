using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Player_HealthBar : MonoBehaviour
{
    public Image BackGroundImg;
    public Image HealthImg;

    public float MaxHealth;
    float CurrentHealth;

    public bool isDead = false;
    bool isHurting = false;

    Animator animator;

    void Start()
    {
        CurrentHealth = MaxHealth;
        animator = GetComponentInChildren<Animator>();
        HealthImg.fillAmount = 1f;
        BackGroundImg.fillAmount = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            GetDamage(10);
        }

        BackGroundImg.fillAmount = Mathf.Lerp(
            BackGroundImg.fillAmount,
            HealthImg.fillAmount,
            Time.deltaTime * 5f
        );
    }

    public void GetDamage(float damage)
    {
        if (isDead || isHurting) return;

        CurrentHealth -= damage;

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            isDead = true;
            animator.SetTrigger("death");
        }
        else
        {
            animator.SetTrigger("hurt");
            StartCoroutine(HurtCooldown());
        }

        HealthImg.fillAmount = CurrentHealth / MaxHealth;
    }

    IEnumerator HurtCooldown()
    {
        isHurting = true;
        yield return new WaitForSeconds(0.5f);
        isHurting = false;
    }
}
