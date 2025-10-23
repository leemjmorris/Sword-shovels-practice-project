using UnityEngine;
using UnityEngine.UI;

public class HeroViewController : MonoBehaviour
{
    [Header("References")]
    public GameObject heroModelPrefab;
    public Transform viewportTransform;
    public Camera previewCamera;  // �̸����� ī�޶�
    public RawImage displayImage;  // ���� �ؽ�ó�� ǥ���� RawImage

    [Header("View Settings")]
    public float rotationSpeed = 30f;
    
    [Header("Lighting")]
    public Light mainLight;
    public float lightIntensity = 1.5f;
    public Color lightColor = Color.white;

    private RenderTexture renderTexture;
    
    private GameObject currentHeroModel;
    
    void Start()
    {
        SetupCamera();
        SetupLighting();
        SpawnHeroModel();
    }

    void SetupCamera()
    {
        if (previewCamera == null)
        {
            // ī�޶� ������ ���� ����
            GameObject cameraObj = new GameObject("Preview Camera");
            cameraObj.transform.parent = transform;
            previewCamera = cameraObj.AddComponent<Camera>();
        }

        // ���� �ؽ�ó ���� �� ����
        renderTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
        previewCamera.targetTexture = renderTexture;
        
        // ī�޶� ����
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = Color.clear;
        previewCamera.cullingMask = 1 << LayerMask.NameToLayer("UI"); // UI ���̾ ������
        
        if (displayImage != null)
        {
            displayImage.texture = renderTexture;
        }

        // ī�޶� ��ġ ����
        previewCamera.transform.position = viewportTransform.position + new Vector3(0, 1, -2);
        previewCamera.transform.LookAt(viewportTransform.position + Vector3.up);
    }
    
    void SetupLighting()
    {
        // ���� ����Ʈ�� ���ٸ� ����
        if (mainLight == null)
        {
            GameObject lightObj = new GameObject("HeroView Light");
            lightObj.transform.parent = transform;
            mainLight = lightObj.AddComponent<Light>();
        }

        // ����Ʈ ����
        mainLight.type = LightType.Directional;
        mainLight.intensity = lightIntensity;
        mainLight.color = lightColor;
        mainLight.renderMode = LightRenderMode.ForcePixel; // �ȼ� ������ ����
        
        // ĳ���͸� �� ���ߵ��� ���� ����
        mainLight.transform.rotation = Quaternion.Euler(30f, -45f, 0f);
    }
    
    void SpawnHeroModel()
    {
        if (heroModelPrefab != null)
        {
            if (currentHeroModel != null)
            {
                Destroy(currentHeroModel);
            }

            // ����� �� ����
            currentHeroModel = Instantiate(heroModelPrefab, 
                                        viewportTransform.position, 
                                        Quaternion.identity, 
                                        viewportTransform);
            
            // �ִϸ����� ����
            Animator animator = currentHeroModel.GetComponent<Animator>();
            if (animator != null)
            {
                // �⺻ Idle �ִϸ��̼� ���
                animator.Play("Idle");
            }
        }
        else
        {
            Debug.LogError("Hero Model Prefab�� �������� �ʾҽ��ϴ�!");
        }
    }
    
    void Update()
    {
        // ĳ���� �ڵ� ȸ��
        if (currentHeroModel != null)
        {
            currentHeroModel.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
    
    public void ChangeHeroModel(GameObject newHeroModel)
    {
        heroModelPrefab = newHeroModel;
        SpawnHeroModel();
    }
    
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }
}