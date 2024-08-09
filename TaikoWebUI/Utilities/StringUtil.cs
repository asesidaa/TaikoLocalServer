namespace TaikoWebUI.Utilities
{
    public interface IStringUtil
    {
        List<string> SplitIntoGroups(string str, int groupSize);
    }

    public class StringUtil : IStringUtil
    {
        public List<string> SplitIntoGroups(string str, int groupSize)
        {
            List<string> groups = new List<string>();
            for (int i = 0; i < str.Length; i += groupSize)
            {
                groups.Add(str.Substring(i, Math.Min(groupSize, str.Length - i)));
            }
            return groups;
        }

        public bool OnlyHexInString(string test)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(test, @"\A\b[0-9a-fA-F]+\b\Z");
        }
    }
}