using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; 

public enum LightMode
{
    DirectionalFollowCamera = 0, // Day
    DirectionalFixed = 1,        // Night
    PointLight = 2               // Spotlight
}

public class LightControl : MonoBehaviour
{
    [Header("Targets")]
    public Transform LightPosition;    
    public Transform MainCamera;
    public Transform DirectionalLight;
    public Transform CueBall;          
    
    [Header("Ground Recognition")]
    [Tooltip("Drag your Table object here.")]
    public Transform GroundObject; 
    public float MinHeightAboveGround = 2.0f;

    [Header("Follow Settings")]
    public bool FollowCueBall = true;  
    public Vector3 FollowOffset = new Vector3(0, 3, 0); 

    [Header("Light Settings")]
    public Color BaseColor = Color.white; 
    public float LightNear = 0.1f;         
    public float LightFar = 50.0f; 
    
    [Header("Spotlight Size")]
    public float SpotlightRadius = 0.8f; 
    
    // --- RESTORED VARIABLE ---
    private float DayMaxTheta = 3.0f;    
    // -------------------------

    [Header("Debug")]
    public bool InvertSpotlightMath = false;

    [Header("UI References")]
    public SliderWithEcho XSlider, YSlider, ZSlider, IntensitySlider; 
    public TextMeshProUGUI LightName;
    public TMP_Dropdown ModeDropdown; 

    [Header("Internal")]
    [SerializeField] private LightMode currentLightMode = LightMode.DirectionalFixed;
    [SerializeField] private float currentIntensity = 1.0f;

    // Cache the renderer to control Emission to control the intensity
    private Renderer bulbRenderer;
    private Light realLightComponent;

    void Awake()
    {
        if (XSlider != null) initSliderVisuals();
        
        if (IntensitySlider != null) IntensitySlider.SetSliderValue(currentIntensity);

        if (ModeDropdown != null)
        {
            ModeDropdown.onValueChanged.RemoveAllListeners();
            ModeDropdown.onValueChanged.AddListener(SetLightMode);
            ModeDropdown.SetValueWithoutNotify((int)currentLightMode);
        }

        // --- AUTO-DETECT VISUAL COMPONENTS ---
        if (LightPosition != null)
        {
            bulbRenderer = LightPosition.GetComponent<Renderer>();
            realLightComponent = LightPosition.GetComponent<Light>();
        }
    }

    void Update()
    {
        // 1. Calculate Color
        Color finalColor = BaseColor * currentIntensity;

        // 2. Send to Table Shader
        Shader.SetGlobalColor("LightColor", finalColor);
        Shader.SetGlobalFloat("LightNear", LightNear);
        Shader.SetGlobalFloat("LightFar", LightFar);

        // --- 3. HOOK DIRECTLY INTO EMISSION & REAL LIGHT ---
        
        // A. Make the Bulb Object Glow (Material Emission)
        if (bulbRenderer != null)
        {
            if (currentLightMode == LightMode.PointLight)
            {
                bulbRenderer.enabled = true;
                // Enable the keyword to make sure Emission is ON
                bulbRenderer.material.EnableKeyword("_EMISSION");
                bulbRenderer.material.SetColor("_EmissionColor", finalColor);
            }
            else
            {
                bulbRenderer.enabled = false;
            }
        }

        // B. Drive Real Unity Light (if one exists)
        if (realLightComponent != null)
        {
            if (currentLightMode == LightMode.PointLight)
            {
                realLightComponent.enabled = true;
                realLightComponent.color = BaseColor;
                realLightComponent.intensity = currentIntensity;
            }
            else
            {
                realLightComponent.enabled = false;
            }
        }
        // --------------------------------------------------

        // 4. Input T Key
        if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            int nextMode = ((int)currentLightMode + 1) % 3;
            SetLightMode(nextMode); 
        }

