using UnityEngine;
using CrusaderUI.Scripts;

//LMJ: Connects Player health to HPFlowController UI
public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The player CharacterStats component")]
    [SerializeField] private CharacterStats playerStats;

    [Tooltip("The HPFlowController component (on HP bar image)")]
    [SerializeField] private HPFlowController hpFlowController;

    [Header("Settings")]
    [Tooltip("Update HP bar every frame")]
    [SerializeField] private bool updateEveryFrame = true;

    [Tooltip("Smooth HP bar transition speed")]
    [SerializeField] private float smoothSpeed = 5f;

    [Tooltip("Enable smooth transition")]
    [SerializeField] private bool useSmoothTransition = true;

    private float currentDisplayedHP = 1f;
    private float targetHP = 1f;

    private void Start()
    {
        //LMJ: Auto-find player if not assigned
        if (playerStats == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerStats = player.GetComponent<CharacterStats>();
            }
        }

        //LMJ: Validate references
        if (playerStats == null)
        {
            Debug.LogError("PlayerHealthUI: Player CharacterStats not found!");
            enabled = false;
            return;
        }

        if (hpFlowController == null)
        {
            Debug.LogError("PlayerHealthUI: HPFlowController not assigned!");
            enabled = false;
            return;
        }

        //LMJ: Initialize HP bar to full (delayed to ensure material is loaded)
        currentDisplayedHP = 1f;
        targetHP = 1f;
        Invoke(nameof(InitializeHPBar), 0.1f);
    }

    private void InitializeHPBar()
    {
        if (hpFlowController != null)
        {
            hpFlowController.SetValue(1f);
        }
    }

    private void Update()
    {
        if (!updateEveryFrame)
            return;

        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (playerStats == null || hpFlowController == null)
            return;

        //LMJ: Calculate HP percentage (0.0 to 1.0)
        float maxHP = GetMaxHealth();
        float currentHP = GetCurrentHealth();

        if (maxHP <= 0)
        {
            targetHP = 0f;
        }
        else
        {
            targetHP = Mathf.Clamp01(currentHP / maxHP);
        }

        //LMJ: Smooth transition or instant update
        if (useSmoothTransition)
        {
            currentDisplayedHP = Mathf.Lerp(currentDisplayedHP, targetHP, Time.deltaTime * smoothSpeed);
        }
        else
        {
            currentDisplayedHP = targetHP;
        }

        //LMJ: Update HP bar visual
        hpFlowController.SetValue(currentDisplayedHP);
    }

    //LMJ: Public method to force immediate update
    public void ForceUpdate()
    {
        UpdateHealthBar();
    }

    //LMJ: Public method to set HP instantly (no smooth transition)
    public void SetHealthInstant(float hpPercentage)
    {
        currentDisplayedHP = Mathf.Clamp01(hpPercentage);
        targetHP = currentDisplayedHP;

        if (hpFlowController != null)
        {
            hpFlowController.SetValue(currentDisplayedHP);
        }
    }

    //LMJ: Get max health
    private float GetMaxHealth()
    {
        return playerStats.MaxHealth;
    }

    //LMJ: Get current health
    private float GetCurrentHealth()
    {
        return playerStats.CurrentHealth;
    }
}
