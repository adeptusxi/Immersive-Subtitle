
using UnityEngine;

public class BrownianMotion : MonoBehaviour
{
    [SerializeField] private bool randomizeOffset = true;
    
    [Header("Position")]
    [SerializeField, Min(0f), Tooltip("in meters")] private float positionAmplitude = 0.1f;
    [SerializeField, Min(0.01f), Tooltip("in seconds")] private float positionChangeInterval = 1f;
    [SerializeField, Min(0.01f), Tooltip("in seconds")] private float positionSmoothTime = 0.5f;
    private Vector3 restPosition;
    private Vector3 currentPositionOffset;
    private Vector3 targetPositionOffset;
    private Vector3 positionVelocity;
    private float positionElapsed;

    [Header("Rotation")]
    [SerializeField, Min(0f), Tooltip("in degrees.")] private float rotationAmplitude = 5f;
    [SerializeField, Min(0.01f), Tooltip("in seconds")] private float rotationChangeInterval = 1.5f;
    [SerializeField, Min(0.01f), Tooltip("in seconds")] private float rotationSmoothTime = 0.5f;
    private Quaternion restRotation;
    private Vector3 currentRotationOffset;
    private Vector3 targetRotationOffset;
    private float rotationElapsed;

    private void Awake()
    {
        restPosition = transform.localPosition;
        restRotation = transform.localRotation;

        if (randomizeOffset)
        {
            currentPositionOffset = Random.insideUnitSphere * positionAmplitude;
            currentRotationOffset = Random.insideUnitSphere * rotationAmplitude;
        }

        ChooseNewPositionTarget();
        ChooseNewRotationTarget();
    }

    private void Update()
    {
        UpdatePosition();
        UpdateRotation();
    }

    private void UpdatePosition()
    {
        positionElapsed += Time.deltaTime;

        if (positionElapsed >= positionChangeInterval)
        {
            positionElapsed %= positionChangeInterval;
            ChooseNewPositionTarget();
        }

        currentPositionOffset = Vector3.SmoothDamp(
            currentPositionOffset,
            targetPositionOffset,
            ref positionVelocity,
            positionSmoothTime
        );

        transform.localPosition = restPosition + currentPositionOffset;
    }

    private void UpdateRotation()
    {
        rotationElapsed += Time.deltaTime;

        if (rotationElapsed >= rotationChangeInterval)
        {
            rotationElapsed %= rotationChangeInterval;
            ChooseNewRotationTarget();
        }

        currentRotationOffset = Vector3.Lerp(
            currentRotationOffset,
            targetRotationOffset,
            1f - Mathf.Exp(-Time.deltaTime / rotationSmoothTime)
        );

        transform.localRotation = restRotation *
            Quaternion.Euler(currentRotationOffset);
    }

    private void ChooseNewPositionTarget()
    {
        targetPositionOffset = Random.insideUnitSphere * positionAmplitude;
    }

    private void ChooseNewRotationTarget()
    {
        targetRotationOffset = Random.insideUnitSphere * rotationAmplitude;
    }
}
