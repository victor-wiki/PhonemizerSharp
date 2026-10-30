namespace PhonemizerSharp.backend.espeak
{
    public class language_switch
    {
        /// <summary>
        /// Returns a language switch processor initialized from `mode`
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public static BaseLanguageSwitch get_language_switch_processor(string mode, string language)
        {
            var processors = new Dictionary<string, Type>()
            {
                {"keep-flags", typeof(KeepFlags) },
                {"remove-flags", typeof(RemoveFlags)},
                {"remove-utterance", typeof(RemoveUtterances)}
            };

            return Activator.CreateInstance(processors[mode], language) as BaseLanguageSwitch;
        }
    }
}
