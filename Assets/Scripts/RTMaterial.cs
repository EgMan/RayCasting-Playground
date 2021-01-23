using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    // public struct Sphere
    // {
    //     public float radius;
    //     public float smoothness;
    //     public float opacity;
    //     public float refractiveSmoothness;
    //     public float refractionIndex;
    //     public Vector3 position;
    //     public Vector3 albedo;
    //     public Vector3 specular;
    //     public Vector3 refractionTint;
    //     public Vector3 emission;
    // };
public class RTMaterial : MonoBehaviour
{
    public float smoothness;
    public float opacity;
    public float refractiveSmoothness;
    public float refractionIndex;
    public Color albedo;
    public Color specular;
    public Color refractionTint;
    public Color emission = Color.black;
}
