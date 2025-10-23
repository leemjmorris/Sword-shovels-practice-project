using UnityEngine;

//LMJ: ScriptableObject that defines skill properties
[CreateAssetMenu(fileName = "New Skill", menuName = "Skills/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Basic Info")]
    public string skillName;
    public Sprite skillIcon;
    [TextArea(3, 5)]
    public string description;

    [Header("Skill Prefab")]
    public GameObject skillPrefab;

    [Header("Execution Settings")]
    public bool isAsync = false;
    [Tooltip("If true, can cast while moving/doing other actions")]
    public bool canCastWhileMoving = true;

    [Header("Cooldown")]
    public float cooldownTime = 1f;

    [Header("Object Pooling")]
    public int poolSize = 5;

    [Header("Spawn Settings")]
    [Tooltip("Which spawn point on the player to use")]
    public string spawnPointName = "SkillSpawnPoint";

    [Header("Audio")]
    [Tooltip("Sound effect to play when skill is cast")]
    public AudioClip skillSound;
    [Tooltip("If true, stops previous instance of this sound when cast again")]
    public bool stopPreviousSound = true;
}
