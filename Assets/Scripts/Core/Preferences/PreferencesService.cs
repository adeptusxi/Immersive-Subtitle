using System;

namespace ImmersiveSubtitles.Core
{
    public sealed class PreferencesService
    {
        public SubtitlePreferences Current { get; private set; }
        
        public event Action Changed; // raised on every SetDefaults/Reset/NotifyChanged

        private SubtitlePreferences defaults = new();

        public PreferencesService()
        {
            Current = defaults.Clone();
        }

        public void SetDefaults(SubtitlePreferences defaults)
        {
            this.defaults = defaults;
            Reset();
        }

        public void Reset()
        {
            Current = defaults.Clone();
            Changed?.Invoke();
        }

        public void NotifyChanged() => Changed?.Invoke();
    }
}
