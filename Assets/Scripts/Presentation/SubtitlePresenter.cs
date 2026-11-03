using System.Collections.Generic;
using ImmersiveSubtitles.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ImmersiveSubtitles.Presentation
{
    // populates and styles text, rotates and scales panel, and handles visibility based on mode. 
    // note: body text's authored size becomes the base em height (scaling applies on top of it) 
    [DefaultExecutionOrder(200)]
    public abstract class SubtitlePresenter : MonoBehaviour
    {
        [SerializeField] private PresentationMode mode;
        [SerializeField] private GameObject root; // should not be the gameobject this script is on
        [SerializeField] private Image background;
        [SerializeField] private GameObject[] additionalObjects;
        
        [Header("Presentation")]
        [SerializeField] private bool overrideTextStyle;
        [SerializeField] private TextStylePreferences textStyle = new();
        [SerializeField] private bool overrideFacePlayer;
        [SerializeField] private bool facePlayer = true;

        private static readonly List<SubtitlePresenter> enabledPresenters = new();

        private Quaternion authoredRotation;

        public static IReadOnlyList<SubtitlePresenter> Enabled => enabledPresenters;
        public PresentationMode Mode => mode;
        public abstract bool Covers(SpeakerProfile speaker);

        protected Image Background => background;
        protected bool IsActive => SubtitleRuntime.Preferences.Current.mode == mode;
        protected abstract bool HasContent { get; }
        protected abstract float AuthoredFontSize { get; }

        protected abstract void Sync(IReadOnlyList<SegmentInstance> visible);
        protected abstract void ApplyStyle(SubtitlePreferences preferences);
        protected abstract void UpdateReveal(double now);
        
        // panel size comes from sizeDegrees
        protected TextStylePreferences TextStyle(SubtitlePreferences preferences) =>
            overrideTextStyle ? textStyle : preferences.text;

        private bool FacePlayer(SubtitlePreferences preferences) => overrideFacePlayer ? facePlayer : preferences.facePlayer;

        #region Unity

        protected virtual void Awake()
        {
            authoredRotation = root.transform.localRotation;
        }

        private void OnEnable()
        {
            enabledPresenters.Add(this);
            SubtitleRuntime.ActiveSubtitles.Changed += HandleVisibleChanged;
            SubtitleRuntime.Preferences.Changed += HandlePreferencesChanged;

            HandlePreferencesChanged();
            HandleVisibleChanged();
        }

        private void OnDisable()
        {
            enabledPresenters.Remove(this);
            SubtitleRuntime.ActiveSubtitles.Changed -= HandleVisibleChanged;
            SubtitleRuntime.Preferences.Changed -= HandlePreferencesChanged;

            root.SetActive(false);
            foreach (var o in additionalObjects)
            {
                o.SetActive(false);
            }
        }

        private void LateUpdate()
        {
            if (!root.activeSelf)
            {
                return;
            }

            UpdateReveal(SubtitleRuntime.ActiveSubtitles.Now);

            SubtitlePreferences preferences = SubtitleRuntime.Preferences.Current;
            Transform panel = root.transform;
            Vector3 head = Player.Instance.Head.position;

            if (FacePlayer(preferences))
            {
                panel.rotation = Quaternion.LookRotation(panel.position - head, Vector3.up);
            }

            if (preferences.sizeMode == SubtitleSizeMode.Angular)
            {
                float distance = Vector3.Distance(panel.position, head);
                SubtitleStyler.SetEmHeight(panel, MathUtils.AngularToWorldSize(TextStyle(preferences).sizeDegrees, distance), AuthoredFontSize);
            }
        }

        #endregion
        
        private void HandleVisibleChanged()
        {
            Sync(SubtitleRuntime.ActiveSubtitles.Visible);
            RefreshRoot();
        }

        private void HandlePreferencesChanged()
        {
            SubtitlePreferences preferences = SubtitleRuntime.Preferences.Current;
            ApplyStyle(preferences);

            if (!FacePlayer(preferences))
            {
                root.transform.localRotation = authoredRotation;
            }

            if (preferences.sizeMode == SubtitleSizeMode.Fixed)
            {
                SubtitleStyler.SetEmHeight(root.transform, MathUtils.AngularToWorldSize(TextStyle(preferences).sizeDegrees, 1f), AuthoredFontSize);
            }

            RefreshRoot();
        }

        private void RefreshRoot()
        {
            bool active = IsActive && HasContent;
            
            root.SetActive(active);
            foreach (var o in additionalObjects)
            {
                o.SetActive(active);
            }
        }
    }
}
