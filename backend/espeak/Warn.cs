using SharpUtility.Helper;

namespace PhonemizerSharp.backend.espeak
{
    /// <summary>
    /// Warns on every mismatch detected
    /// </summary>
    public class Warn : BaseWordsMismatch
    {
        public override List<string> process(List<string> text)
        {
            var mismatch = this._mismatched_lines();

            foreach (var (num, ntxt, nphn) in mismatch)
            {
                LogHelper.LogWarnning($"words count mismatch on line {num + 1} " +
                    $"(expected {ntxt} words but get {nphn})");
            }

            this._resume(mismatch.Count, text.Count);

            return text;
        }
    }
}
