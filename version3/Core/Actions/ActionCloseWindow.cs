namespace TestRecorder.Core.Actions
{
    public class ActionCloseWindow : ActionBase
    {
        public override string ActionName { get { return "Close Window"; } }
        public static string IconFilename { get { return "CloseWindow.bmp"; } }
        internal override string Description { get { return "Close Window"; } }
        public ActionCloseWindow(BrowserWindow window) : base(window) { }

    }
}