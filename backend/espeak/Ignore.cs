namespace PhonemizerSharp.backend.espeak
{
    /// <summary>
    /// Ignores word count mismatches
    /// </summary>
    public class Ignore: BaseWordsMismatch
    {
        public override List<string> process(List<string> text)
        {
            this._resume(this._mismatched_lines().Count, text.Count);

            return text;
        }
    }
}
