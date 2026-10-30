namespace PhonemizerSharp.backend.espeak
{
    public class words_mismatch
    {
        /// <summary>
        /// Returns a word count mismatch processor according to `mode`
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static BaseWordsMismatch get_words_mismatch_processor(string mode)
        {
            var processors = new Dictionary<string, Type>()
            {
                {"ignore", typeof(Ignore) },
                {"warn", typeof(Warn) },
                {"remove", typeof(Remove) }
            };

            return Activator.CreateInstance(processors[mode]) as BaseWordsMismatch;
        }       
    }
}
