using UnityEngine;
using System.Collections.Generic;
public class RayTracingMaster : MonoBehaviour
{
    public ComputeShader RayTracingShader;
    public Texture SkyboxTexture;
    public Light DirectionalLight;
    private RenderTexture _target;
    private Camera _camera;
    private uint _currentSample = 0;
    private Material _addMaterial;
    private ComputeBuffer _sphereBuffer;
    public struct Sphere
    {
        public Vector3 position;
        public float radius;
        public Vector3 albedo;
        public Vector3 specular;
    };

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (transform.hasChanged)
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
        RayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
        RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);
        //RayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));
        RayTracingShader.SetVector("_PixelOffset", new Vector2(0.0f, 0.0f));
        Vector3 l = DirectionalLight.transform.forward;
        RayTracingShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));
        convertAllSpheres();
    }
    private void Render(RenderTexture destination)
    {
        // Make sure we have a current render target
        InitRenderTexture();
        convertAllSpheres();
        // Set the target and dispatch the compute shader
        RayTracingShader.SetTexture(0, "Result", _target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        RayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        // Blit the result texture to the screen
        // if (_addMaterial == null)
        //     _addMaterial = new Material(Shader.Find("Hidden/AddShader"));
        // _addMaterial.SetFloat("_Sample", _currentSample);
        // Graphics.Blit(_target, destination, _addMaterial);
        // _currentSample++;
        Graphics.Blit(_target, destination);
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
    }

    private void convertAllSpheres()
    {
        List<Sphere> spheresConverted = new List<Sphere>();
        GameObject[] spheres = GameObject.FindGameObjectsWithTag("RTsphere");

        foreach (GameObject obj in spheres)
        {
            spheresConverted.Add(toSphere(obj));
        }
        _sphereBuffer?.Dispose();
        _sphereBuffer = new ComputeBuffer(spheres.Length, 40); //todo change 40 to sizeOf
        _sphereBuffer.SetData(spheresConverted);
        RayTracingShader.SetBuffer(0, "_Spheres", _sphereBuffer);
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
        sphere.albedo = new Vector3(0.5f, 0.5f, 0.5f);
        //sphere.specular = new Vector3(Random.value, Random.value, Random.value);
        sphere.specular = new Vector3(1.0f, 0.78f, 0.34f);
        return sphere;
    }
    private void OnApplicationQuit()
    {
        _sphereBuffer?.Dispose();
    }
}
