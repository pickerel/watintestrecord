using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Xml;
using IfacesEnumsStructsClasses;

namespace TestRecorder.Core.Actions
{
    public class SerializeActionHelper
    {
        /// <summary>
        /// Serializes the action list to an XML file that can be deserialized later
        /// </summary>
        /// <param name="actionList">list of actions to serialize</param>
        /// <param name="filename">filename to write to</param>
        public static void SerializeActionListToFile(List<ActionBase> actionList, string filename)
        {
            var settings = new XmlWriterSettings {Indent = true, IndentChars = "\t"};
            var writer = XmlWriter.Create(filename, settings);
            writer.WriteStartElement("RecorderData");
            writer.WriteStartElement("ActionList");
            var windows = new List<BrowserWindow>();
            foreach (ActionBase action in actionList)
            {
                SerializeAction(writer, action);
                if (!windows.Contains(action.ActionWindow))
                    windows.Add(action.ActionWindow);

            }
            writer.WriteEndElement();
            writer.WriteStartElement("WindowList");
            windows.ForEach(w => SerializeWindowInfo(writer, w));
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Close();
        }

        /// <summary>
        /// Private method to serialize a single action
        /// Uses reflection to get information for Public properties only
        /// </summary>
        /// <param name="writer">xml writer to write with</param>
        /// <param name="action">action to serialize</param>
        private static void SerializeAction(XmlWriter writer, ActionBase action)
        {
            PropertyInfo[] arrPropertyInfo = action.GetType().GetProperties();

            writer.WriteStartElement("Action");
            writer.WriteAttributeString("Class", action.GetType().ToString());

            if (action.ActionWindow != null)
                writer.WriteElementString("Window", action.ActionWindow.InternalName);

            foreach (PropertyInfo info in arrPropertyInfo)
            {
                if (!info.CanWrite) continue;
                object objvalue = info.GetValue(action, null);
                string value = objvalue != null ? objvalue.ToString() : null;
                if (info.Name == "ActionWindow") continue;
                if (info.Name == "WrapAction" && action.WrapAction != null)
                {
                    writer.WriteStartElement("Connection");
                    writer.WriteValue(action.WrapAction.Guid);
                    writer.WriteEndElement();
                }
                else if (info.Name == "WrapAction") continue;
                else
                {
                    writer.WriteStartElement(info.Name);
                    writer.WriteAttributeString("type", info.PropertyType.FullName);
                    writer.WriteValue(value);
                    writer.WriteEndElement();
                }
            }

            if (action.GetType().IsSubclassOf(typeof (ActionElementBase)))
            {
                var actionElement = (ActionElementBase) action;
                string tagname = ((ActionElementBase) action).ActionFinder.TagName;
                SerializeFrames(writer, actionElement.ActionFrames);
                SerializeFindAttributes(writer, tagname,
                                        actionElement.ActionFinder.ActionUrl,
                                        actionElement.ActionFinder);
                SerializeAllAttributes(writer, actionElement.AllAttributes);
            }


            writer.WriteEndElement();
        }

        private static void SerializeFrames(XmlWriter writer, IEnumerable<FindAttributeCollection> frameCollection)
        {
            writer.WriteStartElement("FrameCollection");
            foreach (FindAttributeCollection frameFinder in frameCollection)
            {
                SerializeFindAttributes(writer, "Frame", frameFinder.ActionUrl, frameFinder);
            }
            writer.WriteEndElement();
        }

        private static void SerializeAllAttributes(XmlWriter writer, NameValueCollection allAttributes)
        {
            writer.WriteStartElement("AllAttributeCollection");
            foreach (string key in allAttributes)
            {
                writer.WriteElementString(key, allAttributes[key]);
            }
            writer.WriteEndElement();
        }

