using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using TestRecorder.Core;
using TestRecorder.Core.Actions;
using IfacesEnumsStructsClasses;
using NUnit.Framework;

namespace RecorderTest
{
    [TestFixture]
    public class SerializeActionHelperTest
    {
        private IHTMLElement GetInputElement(string tagName)
        {
            var wb = new WebBrowser();
            var strm = new MemoryStream();
            wb.DocumentStream = strm;

            if (wb.Document == null) return null;
            HtmlDocument doc = wb.Document.OpenNew(true);
            string html = File.ReadAllText(@"C:\Work\TestRecorder3\tests\html\main.html");
            if (doc != null) doc.Write(html);

            HtmlElementCollection collection = wb.Document != null ? wb.Document.GetElementsByTagName(tagName) : null;
            if (collection != null) return (IHTMLElement) collection[0].DomElement;
            return null;
        }

        private string WriteClickActionToFile()
        {
            const string filename = @"C:\Work\TestRecorder3\tests\output_click.xml";
            var action = new ActionClick(new BrowserWindow("window"), GetInputElement("input"));
            SerializeActionHelper.SerializeActionListToFile(new List<ActionBase> {action}, filename);
            return filename;
        }

        private string WriteTextActionToFile()
        {
            const string filename = @"C:\Work\TestRecorder3\tests\output_typing.xml";
            var action = new ActionTypeText(new BrowserWindow("window"), GetInputElement("input"));
            SerializeActionHelper.SerializeActionListToFile(new List<ActionBase> {action}, filename);
            return filename;
        }

        private string WriteNavigateActionToFile()
        {
            const string filename = @"C:\Work\TestRecorder3\tests\output_navigate.xml";
            var action = new ActionNavigate(new BrowserWindow("window")) {Url = "http://www.google.com"};
            SerializeActionHelper.SerializeActionListToFile(new List<ActionBase> {action}, filename);
            return filename;
        }

        private string WriteSelectActionToFile()
        {
            const string filename = @"C:\Work\TestRecorder3\tests\output_select.xml";
            var action = new ActionSelect(new BrowserWindow("window"), GetInputElement("select"));
            SerializeActionHelper.SerializeActionListToFile(new List<ActionBase> {action}, filename);
            return filename;
        }

        private string WriteSleepActionToFile()
        {
            const string filename = @"C:\Work\TestRecorder3\tests\output_sleep.xml";
            var action = new ActionSleep(null) {Miliseconds = 5000};
            SerializeActionHelper.SerializeActionListToFile(new List<ActionBase> {action}, filename);
            return filename;
        }

        private string WriteAlertHandlerToFile()
        {
            const string filename = @"C:\Work\TestRecorder3\tests\output_alerthandler.xml";
            var clickAction = new ActionClick(new BrowserWindow("window"), GetInputElement("input"));
            var alertAction = new ActionAlertHandler(new BrowserWindow("window")) { WrapAction = clickAction };
            SerializeActionHelper.SerializeActionListToFile(new List<ActionBase> { clickAction, alertAction }, filename);
            return filename;
        }

        private XmlNode CheckSingleBaseSerial(string outputFile, Type intendedType, bool hasWindow = true)
        {
            Assert.IsTrue(File.Exists(outputFile), "Output File does not exist");

            var actionNodes = LoadFile(outputFile);
            Assert.AreEqual(1, actionNodes.Count, "action node list is not 1");

            XmlNode actionNode = actionNodes[0];
            Assert.IsNotNull(actionNode.Attributes, "No attributes on node");
            Assert.AreEqual(intendedType.ToString(), actionNode.Attributes["Class"].Value);

            if (hasWindow)
            {
                XmlNode windowNode = (actionNode.SelectSingleNode("Window"));
                Assert.IsNotNull(windowNode, "No window element");
                Assert.AreEqual("window", windowNode.InnerText);
            }

            return actionNode;
        }

