namespace PhonemizerSharp.backend.espeak
{
    public enum EspeakError
    {
        EE_OK = 0,
        EE_INTERNAL_ERROR = -1,
        EE_BUFFER_FULL = 1,
        EE_NOT_FOUND = 2
    }

    public enum EspeakAudioOutput
    {
        Playback,
        Retrieval,
        Synchronous,
        SynchronousPlayback
    };
}
