using UnityEngine;
using UnityEngine.UI;
using TMPro;

//LMJ: Skill cooldown UI display that shows cooldown progress with overlay and color
public class CoolDown : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Skill data to track cooldown for")]
    [SerializeField] private SkillData skillData;

    [Tooltip("The skill icon image (optional, will be darkened during cooldown)")]
    [SerializeField] private Image iconImage;

    [Tooltip("The cooldown overlay image (fills from bottom to top as cooldown recovers)")]
    [SerializeField] private Image cooldownOverlay;

    [Header("Visual Settings")]
    [Tooltip("Color tint when skill is on cooldown")]
    [SerializeField] private Color cooldownColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    [Tooltip("Color tint when skill is ready")]
    [SerializeField] private Color readyColor = Color.white;

    [Tooltip("Show remaining cooldown time as text")]
    [SerializeField] private bool showCooldownText = true;

    [Tooltip("TextMeshPro component to display cooldown time (optional)")]
    [SerializeField] private TextMeshProUGUI cooldownText;

    [Header("Animation")]
    [Tooltip("Smooth color transition speed")]
    [SerializeField] private float colorTransitionSpeed = 5f;

    private int slotIndex = -1;
    private Color currentIconColor;
    private bool isInitialized = false;

    private void Start()
    {
        //LMJ: Delay initialization slightly to ensure SkillManager is ready
        Invoke(nameof(Initialize), 0.1f);
    }

    private void Initialize()
    {
        if (isInitialized)
            return;

        //LMJ: Validate references
        if (skillData == null)
        {
            Debug.LogWarning($"CoolDown on {gameObject.name}: No SkillData assigned!");
            return;
        }

        //LMJ: Find slot index in SkillManager
        if (SkillManager.Instance != null)
        {
            slotIndex = FindSkillSlotIndex();
            if (slotIndex == -1)
            {
                Debug.LogWarning($"CoolDown: SkillData '{skillData.skillName}' not found in SkillManager slots!");
                Debug.LogWarning($"Make sure this SkillData is added to SkillManager's Skill Slots!");
            }
            else
            {
                Debug.Log($"CoolDown: Successfully linked to slot {slotIndex} for skill '{skillData.skillName}'");
            }
        }
        else
        {
            Debug.LogError("CoolDown: SkillManager instance not found! Make sure SkillManager exists in the scene.");
            return;
        }

        //LMJ: Setup overlay if assigned
        if (cooldownOverlay != null)
        {
            cooldownOverlay.type = Image.Type.Filled;
            cooldownOverlay.fillMethod = Image.FillMethod.Vertical;
            cooldownOverlay.fillOrigin = (int)Image.OriginVertical.Bottom;
            cooldownOverlay.fillAmount = 0f;
        }

        //LMJ: Initialize icon color
        if (iconImage != null)
        {
            currentIconColor = readyColor;
            iconImage.color = currentIconColor;
        }

        //LMJ: Hide cooldown text initially
        if (cooldownText != null && showCooldownText)
        {
            cooldownText.gameObject.SetActive(false);
        }

        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized || slotIndex == -1 || SkillManager.Instance == null)
            return;

        UpdateCooldownDisplay();
    }

    private void UpdateCooldownDisplay()
    {
        //LMJ: Get cooldown progress (0 = ready, 1 = just used)
        float cooldownProgress = SkillManager.Instance.GetCooldownProgress(slotIndex);
        bool isReady = SkillManager.Instance.IsSkillReady(slotIndex);

        //LMJ: Debug log (remove after testing)
        if (cooldownProgress > 0f)
        {
            Debug.Log($"CoolDown Update: Slot {slotIndex}, Progress {cooldownProgress:F2}, Ready {isReady}");
        }

        //LMJ: Update overlay fill (inverted: 0 = full overlay, 1 = no overlay)
        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = cooldownProgress;
        }

        //LMJ: Update icon color
        if (iconImage != null)
        {
            Color targetColor = isReady ? readyColor : cooldownColor;
            currentIconColor = Color.Lerp(currentIconColor, targetColor, Time.deltaTime * colorTransitionSpeed);
            iconImage.color = currentIconColor;
        }

        //LMJ: Update cooldown text
        if (showCooldownText && cooldownText != null)
        {
            if (isReady)
            {
                cooldownText.gameObject.SetActive(false);
            }
            else
            {
                cooldownText.gameObject.SetActive(true);
                float remainingTime = cooldownProgress * skillData.cooldownTime;
                cooldownText.text = remainingTime.ToString("F1");
            }
        }
    }

    private int FindSkillSlotIndex()
    {
        if (SkillManager.Instance == null || skillData == null)
            return -1;

        return SkillManager.Instance.GetSlotIndex(skillData);
    }

    //LMJ: Public method to manually set slot index
    public void SetSlotIndex(int index)
    {
        slotIndex = index;
        isInitialized = true;
    }

    //LMJ: Public method to set skill data dynamically
    public void SetSkillData(SkillData data)
    {
        skillData = data;
        isInitialized = false;
        Initialize();
    }

    private void OnValidate()
    {
        //LMJ: Only validate references in editor, don't initialize during edit mode
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            //LMJ: Just validate that references are set
            if (skillData != null && cooldownOverlay != null)
            {
                // Setup overlay type in editor for preview
                cooldownOverlay.type = Image.Type.Filled;
                cooldownOverlay.fillMethod = Image.FillMethod.Vertical;
                cooldownOverlay.fillOrigin = (int)Image.OriginVertical.Bottom;
            }
        }
        #endif
    }
}
