using System.Xml;

namespace TestRecorder.Core.CodeGenerators
{
    public class CodeTemplate
    {
        public string TemplateName;
        public string DriverLibrary;
        public string CodeLanguage;
        public bool CanRun;
        public string FileExtension;
        public string CodePageTemplate="";
        public bool PropertiesInSeparateFile;
        public string PropertyPageTemplate="";
        public string PropertyTemplate="";

        public CodeTemplate(string filename)
        {
            if (!System.IO.File.Exists(filename)) return;
            var document = new XmlDocument();
            document.Load(filename);
            XmlNode rootNode = document.DocumentElement;
            if (rootNode == null) return;
            
            XmlNode node = rootNode.SelectSingleNode("TemplateName");
            if (node != null) TemplateName = node.InnerText;

            node = rootNode.SelectSingleNode("CodeLanguage");
            if (node != null) CodeLanguage = node.InnerText;

            node = rootNode.SelectSingleNode("DriverLibrary");
            if (node != null) DriverLibrary = node.InnerText;

            node = rootNode.SelectSingleNode("CanRun");
            if (node != null) CanRun = node.InnerText=="1";

            node = rootNode.SelectSingleNode("FileExtension");
            FileExtension = node != null ? node.InnerText : "";

            node = rootNode.SelectSingleNode("TestCode");
            CodePageTemplate = node != null ? node.InnerText : "";

            node = rootNode.SelectSingleNode("Property");
            PropertyTemplate = node != null ? node.InnerText : "";

            node = rootNode.SelectSingleNode("PropertyPage");
            if (node != null)
            {
                PropertyPageTemplate = node.InnerText;
                if (node.Attributes != null)
                    PropertiesInSeparateFile = node.Attributes["separatefile"].Value == "1";
            }

        }

        public override string ToString()
        {
            return TemplateName;
        }
    }
}
