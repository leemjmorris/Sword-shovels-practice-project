using UnityEngine;
using UnityEngine.UI;

public class HeroViewController : MonoBehaviour
{
    [Header("References")]
    public GameObject heroModelPrefab;
    public Transform viewportTransform;
    public Camera previewCamera;  // 미리보기 카메라
    public RawImage displayImage;  // 렌더 텍스처를 표시할 RawImage

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
            // 카메라가 없으면 새로 생성
            GameObject cameraObj = new GameObject("Preview Camera");
            cameraObj.transform.parent = transform;
            previewCamera = cameraObj.AddComponent<Camera>();
        }

        // 렌더 텍스처 생성 및 설정
        renderTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
        previewCamera.targetTexture = renderTexture;
        
        // 카메라 설정
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = Color.clear;
        previewCamera.cullingMask = 1 << LayerMask.NameToLayer("UI"); // UI 레이어만 렌더링
        
        if (displayImage != null)
        {
            displayImage.texture = renderTexture;
        }

        // 카메라 위치 설정
        previewCamera.transform.position = viewportTransform.position + new Vector3(0, 1, -2);
        previewCamera.transform.LookAt(viewportTransform.position + Vector3.up);
    }
    
    void SetupLighting()
    {
        // 메인 라이트가 없다면 생성
        if (mainLight == null)
        {
            GameObject lightObj = new GameObject("HeroView Light");
            lightObj.transform.parent = transform;
            mainLight = lightObj.AddComponent<Light>();
        }

        // 라이트 설정
        mainLight.type = LightType.Directional;
        mainLight.intensity = lightIntensity;
        mainLight.color = lightColor;
        mainLight.renderMode = LightRenderMode.ForcePixel; // 픽셀 라이팅 강제
        
        // 캐릭터를 잘 비추도록 각도 조정
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

            // 히어로 모델 생성
            currentHeroModel = Instantiate(heroModelPrefab, 
                                        viewportTransform.position, 
                                        Quaternion.identity, 
                                        viewportTransform);
            
            // 애니메이터 설정
            Animator animator = currentHeroModel.GetComponent<Animator>();
            if (animator != null)
            {
                // 기본 Idle 애니메이션 재생
                animator.Play("Idle");
            }
        }
        else
        {
            Debug.LogError("Hero Model Prefab이 설정되지 않았습니다!");
        }
    }
    
    void Update()
    {
        // 캐릭터 자동 회전
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