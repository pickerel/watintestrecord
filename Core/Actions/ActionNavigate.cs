namespace TestRecorder.Core.Actions
{
    public class ActionNavigate : ActionBase
    {
        public static string ActionName { get { return "Navigate"; } }
        public static string IconFilename { get { return "Navigate.bmp"; } }
        internal override string Description { get { return "Go To {0}"; } }
        public string Url { get; set; }
        public ActionNavigate(BrowserWindow window) : base(window) { }

    }
}