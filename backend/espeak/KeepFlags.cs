using SharpUtility.Extension;
using SharpUtility.Helper;

namespace PhonemizerSharp.backend.espeak
{
    /// <summary>
    /// Preserves utterances even if language switch flags are present
    /// </summary>
    public class KeepFlags : BaseLanguageSwitch
    {
        public KeepFlags(string language) : base(language)
        {
        }

        public override (string, bool) process(string utterance)
        {
            return (utterance, is_language_switch(utterance));
        }

        public override void warning(List<int> switches)
        {
            if(!switches.HasValue())
            {
                return;
            }

            var nswitches = switches.Count;

            LogHelper.LogWarnning($"{nswitches} utterances containing language switches on lines {switches.OrderBy(item => item).ToFridenlyString()}");

            LogHelper.LogWarnning($"extra phones may appear in the \"{this._language}\" phoneset");

            LogHelper.LogWarnning("language switch flags have been kept (applying \"keep-flags\" policy)");
        }
    }
}
