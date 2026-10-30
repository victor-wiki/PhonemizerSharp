using SharpUtility.Extension;
using SharpUtility.Helper;

namespace PhonemizerSharp.backend.espeak
{
    public class RemoveUtterances : BaseLanguageSwitch
    {
        public RemoveUtterances(string language) : base(language)
        {
        }

        public override (string, bool) process(string utterance)
        {
            if(this.is_language_switch(utterance))
            {
                // drop the entire utterance
                return ("", true);
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

            LogHelper.LogWarnning($"removed {nswitches} utterances containing language switches (applying \"remove-utterance\" policy)");
        }
    }
}
