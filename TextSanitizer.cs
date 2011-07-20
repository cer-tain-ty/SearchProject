/*
 * Reuters XML Search
 * 
 * TextSanitizer.cs
 *
 * Logic to clean text of unwanted characters.
 * 
 * @author <a href="mailto:arunsun@gmail.com">Arun Sundaram</a>
 * Date - June 23, 2011
 */
namespace VectorModelIRS
{
    public class TextSanitizer
    {
        public static string Sanitize(string s)
        {

            s = s.Replace("\"", "");
            s = s.Replace("\\", " ");
            s = s.Replace("/", " ");
            s = s.Replace("(", "");
            s = s.Replace(")", "");
            s = s.Replace("-", "");
            s = s.Replace("<", "");
            s = s.Replace(">", "");
            s = s.Replace("+", "");
            s = s.Replace("'", "");

            if (s.StartsWith("$"))
            {
                return string.Empty;
            }

            if (s.EndsWith("'s"))
            {
                s = s.Remove(s.Length - 2);
            }

            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            char[] chars = s.ToCharArray();

            if (char.IsNumber(chars[0]))
            {
                return string.Empty;
            }

            return s;
        }
    }
}
