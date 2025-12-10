using System;
using System.Collections.Generic;
using System.Linq;
using SplineMesh;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Spline))]
public class WaterProjectile : MonoBehaviour
{
    public Spline spline;
    private float rate = 0;
    private readonly float startPosition = 0;
    private MeshBender meshBender;

    [HideInInspector]
    public GameObject generated;
    private bool initialized = false;

    public bool ShouldRun
    {
        get => shouldRun;
        set
        {
            if (value != shouldRun)
            {
                generated.SetActive(value);
            }
            shouldRun = value;
        }
    }
    
    public bool shouldRun = false;
    public bool shouldRepeat = false;
    public Action<WaterProjectile> OnFinished;
    
    public Mesh mesh;
    public Material material;
    public Vector3 rotation;
    public Vector3 scale;
    private float scaleX = 0; // used to start with scale 0

    public float usedSpeed;
    private float speed;
    [HideInInspector]
    public bool shouldMoveAlongSpline;

    private void OnEnable() {
        rate = 0;
#if UNITY_EDITOR
        if (initialized) return;
        Init();
        EditorApplication.update += CustomUpdate;
#endif
    }

    void OnDisable() {
#if UNITY_EDITOR
        scaleX = 0;
        shouldMoveAlongSpline = false;
        if (initialized) return;
        initialized = false;
        EditorApplication.update -= CustomUpdate;
#endif
    }
    
    private void OnValidate()
    {
        Init();
    }

    public void Init()
    {
        if (mesh ==null)
            return;
        
        var generatedName = $"{GetType().Name}: {gameObject.name}";
        var generatedTransform = transform.Find(generatedName);
        if (generatedTransform != null)
            generated = generatedTransform.gameObject;
        else
        {
            generated = new GameObject(generatedName, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshBender));
            generated.transform.SetParent(transform);
            generated.transform.localPosition = Vector3.zero;
            generated.transform.localRotation = Quaternion.identity;
            generated.transform.localScale = Vector3.one;
        }
        
        generated.GetComponent<MeshRenderer>().material = material;
        
        meshBender = generated.GetComponent<MeshBender>();
        spline = GetComponent<Spline>();
        meshBender.Mode = MeshBender.FillingMode.Once;
        meshBender.SetInterval(spline, 0);

        var percentage = 1f / spline.Length * GetMeshLength(mesh);
        speed = usedSpeed * (1 + percentage);
        
        meshBender.Source = SourceMesh.Build(mesh)
            .Rotate(Quaternion.Euler(rotation))
            .Scale(new Vector3(0, scale.y, scale.z));

        initialized = true;
    }

    // Based on SourceMesh 's buildData function
    private float GetMeshLength(Mesh mesh1)
    {
        // if the mesh is reversed by scale, we must change the culling of the faces by inverting all triangles.
        // the mesh is reverse only if the number of reversing axes is impair.
        bool reversed = scale.x < 0;
        if (scale.y < 0) reversed = !reversed;
        if (scale.z < 0) reversed = !reversed;

        // we transform the source mesh vertices according to rotation/translation/scale
        var i = 0;
        var vertices = new List<MeshVertex>(mesh1.vertexCount);
        var rot = Quaternion.Euler(rotation);
        foreach (var vert in mesh1.vertices) {
            var transformed = new MeshVertex(vert, mesh1.normals[i++]);
            //  application of rotation
            if (rot != Quaternion.identity) {
                transformed.position = rot * transformed.position;
                transformed.normal = rot * transformed.normal;
            }
            if (scale != Vector3.one) {
                transformed.position = Vector3.Scale(transformed.position, scale);
                transformed.normal = Vector3.Scale(transformed.normal, scale);
            }
            vertices.Add(transformed);
        }

        // find the bounds along x
        var minX = float.MaxValue;
        var maxX = float.MinValue;
        foreach (var p in vertices.Select(vert => vert.position))
        {
            maxX = Math.Max(maxX, p.x);
            minX = Math.Min(minX, p.x);
        }
        return Math.Abs(maxX - minX);
    }

    private void Update()
    {
        CustomUpdate();
    }

    public void RefreshCurves()
    {
        spline.RefreshCurves();
    }

    private void CustomUpdate()
    {
        if (generated == null || !shouldRun) return;
        if (shouldMoveAlongSpline)
        {
            rate += usedSpeed * Time.deltaTime;
            if (rate >= 1)
            {
                ResetToStart();
            }
            meshBender.SetInterval(spline, spline.Length * rate);
        }
        else
        {
            scaleX += speed * Time.deltaTime;
            if (scaleX >= scale.x)
            {
                scaleX = scale.x;
                shouldMoveAlongSpline = true;
            }

            meshBender.Source = SourceMesh.Build(mesh)
                .Rotate(Quaternion.Euler(rotation))
                .Scale(new Vector3(scaleX, scale.y, scale.z));
        }

        if (shouldRun)
            meshBender.ComputeIfNeeded();
    }

    private void ResetToStart()
    {
        rate = startPosition;
        scaleX = 0;
        shouldMoveAlongSpline = false;
        meshBender.Source = SourceMesh.Build(mesh)
            .Rotate(Quaternion.Euler(rotation))
            .Scale(new Vector3(0, scale.y, scale.z));
        if (!shouldRepeat)
        {
            ShouldRun = false;
            OnFinished?.Invoke(this);
        }
    }
}