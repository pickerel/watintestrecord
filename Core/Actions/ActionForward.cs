namespace TestRecorder.Core.Actions
{
    public class ActionForward : ActionBase
    {
        public override string ActionName { get { return "Forward"; } }
        public static string IconFilename { get { return "Forward.bmp"; } }
        internal override string Description { get { return "Browser Forward"; } }
        public ActionForward(BrowserWindow window) : base(window) { }

    }
}