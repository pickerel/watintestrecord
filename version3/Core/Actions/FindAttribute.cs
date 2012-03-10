namespace TestRecorder.Core.Actions
{
    public class FindAttribute
    {
        public string FindValue { get; set; }
        public string FindName { get; set; }
        public bool Regex { get; set; }

        public FindAttribute() { }

        public FindAttribute(string findname, string findvalue, bool regex=false)
        {
            FindName = findname;
            FindValue = findvalue;
            Regex = regex;
        }
    }
}
