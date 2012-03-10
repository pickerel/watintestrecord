using System.IO;
using System.Windows.Forms;
using TestRecorder.Core;
using TestRecorder.Core.Actions;
using IfacesEnumsStructsClasses;
using NUnit.Framework;

namespace RecorderTest
{
    [TestFixture]
    public class ScriptFactoryManagerTest
    {
        private ScriptFactoryManager _mgr;
        private WebBrowser _wb;

        [SetUp]
        public void Setup()
        {
            _mgr = new ScriptFactoryManager();
            _mgr.AddBrowserWindow("normalWindow");
            _wb = new WebBrowser();
            var strm = new MemoryStream();
            _wb.DocumentStream = strm;

            if (_wb.Document == null) return;
            HtmlDocument doc = _wb.Document.OpenNew(true);
            string html = File.ReadAllText(@"C:\Work\TestRecorder3\tests\html\main.html");
            if (doc != null) doc.Write(html);
        }

        private HtmlElementCollection GetInputElements()
        {
            return _wb.Document != null ? _wb.Document.GetElementsByTagName("input") : null;
        }

        private HtmlElementCollection GetSelectElements()
        {
            return _wb.Document != null ? _wb.Document.GetElementsByTagName("select") : null;
        }



        [Test]
        public void SimpleClickToAddAction()
        {
            AddClick();

            Assert.AreEqual(1,_mgr.ActionCount,"Action Not Added");
        }

        private ActionBase AddClick()
        {
            HtmlElementCollection collection = GetInputElements();
            var element = (IHTMLElement) collection[0].DomElement;
            return _mgr.AddClick("normalWindow", element, "");
        }

        [Test]
        public void SelectToAddAction()
        {
            HtmlElementCollection collection = GetSelectElements();

            var element = (IHTMLSelectElement)collection[0].DomElement;
            element.selectedIndex = 1;
            var action = _mgr.AddSelect("normalWindow", element, "");

            Assert.AreEqual(1, _mgr.ActionCount, "Action Not Added");
            Assert.AreEqual("Second text", action.SelectedText, "Text value did not match");
            Assert.AreEqual("tweede tekst", action.SelectedValue, "Text value did not match");
        }

        [Test]
        public void TextToAddAction()
        {
            HtmlElementCollection collection = GetInputElements();
            collection = collection.GetElementsByName("textinput1");

            var element = (IHTMLInputElement)collection[0].DomElement;
            element.value = "Testing Text";
            
            var action = _mgr.AddText("normalWindow", element, "");

            Assert.AreEqual(1, _mgr.ActionCount, "Action Not Added");
            Assert.AreEqual("Testing Text", action.TextToType, "Text value did not match");
        }

        [Test]
        public void AlertHandlerAdd()
        {
            var actionClick = (ActionClick) AddClick();
            ActionAlertHandler actionAlertHandler = _mgr.AddAlertHandler("normalWindow");

            Assert.AreEqual(2, _mgr.ActionCount, "Actions Not Added");
            Assert.AreEqual(actionClick, actionAlertHandler.WrapAction, "Alert handler not connected");
        }

        [Test]
        public void AlertHandlerRemovedWhenConnectedIsRemoved()
        {
            var actionClick = (ActionClick) AddClick();
            ActionAlertHandler actionAlertHandler = _mgr.AddAlertHandler("normalWindow");

            Assert.AreEqual(2, _mgr.ActionCount, "Actions Not Added");
            Assert.AreEqual(actionClick, actionAlertHandler.WrapAction, "Alert handler not connected");

            _mgr.RemoveAction(actionClick);
            Assert.AreEqual(0, _mgr.ActionCount, "actions not removed");
        }
    }
}
