namespace TestRecorder.Core
{
    public class BrowserWindow
    {
        public string InternalName = "";
        public string InitialUrl = "";
        public string WindowTitle = "";

        public BrowserWindow(string internalName="", string url="", string title="")
        {
            InternalName = internalName;
            InitialUrl = url;
            WindowTitle = title;
        }
    }
}