        private static XmlNodeList LoadFile(string outputFile)
        {
            var doc = new XmlDocument();
            doc.Load(outputFile);
            XmlNode root = doc.DocumentElement;
            Assert.IsNotNull(root, "document element not found");

            XmlNodeList actionNodes = root.SelectNodes("ActionList/Action");
            Assert.IsNotNull(actionNodes, "list of action nodes is null");
            return actionNodes;
        }

        private void CheckSingleElementSerial(string outputFile, Type intendedType, string tagName, FindAttribute finder)
        {
            XmlNode actionNode = CheckSingleBaseSerial(outputFile, intendedType);

            XmlNode findCollectionNode = (actionNode.SelectSingleNode("FindAttributeCollection"));
            Assert.IsNotNull(findCollectionNode, "find collection node not found");
            Assert.IsNotNull(findCollectionNode.Attributes);
            Assert.IsNotNull(findCollectionNode.Attributes["TagName"]);
            Assert.AreEqual(tagName, findCollectionNode.Attributes["TagName"].Value);

            XmlNodeList findAttributeList = findCollectionNode.SelectNodes("FindAttribute");
            Assert.IsNotNull(findAttributeList, "list of action nodes is null");
            Assert.AreEqual(1, findAttributeList.Count, "find node list is not 1");

            XmlNode findAttribute = findAttributeList[0];
            Assert.IsNotNull(findAttribute, "find attribute is null");
            Assert.IsNotNull(findAttribute.Attributes, "findAttribute.Attributes == null");
            Assert.AreEqual(finder.FindName, findAttribute.Attributes["FindName"].Value);
            Assert.AreEqual(finder.FindValue, findAttribute.InnerText);
        }


        [Test]
        public void SerializeSingleClickElementAction()
        {
            string filename = WriteClickActionToFile();
            CheckSingleElementSerial(filename, typeof(ActionClick), "Text", new FindAttribute("id", "name"));
        }

        [Test]
        public void SerializeSingleSelectElementAction()
        {
            string filename = WriteSelectActionToFile();
            CheckSingleElementSerial(filename, typeof(ActionSelect), "Select", new FindAttribute("id", "Select1"));
        }

        [Test]
        public void SerializeSingleTypingElementAction()
        {
            string filename = WriteTextActionToFile();
            CheckSingleElementSerial(filename, typeof(ActionTypeText), "Text", new FindAttribute("id", "name"));
        }

        [Test]
        public void SerializeSingleNavigateAction()
        {
            string filename = WriteNavigateActionToFile();
            XmlNode actionNode = CheckSingleBaseSerial(filename, typeof(ActionNavigate));
            XmlNode urlNode = actionNode.SelectSingleNode("Url");
            Assert.IsNotNull(urlNode, "Url node not written");
            Assert.AreEqual("http://www.google.com",urlNode.InnerText);
        }

        [Test]
        public void SerializeSingleSleepAction()
        {
            string filename = WriteSleepActionToFile();
            XmlNode actionNode = CheckSingleBaseSerial(filename, typeof(ActionSleep), false);
            XmlNode milisecondNode = actionNode.SelectSingleNode("Miliseconds");
            Assert.IsNotNull(milisecondNode, "Miliseconds node not written");
            Assert.AreEqual("5000", milisecondNode.InnerText);
        }

