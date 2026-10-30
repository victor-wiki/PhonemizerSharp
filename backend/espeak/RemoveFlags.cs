using SharpUtility.Extension;
using SharpUtility.Helper;
using System.Text.RegularExpressions;

namespace PhonemizerSharp.backend.espeak
{
    /// <summary>
    /// Removes the language switch flags when detected
    /// </summary>
    public class RemoveFlags : BaseLanguageSwitch
    {
        public RemoveFlags(string language) : base(language)
        {
        }

        public override (string, bool) process(string utterance)
        {
            if(this.is_language_switch(utterance))
            {
                // remove all the (lang) flags in the current utterance
                return (Regex.Replace(utterance, _ESPEAK_FLAGS_RE, ""), true);
            }

            return (utterance, false);
        }

        public override void warning(List<int> switches)
        {
            if (!switches.HasValue())
            {
                return;
            }

            var nswitches = switches.Count;

            LogHelper.LogWarnning($"{nswitches} utterances containing language switches on lines {switches.OrderBy(item => item).ToFridenlyString()}");

            LogHelper.LogWarnning($"extra phones may appear in the \"{this._language}\" phoneset");

            LogHelper.LogWarnning("language switch flags have been removed (applying \"remove-flags\" policy)");
        }
    }
}