        /// <summary>
        /// Private method to serialize the find attributes of an action with an element
        /// </summary>
        /// <param name="writer">xml writer to write with</param>
        /// <param name="tagname">tag to find</param>
        /// <param name="attributeCollection">collection of attributes to serialize</param>
        private static void SerializeFindAttributes(XmlWriter writer, string tagname, string actionUrl,
                                                    FindAttributeCollection attributeCollection)
        {
            writer.WriteStartElement("FindAttributeCollection");
            writer.WriteAttributeString("TagName", tagname);
            writer.WriteAttributeString("ActionUrl", actionUrl);
            foreach (FindAttribute findAttribute in attributeCollection.AttributeList)
            {
                writer.WriteStartElement("FindAttribute");
                writer.WriteAttributeString("FindName", findAttribute.FindName);
                writer.WriteAttributeString("Regex", findAttribute.Regex ? "1" : "0");
                writer.WriteString(System.Web.HttpUtility.HtmlEncode(findAttribute.FindValue));
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        /// <summary>
        /// Serialize the window information
        /// </summary>
        /// <param name="writer">xml writer to write with</param>
        /// <param name="window">window object to write about</param>
        private static void SerializeWindowInfo(XmlWriter writer, BrowserWindow window)
        {
            if (window == null) return;
            writer.WriteStartElement("Window");
            writer.WriteAttributeString("name", window.InternalName);
            writer.WriteElementString("Title", window.WindowTitle);
            writer.WriteElementString("Url", window.InitialUrl);
        }

        /// <summary>
        /// Loads an action list from a given file
        /// a rather verbose way to say LoadFromFile
        /// </summary>
        /// <param name="filename">file to load from</param>
        /// <param name="actionList">action list to output</param>
        /// <param name="windowList">window list to output</param>
        public static void DeserializeActionListFromFile(string filename, out List<ActionBase> actionList, out Dictionary<string, BrowserWindow> windowList)
        {
            var actions = new List<ActionBase>();
            actionList = actions;

            var windows = new Dictionary<string, BrowserWindow>();
            windowList = windows;

            var doc = new XmlDocument();
            doc.Load(filename);
            XmlNode rootNode = doc.DocumentElement;
            if (rootNode == null) return;

            // load the windows
            XmlNodeList windowNodes = rootNode.SelectNodes("WindowList/Window");
            if (windowNodes != null)
            {
                foreach (var window in from XmlNode node in windowNodes select DeserializeWindowInfo(node))
                {
                    windowList.Add(window.InternalName, window);
                }
            }

            // load the actions
            XmlNodeList actionNodes = rootNode.SelectNodes("ActionList/Action");
            if (actionNodes == null) return;
            actionList = new List<ActionBase>();
            var connectedActions = new Dictionary<ActionBase, string>();
            foreach (XmlNode actionNode in actionNodes)
            {
                ActionBase action = DeserializeAction(actionNode, windows);
                actionList.Add(action);

                var connectionNode = actionNode.SelectSingleNode("Connection");
                string connectionGuid = connectionNode == null ? "" : connectionNode.InnerText;
                if (!string.IsNullOrEmpty(connectionGuid))
                {
                    connectedActions.Add(action, connectionGuid);
                }
            }

            // reconnect the connected actions
            foreach (KeyValuePair<ActionBase, string> action in connectedActions)
            {
                action.Key.WrapAction = actionList.Find(a => a.Guid == action.Value);
            }
        }

        /// <summary>
        /// deserializes a single action
        /// </summary>
        /// <param name="actionNode">action node to load from</param>
        /// <param name="browsers">browser window list to indicate attachment</param>
        /// <returns>loaded action</returns>
        private static ActionBase DeserializeAction(XmlNode actionNode, Dictionary<string, BrowserWindow> browsers)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            if (actionNode.Attributes == null) return null;
            string className = actionNode.Attributes["Class"].Value;
            var windowNode = actionNode.SelectSingleNode("Window");
            string windowName = windowNode == null ? "" : windowNode.InnerText;
            BrowserWindow browser = browsers.ContainsKey(windowName)?browsers[windowName]:null;

            var actionUrlNode = actionNode.SelectSingleNode("ActionUrl");
            string actionUrl = actionUrlNode == null ? "" : actionUrlNode.InnerText;

            Type classType = asm.GetType(className);
            ConstructorInfo ci;
            ActionBase action;
            if (classType.IsSubclassOf(typeof (ActionElementBase)))
            {
                ci = classType.GetConstructor(new[] {typeof (BrowserWindow), typeof (IHTMLElement), typeof(string)});
                if (ci == null) return null;
                action = (ActionBase) ci.Invoke(new object[] {browser, null, actionUrl});
                var actionElement = (ActionElementBase) action;
                actionElement.ActionFinder = DeserializeFindAttributes(actionNode);
                actionElement.ActionFrames = DeserializeFrames(actionNode);
                actionElement.AllAttributes = DeserializeAllAttributes(actionNode);
            }
            else
            {
                ci = classType.GetConstructor(new[] {typeof (BrowserWindow)});
                if (ci == null) return null;
                action = (ActionBase)ci.Invoke(new object[] { browser });
            }

            PropertyInfo[] arrPropertyInfo = classType.GetProperties();

            foreach (PropertyInfo info in arrPropertyInfo)
            {
                SetParameterValue(actionNode, action, info);
            }

            return action;
        }

        /// <summary>
        /// set a single parameter's value
        /// </summary>
        /// <param name="node">node to load from</param>
        /// <param name="action">action to populate</param>
        /// <param name="info">propertyinfo object to get the value from</param>
        private static void SetParameterValue(XmlNode node, ActionBase action, PropertyInfo info)
        {
            if (!info.CanWrite) return;
            var singleNode = node.SelectSingleNode(info.Name);
            if (singleNode == null || singleNode.Attributes == null) return;
            string attributeType = singleNode.Attributes["type"].Value;
            Type conversionType = Type.GetType(attributeType);
            if (conversionType == null || !conversionType.IsSerializable) return;
            object newvalue = Convert.ChangeType(singleNode.InnerText, conversionType);
            info.SetValue(action, newvalue, null);
        }

        private static NameValueCollection DeserializeAllAttributes(XmlNode actionNode)
        {
            var allattributes = new NameValueCollection();
            XmlNode attributeCollectionNode = actionNode.SelectSingleNode("AllAttributeCollection");
            if (attributeCollectionNode != null)
                foreach (XmlNode node in attributeCollectionNode.ChildNodes)
                {
                    allattributes.Add(node.Name, node.InnerText);
                }
            return allattributes;
        }

        private static List<FindAttributeCollection> DeserializeFrames(XmlNode actionNode)
        {
            var frames = new List<FindAttributeCollection>();
            XmlNode frameCollectionNode = actionNode.SelectSingleNode("FrameCollection");
            if (frameCollectionNode == null) return frames;
            foreach (XmlNode frameNode in frameCollectionNode.ChildNodes)
            {
                DeserializeFindAttributes(frameNode);
            }
            return frames;
        }

        /// <summary>
        /// deserialize the list of FindAttributes
        /// </summary>
        /// <param name="actionNode">action node to get the attributes for</param>
        /// <returns>filled collection of FindAttributes</returns>
        private static FindAttributeCollection DeserializeFindAttributes(XmlNode actionNode)
        {
            XmlNode findCollectionNode = actionNode.SelectSingleNode("FindAttributeCollection");
            if (findCollectionNode == null) return null;

            XmlNode actionUrlNode = findCollectionNode.SelectSingleNode("ActionUrl");
            string actionUrl = actionUrlNode == null ? "" : actionUrlNode.InnerText;

            var collection = (from XmlNode childNode in findCollectionNode.SelectNodes("FindAttribute")
                              let xmlAttributeCollection = childNode.Attributes
                              where xmlAttributeCollection != null
                              let name = xmlAttributeCollection["FindName"].Value
                              let regex = xmlAttributeCollection["Regex"].Value
                              select new FindAttribute(name, childNode.InnerText, regex == "1")).ToList();
            string tagname = findCollectionNode.Attributes != null ? findCollectionNode.Attributes["TagName"].Value : "";
            return new FindAttributeCollection {AttributeList = collection, ActionUrl = actionUrl, TagName = tagname};
        }

        /// <summary>
        /// deserialize a single window's information
        /// </summary>
        /// <param name="windowNode">parent window node</param>
        /// <returns>filled window object</returns>
        private static BrowserWindow DeserializeWindowInfo(XmlNode windowNode)
        {
            var window = new BrowserWindow {InternalName = "window" + windowNode.GetHashCode()};
            XmlAttributeCollection attributes = windowNode.Attributes;
            if (attributes != null) window.InternalName = attributes["name"].Value;
            XmlNode urlNode = windowNode.SelectSingleNode("Url");
            if (urlNode != null) window.InitialUrl = urlNode.InnerText;
            XmlNode titleNode = windowNode.SelectSingleNode("Title");
            if (titleNode != null) window.WindowTitle = titleNode.InnerText;
            return window;
        }
    }
}