        // 5. Logic
        switch (currentLightMode)
        {
            case LightMode.DirectionalFollowCamera: dirLightFollowCamera(); break;
            case LightMode.DirectionalFixed: dirLightFixed(); break;
            case LightMode.PointLight: pointLightOn(); break;
        }
    }

    // --- PUBLIC EVENTS ---
    public void SetLightMode(int modeIndex)
    {
        currentLightMode = (LightMode)modeIndex;
        if (ModeDropdown != null && ModeDropdown.value != modeIndex) ModeDropdown.SetValueWithoutNotify(modeIndex);
    }
    public void IntensityValueChanged(float newValue) { currentIntensity = newValue; }
    public void XValueChanged(float newValue) { if (FollowCueBall && currentLightMode == LightMode.PointLight) FollowOffset.x = newValue; else LightPosition.localPosition = new Vector3(newValue, LightPosition.localPosition.y, LightPosition.localPosition.z); }
    public void ZValueChanged(float newValue) { if (FollowCueBall && currentLightMode == LightMode.PointLight) FollowOffset.z = newValue; else LightPosition.localPosition = new Vector3(LightPosition.localPosition.x, LightPosition.localPosition.y, newValue); }

    public void YValueChanged(float newValue)
    {
        float limitY = 0.0f;
        if (GroundObject != null)
        {
            Collider col = GroundObject.GetComponent<Collider>();
            if (col != null) limitY = col.bounds.max.y;
            else limitY = GroundObject.position.y;
        }

        float absoluteMinHeight = limitY + MinHeightAboveGround;

        if (FollowCueBall && currentLightMode == LightMode.PointLight)
        {
            float safeOffset = Mathf.Max(newValue, MinHeightAboveGround);
            FollowOffset.y = safeOffset;
        }
        else
        {
            float safeHeight = Mathf.Max(newValue, absoluteMinHeight);
            LightPosition.localPosition = new Vector3(LightPosition.localPosition.x, safeHeight, LightPosition.localPosition.z);
        }
    }

    // --- MODES ---
    private void dirLightFollowCamera() {
        if(XSlider != null) slidersOff();
        DirectionalLight.gameObject.SetActive(true);
        // Hide the mesh using bulbRenderer.enabled in Update now
        LightPosition.gameObject.SetActive(false);
        if(LightName) LightName.text = "Day";

        DirectionalLight.rotation = Quaternion.LookRotation(MainCamera.forward, Vector3.up);
        DirectionalLight.position = MainCamera.position;
        Shader.SetGlobalFloat("_MaxTheta", DayMaxTheta); Shader.SetGlobalFloat("_MinTheta", 0.0f);
        Shader.SetGlobalVector("LightDirection", -DirectionalLight.forward);
        Shader.SetGlobalVector("SlightPos", DirectionalLight.position + (-DirectionalLight.forward * 100));
    }

    private void dirLightFixed() {
        if(XSlider != null) slidersOff(); 
        DirectionalLight.gameObject.SetActive(true);
        LightPosition.gameObject.SetActive(false);
        if(LightName) LightName.text = "Night";
        
        DirectionalLight.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        Shader.SetGlobalFloat("_MaxTheta", DayMaxTheta); Shader.SetGlobalFloat("_MinTheta", 0.0f);
        Shader.SetGlobalVector("LightDirection", -DirectionalLight.forward);
        Shader.SetGlobalVector("SlightPos", DirectionalLight.position + (-DirectionalLight.forward * 100));
    }

    private void pointLightOn() {
        if(XSlider != null) slidersOn();
        DirectionalLight.gameObject.SetActive(false);
        LightPosition.gameObject.SetActive(true);
        if(LightName) LightName.text = "Spotlight";

        if (FollowCueBall && CueBall != null) {
            LightPosition.position = CueBall.position + FollowOffset;
            LightPosition.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
        }

        Shader.SetGlobalFloat("_MaxTheta", SpotlightRadius); 
        Shader.SetGlobalFloat("_MinTheta", 0.0f);
        Shader.SetGlobalVector("SlightPos", LightPosition.position); 
        
        Vector3 dir = InvertSpotlightMath ? Vector3.down : Vector3.up;
        Shader.SetGlobalVector("LightDirection", dir);
    }

    void OnDrawGizmos()
    {
        if (currentLightMode == LightMode.PointLight)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(LightPosition.position, 0.2f);

            float limitY = 0.0f;
            if (GroundObject != null)
            {
                Collider col = GroundObject.GetComponent<Collider>();
                if (col != null) limitY = col.bounds.max.y;
                else limitY = GroundObject.position.y;
            }
            limitY += MinHeightAboveGround;
            Gizmos.color = Color.red;
            Vector3 center = new Vector3(LightPosition.position.x, limitY, LightPosition.position.z);
            Gizmos.DrawWireCube(center, new Vector3(2, 0.05f, 2));
            Gizmos.DrawLine(LightPosition.position, center);
        }
    }

    private void initSliderVisuals() { 
        if (FollowCueBall) { 
            XSlider.SetSliderValue(FollowOffset.x); 
            YSlider.SetSliderValue(FollowOffset.y); 
            ZSlider.SetSliderValue(FollowOffset.z); 
        } else { 
            XSlider.SetSliderValue(LightPosition.localPosition.x); 
            YSlider.SetSliderValue(LightPosition.localPosition.y); 
            ZSlider.SetSliderValue(LightPosition.localPosition.z); 
        } 
    }
    private void slidersOff() { XSlider.DisableSlider(); YSlider.DisableSlider(); ZSlider.DisableSlider(); }
    private void slidersOn()  { XSlider.EnableSlider(); YSlider.EnableSlider(); ZSlider.EnableSlider(); }
}