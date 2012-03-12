using TestRecorder.Core.Actions;

namespace TestRecorder.Core.CodeGenerators
{
    public class ScriptProperty
    {
        public string WindowName;
        public string PropertyCode;
        public FindAttributeCollection Finder;

        public ScriptProperty(string windowName, string propertyCode, FindAttributeCollection finder=null)
        {
            WindowName = windowName;
            PropertyCode = propertyCode.Trim();
            if (finder != null)
                Finder = finder;
        }
    }
}
