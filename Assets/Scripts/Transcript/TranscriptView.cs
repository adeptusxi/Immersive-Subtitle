using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ImmersiveSubtitles.Transcript
{
    // populates the transcript UI 
    [DefaultExecutionOrder(200)]
    public sealed class TranscriptView : MonoBehaviour
    {
        private const float BottomThreshold = 1f; // canvas units

        [SerializeField] private GameObject canvasRoot;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private TMP_Text text;

        private readonly StringBuilder builder = new();
        private TranscriptLog log;
        private bool pinnedToBottom = true;
        private float lastContentHeight;

        private void Awake()
        {
            log = new TranscriptLog();
            log.EntryAdded += _ => RebuildText();
            log.EntryUpdated += _ => RebuildText();
            log.InitSubscriptions();
            
            RebuildText();
        }

        private void OnDestroy()
        {
            log.CleanupSubscriptions();
        }

        private void LateUpdate()
        {
            if (OVRInput.GetDown(OVRInput.Button.Two))
            {
                Toggle();
            }

            if (canvasRoot.activeSelf)
            {
                UpdateAutoScroll();
            }
        }

        public void Clear()
        {
            log.Clear();
            RebuildText();
            pinnedToBottom = true;
        }

        private void Toggle()
        {
            if (canvasRoot.activeSelf)
            {
                canvasRoot.SetActive(false);
                return;
            }

            canvasRoot.SetActive(true);
            pinnedToBottom = true;
            lastContentHeight = -1f; 
        }

        private void UpdateAutoScroll()
        {
            RectTransform content = scrollRect.content;
            float contentHeight = content.rect.height;
            float maxScroll = contentHeight - scrollRect.viewport.rect.height;
            
            if (contentHeight == lastContentHeight)
            {
                pinnedToBottom = maxScroll - content.anchoredPosition.y <= BottomThreshold;
                return;
            }

            lastContentHeight = contentHeight;
            if (pinnedToBottom)
            {
                content.anchoredPosition = new Vector2(content.anchoredPosition.x, Mathf.Max(0f, maxScroll));
                scrollRect.velocity = Vector2.zero;
            }
        }
        
        private void RebuildText()
        {
            builder.Clear();
            foreach (TranscriptEntry entry in log.Entries)
            {
                if (builder.Length > 0)
                {
                    builder.Append("\n\n");
                }

                Append(entry);
            }

            text.text = builder.ToString();
        }

        private void Append(TranscriptEntry entry)
        {
            if (entry.type == TranscriptEntryType.Choice)
            {
                builder.Append("› ").Append(entry.Paragraph);
                return;
            }

            builder.Append("<color=#")
                .Append(ColorUtility.ToHtmlStringRGB(entry.speaker.color))
                .Append('>')
                .Append(entry.speaker.displayName)
                .Append("</color>\n")
                .Append(entry.Paragraph);
            
            if (entry.interrupted)
            {
                builder.Append(" —");
            }
        }
    }
}
