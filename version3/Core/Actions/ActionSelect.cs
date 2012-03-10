using IfacesEnumsStructsClasses;

namespace TestRecorder.Core.Actions
{
    public class ActionSelect : ActionElementBase
    {
        public static string ActionName { get { return "Select List"; } }
        public bool ByValue { get; set; }
        public string SelectedValue { get; set; }
        public string SelectedText { get; set; }
        public static string IconFilename { get { return "SelectList.bmp"; } }

        internal override string Description
        {
            get
            {
                return string.Format(ByValue
                           ? "Select {0}, by value " + SelectedValue
                           : "Select {0}, by text " + SelectedText,ActionFinder.GetDescription());
            }
        }

        public ActionSelect(BrowserWindow window, IHTMLElement actionElement = null, string url=null) 
            : base(window, actionElement, url)
        {
            if (actionElement != null) GetSelected();
        }

        public void GetSelected()
        {
            var sel = ActionFinder.ActionElement as IHTMLSelectElement;
            if (sel == null) return;
            for (int i = 0; i < sel.length; i++)
            {
                var op = sel.item(i, i) as IHTMLOptionElement;
                if (op != null && op.selected)
                {
                    SelectedValue = op.value;
                    SelectedText = op.text;
                    break;
                }
            }
        }
    }
}
