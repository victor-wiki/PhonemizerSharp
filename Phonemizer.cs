using PhonemizerSharp.backend;
using PhonemizerSharp.backend.espeak;
using PyException.common;
using SharpUtility.Extension;
using SharpUtility.Helper;

namespace PhonemizerSharp
{
    public class Phonemizer
    {
        public static Separator default_separator = new Separator(phone: "", syllable: "", word: " ");
        public static readonly Dictionary<string, Type> BACKENDS = new Dictionary<string, Type>() { { "EspeakBackend", typeof(EspeakBackend) }, { "FestivalBackend", null }, { "EspeakMbrolaBackend", null } }; ////to do

        public static dynamic phonemize(dynamic text, string language = "en-us",
                        string backend = "espeak",
                        Separator separator = null,
                        bool strip = false,
                        bool prepend_text = false,
                        bool preserve_empty_lines = false,
                        bool preserve_punctuation = false,
                        string punctuation_marks = null,
                        bool is_regex_marks= false,
                        bool with_stress = false,
                        dynamic tie = null,
                        string language_switch = "keep-flags",
                        string words_mismatch = "ignore",
                        int njobs = 1
                     )
        {
            separator ??= default_separator;
            punctuation_marks ??= Punctuation.default_marks();
            tie ??= false;

            // ensure the arguments are valid
            _check_arguments(backend, with_stress, tie, separator, language_switch, words_mismatch);

            // preserve_punctuation and word separator not valid for espeak-mbrola
            if (backend == "espeak-mbrola" && preserve_punctuation)
            {
                LogHelper.LogWarnning("espeak-mbrola backend cannot preserve punctuation");
            }

            if (backend == "espeak-mbrola" && !string.IsNullOrEmpty(separator.word))
            {
                LogHelper.LogWarnning("espeak-mbrola backend cannot preserve word separation");
            }

            BaseBackend phonemizer = null;

            // initialize the phonemization backend, reusing a cached instance when
            // this exact configuration has been used before. The logger is part of
            // the key because backends hold on to it.
            if (backend == "espeak")
            {
                phonemizer = new EspeakBackend(
                    language,
                    punctuation_marks: punctuation_marks,                   
                    preserve_punctuation: preserve_punctuation,
                    with_stress: with_stress,
                    tie: tie,
                    language_switch: language_switch,
                    words_mismatch: words_mismatch);
            }
            else if (backend == "espeak-mbrola")
            {
                throw new NotImplementedException(); ////to do
            }
            else //festival or segments
            {
                throw new NotImplementedException(); ////to do
            }

            // do the phonemization
            return _phonemize(phonemizer, text, separator, strip, njobs, prepend_text, preserve_empty_lines);
        }

        /// <summary>
        /// Auxiliary function to phonemize()
        /// </summary>
        /// <param name="backend"></param>
        /// <param name="text"></param>
        /// <param name="separator"></param>
        /// <param name="strip"></param>
        /// <param name="njobs"></param>
        /// <param name="prepend_text"></param>
        /// <param name="preserve_empty_lines"></param>
        private static dynamic _phonemize(BaseBackend backend, dynamic text, Separator separator, bool strip, int njobs, bool prepend_text, bool preserve_empty_lines)
        {
            // remember the text type for output (either list or string)
            Type text_type = text.GetType();

            // force the text as a list
            text = (Utils.str2list(text) as List<string>).Select(line => line.Trim(Environment.NewLine.ToCharArray())).ToList();

            List<int> empty_lines = null;

            // if preserving empty lines, note the index of each empty line
            if (preserve_empty_lines)
            {
                empty_lines = (text as List<string>).Select((line, n) => new { n, line }).Where(item => string.IsNullOrEmpty(item.line)).Select(item => item.n).ToList();
            }

            // ignore empty lines
            text = (text as List<string>).Where(line => !string.IsNullOrEmpty(line.Trim())).ToList();

            List<string> phonemized = null;

            if (text.Count > 0)
            {
                // phonemize the text
                phonemized = backend.phonemize(text, separator: separator, strip: strip, njobs: njobs);
            }
            else
            {
                phonemized = [];
            }

            // if preserving empty lines, reinsert them into text and phonemized lists
            if (preserve_empty_lines)
            {
                foreach(var i in empty_lines) //noqa
                {
                    if(prepend_text)
                    {
                        text.Insert(i, "");
                    }

                    phonemized.Insert(i, "");
                }
            }

            // at that point, the phonemized text is a list of str. Format it as
            // expected by the parameters
            if(prepend_text)
            {
                return (text as List<string>).Zip(phonemized).ToList();
            }

            if(text_type == typeof(string))
            {
                return Utils.list2str(phonemized);
            }

            return phonemized;
        }

        /// <summary>
        /// Ensures the parameters are compatible with each other, raises a
        /// RuntimeError the first encountered error.
        /// </summary>
        /// <param name="backend"></param>
        /// <param name="with_stress"></param>
        /// <param name="tie"></param>
        /// <param name="separator"></param>
        /// <param name="language_switch"></param>
        /// <param name="words_mismatch"></param>
        private static void _check_arguments(string backend, bool with_stress, dynamic tie, Separator separator,
                                             string language_switch, string words_mismatch)
        {
            // ensure the backend is either espeak, festival or segments
            string[] options = ["espeak", "espeak-mbrola", "festival", "segments"];

            if (!options.Contains(backend))
            {
                throw new RuntimeError($"{backend} is not a supported backend, choose in {options.ToFridenlyString()}.");
            }

            // with_stress option only valid for espeak
            if (with_stress && backend != "espeak")
            {
                throw new RuntimeError(@"the ""with_stress"" option is available for espeak backend only, " +
                    $"but you are using {backend} backend");
            }

            // tie option only valid for espeak
            if (tie == true && backend != "espeak")
            {
                throw new RuntimeError(@"the ""tie"" option is available for espeak backend only, " +
                    $"but you are using {backend} backend");
            }

            // tie option incompatible with phone separator
            if (tie == true && !string.IsNullOrEmpty(separator.phone))
            {
                throw new RuntimeError(@"the ""tie"" option is incompatible with phone separator " +
                    $@"(which is ""{separator.phone}"")");
            }

            // language_switch option only valid for espeak
            if (language_switch != "keep-flags" && backend != "espeak")
            {
                throw new RuntimeError(@"the ""language_switch"" option is available for espeak backend " +
                    $"only, but you are using {backend} backend");
            }

            // words_mismatch option only valid for espeak
            if (words_mismatch != "ignore" && backend != "espeak")
            {
                throw new RuntimeError(@"the ""words_mismatch"" option is available for espeak backend " +
                    $"only, but you are using {backend} backend");
            }


        }
    }
}
