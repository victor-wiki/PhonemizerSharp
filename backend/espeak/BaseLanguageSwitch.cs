using System.Text.RegularExpressions;

namespace PhonemizerSharp.backend.espeak
{
    /// <summary>
    /// The base class for language switch processors
    /// </summary>
    public class BaseLanguageSwitch
    {
        // a regular expression to find language switch flags in espeak output,
        // Switches have the following form (here a switch from English to French):
        // "something (fr)quelque chose(en) another thing".
        protected const string _ESPEAK_FLAGS_RE = @"\(.+?\)";
        protected string _language = null;

        public BaseLanguageSwitch(string language)
        {
            this._language = language;
        }

        /// <summary>
        /// Returns True is a language switch is present in the `utterance
        /// </summary>
        /// <param name="utterance"></param>
        public bool is_language_switch(string utterance)
        {
            return Regex.IsMatch(utterance, _ESPEAK_FLAGS_RE);
        }

        /// <summary>
        /// Detects and process language switches according to the mode
        /// 
        /// This method is called on each utterance as a phonemization
        /// post-processing step.
        /// </summary>
        /// <param name="utterance"></param>
        /// <returns></returns>
        public virtual (string, bool) process(string utterance)
        {
            throw new NotImplementedException();
        }

        public virtual void warning(List<int> switches)
        {
            throw new NotImplementedException();
        }
    }
}
