namespace TestRecorder.Core.Actions
{
    public class ActionNavigate : ActionBase
    {
        public override string ActionName { get { return "Navigate"; } }
        public static string IconFilename { get { return "Navigate.bmp"; } }
        internal override string Description { get { return string.Format("Go To {0}", Url); } }
        public string Url { get; set; }
        public ActionNavigate(BrowserWindow window) : base(window) { }

    }
}