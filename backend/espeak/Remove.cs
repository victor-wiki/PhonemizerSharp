using SharpUtility.Helper;

namespace PhonemizerSharp.backend.espeak
{
    /// <summary>
    /// Removes any utterance containing a word count mismatch
    /// </summary>
    public class Remove: BaseWordsMismatch
    {
        public override List<string> process(List<string> text)
        {
            var mismatch = this._mismatched_lines().Select(line => line.Item1).ToList();

            this._resume(mismatch.Count, text.Count);

            LogHelper.LogWarnning("removing the mismatched lines");

            foreach(var index in mismatch)
            {
                text[index] = "";
            }

            return text;
        }
    }
}
