using PyException.common;
using SharpUtility.Helper;

namespace PhonemizerSharp
{
    /// <summary>
    /// Defines phone, syllable and word boundary tokens
    /// </summary>
    public class Separator
    {
        private string _phone = null;
        private string _syllable = null;
        private string _word = null;

        public string phone => this._phone;
        public string syllable => this.syllable;
        public string word => this._word;

        public Separator(string word = " ", string syllable = null, string phone = null)
        {
            // check we have different separators, None excluded
            var sep1 = ArrayHelper.GetArray(phone, syllable, word).Where(sep => !string.IsNullOrEmpty(sep)).ToList();
            var sep2 = sep1.ToHashSet();

            if(sep1.Count != sep2.Count)
            {
                throw new ValueError($@"illegal separator with word=""{word}"", syllable=""{syllable}"" and" +
                    $@"phone=""{phone}"", must be all differents if not empty");
            }

            this._phone = !string.IsNullOrEmpty(phone) ? phone : "";
            this._syllable = !string.IsNullOrEmpty(syllable) ? syllable : "";
            this._word = !string.IsNullOrEmpty(word) ? word : "";
        }
    }
}
