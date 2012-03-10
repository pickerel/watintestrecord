using IfacesEnumsStructsClasses;

namespace TestRecorder.Core.Actions
{
    public class ActionElementBase : ActionBase
    {
        public bool NoWait;
        public FindAttributeCollection ActionFinder;

        public ActionElementBase(BrowserWindow windowName, IHTMLElement element, string url)
            : base(windowName)
        {
            if (element != null)
                ActionFinder = new FindAttributeCollection(element, null, url);
        }

        public string ActiveElementAttribute(IHTMLElement element, string attributeName)
        {
            if (element == null)
            {
                return "";
            }

            string strValue = "";
            try
            {
                strValue = element.getAttribute(attributeName, 0) as string ?? "";
            }
// ReSharper disable EmptyGeneralCatchClause
            catch
// ReSharper restore EmptyGeneralCatchClause
            {
            }

            return strValue;
        }
    }
}
