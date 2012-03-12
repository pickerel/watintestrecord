using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using TestRecorder.Core.Actions;

namespace TestRecorder.Core.CodeGenerators
{
    public class SodaBase : CodeGenerator
    {
        /// <summary>
        /// line ending for the different languages
        /// </summary>
        internal string LineEnding = "";

        /// <summary>
        /// ending structure for an statement group
        /// I'm doing this for VB.NET so it's obvious what I need to change for my normal C#
        /// </summary>
        internal string StatementClosure = "end";

        private readonly List<string> _openedWindows = new List<string>();

        /// <summary>
        /// flag indicating a wrapping statement requires a closing brace
        /// </summary>
        internal int RequiresClosureAfterLine;

        public SodaBase(CodeTemplate template) : base(template) { }

        public override string FileCodeWrapping(string code)
        {
            return string.Format("<soda>\r\n{0}\r\n</soda>",code);
        }

        /// <summary>
        /// The rubber starts meeting the road here. Converts an action object to code
        /// </summary>
        /// <param name="action">action object to convert</param>
        public override void ActionToCode(ActionBase action)
        {
            string strtype = Regex.Match(action.GetType().ToString(), "\\.(\\w+)$").Groups[1].Value;

            string friendlyName = "";
            if (action.GetType().IsSubclassOf(typeof (ActionElementBase)))
            {
                friendlyName = GetPropertyType(((ActionElementBase) action).ActionFinder.TagName);
            }

            string pagename = action.ActionWindow.InternalName;
            if (!_openedWindows.Contains(action.ActionWindow.InternalName))
            {
                Code.Add(CreateBrowser(pagename, BrowserType, action.ActionWindow.InitialUrl));
                _openedWindows.Add(action.ActionWindow.InternalName);
            }

            string code;
            switch (strtype)
            {
                case "ActionBackward":
                    Code.Add(CreateBrowserAction("back"));
                    break;
                case "ActionForward":
                    Code.Add(CreateBrowserAction("forward"));
                    break;
                case "ActionRefresh":
                    Code.Add(CreateBrowserAction("refresh"));
                    break;
                case "ActionFocus":
                    // does not appear to be supported
                    break;
                case "ActionCloseWindow":
                    Code.Add(CreateBrowserAction("close"));
                    _openedWindows.Remove(action.ActionWindow.InternalName);
                    break;
                case "ActionNavigate":
                    code = CommandToString("browser", null, new NameValueCollection {{"url", ((ActionNavigate) action).Url}});
                    Code.Add(code);
                    break;
                case "ActionClick":
                    code = CommandToString(friendlyName, ((ActionElementBase) action).ActionFinder, new NameValueCollection {{"click", "true"}});
                    Code.Add(code);
                    break;
                case "ActionSelect":
                    code = CommandToString(friendlyName, ((ActionElementBase)action).ActionFinder, ((ActionSelect)action).ByValue ? new NameValueCollection { { "set", ((ActionSelect)action).SelectedValue } } : new NameValueCollection { { "set", ((ActionSelect)action).SelectedText } });
                    Code.Add(code);
                    break;
                case "ActionTypeText":
                    var textaction = (ActionTypeText) action;
                    code = CommandToString(friendlyName, ((ActionElementBase)action).ActionFinder, textaction.Overwrite ? new NameValueCollection { { "set", textaction.TextToType } } : new NameValueCollection { { "append", textaction.TextToType } });
                    Code.Add(code);
                    break;
                default:
                    Code.Add("<!-- UNHANDLED CODE OBJECT: " + action.GetType()+" *** This means I forgot to add it to the code generator... -->");
                    break;
            }

            if (RequiresClosureAfterLine == Code.Count)
                Code.Add(LineEnding);
        }

        /// <summary>
        /// creates a finder string for action object
        /// </summary>
        /// <param name="finder">list of finder attributes necessary to locate this element in the DOM</param>
        /// <returns>WatiN-suitable finder string</returns>
        public override string GetPropertyAttributeString(FindAttributeCollection finder)
        {
            var builder = new StringBuilder();
            foreach (FindAttribute attribute in finder.AttributeList)
            {
                string findName = attribute.FindName.ToLower();
                if (findName == "href") findName = "url";
                builder.AppendFormat(" {0}=\"{1}\"",findName, attribute.FindValue);
            }
            return builder.ToString();
        }

        /// <summary>
        /// converts HTML property type object to WatiN specific name
        /// </summary>
        /// <param name="elementType">HTML property type to convert</param>
        /// <returns>WatiN property type string</returns>
        internal override string GetPropertyType(string elementType)
        {
            string propertyType = elementType == "" ? "TextField" : elementType;

            switch (propertyType.ToLower())
            {
                case "text":
                case "textfield":
                case "textarea":
                    propertyType = "textfield";
                    break;
                case "div":
                    propertyType = "div";
                    break;
                case "checkbox":
                    propertyType = "checkbox";
                    break;
                case "select":
                case "selectlist":
                    propertyType = "select";
                    break;
                case "radio":
                    propertyType = "radio";
                    break;
                case "td":
                    propertyType = "td";
                    break;
                case "a":
                    propertyType = "link";
                    break;
                default:
                    propertyType = "div";
                    break;
            }

            return propertyType.ToLower();
        }

        internal override string GetFrames(List<FindAttributeCollection> frames)
        {
            // frames not supported
            return "";
        }

        public override string CreateBrowser(string windowName, BrowserTypes browser = BrowserTypes.IE, string initialUrl = "")
        {
            // browser not created like this
            return "";
        }

        private string CreateBrowserAction(string action)
        {
            return CommandToString("browser", null, new NameValueCollection {{"action", action}});
        }

        /// <summary>
        /// makes a string out of a command
        /// </summary>
        /// <param name="elementName">object to command</param>
        /// <param name="nvcAttributes">parameters to the command</param>
        /// <returns>code line string</returns>
        private string CommandToString(string elementName, FindAttributeCollection finder,  NameValueCollection nvcAttributes)
        {
            var builder = new StringBuilder();
            builder.Append("<");
            builder.Append(elementName);

            if (finder != null)
                builder.Append(GetPropertyAttributeString(finder));

            foreach (string key in nvcAttributes)
            {
                builder.Append(" " + key + "=\"" + nvcAttributes[key] + "\"");
            }

            builder.Append("/>");

            return builder.ToString();
        }
    }
}
