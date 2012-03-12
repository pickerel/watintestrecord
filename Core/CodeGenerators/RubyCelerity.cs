
namespace TestRecorder.Core.CodeGenerators
{
    public class RubyCelerity : RubyBase
    {
        public RubyCelerity(CodeTemplate template) : base(template) {}

        /// <summary>
        /// Creates a browser for the code page
        /// </summary>
        /// <param name="windowName">name of the browser window</param>
        /// <param name="browser">type of browser</param>
        /// <param name="initialUrl">initial URL, if provided</param>
        /// <returns>browser creation code string</returns>
        public override string CreateBrowser(string windowName, BrowserTypes browser = BrowserTypes.IE, string initialUrl = "")
        {
            string code = ClassCreateToString("Page" + windowName, windowName, "Celerity::Browser");
            return code;
        }
    }
}
