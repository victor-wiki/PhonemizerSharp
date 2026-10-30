using PhonemizerSharp.Model;
using PyException.common;
using SharpUtility.Helper;
using System.Text.RegularExpressions;

namespace PhonemizerSharp.backend.espeak
{
    /// <summary>
    /// Espeak backend for the phonemizer
    /// </summary>
    public class EspeakBackend : BaseEspeakBackend
    {
        // a regular expression to find phonemes stresses in espeak output
        private const string _ESPEAK_STRESS_RE = @"[ˈˌ'-]+";
        private bool _with_stress = false;
        private dynamic _tie = null;
        private BaseLanguageSwitch _lang_switch = null;
        private BaseWordsMismatch _words_mismatch = null;

        public EspeakBackend(string language, string punctuation_marks = null, bool preserve_punctuation = false,
            bool with_stress = false, dynamic tie = null, string language_switch = "keep-flags", string words_mismatch = "ignore") : base(language, punctuation_marks, preserve_punctuation)
        {
            tie ??= false;

            this._espeak.set_voice(language);

            this._with_stress = with_stress;
            this._tie = this._init_tie(tie);
            this._lang_switch = espeak.language_switch.get_language_switch_processor(language_switch, language);
            this._words_mismatch = espeak.words_mismatch.get_words_mismatch_processor(words_mismatch);
        }

        private string _init_tie(dynamic tie)
        {
            if ((tie is bool && !tie) || (tie is string && string.IsNullOrEmpty(tie)))
            {
                return null;
            }

            if (tie == true) // default U+361 tie character
            {
                return "\u0361";
            }

            // non default tie charcacter
            tie = tie.ToString();

            if (tie.Length != 1)
            {
                throw new RuntimeError($"explicit tie must be a single charcacter but is {tie}");
            }

            return tie;
        }

        protected override (List<string>, List<int>) _phonemize_aux(List<string> text, int offset, Separator separator, bool strip)
        {
            if (this._tie is not null && !string.IsNullOrEmpty(separator.phone))
            {
                LogHelper.LogWarnning("cannot use ties AND phone separation, ignoring phone separator");
            }

            var output = new List<string>();
            var lang_switches = new List<int>();

            for (var i = 0; i < text.Count; i++)
            {
                var num = i + 1;
                var line = text[i];

                line = this._espeak.text_to_phonemes(line, this._tie == true);

                bool has_switch = false;

                (line, has_switch) = this._postprocess_line(line, num, separator, strip);

                output.Add(line);

                if (has_switch)
                {
                    lang_switches.Add(num + offset);
                }
            }

            return (output, lang_switches);
        }

        private (string, bool) _postprocess_line(string line, int num, Separator separator, bool strip)
        {
            // espeak can split an utterance into several lines because
            // of punctuation, here we merge the lines into a single one

            line = line.Trim().Replace('\n', ' ').Replace("  ", " ");

            // due to a bug in espeak-ng, some additional separators can be
            // added at the end of a word. Here a quick fix to solve that
            // issue. See https://github.com/espeak-ng/espeak-ng/issues/694
            line = Regex.Replace(line, @"_+", "_");
            line = Regex.Replace(line, @"_ ", " ");

            bool has_switch = false;

            (line, has_switch) = this._lang_switch.process(line);

            if (string.IsNullOrEmpty(line))
            {
                return ("", has_switch);
            }

            var out_line = "";
            var words = line.Split(' ');

            for (var i = 0; i < words.Length; i++)
            {
                var word = words[i];
                word = this._process_stress(word.Trim());

                if (!strip && this._tie is null)
                {
                    word += "_";
                }

                word = this._process_tie(word, separator);
                out_line += word + separator.word;
            }

            if (strip && !string.IsNullOrEmpty(separator.word))
            {
                // erase the last word separator from the line
                out_line = String.Join("", out_line.SkipLast(separator.word.Length));
            }

            return (out_line, has_switch);
        }

        private string _process_tie(string word, Separator separator)
        {
            // NOTE a bug in espeak append ties to (en) flags so as (͡e͡n).
            // We do not correct it here.
            if (this._tie is not null && this._tie != "͡")
            {
                // replace default '͡' by the requested one
                return word.Replace("͡", this._tie);
            }

            return word.Replace("_", separator.phone);
        }

        private string _process_stress(string word)
        {
            if (this._with_stress)
            {
                return word;
            }

            // remove the stresses on phonemes
            return Regex.Replace(word, _ESPEAK_STRESS_RE, "");
        }

        protected override (List<string>, List<MarkIndex>) _phonemize_preprocess(List<string> text)
        {
            List<MarkIndex> punctuation_marks = null;

            (text, punctuation_marks) = base._phonemize_preprocess(text);

            this._words_mismatch.count_text(text);

            return (text, punctuation_marks);
        }

        protected override List<string> _phonemize_postprocess(dynamic phonemized, List<MarkIndex> punctuation_marks, Separator separator, bool strip)
        {
            var text = phonemized.Item1 as List<string>;
            var switches = phonemized.Item2 as List<int>;

            this._words_mismatch.count_phonemized(text, separator);
            this._lang_switch.warning(switches);

            phonemized = base._phonemize_postprocess(text, punctuation_marks, separator, strip);

            return this._words_mismatch.process(phonemized);
        }

        protected override List<string> _flatten(dynamic phonemized)
        {
            return phonemized.Item1 as List<string>; ////to do
        }
    }
}
