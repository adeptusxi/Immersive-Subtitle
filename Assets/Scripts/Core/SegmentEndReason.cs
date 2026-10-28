namespace ImmersiveSubtitles.Core
{
    public enum SegmentEndReason
    {
        Finished, // duration elapsed 
        Interrupted // stop()/Speaker.Stop()/speaker disabled
    }
}
