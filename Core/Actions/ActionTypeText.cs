using IfacesEnumsStructsClasses;

namespace TestRecorder.Core.Actions
{
    public class ActionTypeText : ActionElementBase
    {
        public static string ActionName { get { return "Type Text"; } }
        public bool Overwrite = true;
        public string TextToType { get; set; }
        public static string IconFilename { get { return "TypeText.bmp"; } }
        internal override string Description { get { return string.Format("Type \"{1}\" into {0}", ActionFinder.GetDescription(),TextToType); } }

        public ActionTypeText(BrowserWindow window, IHTMLElement actionElement = null, string url = null)
            : base(window, actionElement, url)
        {
            var htmlInputElement = (IHTMLInputElement) actionElement;
            if (htmlInputElement != null) TextToType = htmlInputElement.value;
        }
    }
}
