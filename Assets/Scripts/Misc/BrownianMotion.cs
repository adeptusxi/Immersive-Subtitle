
using UnityEngine;

public class BrownianMotion : MonoBehaviour
{
    [SerializeField] private bool randomizeOffset = true;

    [Header("Position")]
    [SerializeField, Min(0f), Tooltip("in meters")] private float positionAmplitude = 0.1f;
    [SerializeField, Min(0.001f)] private float positionFrequency = 0.2f;

    [Header("Rotation")]
    [SerializeField, Min(0f), Tooltip("in degrees")] private float rotationAmplitude = 5f;
    [SerializeField, Min(0.001f)] private float rotationFrequency = 0.15f;

    private Vector3 restPosition;
    private Quaternion restRotation;

    private Vector3 positionSeed;
    private Vector3 rotationSeed;

    private void Awake()
    {
        restPosition = transform.localPosition;
        restRotation = transform.localRotation;

        if (randomizeOffset)
        {
            positionSeed = Random.insideUnitSphere * 1000f;
            rotationSeed = Random.insideUnitSphere * 1000f;
        }
    }

    private void Update()
    {
        float time = Time.time;

        Vector3 positionOffset = new Vector3(
            Noise(positionSeed.x, time, positionFrequency),
            Noise(positionSeed.y, time, positionFrequency),
            Noise(positionSeed.z, time, positionFrequency)
        ) * positionAmplitude;

        Vector3 rotationOffset = new Vector3(
            Noise(rotationSeed.x, time, rotationFrequency),
            Noise(rotationSeed.y, time, rotationFrequency),
            Noise(rotationSeed.z, time, rotationFrequency)
        ) * rotationAmplitude;

        transform.localPosition = restPosition + positionOffset;

        transform.localRotation =
            restRotation * Quaternion.Euler(rotationOffset);
    }

    private static float Noise(float seed, float time, float frequency)
    {
        return Mathf.PerlinNoise(seed, time * frequency) * 2f - 1f;
    }
}
