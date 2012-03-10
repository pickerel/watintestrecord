using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TestRecorder.Core.Actions;
using IfacesEnumsStructsClasses;
using NUnit.Framework;

namespace RecorderTest
{
    [TestFixture]
    public class FindAttributeCollectionTest
    {
        private WebBrowser _wb;

        [SetUp]
        public void Setup()
        {
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

        [Test]
        public void DetermineFinderById()
        {
            HtmlElementCollection collection = GetInputElements();
            var element = (IHTMLElement)collection[0].DomElement;

            var findCollection = new FindAttributeCollection(element);
            Assert.AreEqual(1, findCollection.AttributeList.Count, "Find List count not working");
            Assert.AreEqual("id", findCollection.AttributeList[0].FindName);
        }

        [Test]
        public void DetermineFinderForNoNameOrId()
        {
            HtmlElementCollection collection = GetInputElements();

            // get the first hidden element
            var hiddenElement = collection.Cast<HtmlElement>().FirstOrDefault(element => element.GetAttribute("type") == "hidden");

            Assert.IsNotNull(hiddenElement, "hiddenElement == null");
            var findCollection = new FindAttributeCollection((IHTMLElement) hiddenElement.DomElement);
            Assert.AreEqual(2, findCollection.AttributeList.Count, "More attributes than expected");
            Assert.AreEqual("value",findCollection.AttributeList[0].FindName,"Not found on value");
        }

        [Test]
        public void DetermineFinderByInnerText()
        {
            HtmlElementCollection collection = _wb.Document.GetElementsByTagName("a");
            var element = (IHTMLElement)collection[1].DomElement;

            var findCollection = new FindAttributeCollection(element);
            Assert.AreEqual(5, findCollection.AttributeList.Count, "Find List count not working");
            Assert.That(findCollection.AttributeList.Exists(a=>a.FindName=="Text"));
        }

        [Test]
        public void FindOnDuplicateName()
        {
            HtmlElementCollection collection = GetInputElements();

            // get the first hidden element
            var radioElement = collection.Cast<HtmlElement>().FirstOrDefault(element => element.GetAttribute("type") == "radio");

            Assert.IsNotNull(radioElement, "radioElement == null");

            // set the pattern to exclude id
            var pattern = new List<string> { "name", "title", "href", "url", "src", "value", "style", "text" };
            var findCollection = new FindAttributeCollection((IHTMLElement)radioElement.DomElement, pattern);

            Assert.That(findCollection.AttributeList.Count>=2, "Different attribute count than expected");
            Assert.IsNotNull(findCollection.AttributeList.Find(a=>a.FindName=="name"), "does not contain name attribute");
            Assert.IsNotNull(findCollection.AttributeList.Find(a => a.FindName == "value"), "does not contain value attribute");
        }

        [Test]
        public void UnavoidableDuplicateTakesFirst()
        {
            // test file should be just two tags that look exactly the same
            Assert.That(_wb.Document != null, "_wb.Document == null");
            HtmlDocument doc = _wb.Document.OpenNew(true);
            string html = File.ReadAllText(@"C:\Work\TestRecorder3\tests\html\UnavoidableDuplicates.html");
            if (doc != null) doc.Write(html);

            HtmlElementCollection collection = GetInputElements();

            int counter = collection.Cast<HtmlElement>().Count(element => element.GetAttribute("type") == "hidden" && element.GetAttribute("value") == "second");
            Assert.AreEqual(2,counter,"Test not set up properly-- need two tag matches");

            var hiddenElement = collection.Cast<HtmlElement>().FirstOrDefault(element => element.GetAttribute("type") == "hidden" && element.GetAttribute("value") == "second");

            Assert.IsNotNull(hiddenElement, "hiddenElement == null");
            var findCollection = new FindAttributeCollection((IHTMLElement)hiddenElement.DomElement);

            Assert.That(findCollection.AttributeList.Count==2, "Different attribute count than expected");
            Assert.IsNotNull(findCollection.AttributeList.Find(a => a.FindName == "type"), "does not contain type attribute");
            Assert.IsNotNull(findCollection.AttributeList.Find(a => a.FindName == "value"), "does not contain value attribute");
        }

        [Test]
        public void GetFriendlyNameShort()
        {
            var findCollection = new FindAttributeCollection {TagName = "span"};
            findCollection.AttributeList.Add(new FindAttribute("name","itemname"));
            Assert.That(findCollection.FriendlyName=="spanItemname");
        }

        [Test]
        public void GetFriendlyNameLong()
        {
            var findCollection = new FindAttributeCollection { TagName = "span" };
            findCollection.AttributeList.Add(new FindAttribute("name", "thisIsAVeryLongNameThatKeepsOnGoing"));
            Assert.AreEqual(findCollection.FriendlyName, "spanThisisaverylong");
        }

        [Test]
        public void GetDescription()
        {
            HtmlElementCollection collection = GetInputElements();
            var element = (IHTMLElement)collection[0].DomElement;
            var findCollection = new FindAttributeCollection(element);
            findCollection.AttributeList.Add(new FindAttribute("value","321"));
            Assert.That(findCollection.GetDescription()=="id=name, value=321");
        }
    }
}
