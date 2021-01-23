using UnityEngine;
using System.Collections.Generic;
public class RayTracer : MonoBehaviour
{
    public bool realTime = true;
    public ComputeShader RayTracingShader;
    public Texture SkyboxTexture;
    public Light DirectionalLight;
    private RenderTexture _target;
    private RenderTexture _converged;
    private Camera _camera;
    private uint _currentSample = 0;
    private Material _addMaterial;
    private ComputeBuffer _sphereBuffer;
    private bool lastRealTimeVal = true;
    public struct Sphere
    {
        public float radius;
        public float smoothness;
        public float opacity;
        public float refractiveSmoothness;
        public float refractionIndex;
        public Vector3 position;
        public Vector3 albedo;
        public Vector3 specular;
        public Vector3 refractionTint;
        public Vector3 emission;
    };

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Start()
    {
        if (!realTime)
        {
        convertScene();
        }
    }

    private void Update()
    {
        if (!realTime && transform.hasChanged)
        {
            _currentSample = 0;
            transform.hasChanged = false;
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SetShaderParameters();
        Render(destination);
    }

    private void SetShaderParameters()
    {
        Vector3 l = DirectionalLight.transform.forward;
        RayTracingShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));
        RayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
        RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);
        RayTracingShader.SetFloat("_Seed", Random.value);
        RayTracingShader.SetBool("_RealTime", realTime);
        if (realTime)
        {
            RayTracingShader.SetVector("_PixelOffset", new Vector2(0.0f, 0.0f));
        }
        else
        {
            RayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));
        }
    }

    private void Render(RenderTexture destination)
    {
        // Make sure we have a current render target
        InitRenderTexture();
        if (realTime)
        {
            convertScene();
        }
        // Set the target and dispatch the compute shader
        RayTracingShader.SetTexture(0, "Result", _target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        RayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        if (realTime)
        {
            Graphics.Blit(_target, destination);
        }
        else {
            // if this is the first static frame
            if (lastRealTimeVal)
            {
                _currentSample = 0;
                transform.hasChanged = false;
            }

            // Blit the result texture to the screen
            if (_addMaterial == null)
                _addMaterial = new Material(Shader.Find("Hidden/SummationShader"));
            _addMaterial.SetFloat("_Sample", _currentSample);
            Graphics.Blit(_target, _converged, _addMaterial);
            Graphics.Blit(_converged, destination);
            _currentSample++;
        }
        lastRealTimeVal = realTime;
    }
    private void InitRenderTexture()
    {
        if (_target == null || _target.width != Screen.width || _target.height != Screen.height)
        {
            // Release render texture if we already have one
            if (_target != null)
                _target.Release();
            // Get a render target for Ray Tracing
            _target = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.Create();
        }

        if (_converged == null || _converged.width != Screen.width || _converged.height != Screen.height)
        {
            // Release render texture if we already have one
            if (_converged != null)
                _converged.Release();
            // Get a render target for Ray Tracing
            _converged = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _converged.enableRandomWrite = true;
            _converged.Create();
        }
    }

    private void convertScene()
    {
        List<Sphere> spheresConverted = new List<Sphere>();
        GameObject[] spheres = GameObject.FindGameObjectsWithTag("RTsphere");

        foreach (GameObject obj in spheres)
        {
            spheresConverted.Add(toSphere(obj));
        }
        _sphereBuffer?.Dispose();
        int sizeOfSphere = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Sphere));
        Debug.Log($"sizeof: {sizeOfSphere}");
        _sphereBuffer = new ComputeBuffer(spheres.Length, sizeOfSphere);
        _sphereBuffer.SetData(spheresConverted);
        RayTracingShader.SetBuffer(0, "_Spheres", _sphereBuffer);
    }

    private Vector3 colToVect(Color col)
    {
        return new Vector3(col.r, col.g, col.b);
    }

    private Sphere toSphere(GameObject obj)
    {
        Sphere sphere = new Sphere();
        SphereCollider collider = obj.GetComponent<SphereCollider>();
        if (collider == null)
        {
            Debug.LogError($"object: {obj.name} has no associated sphere collider, and is probably not a sphere object.");
        }
        sphere.position = obj.transform.position; //todo handle local to global conversion
        sphere.radius = obj.transform.localScale.x * collider.radius;

        RTMaterial mat = obj.GetComponent<RTMaterial>();
        if (mat != null)
        {
            sphere.smoothness = mat.smoothness;
            sphere.opacity = mat.opacity;
            sphere.refractiveSmoothness = mat.refractiveSmoothness;
            sphere.refractionIndex = mat.refractionIndex;
            sphere.albedo = colToVect(mat.albedo);
            sphere.specular = colToVect(mat.specular);
            sphere.refractionTint = colToVect(mat.refractionTint);
            sphere.emission = colToVect(mat.emission);
        }
        else
        {
            sphere.smoothness = 1000000f;
            sphere.opacity = 0.25f;
            sphere.refractiveSmoothness = 100000f;
            sphere.refractionIndex = 1.5f;
            sphere.albedo = new Vector3(1.0f, 0.78f, 0.34f);
            sphere.specular = new Vector3(1.0f, 0.78f, 0.34f);
            // sphere.refractionTint = new Vector3(1.0f, 0.78f, 0.34f);
            sphere.refractionTint = new Vector3(0.30f, 0.30f, 0.90f);
            sphere.emission = new Vector3(0.01f, 0.01f, 0.01f);
        }
        return sphere;
    }
    private void OnApplicationQuit()
    {
        _sphereBuffer?.Dispose();
    }
}
