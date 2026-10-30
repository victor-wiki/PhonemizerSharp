namespace PhonemizerSharp.backend.espeak
{
    /// <summary>
    /// Abstract espeak backend for the phonemizer
    /// </summary>
    public class BaseEspeakBackend : BaseBackend
    {
        protected EspeakPhonemizer _espeak = null;

        public BaseEspeakBackend(string language, string punctuation_marks = null, bool preserve_punctuation = false) : base(language, punctuation_marks, preserve_punctuation)
        {
            this._espeak = new EspeakPhonemizer();
        }
    }
}
