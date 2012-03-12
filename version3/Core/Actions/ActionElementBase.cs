using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using mshtml;
using IHTMLDocument2 = IfacesEnumsStructsClasses.IHTMLDocument2;
using IHTMLElement = IfacesEnumsStructsClasses.IHTMLElement;

namespace TestRecorder.Core.Actions
{
    public class ActionElementBase : ActionBase
    {
        public bool NoWait;
        public FindAttributeCollection ActionFinder;
        public List<FindAttributeCollection> ActionFrames;
        public NameValueCollection AllAttributes;

        public ActionElementBase(BrowserWindow windowName, IHTMLElement element, string url)
            : base(windowName)
        {
            if (element == null) return;
            ActionFinder = new FindAttributeCollection(element, null, url);
            AllAttributes = ActionFinder.GetAvailableAttributes(element);
            ActionFrames = new List<FindAttributeCollection>();
            SetFrameList(element);
        }

        private void SetFrameList(IHTMLElement element)
        {
            IHTMLDocument2 doc;
            try
            {
                doc = element.document as IHTMLDocument2;
            }
            catch (Exception)
            {
                return;
            }

            var frameElement = SetFrame(doc);

            while (frameElement != null 
                && frameElement.parentElement != null)
            {
                doc = frameElement.parentElement.document as IHTMLDocument2;
                if (doc == null) return;
                frameElement = SetFrame(doc);
            }
            ActionFrames.Reverse();
        }

        private HTMLFrameBase SetFrame(IHTMLDocument2 doc)
        {
            if (doc == null) return null;
            HTMLWindow2 frameCollection;
            try
            {
                frameCollection = doc.frames as HTMLWindow2;
                if (frameCollection == null) return null;
                frameCollection = frameCollection.frames as HTMLWindow2;
                if (frameCollection == null || frameCollection.frameElement == null) return null;
            }
            catch (Exception)
            {
                return null;
            }

            if (frameCollection.frameElement.GetType().GetInterface("IHTMLFrameElement") == null
                && frameCollection.frameElement.GetType().GetInterface("IHTMLIFrameElement") == null)
            {
                return null;
            }

            if (frameCollection.frameElement == null) return null;
            var frameElement = (IHTMLElement) frameCollection.frameElement;
            var actionfinder = new FindAttributeCollection();
            actionfinder.ActionUrl = frameCollection.location.toString();
            List<FindAttribute> attributes = actionfinder.GetFindAttributes(frameElement);
            if (attributes.Count > 0) ActionFrames.Add(actionfinder);
            return (HTMLFrameBase) frameElement;

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
