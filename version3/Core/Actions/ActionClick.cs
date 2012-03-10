using IfacesEnumsStructsClasses;

namespace TestRecorder.Core.Actions
{
    public class ActionClick : ActionElementBase
    {
        public static string ActionName { get { return "Click"; } }
        public static string IconFilename { get { return "Click.bmp"; } }
        internal override string Description { get { return string.Format("Click {0}", ActionFinder.GetDescription()); } }

        public ActionClick(BrowserWindow window, IHTMLElement actionElement = null, string url = null) : base(window, actionElement, url) { }
    }
}
