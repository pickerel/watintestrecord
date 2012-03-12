using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TestRecorder.Core.Actions;

namespace TestRecorder.Core.CodeGenerators
{
    public abstract class CodeGenerator
    {
        /// <summary>
        /// current template in use
        /// </summary>
        internal CodeTemplate Template;
        /// <summary>
        /// list of created properties to be written
        /// </summary>
        public List<ScriptProperty> Properties = new List<ScriptProperty>();
        /// <summary>
        /// list of code lines to be written
        /// </summary>
        public List<string> Code = new List<string>();
        /// <summary>
        /// browser type indicator
        /// </summary>
        public BrowserTypes BrowserType { get; set; }

        /// <summary>
        /// list of friendly names already being used
        /// </summary>
        private readonly List<FindAttributeCollection> _usedFriendlyNames = new List<FindAttributeCollection>(); 

        protected CodeGenerator(CodeTemplate template)
        {
            Template = template;
        }

        /// <summary>
        /// static method to retrieve a generator using the template
        /// </summary>
        /// <param name="template">template used to find generator</param>
        /// <returns>CodeGenerator object</returns>
        public static CodeGenerator GetGenerator(CodeTemplate template)
        {
            CodeGenerator generator = null;

            if (template.DriverLibrary == "SODA")
            {
                generator = new SodaBase(template);
            }
            else if (template.CodeLanguage == "Ruby")
            {
                if (template.DriverLibrary == "Celerity")
                    generator = new RubyCelerity(template);
                else if (template.DriverLibrary == "WatiR")
                    generator = new RubyWatiR(template);
            }
            else if (template.DriverLibrary == "WatiN")
            {
                if (template.CodeLanguage == "C#")
                    generator = new WatiNCSharp(template);
                else if (template.CodeLanguage == "VB.Net")
                    generator = new WatiNCSharp(template);
            }
            if (generator == null)
            {
                throw new FileNotFoundException("No mapping found for generator template");
            }
            return generator;
        }

        public virtual string FileCodeWrapping(string code)
        {
            return code;
        }
        public abstract void ActionToCode(ActionBase action);
        public abstract string GetPropertyAttributeString(FindAttributeCollection finder);
        internal abstract string GetPropertyType(string elementType);
        internal abstract string GetFrames(List<FindAttributeCollection> frames);
        public abstract string CreateBrowser(string windowName, BrowserTypes browser = BrowserTypes.IE, string initialUrl = "");

        /// <summary>
        /// What a surprise, it calls the generator to do code replacements, saving the code to file(s)
        /// </summary>
        /// <param name="filename">filename of the main code file. If the template separates the properties, the property file is saved by the window name</param>
        /// <param name="actionList">list of actions to generate code on</param>
        /// <param name="browserType">browser type to use</param>
        public void SaveCodeToFile(string filename, List<ActionBase> actionList, BrowserTypes browserType = BrowserTypes.IE)
        {
            var codeBuilder = new StringBuilder();

            BrowserType = browserType;

            foreach (ActionBase action in actionList)
            {
                ActionToCode(action);
            }

            // do the code page replacement
            Code.ForEach(line => codeBuilder.AppendLine(line));
            string codePage = Regex.Replace(Template.CodePageTemplate, "TESTCODE", codeBuilder.ToString().Trim());

            // now for the properties
            var pageBuilder = new StringBuilder();
            codeBuilder.Length = 0;
            Properties.Sort((a, b) => String.CompareOrdinal(a.WindowName, b.WindowName));
            string lastWindow = Properties.Count > 0 ? Properties[0].WindowName : "";
            string leadingSpace = Regex.Match(Template.PropertyPageTemplate, @"([ \t]+)PROPERTYCODE", RegexOptions.IgnoreCase).Groups[1].Value;
            foreach (ScriptProperty scriptProperty in Properties)
            {
                if (scriptProperty.WindowName != lastWindow
                    && Template.PropertiesInSeparateFile)
                {
                    WritePropertyPage(filename, lastWindow, codeBuilder.ToString());
                    lastWindow = scriptProperty.WindowName;
                    codeBuilder.Clear();
                }
                else if (scriptProperty.WindowName != lastWindow)
                {
                    pageBuilder.AppendLine(GetPropertyClass(lastWindow, codeBuilder.ToString()));
                    lastWindow = scriptProperty.WindowName;
                    codeBuilder.Clear();
                }

                if (codeBuilder.Length > 0) codeBuilder.Append(leadingSpace);
                codeBuilder.AppendLine(scriptProperty.PropertyCode);
            }

            // treat the last property class
            if (Template.PropertiesInSeparateFile)
            {
                WritePropertyPage(filename, lastWindow, codeBuilder.ToString());
            }
            else 
            {
                pageBuilder.AppendLine(GetPropertyClass(lastWindow, codeBuilder.ToString()));
            }

            if (Template.PropertiesInSeparateFile)
            {
                WritePropertyPage(filename, lastWindow, codeBuilder.ToString());
            }
            else
            {
                codePage = Regex.Replace(codePage, "PAGECODE", pageBuilder.ToString());                
            }

            codePage = FileCodeWrapping(codePage);
            File.WriteAllText(filename, codePage);
        }

