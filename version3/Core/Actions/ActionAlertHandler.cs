namespace TestRecorder.Core.Actions
{
    public class ActionAlertHandler : ActionBase
    {
        public override string ActionName { get { return "AlertHandler"; } }
        public static string IconFilename { get { return "AlertHandler.bmp"; } }
        internal override string Description { get { return "Alert Handler"; } }
        public ActionAlertHandler(BrowserWindow window) : base(window){}
    }
}