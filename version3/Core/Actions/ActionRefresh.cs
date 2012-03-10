namespace TestRecorder.Core.Actions
{
    public class ActionRefresh : ActionBase
    {
        public static string ActionName { get { return "Refresh"; } }
        public static string IconFilename { get { return "Refresh.bmp"; } }
        internal override string Description { get { return "Browser Refresh"; } }
        public ActionRefresh(BrowserWindow window) : base(window) { }

    }
}