        [Test]
        public void SerializeAlertHandlerAction()
        {
            string filename = WriteAlertHandlerToFile();
            XmlNodeList actionNodeList = LoadFile(filename);
            Assert.AreEqual(2, actionNodeList.Count, "did not find 2 actions");
            XmlNode connectionNode = actionNodeList[1].SelectSingleNode("Connection");
            Assert.IsNotNull(connectionNode, "Connection node not written");
            Assert.IsNotEmpty(connectionNode.InnerText, "Connection node is blank");
        }

// ReSharper disable UnusedParameter.Local
        private ActionBase CheckSingleBaseDeserial(string filename, Type intendedType, bool hasWindow=true)
// ReSharper restore UnusedParameter.Local
        {
            Assert.IsTrue(File.Exists(filename), "Output File does not exist");
            List<ActionBase> actionList;
            Dictionary<string, BrowserWindow> windowList;
            SerializeActionHelper.DeserializeActionListFromFile(filename, out actionList, out windowList);
            Assert.AreEqual(1, actionList.Count, "Action list is not 1");

            Assert.IsTrue(intendedType == actionList[0].GetType(), "Action is not click");
            if (hasWindow)
            {
                Assert.IsNotNull(actionList[0].ActionWindow, "window is null");
                Assert.AreEqual("window", actionList[0].ActionWindow.InternalName, "internal window name not 'window'");
            }
            return actionList[0];
        }

        private ActionElementBase CheckSingleElementDeserial(string filename, Type intendedType, string tagName, FindAttribute finder)
        {
            var firstAction = (ActionElementBase) CheckSingleBaseDeserial(filename, intendedType);
            Assert.AreEqual(tagName, firstAction.ActionFinder.TagName, "Tag name not text");
            Assert.AreEqual(1, firstAction.ActionFinder.AttributeList.Count, "attribute list count is not 1");
            FindAttribute firstAttribute = firstAction.ActionFinder.AttributeList[0];
            Assert.AreEqual(finder.FindName, firstAttribute.FindName, "name is not 'id'");
            Assert.AreEqual(finder.FindValue, firstAttribute.FindValue, "value is not 'name'");
            return firstAction;
        }

        [Test]
        public void DeserializeSingleClickElement()
        {
            string filename = WriteClickActionToFile();
            var action = (ActionClick) CheckSingleElementDeserial(filename, typeof(ActionClick), "Text", new FindAttribute("id","name"));
            Assert.AreEqual(false, action.NoWait);
        }

        [Test]
        public void DeserializeSingleSelectElement()
        {
            string filename = WriteSelectActionToFile();
            var action = (ActionSelect)CheckSingleElementDeserial(filename, typeof(ActionSelect), "Select", new FindAttribute("id", "Select1"));
            Assert.AreEqual(false, action.ByValue);
            Assert.AreEqual("1",action.SelectedValue);
            Assert.AreEqual("First text",action.SelectedText);
        }

        [Test]
        public void DeserializeSingleTypeElement()
        {
            string filename = WriteTextActionToFile();
            var action = (ActionTypeText)CheckSingleElementDeserial(filename, typeof(ActionTypeText), "Text", new FindAttribute("id", "name"));
            Assert.AreEqual(false, action.NoWait);
        }

        [Test]
        public void DeserializeSingleNavigate()
        {
            string filename = WriteNavigateActionToFile();
            var action = (ActionNavigate)CheckSingleBaseDeserial(filename, typeof(ActionNavigate));
            Assert.AreEqual("http://www.google.com", action.Url);
        }

        [Test]
        public void DeserializeSingleSleep()
        {
            string filename = WriteSleepActionToFile();
            var action = (ActionSleep) CheckSingleBaseDeserial(filename, typeof (ActionSleep), false);
            Assert.AreEqual(5000, action.Miliseconds);
        }

        [Test]
        public void DeserializeAlertHandler()
        {
            string filename = WriteAlertHandlerToFile();
            Assert.IsTrue(File.Exists(filename), "Output File does not exist");
            List<ActionBase> actionList;
            Dictionary<string, BrowserWindow> windowList;
            SerializeActionHelper.DeserializeActionListFromFile(filename, out actionList, out windowList);
            Assert.AreEqual(2, actionList.Count, "Action list is not 2");
            ActionBase firstClick = actionList.Find(a => a.GetType() == typeof(ActionClick));
            ActionBase firstAlert = actionList.Find(a => a.GetType() == typeof (ActionAlertHandler));
            Assert.AreEqual(firstClick, firstAlert.WrapAction, "action not connected");
        }
    }
}
