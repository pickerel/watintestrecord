namespace TestRecorder.Core.Actions
{
    public class ActionBackward : ActionBase
    {
        public static string ActionName { get { return "Backward"; } }
        public static string IconFilename { get { return "Backward.bmp"; } }
        internal override string Description { get { return "Browser Backward"; } }
        public ActionBackward(BrowserWindow window) : base(window) { }

    }
}