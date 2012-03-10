using System.IO;
using System.Linq;
using System.Windows.Forms;
using IfacesEnumsStructsClasses;
using NUnit.Framework;
using TestRecorder.Core;
using TestRecorder.Core.Actions;
using TestRecorder.Core.CodeGenerators;

namespace RecorderTest
{
    [TestFixture]
// ReSharper disable InconsistentNaming
    public class CodeGenCSTest
// ReSharper restore InconsistentNaming
    {
        private CodeTemplate GetNUnitTemplate()
        {
            const string path = @"C:\Work\TestRecorder3\templates\CSharpNUnit.trt";
            Assert.IsTrue(File.Exists(path), "No template file-- path may not be correct");
            var template = new CodeTemplate(path);
            return template;
        }

        internal class FakeActionToTestFailure : ActionBase
        {
            public FakeActionToTestFailure(BrowserWindow window) : base(window) {}
        }

        [Test]
        public void WriteFailingLine()
        {
            var generator = new WatiNCSharp(GetNUnitTemplate());
            var failingAction = new FakeActionToTestFailure(new BrowserWindow("window"));
            generator.ActionToCode(failingAction);

            Assert.AreEqual(1, generator.Code.Count, "different than 1 code line");
            Assert.That(generator.Code[0].StartsWith("//UNKNOWN CODE OBJECT"), "other than valid code");
        }

        [Test]
        public void WriteForward()
        {
            var generator = new WatiNCSharp(GetNUnitTemplate());
            var forward = new ActionForward(new BrowserWindow("window"));
            generator.ActionToCode(forward);

            Assert.AreEqual(1, generator.Code.Count, "different than 1 code line");
            Assert.That(generator.Code[0]=="window.Forward();", "other than valid code");
        }

        [Test]
        public void WriteBackward()
        {
            var generator = new WatiNCSharp(GetNUnitTemplate());
            var back = new ActionBackward(new BrowserWindow("window"));
            generator.ActionToCode(back);

            Assert.AreEqual(1, generator.Code.Count, "different than 1 code line");
            Assert.That(generator.Code[0] == "window.Back();", "other than valid code");
        }

        [Test]
        public void WriteRefresh()
        {
            var generator = new WatiNCSharp(GetNUnitTemplate());
            var refresh = new ActionRefresh(new BrowserWindow("window"));
            generator.ActionToCode(refresh);

            Assert.AreEqual(1, generator.Code.Count, "different than 1 code line");
            Assert.That(generator.Code[0] == "window.Refresh();", "other than valid code");
        }

        [Test]
        public void WriteCloseWindow()
        {
            var generator = new WatiNCSharp(GetNUnitTemplate());
            var closeWin = new ActionCloseWindow(new BrowserWindow("window"));
            generator.ActionToCode(closeWin);

            Assert.AreEqual(1, generator.Code.Count, "different than 1 code line");
            Assert.That(generator.Code[0] == "window.CloseWindow();", "other than valid code");
        }

        [Test]
        public void WriteNavigate()
        {
            var generator = new WatiNCSharp(GetNUnitTemplate());
            var nav = new ActionNavigate(new BrowserWindow("window")) {Url = "http://www.google.com"};
            generator.ActionToCode(nav);

            Assert.AreEqual(1, generator.Code.Count, "different than 1 code line");
            Assert.That(generator.Code[0] == "window.GoTo(\"http://www.google.com\");", "other than valid code");
        }

        [Test]
        public void WriteSleep()
        {
            var generator = new WatiNCSharp(GetNUnitTemplate());
            var sleep = new ActionSleep(null) {Miliseconds = 6000};
            generator.ActionToCode(sleep);

            Assert.AreEqual(1, generator.Code.Count, "different than 1 code line");
            Assert.That(generator.Code[0] == "Sleep(6000);", "other than valid code");
        }

        [Test]
        public void WriteDivClick()
        {
            var generator = new WatiNCSharp(GetNUnitTemplate());
            var clicker = new ActionClick(new BrowserWindow("window"))
                              {ActionFinder = new FindAttributeCollection()};
            clicker.ActionFinder.AttributeList.Add(new FindAttribute("id","div1"));
            clicker.ActionFinder.TagName = "Div";
            generator.ActionToCode(clicker);

            Assert.AreEqual(1, generator.Properties.Count, "different than 1 property");
            Assert.IsNotNull(generator.Properties[0], "property is blank");

            Assert.AreEqual(1, generator.Code.Count, "different than 1 code line");
            Assert.That(generator.Code[0] == "divDiv1.Click();", "other than valid code");
        }

        [Test]
        public void WriteLinkClick()
        {
            var generator = new WatiNCSharp(GetNUnitTemplate());
            var clicker = new ActionClick(new BrowserWindow("window")) { ActionFinder = new FindAttributeCollection() };
            clicker.ActionFinder.AttributeList.Add(new FindAttribute("id", "testlinkid"));
            clicker.ActionFinder.TagName = "a";
            generator.ActionToCode(clicker);

            Assert.AreEqual(1, generator.Properties.Count, "different than 1 property");
            Assert.IsNotNull(generator.Properties[0], "property is blank");
            Assert.That(generator.Properties[0].PropertyCode.Contains("_browser.Link(Find"), "Link code not working");

            Assert.AreEqual(1, generator.Code.Count, "different than 1 code line");
            Assert.That(generator.Code[0] == "aTestlinkid.Click();", "other than valid code");
        }

        [Test]
        public void WriteRadioClick()
        {
            var generator = new WatiNCSharp(GetNUnitTemplate());

            var wb = new WebBrowser();
            var strm = new MemoryStream();
            wb.DocumentStream = strm;

            if (wb.Document == null) return;
            HtmlDocument doc = wb.Document.OpenNew(true);
            string html = File.ReadAllText(@"C:\Work\TestRecorder3\tests\html\main.html");
            if (doc != null) doc.Write(html);

            HtmlElementCollection collection = wb.Document != null ? wb.Document.GetElementsByTagName("input") : null;
            if (collection != null)
            {
                var radio = collection.Cast<HtmlElement>().FirstOrDefault(element => element.GetAttribute("type") == "radio");

                if (radio != null)
                {
                    var radioElement = new ActionClick(new BrowserWindow("window"), (IHTMLElement) radio.DomElement);
                    generator.ActionToCode(radioElement);
                }
            }

            Assert.AreEqual(1, generator.Properties.Count, "different than 1 property");
            Assert.IsNotNull(generator.Properties[0], "property is blank");
            Assert.That(generator.Properties[0].PropertyCode.Contains("_browser.RadioButton(Find"), "Radio code not working");

            Assert.AreEqual(1, generator.Code.Count, "different than 1 code line");
            Assert.AreEqual("radioRadio1.Click();", generator.Code[0], "other than valid code");
        }

        [Test]
        public void WriteSelectByText()
        {
            var generator = new WatiNCSharp(GetNUnitTemplate());
            var select = new ActionSelect(new BrowserWindow("window"));
            select.ByValue = false;
            select.SelectedText = "2011";
            select.ActionFinder = new FindAttributeCollection {TagName = "select"};
            select.ActionFinder.AttributeList.Add(new FindAttribute("id", "years"));
            generator.ActionToCode(select);

            Assert.AreEqual(1, generator.Properties.Count, "different than 1 property");
            Assert.IsNotNull(generator.Properties[0], "property is blank");
            Assert.That(generator.Properties[0].PropertyCode.Contains("_browser.SelectList(Find"), "select code not working");

            Assert.AreEqual(1, generator.Code.Count, "different than 1 code line");
            Assert.That(generator.Code[0] == "selectYears.Select(\"2011\");", "other than valid code");
        }

        [Test]
        public void WriteSelectByValue()
        {
            var generator = new WatiNCSharp(GetNUnitTemplate());
            var select = new ActionSelect(new BrowserWindow("window"));
            select.ByValue = true;
            select.SelectedValue = "2011";
            select.ActionFinder = new FindAttributeCollection {TagName = "select"};
            select.ActionFinder.AttributeList.Add(new FindAttribute("id", "years"));
            generator.ActionToCode(select);

            Assert.AreEqual(1, generator.Properties.Count, "different than 1 property");
            Assert.IsNotNull(generator.Properties[0], "property is blank");
            Assert.That(generator.Properties[0].PropertyCode.Contains("_browser.SelectList(Find"), "select code not working");

            Assert.AreEqual(1, generator.Code.Count, "different than 1 code line");
            Assert.That(generator.Code[0] == "selectYears.SelectByValue(\"2011\");", "other than valid code");
        }

        [Test]
        public void WriteTypeTextOverwrite()
        {
            var generator = new WatiNCSharp(GetNUnitTemplate());
            var typing = new ActionTypeText(new BrowserWindow("window"))
                             {
                                 Overwrite = true,
                                 TextToType = "type this",
                                 ActionFinder = new FindAttributeCollection {TagName = "TextField"}
                             };
            typing.ActionFinder.AttributeList.Add(new FindAttribute("id", "txtType"));
            generator.ActionToCode(typing);

            Assert.AreEqual(1, generator.Properties.Count, "different than 1 property");
            Assert.IsNotNull(generator.Properties[0], "property is blank");
            Assert.That(generator.Properties[0].PropertyCode.Contains("_browser.TextField(Find"), "select code not working");

            Assert.AreEqual(1, generator.Code.Count, "different than 1 code line");
            Assert.That(generator.Code[0] == "textfieldTxttype = \"type this\";", "other than valid code");
        }

        [Test]
        public void WriteAlertHandler()
        {
            var generator = new WatiNCSharp(GetNUnitTemplate());
            var clicker = new ActionClick(new BrowserWindow("window")) { ActionFinder = new FindAttributeCollection() };
            clicker.ActionFinder.AttributeList.Add(new FindAttribute("id", "div1"));
            clicker.ActionFinder.TagName = "Div";

            var alerter = new ActionAlertHandler(new BrowserWindow("window")) {WrapAction = clicker};
            generator.ActionToCode(alerter);
            generator.ActionToCode(clicker);

            Assert.AreEqual(3, generator.Code.Count, "different than 2 code lines");
            Assert.AreEqual(generator.Code[0], "UseDialogOnce(new AlertHandler()){", "alert line other than valid code");
            Assert.AreEqual(generator.Code[1], "divDiv1.Click();", "action line other than valid code");
        }
        
        [Test]
        public void CheckUniqueProperty()
        {
            var generator = new WatiNCSharp(GetNUnitTemplate());
            var clicker1 = new ActionClick(new BrowserWindow("window")) { ActionFinder = new FindAttributeCollection() };
            clicker1.ActionFinder.AttributeList.Add(new FindAttribute("id", "div1"));
            clicker1.ActionFinder.TagName = "Div";
            generator.ActionToCode(clicker1);

            var clicker2 = new ActionClick(new BrowserWindow("window")) { ActionFinder = new FindAttributeCollection() };
            clicker2.ActionFinder.AttributeList.Add(new FindAttribute("id", "div1"));
            clicker2.ActionFinder.TagName = "Div";
            generator.ActionToCode(clicker2);

            Assert.AreEqual(2, generator.Code.Count, "different than 2 code lines");
            Assert.AreEqual(generator.Code[1], "divDiv11.Click();", "action line other than valid code");
        }

        [Test]
        public void DontDuplicateSameProperty()
        {
            var generator = new WatiNCSharp(GetNUnitTemplate());
            var clicker = new ActionClick(new BrowserWindow("window")) { ActionFinder = new FindAttributeCollection() };
            clicker.ActionFinder.AttributeList.Add(new FindAttribute("id", "div1"));
            clicker.ActionFinder.TagName = "Div";
            clicker.ActionFinder.ActionUrl = "http://www.fakeurl.com";
            generator.ActionToCode(clicker);
            generator.ActionToCode(clicker);

            Assert.AreEqual(1, generator.Properties.Count, "More than 1 property created");
        }

        [Test]
        public void CheckSinglePropertyAttributes()
        {
            var generator = new WatiNCSharp(GetNUnitTemplate());
            var clicker = new ActionClick(new BrowserWindow("window")) { ActionFinder = new FindAttributeCollection() };
            clicker.ActionFinder.AttributeList.Add(new FindAttribute("id", "div1"));
            clicker.ActionFinder.TagName = "Div";
            clicker.ActionFinder.ActionUrl = "http://www.fakeurl.com";
            string attribute = generator.GetPropertyAttributeString(clicker.ActionFinder);

            Assert.AreEqual("Find.ById(\"div1\")", attribute);
        }

        [Test]
        public void CheckMultiplePropertyAttributes()
        {
            var generator = new WatiNCSharp(GetNUnitTemplate());
            var clicker = new ActionClick(new BrowserWindow("window")) { ActionFinder = new FindAttributeCollection() };
            clicker.ActionFinder.AttributeList.Add(new FindAttribute("id", "div1"));
            clicker.ActionFinder.AttributeList.Add(new FindAttribute("name", "div1"));
            clicker.ActionFinder.TagName = "Div";
            clicker.ActionFinder.ActionUrl = "http://www.fakeurl.com";
            string attribute = generator.GetPropertyAttributeString(clicker.ActionFinder);

            Assert.AreEqual("Find.ById(\"div1\") && Find.ByName(\"div1\")", attribute);
        }
    }
}
