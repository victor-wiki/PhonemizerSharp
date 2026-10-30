using PyException.common;
using SharpUtility.Helper;
using System.Text.RegularExpressions;

namespace PhonemizerSharp.backend.espeak
{
    /// <summary>
    /// The base class of all word count mismatch processors
    /// </summary>
    public class BaseWordsMismatch
    {
        protected static readonly Regex _RE_SPACES = new Regex(@"\s+");

        private List<int> _count_txt = null;
        private List<int> _count_phn = null;

        public BaseWordsMismatch()
        {
            this._count_txt = [];
            this._count_phn = [];
        }

        /// <summary>
        /// Return the number of words contained in each line of `text
        /// </summary>
        /// <param name="text"></param>
        /// <param name="wordsep"></param>
        private List<int> _count_words(List<string> text, dynamic wordsep = null)
        {
            wordsep ??= _RE_SPACES;

            string pattern = null;

            if (wordsep is Regex regex)
            {
                pattern = regex.ToString();
            }
            else
            {
                pattern = Regex.Escape(wordsep);
            }

            return text.Select(line => Regex.Split(line, pattern).Where(w => !string.IsNullOrEmpty(w)).Count()).ToList();
        }

        /// <summary>
        /// Returns a list of (num_line, nwords_input, nwords_output)
        /// 
        /// Consider only the lines where nwords_input != nwords_output. Raises a
        /// RuntimeError if input and output do not have the same number of lines.
        /// </summary>
        /// <returns></returns>
        protected List<(int, int, int)> _mismatched_lines()
        {
            if (this._count_txt.Count != this._count_phn.Count)
            {
                throw new RuntimeError($"number of lines in input and output must be equal, " +
                    $"we have: input={this._count_txt.Count}, " +
                    $"output={this._count_phn.Count}");
            }

            var ziped = this._count_txt.Zip(this._count_phn);
            var result = new List<(int, int, int)>();

            int i = 0;

            foreach (var (t, p) in ziped)
            {
                if (t != p)
                {
                    result.Add((i, t, p));
                }

                i++;
            }

            return result;
        }

        /// <summary>
        /// Logs a high level undetailed warning
        /// </summary>
        /// <param name="nmismatch"></param>
        /// <param name="nlines"></param>
        protected void _resume(int nmismatch, int nlines)
        {
            if (nmismatch != 0)
            {
                LogHelper.LogWarnning($"words count mismatch on {Math.Round(nmismatch * 1.0 / nlines * 100, 2)}% of the lines ({nmismatch}/{nlines})");
            }
        }

        /// <summary>
        /// Stores the number of words in each input line
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public void count_text(List<string> text)
        {
            this._count_txt = this._count_words(text);
        }

        /// <summary>
        /// Stores the number of words in each output line
        /// </summary>
        /// <param name="text"></param>
        /// <param name="separator"></param>
        public void count_phonemized(List<string> text, Separator separator)
        {
            this._count_phn = this._count_words(text, separator.word);
        }

        /// <summary>
        /// Detects and process word count misatches according to the mode
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual List<string> process(List<string> text)
        {
            throw new NotImplementedException();
        }
    }
}
