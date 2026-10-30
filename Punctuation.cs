using PhonemizerSharp.Model;
using PyException.common;
using SharpUtility.Extension;
using SharpUtility.Helper;
using System.Text.RegularExpressions;

namespace PhonemizerSharp
{
    /// <summary>
    /// Preserve or remove the punctuation during phonemization
    /// </summary>
    public class Punctuation
    {
        private string _marks = null;
        private Regex _marks_re = null;      

        // The punctuation marks considered by default.
        public const string _DEFAULT_MARKS = @";:,.!?¡¿—…""«»“”(){}[]";

        // Marks that are also used as a decimal separator, and so must not be treated
        // as punctuation when they sit between two digits.
        public const string _DECIMAL_SEPARATORS = ",.";

        public dynamic marks
        {
            get
            {
                if (!string.IsNullOrEmpty(this._marks))
                {
                    return this._marks;
                }
                else
                {
                    throw new ValueError("punctuation initialized from regex, cannot access marks as a string");
                }
            }
            set
            {
                if (value is Regex regex)
                {
                    this._marks_re =new Regex(@"((" + value + @")|\s)+");
                    this._marks = null;
                }
                else
                {
                    this._marks = string.Join("", (value as string).ToCharArray().ToHashSet());

                    // catching all the marks in one regular expression: zero or more spaces
                    // + one or more marks + zero or more spaces.
                    //
                    // A mark that doubles as a decimal separator is excluded when it sits
                    // between two digits, so that a decimal number reaches the backend
                    // intact. Splitting "1,5" into "1" and "5" makes espeak read two
                    // separate numbers ("one five" instead of "one comma five"), which
                    // changes the value being spoken -- "19,99 euro" became "nineteen
                    // ninety-nine euro". A separator adjacent to a non-digit, or at the
                    // end of a token, is ordinary punctuation and still split as before.
                    var decimals = string.Join("", this._marks.Where(m => _DECIMAL_SEPARATORS.Contains(m)));
                    var others = string.Join("", this._marks.Where(m => !_DECIMAL_SEPARATORS.Contains(m)));

                    var alternatives = new List<string>();

                    if (others.HasValue())
                    {
                        alternatives.Add($"[{Regex.Escape(others)}]");
                    }

                    if (decimals.HasValue())
                    {
                        var escaped = Regex.Escape(decimals);

                        // not preceded by a digit, or not followed by one
                        alternatives.Add($"(?<![0-9])[{escaped}]");
                        alternatives.Add($"[{escaped}](?![0-9])");
                    }

                    this._marks_re = new Regex($@"'(\s*(?:{string.Join("|", alternatives)})+\s*)+'");
                }
            }
        }

        public Punctuation(string marks = _DEFAULT_MARKS)
        {
            this._marks = null;  // noqa
            this._marks_re = null;  // noqa
            this.marks = marks;           
        }

        public static string default_marks()
        {
            return _DEFAULT_MARKS;
        }

        /// <summary>
        /// Removes punctuation from `text`, allowing for furter restoration
        /// </summary>
        /// <param name="text"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public (List<string>, List<MarkIndex>) preserve(List<string> text)
        {
            var preserved_text = new List<string>();
            var preserved_marks = new List<MarkIndex>();

            for (var num = 0; num < text.Count; num++)
            {
                var line = text[num];
                var (lines, marks) = this._preserve_line(line, num);
                preserved_text.AddRange(lines);
                preserved_marks.AddRange(marks);
            }

            return (preserved_text.Where(line => !string.IsNullOrEmpty(line)).ToList(), preserved_marks);
        }