        private string GetPropertyClass(string windowName, string propertyCode)
        {
            string property = Regex.Replace(Template.PropertyPageTemplate, "PROPERTYCODE", propertyCode.TrimEnd());
            property = Regex.Replace(property, "WINDOWNAME", "Page" + windowName);
            return property;
        }

        /// <summary>
        /// writes a single property page to a file
        /// </summary>
        /// <param name="codeFilename">filename of the code file, used to get the path and file extension</param>
        /// <param name="windowName">name of the window for writing, used as filename</param>
        /// <param name="code">actual code to write to disk</param>
        private void WritePropertyPage(string codeFilename, string windowName, string code)
        {
            string property = Regex.Replace(Template.PropertyPageTemplate, "PROPERTYCODE", code);
            property = Regex.Replace(property, "WINDOWNAME", "Page"+windowName);
            if (codeFilename != null)
            {
                string propertyFilename = Path.Combine(Path.GetDirectoryName(codeFilename),
                                                       windowName + Path.GetExtension(codeFilename));            
                File.WriteAllText(propertyFilename, property);
            }
        }

        /// <summary>
        /// static method to enumerate available templates
        /// </summary>
        /// <param name="fileExtension">extension to find in templates. *.* is treated as blank</param>
        /// <param name="templatepath">path to look for template files</param>
        /// <returns>list of CodeTemplate objects matching the inputs</returns>
        public static List<CodeTemplate> GetAvailableTemplates(string fileExtension="", string templatepath = "")
        {
            if (fileExtension == "*.*") fileExtension = "";
            string path = Path.GetDirectoryName(Application.ExecutablePath);
            if (path != null) path = Path.Combine(path, "templates");
            if (!string.IsNullOrEmpty(templatepath)) path = templatepath; 

            var templateList = new List<CodeTemplate>();
            if (!Directory.Exists(path)) return templateList;
            
            string[] arrFiles = Directory.GetFiles(path, "*.trt");

            Exception lastException = null;
            string errorFile = "";
            foreach (string templatefile in arrFiles)
            {
                try
                {
                    var template = new CodeTemplate(templatefile);
                    if (template.FileExtension.ToLower() == fileExtension.ToLower() || fileExtension == "")
                        templateList.Add(template);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    errorFile = Path.GetFileName(templatefile);
                }
            }

            if (lastException != null)
            {
                MessageBox.Show("At least one of the templates to be loaded had an error.\r\nThe last error was on file " + errorFile + "\r\n\r\nThe error was:\r\n\r\n" + lastException.Message + "\r\n\r\nUntil the issue is corrected, this template cannot be loaded.\r\nTo stop getting this error, rename the file extension or delete the template.",
                                "Template Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return templateList;
        }

        /// <summary>
        /// ensures that the friendly name is unique among the properties
        /// </summary>
        /// <param name="finder">attribute collection to create the name from</param>
        /// <returns>verified friendly name</returns>
        internal string GetUniqueFriendlyName(FindAttributeCollection finder)
        {
            string friendlyName = finder.FriendlyName;
            string verifiedName = friendlyName;
            int counter = 1;

            while (_usedFriendlyNames.Exists(n => n.FriendlyName == verifiedName))
            {
                verifiedName = friendlyName + (counter++);
            }
            _usedFriendlyNames.Add(finder);
            return verifiedName;
        }

        /// <summary>
        /// given an element, it creates a property string for writing
        /// </summary>
        /// <param name="friendlyName">friendly name of the element</param>
        /// <param name="action">action object used to get the element's finder and window name</param>
        internal void ElementToProperty(string friendlyName, ActionElementBase action)
        {
            string elementConstraintString = GetPropertyAttributeString(action.ActionFinder);

            // check for duplicate properties
            if (Properties.Exists(p => p.Finder.TagName == action.ActionFinder.TagName
                                   && p.Finder.ActionUrl == action.ActionFinder.ActionUrl
                                   && GetPropertyAttributeString(p.Finder) == elementConstraintString))
                return;

            string propertyType = GetPropertyType(action.ActionFinder.TagName);
            string frames = GetFrames(action.ActionFrames);

            string property = Template.PropertyTemplate;
            property = Regex.Replace(property, "FRAMES\\.", frames); // replacing the dot too
            property = Regex.Replace(property, "ELEMENTTYPE", propertyType);
            property = Regex.Replace(property, "ELEMENTNAME", friendlyName);
            property = Regex.Replace(property, "ELEMENTDESCRIPTION", action.ActionFinder.GetDescription());
            property = Regex.Replace(property, "ELEMENTFINDCOLLECTION", GetPropertyAttributeString(action.ActionFinder));
            Properties.Add(new ScriptProperty(action.ActionWindow.InternalName, property, action.ActionFinder));
        }
    }
}
