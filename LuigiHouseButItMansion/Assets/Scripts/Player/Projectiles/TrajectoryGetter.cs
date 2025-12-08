
using System;
using UnityEngine;

public class TrajectoryGetter
{
    public struct TrajectoryData
    {
        public Vector3 startPosition;
        public Vector3 highestPosition;
        public Vector3 endPosition;
    }

    public class TrajectoryCalculation
    {
        public GameObject instance;
        public Rigidbody rb;
        public TrajectoryData trajectory;
        public Action<TrajectoryData> onEnd;

        public void End()
        {
            onEnd?.Invoke(trajectory);
        }
    }

    private Mesh mesh;
    private Vector3 scale;
    private ObjectPool<TrajectoryCalculation> calculationPool = new();

    public TrajectoryGetter(Mesh mesh, Vector3 scale)
    {
        this.mesh = mesh;
        this.scale = scale;
    }

    public void GetTrajectory(Vector3 position, Quaternion rotation, Vector3 direction, float strength, Action<TrajectoryData> onEnd)
    {
        TrajectoryCalculation activeCalculation;
        if (calculationPool.GetInactiveObject(out var activeObject))
            activeCalculation = (TrajectoryCalculation)activeObject;
        else
            activeCalculation = new TrajectoryCalculation();
        
        activeCalculation.onEnd = onEnd;
        activeCalculation.trajectory.startPosition = position;
        
        CreateObject(position, rotation, activeCalculation);
        activeCalculation.rb.AddForce(strength * direction, ForceMode.Impulse);
    }

    private void CreateObject(Vector3 position, Quaternion rotation, TrajectoryCalculation activeCalculation)
    {
        TrajectoryNotator notator = null;
        if (activeCalculation.instance == null)
        {
            activeCalculation.instance = new GameObject($"trajectoryCalculation {typeof(TrajectoryGetter)}",
                typeof(Rigidbody), typeof(MeshCollider));
            
            notator = activeCalculation.instance.AddComponent<TrajectoryNotator>();
            
            var meshCollider = activeCalculation.instance.GetComponent<MeshCollider>();
            meshCollider.convex = true;
            meshCollider.sharedMesh = mesh;
            
            activeCalculation.rb = activeCalculation.instance.GetComponent<Rigidbody>();
        }
        else
            notator = activeCalculation.instance.GetComponent<TrajectoryNotator>();
        if (notator == null) return;
        
        activeCalculation.instance.SetActive(true);
        notator.listening = false;
        notator.enabled = true;

        activeCalculation.instance.transform.position = position;
        activeCalculation.instance.transform.rotation = rotation;
        activeCalculation.instance.transform.localScale = scale;

        notator.SetValues(activeCalculation.rb, activeCalculation);
        notator.listening = true;
    }

    public class TrajectoryNotator : MonoBehaviour
    {
        public Rigidbody rb;
        private TrajectoryCalculation calculation;
        public bool listening = false;

        public void SetValues(Rigidbody rb, TrajectoryCalculation calculation)
        {
            this.rb = rb; 
            this.calculation = calculation;
            enabled = true;
        }

        private void Awake()
        {
            enabled = false;
        }

        private void Update()
        {
            if (calculation == null)
                return;
            if (calculation.trajectory.highestPosition.y < calculation.rb.position.y)
                calculation.trajectory.highestPosition = rb.position;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (listening == false)
                return;
            calculation.trajectory.endPosition = other.contacts[0].point;
            calculation.instance.SetActive(false);
            enabled = false;
            calculation.End();
        }
    }
}