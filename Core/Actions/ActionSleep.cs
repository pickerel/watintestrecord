namespace TestRecorder.Core.Actions
{
    public class ActionSleep : ActionBase
    {
        public int Miliseconds { get; set; }
        public static string ActionName { get { return "Sleep"; } }
        public static string IconFilename { get { return "Sleep.bmp"; } }
        internal override string Description { get { return "Sleep"; } }
        public ActionSleep(BrowserWindow window) : base(window) { }

    }
}