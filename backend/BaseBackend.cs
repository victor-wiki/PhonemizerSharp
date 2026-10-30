using PhonemizerSharp.Model;

namespace PhonemizerSharp.backend
{
    public class BaseBackend
    {
        private string _language = null;
        private bool _preserve_punctuation = false;
        private Punctuation _punctuator = null;

        public BaseBackend(string language, string punctuation_marks = null, bool preserve_punctuation = false)
        {
            if (punctuation_marks is null)
            {
                punctuation_marks = Punctuation.default_marks();
            }

            this._language = language;

            // setup punctuation processing
            this._preserve_punctuation = preserve_punctuation;
            this._punctuator = new Punctuation(punctuation_marks);
        }

        /// <summary>
        /// Returns the `text` phonemized for the given language
        /// </summary>
        /// <param name="text"></param>
        /// <param name="separator"></param>
        /// <param name="strip"></param>
        /// <param name="njobs"></param>
        /// <returns></returns>
        public dynamic phonemize(List<string> text, Separator separator = null, bool strip = false, int njobs = 1)
        {
            if (separator is null)
            {
                separator = Phonemizer.default_separator;
            }

            var result = this._phonemize_preprocess(text);

            text = result.Item1;
            var punctuation_marks = result.Item2;

            (List<string>, List<int>) phonemized = default;

            if (njobs == 1)
            {
                // phonemize the text forced as a string
                phonemized = this._phonemize_aux(text, 0, separator, strip);
            }
            else
            {
                // we have here a list of phonemized chunks
                var chunks = text.Chunk(njobs).ToArray();

                Dictionary<int, (List<string>,List<int>)> dict = new Dictionary<int, (List<string>,List<int>)>();

                Parallel.For(0, njobs-1, index =>
                {
                    var chunk = chunks[index].ToList();

                    var phonemized_chunk = this._phonemize_aux(chunk, index, separator, strip);

                    dict.Add(index, phonemized_chunk);
                });

                phonemized = (new List<string>(), new List<int>());

                foreach (var item in dict.OrderBy(item=>item.Key))
                {
                    phonemized.Item1.AddRange(item.Value.Item1);
                    phonemized.Item2.AddRange(item.Value.Item2);
                }

                // flatten them in a single list
                //phonemized = this._flatten(phonemized as dynamic); ////to do
            }

            return this._phonemize_postprocess(phonemized, punctuation_marks, separator, strip);
        }

        protected virtual List<string> _flatten(dynamic phonemized)
        {
            return (phonemized as List<List<string>>).SelectMany(item => item).ToList();
        }

        /// <summary>
        /// Postprocess the raw phonemized output
        /// 
        /// Restores the punctuation as needed.
        /// </summary>
        /// <param name="phonemized"></param>
        /// <param name="punctuation_marks"></param>
        /// <param name="separator"></param>
        /// <param name="strip"></param>
        /// <returns></returns>
        protected virtual  List<string> _phonemize_postprocess(dynamic phonemized, List<MarkIndex> punctuation_marks, Separator separator, bool strip)
        {
            if(this._preserve_punctuation)
            {
                return this._punctuator.restore(phonemized as List<string>, punctuation_marks, separator, strip);
            }

            return phonemized;
        }

        protected virtual (List<string>,List<int>) _phonemize_aux(List<string> text, int offset, Separator separator, bool strip)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Preprocess the text before phonemization
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected virtual (List<string>, List<MarkIndex>) _phonemize_preprocess(List<string> text)
        {
            if (_preserve_punctuation)
            {
                // a tuple (text, punctuation marks)
                return this._punctuator.preserve(text);
            }

            return (this._punctuator.remove(text), []);
        }
    }
}