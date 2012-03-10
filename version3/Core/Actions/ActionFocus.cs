using IfacesEnumsStructsClasses;

namespace TestRecorder.Core.Actions
{
    public class ActionFocus : ActionElementBase
    {
        public static string ActionName { get { return "Focus"; } }
        public static string IconFilename { get { return "Focus.bmp"; } }
        internal override string Description { get { return string.Format("Focus {0}", ActionFinder.GetDescription()); } }

        public ActionFocus(BrowserWindow window, IHTMLElement actionElement = null, string url = null) : base(window, actionElement, url) { }
    }
}
