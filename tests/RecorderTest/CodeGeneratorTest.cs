using System.Collections.Generic;
using NUnit.Framework;
using TestRecorder.Core.Actions;
using TestRecorder.Core.CodeGenerators;

namespace RecorderTest
{
    [TestFixture]
    public class CodeGeneratorTest
    {
        const string Path = @"C:\Work\TestRecorder3\templates\CSharpNUnit.trt";

        [Test]
        public void LoadATemplate()
        {
            Assert.IsTrue(System.IO.File.Exists(Path), "No template file-- path may not be correct");
            var template = new CodeTemplate(Path);
            Assert.AreEqual("WatiN", template.DriverLibrary);
            Assert.AreEqual(false, template.CanRun);
            Assert.AreEqual("*.cs", template.FileExtension);
        }

        [Test]
        public void GetAllAvailableTemplates()
        {
            string findpath = System.IO.Path.GetDirectoryName(Path);
            List<CodeTemplate> templateList = CodeGenerator.GetAvailableTemplates("", findpath);
            Assert.That(2<templateList.Count, "Less than 2 templates found, path may be incorrect");
            CodeTemplate nunitTemplate = templateList.Find(t => t.CodeLanguage == "C#" && t.DriverLibrary == "WatiN");
            Assert.IsNotNull(nunitTemplate, "template not found");
        }

        [Test]
        public void SaveWithNoCode()
        {
            var generator = new WatiNCSharp(new CodeTemplate(""));
            generator.SaveCodeToFile(@"C:\Work\TestRecorder3\tests\NoCode.cs",new List<ActionBase>());
        }
    }
}
