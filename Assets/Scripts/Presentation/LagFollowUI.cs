using ImmersiveSubtitles.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace ImmersiveSubtitles.Presentation
{
    [DefaultExecutionOrder(200)]
    public sealed class LagFollowUI : MonoBehaviour
    {
        [SerializeField] private RectTransform visibleArea;
        [SerializeField] private float thresholdDegrees = 15f;
        [SerializeField] private float followSpeed = 4f;
        
        private const float ArriveDegrees = 1f;

        private Transform anchor;
        private Vector3 authoredLocalPosition;
        private Quaternion authoredLocalRotation;
        private Vector3 worldPosition;
        private Quaternion worldRotation;
        private bool following;

        private static bool IsOn => SubtitleRuntime.Preferences.Current.lagFollow;

        private void Awake()
        {
            anchor = transform.parent;
            authoredLocalPosition = transform.localPosition;
            authoredLocalRotation = transform.localRotation;
        }

        private void OnEnable()
        {
            ResetToAuthored();
            RenderPipelineManager.beginCameraRendering += HandleBeginCameraRendering;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= HandleBeginCameraRendering;
        }

        private void LateUpdate()
        {
            if (!IsOn)
            {
                ResetToAuthored();
                return;
            }

            Transform head = Player.Instance.Head;
            if (!following && LookAwayDegrees(head) > thresholdDegrees)
            {
                following = true;
            }

            if (following)
            {
                float t = 1f - Mathf.Exp(-followSpeed * Time.unscaledDeltaTime);
                Vector3 localPosition = Vector3.Slerp(anchor.InverseTransformPoint(worldPosition), authoredLocalPosition, t);
                Quaternion localRotation = Quaternion.Slerp(Quaternion.Inverse(anchor.rotation) * worldRotation, authoredLocalRotation, t);
                worldPosition = anchor.TransformPoint(localPosition);
                worldRotation = anchor.rotation * localRotation;
                if (Vector3.Angle(localPosition, authoredLocalPosition) < ArriveDegrees)
                {
                    following = false;
                }
            }

            transform.SetPositionAndRotation(worldPosition, worldRotation);
        }

        private void ResetToAuthored()
        {
            transform.SetLocalPositionAndRotation(authoredLocalPosition, authoredLocalRotation);
            worldPosition = transform.position;
            worldRotation = transform.rotation;
            following = false;
        }

        // angle between player forward and nearest point on the visible area. 0 while looking at visible area 
        private float LookAwayDegrees(Transform head)
        {
            Ray gaze = new Ray(head.position, head.forward);
            Plane plane = new Plane(visibleArea.forward, visibleArea.position);
            if (!plane.Raycast(gaze, out float enter))
            {
                return 180f;
            }

            Vector3 local = visibleArea.InverseTransformPoint(gaze.GetPoint(enter));
            Rect rect = visibleArea.rect;
            Vector3 nearest = visibleArea.TransformPoint(new Vector3(
                Mathf.Clamp(local.x, rect.xMin, rect.xMax),
                Mathf.Clamp(local.y, rect.yMin, rect.yMax),
                0f));
            return Vector3.Angle(head.forward, nearest - head.position);
        }
        
        private void HandleBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (IsOn)
            {
                transform.SetPositionAndRotation(worldPosition, worldRotation);
            }
        }
    }
}
