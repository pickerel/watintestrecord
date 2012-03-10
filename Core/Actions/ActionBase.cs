namespace TestRecorder.Core.Actions
{
    public class ActionBase
    {
        /// <summary>
        /// Guid to identify this action on reload
        /// </summary>
        public string Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }
        private string _guid = System.Guid.NewGuid().ToString();

        /// <summary>
        /// window this action will act on
        /// </summary>
        public BrowserWindow ActionWindow { get; set; }

        /// <summary>
        /// action this one wraps, like in dialog handlers
        /// </summary>
        public ActionBase WrapAction { get; set; }

        internal virtual string Description { get; set; }

        /// <summary>
        /// simple constructor to set the window
        /// </summary>
        /// <param name="window">window object to set</param>
        public ActionBase(BrowserWindow window)
        {
            ActionWindow = window;
        }
    }
}
