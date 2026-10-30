using System.Reflection;

namespace PhonemizerSharp.backend.espeak
{
    public class EspeakPhonemizer : IDisposable
    {
        public bool with_stress { get; set; }
        public bool preserve_punctuation { get; set; }

        public EspeakPhonemizer()
        {
            string location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string dataFolder = Path.Combine(location, "data", "espeak");

            EspeakClient.Initialize(dataFolder);
        }

        private static string GetLanguage(string lang)
        {
            if (lang == "en" || string.IsNullOrEmpty(lang))
            {
                lang = "en-us";
            }

            return lang;
        }

        public void set_voice(string language)
        {
            EspeakClient.SetVoiceByName(GetLanguage(language));
        }

        public string text_to_phonemes(string text, bool tie)
        {
            return EspeakClient.TextToPhonemes(text, tie);
        }  

        public void Dispose()
        {
            EspeakClient.Stop();
        }

        #region Call directly
        public string[] Phonemize(string text, string lang)
        {
            EspeakClient.SetVoiceByName(GetLanguage(lang));

            var ipas = EspeakClient.TextToPhonemes(text).Split(' ').Select(item => HandleResult(item)).ToArray();

            return ipas;
        }

        public List<string[]> Phonemize(List<string> words, string lang)
        {
            EspeakClient.SetVoiceByName(GetLanguage(lang));

            List<string[]> result = new List<string[]>();

            foreach (var word in words)
            {
                var ipas = Phonemize(word, lang);

                result.Add(ipas);
            }

            return result;
        }

        private static string HandleResult(string item)
        {
            return item.Replace("_", "").Replace("ˈ", "");
        }
        #endregion
    }
}
