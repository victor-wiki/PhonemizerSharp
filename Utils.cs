namespace PhonemizerSharp
{
    public class Utils
    {
        /// <summary>
        /// Returns the string `text` as a list of lines, split by \n
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static List<string> str2list(dynamic text)
        {
            if(text is string str)
            {
                return str.Trim(Environment.NewLine.ToArray()).Split(Environment.NewLine).ToList();
            }

            return text;
        }

        /// <summary>
        /// Returns the list of lines `text` as a single string separated by \n
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string list2str(dynamic text)
        {
            if(text is string str)
            {
                return str;
            }

            return string.Join(Environment.NewLine, text);
        }
    }
}