        /// <summary>
        /// Auxiliary method for Punctuation.preserve()
        /// </summary>
        /// <param name="line"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        private (List<string>, List<MarkIndex>) _preserve_line(string line, int num)
        {
            var matches = this._marks_re.Matches(line);

            if (matches.Count == 0)
            {
                return ([line], []);
            }

            // the line is made only of punctuation marks
            if (matches.Count == 1 && matches[0].Groups[0].Value == line)
            {
                return ([], [new MarkIndex() { index = num, mark = line, position = "A" }]);
            }

            // build the list of mark indexes required to restore the punctuation
            var marks = new List<MarkIndex>();

            foreach (Match match in matches)
            {
                // find the position of the punctuation mark in the utterance:
                // begin (B), end (E), in the middle (I) or alone (A)
                var position = "I";

                if (match == matches[0] && line.StartsWith(match.Groups[0].Value))
                {
                    position = "B";
                }
                else if (match == matches.LastOrDefault() && line.EndsWith(match.Groups[0].Value))
                {
                    position = "E";
                }

                marks.Add(new MarkIndex() { index = num, mark = match.Groups[0].Value, position = position });
            }

            // Cut at the matched spans. Searching for the mark text would also
            // cut a decimal point that uses the same character.
            var preserved_line = new List<string>();
            var cursor = 0;

            foreach (Match match in matches)
            {
                preserved_line.Add(line.Substring(cursor, match.Index - cursor));
                cursor = match.Index + match.Length;
            }

            // append any trailing text to the preserved line
            return (EnumerableHelper.Concat(preserved_line, [line.Substring(cursor)]), marks);
        }

        /// <summary>
        /// Returns the `text` with all punctuation marks replaced by spaces
        /// 
        /// The input `text` can be a string or a list and is returned with the
        /// same type and punctuation removed.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public List<string> remove(List<string> text)
        {
            Func<string, string> aux = (text) =>
            {
                return this._marks_re.Replace(text, " ");
            };

            return text.Select(line => aux(line)).ToList();
        }

        /// <summary>
        /// Restore punctuation in a text.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="marks"></param>
        /// <param name="separator"></param>
        /// <param name="strip"></param>
        /// <returns></returns>
        public List<string> restore(List<string> text, List<MarkIndex> marks, Separator sep, bool strip)
        {
            var punctuated_text = new List<string>();
            var pos = 0;

            while (text.HasValue() || marks.HasValue())
            {
                if (!marks.HasValue())
                {
                    for (var i = 0; i < text.Count; i++)
                    {
                        var line = text[i];

                        // if strip is False, ensure the final word ends with a word separator
                        if (!strip && !string.IsNullOrEmpty(sep.word) && !line.EndsWith(sep.word))
                        {
                            line = line + sep.word;
                        }

                        punctuated_text.Add(line);
                    }

                    text = new List<string>();
                }
                else if (!text.HasValue())
                {
                    // nothing has been phonemized, returns the marks alone, with internal
                    // spaces replaced by the word separator
                    punctuated_text.Add(Regex.Replace(string.Join("", marks.Select(m => m.mark)), " ", sep.word));

                    marks = new List<MarkIndex>();
                }
                else
                {
                    var current_mark = marks[0];

                    if (current_mark.index == pos)
                    {
                        // place the current mark here
                        var mark = marks[0];
                        marks = marks.Skip(1).ToList();

                        // replace internal spaces in the current mark with the word separator
                        var _mark = mark.mark.Replace(" ", sep.word);

                        // remove the word last separator from the current word
                        if (!string.IsNullOrEmpty(sep.word) && text[0].EndsWith(sep.word))
                        {
                            text[0] =string.Join("", text[0].SkipLast(sep.word.Length));
                        }

                        if (current_mark.position == "B")
                        {
                            text[0] = _mark + text[0];
                        }
                        else if (current_mark.position == "E")
                        {
                            punctuated_text.Add(text[0] + _mark + ((strip || _mark.EndsWith(sep.word)) ? "" : sep.word));
                            text = text.Skip(1).ToList();
                            pos = pos + 1;
                        }
                        else if(current_mark.position == "A")
                        {
                            punctuated_text.Add(_mark + ((strip || _mark.EndsWith(sep.word)) ? "" : sep.word));
                            pos = pos + 1;
                        }
                        else
                        {
                            // position == 'I'
                            if(text.Count ==1) // pragma: nocover
                            {
                                // a corner case where the final part of an intermediate
                                // mark (I) has not been phonemized
                                text[0] = text[0] + _mark;
                            }
                            else
                            {
                                var first_word = text[0];
                                text = text.Skip(1).ToList();
                                text[0] = first_word + _mark + text[0];
                            }
                        }
                    }
                    else
                    {
                        punctuated_text.Add(text[0]);
                        text = text.Skip(1).ToList();
                        pos = pos + 1;
                    }
                }
            }

            return punctuated_text;
        }
    }
}
