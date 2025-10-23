using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

//LMJ: Manages all player skills, key bindings, cooldowns, and object pooling
public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    [Header("Player Reference")]
    [SerializeField] private GameObject hero;

    [Header("Skill Slots")]
    [SerializeField] private List<SkillSlot> skillSlots = new List<SkillSlot>();

    //LMJ: Dictionary for quick key lookup
    private Dictionary<KeyCode, int> keyToSlotIndex = new Dictionary<KeyCode, int>();

    //LMJ: Object pools for each skill
    private Dictionary<int, Queue<GameObject>> skillPools = new Dictionary<int, Queue<GameObject>>();

    //LMJ: Cooldown tracking
    private Dictionary<int, float> skillCooldowns = new Dictionary<int, float>();

    //LMJ: Track if a synchronous skill is being cast
    private bool isCastingSyncSkill = false;

    //LMJ: Track currently playing audio sources for each skill slot
    private Dictionary<int, AudioSource> playingAudioSources = new Dictionary<int, AudioSource>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeSkillSystem();
    }

    private void Update()
    {
        //LMJ: Check for skill key inputs
        foreach (var kvp in keyToSlotIndex)
        {
            if (Input.GetKeyDown(kvp.Key))
            {
                Debug.Log($"Key pressed: {kvp.Key}, Slot: {kvp.Value}");
                int slotIndex = kvp.Value;
                TryCastSkill(slotIndex).Forget();
            }
        }

        //LMJ: Update cooldowns
        UpdateCooldowns();
    }

    private void InitializeSkillSystem()
    {
        Debug.Log($"Initializing skill system with {skillSlots.Count} slots");

        //LMJ: Build key binding dictionary
        for (int i = 0; i < skillSlots.Count; i++)
        {
            SkillSlot slot = skillSlots[i];

            if (slot.skillData != null)
            {
                //LMJ: Register key binding
                if (!keyToSlotIndex.ContainsKey(slot.keyBinding))
                {
                    keyToSlotIndex.Add(slot.keyBinding, i);
                    Debug.Log($"Registered: {slot.keyBinding} -> Slot {i} ({slot.skillData.skillName})");
                }

                //LMJ: Initialize object pool
                InitializeSkillPool(i, slot.skillData);

                //LMJ: Initialize cooldown
                skillCooldowns[i] = 0f;
            }
            else
            {
                Debug.LogWarning($"Slot {i} has no skill data assigned!");
            }
        }

        Debug.Log($"Total registered key bindings: {keyToSlotIndex.Count}");
    }

    private void InitializeSkillPool(int slotIndex, SkillData skillData)
    {
        if (skillData.skillPrefab == null)
            return;

        Queue<GameObject> pool = new Queue<GameObject>();

        for (int i = 0; i < skillData.poolSize; i++)
        {
            GameObject skillObj = Instantiate(skillData.skillPrefab, transform);
            skillObj.name = $"{skillData.skillName}_Pool_{i}";
            skillObj.SetActive(false);
            pool.Enqueue(skillObj);
        }

        skillPools[slotIndex] = pool;
    }

    private async UniTaskVoid TryCastSkill(int slotIndex)
    {
        Debug.Log($"TryCastSkill called for slot {slotIndex}");

        //LMJ: Validate slot
        if (slotIndex < 0 || slotIndex >= skillSlots.Count)
        {
            Debug.LogWarning($"Invalid slot index: {slotIndex}");
            return;
        }

        SkillSlot slot = skillSlots[slotIndex];
        if (slot.skillData == null)
        {
            Debug.LogWarning($"Slot {slotIndex} has no skill data!");
            return;
        }

        //LMJ: Check if on cooldown
        if (skillCooldowns[slotIndex] > 0f)
        {
            Debug.Log($"Skill {slot.skillData.skillName} on cooldown: {skillCooldowns[slotIndex]:F2}s");
            return;
        }

        //LMJ: Check if sync skill is being cast
        if (isCastingSyncSkill && !slot.skillData.isAsync)
        {
            Debug.Log($"Cannot cast sync skill while another sync skill is active");
            return;
        }

        //LMJ: Get spawn point
        Transform spawnPoint = GetSpawnPoint(slot.skillData.spawnPointName);
        if (spawnPoint == null)
        {
            Debug.LogWarning($"Spawn point not found: {slot.skillData.spawnPointName}");
            return;
        }
        Debug.Log($"Spawn point found: {spawnPoint.name} at {spawnPoint.position}");

        //LMJ: Get skill from pool
        GameObject skillObj = GetSkillFromPool(slotIndex);
        if (skillObj == null)
        {
            Debug.LogWarning($"No skill available in pool for slot {slotIndex}");
            return;
        }
        Debug.Log($"Got skill from pool: {skillObj.name}");

        //LMJ: Get Skill component
        Skill skill = skillObj.GetComponent<Skill>();
        if (skill == null)
        {
            Debug.LogError($"Skill prefab missing Skill component!");
            return;
        }

        //LMJ: Initialize skill at spawn point
        Debug.Log($"Casting {slot.skillData.skillName} at {spawnPoint.position}");
        skill.Initialize(spawnPoint.position, spawnPoint.rotation);

        //LMJ: Play skill sound
        PlaySkillSound(slotIndex, slot.skillData);

        //LMJ: Start cooldown
        skillCooldowns[slotIndex] = slot.skillData.cooldownTime;

        //LMJ: Execute skill
        if (slot.skillData.isAsync)
        {
            //LMJ: Async: Fire and forget
            skill.ExecuteSkill().Forget();
        }
        else
        {
            //LMJ: Sync: Wait for completion
            isCastingSyncSkill = true;
            await skill.ExecuteSkill();
            isCastingSyncSkill = false;
        }
    }

    private GameObject GetSkillFromPool(int slotIndex)
    {
        if (!skillPools.ContainsKey(slotIndex))
            return null;

        Queue<GameObject> pool = skillPools[slotIndex];

        if (pool.Count == 0)
            return null;

        GameObject skillObj = pool.Dequeue();

        //LMJ: Return to pool after use
        ReturnSkillToPoolDelayed(slotIndex, skillObj).Forget();

        return skillObj;
    }

    private async UniTaskVoid ReturnSkillToPoolDelayed(int slotIndex, GameObject skillObj)
    {
        //LMJ: Wait for skill to deactivate
        await UniTask.WaitUntil(() => !skillObj.activeInHierarchy);

        //LMJ: Return to pool
        if (skillPools.ContainsKey(slotIndex))
        {
            skillPools[slotIndex].Enqueue(skillObj);
        }
    }

    private Transform GetSpawnPoint(string spawnPointName)
    {
        if (hero == null)
            return null;

        //LMJ: Find spawn point by name
        Transform spawnPoint = hero.transform.Find(spawnPointName);

        if (spawnPoint == null)
        {
            //LMJ: Fallback to hero position
            return hero.transform;
        }

        return spawnPoint;
    }

    private void UpdateCooldowns()
    {
        //LMJ: Decrease all active cooldowns
        List<int> keys = new List<int>(skillCooldowns.Keys);

        foreach (int key in keys)
        {
            if (skillCooldowns[key] > 0f)
            {
                skillCooldowns[key] -= Time.deltaTime;

                if (skillCooldowns[key] < 0f)
                {
                    skillCooldowns[key] = 0f;
                }
            }
        }
    }

    //LMJ: Public method to get cooldown progress (for UI)
    public float GetCooldownProgress(int slotIndex)
    {
        if (!skillCooldowns.ContainsKey(slotIndex))
            return 0f;

        if (slotIndex < 0 || slotIndex >= skillSlots.Count)
            return 0f;

        float currentCooldown = skillCooldowns[slotIndex];
        float maxCooldown = skillSlots[slotIndex].skillData.cooldownTime;

        if (maxCooldown <= 0f)
            return 0f;

        return currentCooldown / maxCooldown;
    }

    //LMJ: Public method to check if skill is ready
    public bool IsSkillReady(int slotIndex)
    {
        if (!skillCooldowns.ContainsKey(slotIndex))
            return false;

        return skillCooldowns[slotIndex] <= 0f;
    }

    private void PlaySkillSound(int slotIndex, SkillData skillData)
    {
        if (skillData.skillSound == null)
            return;

        //LMJ: Stop previous sound if enabled
        if (skillData.stopPreviousSound && playingAudioSources.ContainsKey(slotIndex))
        {
            AudioSource prevSource = playingAudioSources[slotIndex];
            if (prevSource != null)
            {
                Destroy(prevSource.gameObject);
            }
            playingAudioSources.Remove(slotIndex);
        }

        //LMJ: Create new audio source
        GameObject audioObj = new GameObject($"{skillData.skillName}_Audio");
        audioObj.transform.position = hero != null ? hero.transform.position : transform.position;
        audioObj.transform.SetParent(transform);

        AudioSource audioSource = audioObj.AddComponent<AudioSource>();
        audioSource.clip = skillData.skillSound;
        audioSource.spatialBlend = 0f; //LMJ: 2D sound
        audioSource.Play();

        //LMJ: Track this audio source
        playingAudioSources[slotIndex] = audioSource;

        //LMJ: Destroy after sound finishes
        CleanupAudioSource(slotIndex, audioObj, skillData.skillSound.length).Forget();
    }

    private async UniTaskVoid CleanupAudioSource(int slotIndex, GameObject audioObj, float duration)
    {
        //LMJ: Wait for sound to finish
        await UniTask.Delay((int)(duration * 1000));

        //LMJ: Cleanup
        if (audioObj != null)
        {
            Destroy(audioObj);
        }

        if (playingAudioSources.ContainsKey(slotIndex))
        {
            playingAudioSources.Remove(slotIndex);
        }
    }
}

//LMJ: Skill slot data structure
[System.Serializable]
public class SkillSlot
{
    public SkillData skillData;
    public KeyCode keyBinding = KeyCode.Q;
}
