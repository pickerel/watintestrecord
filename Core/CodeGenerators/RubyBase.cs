using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using TestRecorder.Core.Actions;

namespace TestRecorder.Core.CodeGenerators
{
    public abstract class RubyBase : CodeGenerator
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

        protected RubyBase(CodeTemplate template) : base(template) { }

        /// <summary>
        /// The rubber starts meeting the road here. Converts an action object to code
        /// </summary>
        /// <param name="action">action object to convert</param>
        public override void ActionToCode(ActionBase action)
        {
            string strtype = Regex.Match(action.GetType().ToString(), "\\.(\\w+)$").Groups[1].Value;

            string friendlyName = "";
            if (action.GetType().IsSubclassOf(typeof(ActionElementBase)))
            {
                //friendlyName = GetUniqueFriendlyName(((ActionElementBase) action).ActionFinder);
                friendlyName = ((ActionElementBase)action).ActionFinder.FriendlyName;
                ElementToProperty(friendlyName, (ActionElementBase)action);
            }

            string pagename = action.ActionWindow.InternalName;
            if (!_openedWindows.Contains(action.ActionWindow.InternalName))
            {
                Code.Add(CreateBrowser(pagename, BrowserType, action.ActionWindow.InitialUrl));
                _openedWindows.Add(action.ActionWindow.InternalName);
            }

            switch (strtype)
            {
                case "ActionBackward":
                    Code.Add(CommandToString(pagename, null, "back"));
                    break;
                case "ActionForward":
                    Code.Add(CommandToString(pagename, null, "forward"));
                    break;
                case "ActionRefresh":
                    Code.Add(CommandToString(pagename, null, "refresh"));
                    break;
                case "ActionFocus":
                    Code.Add(CommandToString(pagename, friendlyName, "focus"));
                    break;
                case "ActionCloseWindow":
                    Code.Add(CommandToString(action.ActionWindow.InternalName, null, "close"));
                    _openedWindows.Remove(action.ActionWindow.InternalName);
                    break;
                case "ActionNavigate":
                    Code.Add(CommandToString(action.ActionWindow.InternalName, null, "goto",
                        new List<string> { "\"" + ((ActionNavigate)action).Url + "\"" }));
                    break;
                case "ActionClick":
                    Code.Add(((ActionElementBase)action).NoWait
                                 ? CommandToString(pagename, friendlyName, "click_no_wait")
                                 : CommandToString(pagename, friendlyName, "click"));
                    break;
                case "ActionSelect":
                    if (((ActionSelect)action).ByValue)
                        Code.Add(CommandToString(pagename, friendlyName, "select_value",
                        new List<string> { "\"" + ((ActionSelect)action).SelectedValue + "\"" }));
                    else Code.Add(CommandToString(pagename, friendlyName, "select",
                        new List<string> { "\"" + ((ActionSelect)action).SelectedText + "\"" }));
                    break;
                case "ActionTypeText":
                    var textaction = (ActionTypeText)action;
                    if (textaction.Overwrite)
                        Code.Add(SetElementToString(pagename + "." + friendlyName + ".value", "\"" + textaction.TextToType + "\""));
                    else Code.Add(CommandToString(pagename, friendlyName, "append",
                        new List<string> { "\"" + textaction.TextToType + "\"" }));
                    break;
                case "ActionAlertHandler":
                    Code.Add(AlertDialog());
                    RequiresClosureAfterLine = Code.Count + 1;
                    break;
                default:
                    Code.Add("# UNHANDLED CODE OBJECT: " + action.GetType());
                    Code.Add("# This means I forgot to add it to the code generator...");
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
                switch (findName)
                {
                    case "alt":
                        findName = ":alt => \"{0}\"";
                        break;
                    case "class":
                        findName = ":class => \"{0}\"";
                        break;
                    case "for":
                        findName = ":for => \"{0}\"";
                        break;
                    case "id":
                        findName = ":id => \"{0}\"";
                        break;
                    case "name":
                        findName = ":name => \"{0}\"";
                        break;
                    case "text":
                        findName = ":text => \"{0}\"";
                        break;
                    case "url":
                    case "href":
                        findName = ":url => \"{0}\"";
                        break;
                    case "title":
                        findName = ":title => \"{0}\"";
                        break;
                    case "value":
                        findName = ":value => \"{0}\"";
                        break;
                    case "src":
                        findName = ":src => \"{0}\"";
                        break;
                    default:
                        findName = ":\"{0}\" => \"{1}\"";
                        break;
                }
                if (findName.Contains("{1}"))
                    builder.AppendFormat(findName + ", ", attribute.FindName, attribute.FindValue);
                else builder.AppendFormat(findName + ", ", attribute.FindValue);
            }
            builder = builder.Remove(builder.Length - 2, 2);
            return builder.ToString();
        }

        /// <summary>
        /// creates a string from the list of frames internally
        /// </summary>
        /// <param name="frames">list of frames determined for the element</param>
        /// <returns>string of frames for replacement</returns>
        internal override string GetFrames(List<FindAttributeCollection> frames)
        {
            var builder = new StringBuilder();

            foreach (FindAttributeCollection findAttributes in frames)
            {
                builder.Append("Frame(" + GetPropertyAttributeString(findAttributes) + ").");
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
                case "select":
                    propertyType = "SelectList";
                    break;
                case "radio":
                    propertyType = "Radio";
                    break;
                case "checkbox":
                    propertyType = "CheckBox";
                    break;
                case "tr":
                    propertyType = "TableRow";
                    break;
                case "td":
                    propertyType = "TableCell";
                    break;
                case "a":
                    propertyType = "Link";
                    break;
                case "text":
                case "textarea":
                    propertyType = "TextField";
                    break;
                default:
                    propertyType = "Element";
                    break;
            }
            return propertyType;
        }

        /// <summary>
        /// makes a string out of a command
        /// </summary>
        /// <param name="pagename">name of the page object for this element</param>
        /// <param name="elementName">object to command</param>
        /// <param name="command">command to call</param>
        /// <param name="parameters">parameters to the command</param>
        /// <returns>code line string</returns>
        private string CommandToString(string pagename, string elementName, string command, IEnumerable<string> parameters = null)
        {
            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(pagename))
            {
                builder.Append(pagename);
                builder.Append(".");
            }
            if (!string.IsNullOrEmpty(elementName))
            {
                builder.Append(elementName);
                builder.Append(".");
            }
            builder.Append(command);
            builder.Append("(");
            if (parameters != null)
            {
                foreach (string parameter in parameters)
                {
                    builder.Append(parameter + ", ");
                }
                builder.Remove(builder.Length - 2, 2);
            }
            builder.Append(")");
            builder.Append(LineEnding);

            return builder.ToString();
        }


        public string ClassCreateToString(string pageClass, string classVariable, string browserClass, params object[] constructorParameters)
        {
            string cmd = string.Format("{0} = {1}.new({2}.new)", classVariable, pageClass, browserClass);
            return cmd;
        }

        public string SetElementToString(string elementVariable, string elementValue)
        {
            return elementVariable + " = " + elementValue;
        }

        public string AlertDialog()
        {
            return "";
        }
    }
}
