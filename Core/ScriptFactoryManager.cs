using System.Collections.Generic;
using IfacesEnumsStructsClasses;
using TestRecorder.Core.Actions;
using TestRecorder.Core.CodeGenerators;

namespace TestRecorder.Core
{
    /// <summary>
    /// this class hides the implementation details from the interface
    /// </summary>
    public class ScriptFactoryManager
    {
        /// <summary>
        /// list of actions performed by the user
        /// </summary>
        private List<ActionBase> _actionList = new List<ActionBase>();

        /// <summary>
        /// list of browser windows used in actions
        /// </summary>
        private Dictionary<string, BrowserWindow> _browsers = new Dictionary<string, BrowserWindow>();

        /// <summary>
        /// count of actions in list (mainly for testing)
        /// </summary>
        public int ActionCount
        {
            get { return _actionList.Count; }
        }

        public delegate void RemovedActionEvent(ActionBase action, int index);
        public RemovedActionEvent OnActionRemoved;
        public delegate void AddedActionEvent(ActionBase action, int index);
        public AddedActionEvent OnActionAdded;

        /// <summary>
        /// loads actions from file
        /// </summary>
        /// <param name="filename">filename to load from</param>
        public void LoadActionsFromFile(string filename)
        {
            SerializeActionHelper.DeserializeActionListFromFile(filename, out _actionList, out _browsers);
            int index = 0;
            foreach (ActionBase action in _actionList)
            {
                OnActionAdded(action, index++);
            }
        }

        /// <summary>
        /// saves actions to file
        /// </summary>
        /// <param name="filename">filename to save</param>
        public void SaveActionsToFile(string filename)
        {
            SerializeActionHelper.SerializeActionListToFile(_actionList, filename);
        }

        /// <summary>
        /// generate code to file
        /// </summary>
        /// <param name="template">template object to use</param>
        /// <param name="filename">filename to save</param>
        public void SaveCodeToFile(CodeTemplate template, string filename)
        {
            CodeGenerator generator = CodeGenerator.GetGenerator(template);
            generator.SaveCodeToFile(filename, _actionList);
        }

        /// <summary>
        /// Adds a click action to the action list
        /// </summary>
        /// <param name="windowName">window where it was performed</param>
        /// <param name="activeElement">element the click was performed on</param>
        /// <param name="url">current url for action</param>
        /// <returns>action created (mainly for testing)</returns>
        public ActionBase AddClick(string windowName, IHTMLElement activeElement, string url)
        {
            ActionBase action = null;
            if (activeElement == null) return null;

            if (activeElement is IHTMLSelectElement
                || activeElement is IHTMLTextAreaElement
                || (activeElement is IHTMLInputElement && (activeElement as IHTMLInputElement).type == "text"))
            {
                action = new ActionFocus(_browsers[windowName], activeElement, url);
            }
            else
            {
                action = new ActionClick(_browsers[windowName], activeElement, url);
            }

            AddAction(action);
            return action;
        }

        /// <summary>
        /// Adds a selectlist action to the action list
        /// </summary>
        /// <param name="windowName">window where it was performed</param>
        /// <param name="activeElement">element the click was performed on</param>
        /// <param name="url">current url for action</param>
        /// <returns>action created (mainly for testing)</returns>
        public ActionSelect AddSelect(string windowName, IHTMLSelectElement activeElement, string url)
        {
            var action = new ActionSelect(_browsers[windowName], (IHTMLElement)activeElement, url);
            AddAction(action);
            return action;
        }

        /// <summary>
        /// Adds a typing action to the action list
        /// </summary>
        /// <param name="windowName">window where it was performed</param>
        /// <param name="activeElement">element the click was performed on</param>
        /// <param name="url">current url for action</param>
        /// <returns>action created (mainly for testing)</returns>
        public ActionTypeText AddText(string windowName, IHTMLInputElement activeElement, string url)
        {
            var action = new ActionTypeText(_browsers[windowName], (IHTMLElement)activeElement, url);
            AddAction(action);
            return action;
        }

        /// <summary>
        /// Adds a navigation action to the action list
        /// </summary>
        /// <param name="windowName">window where it was performed</param>
        /// <param name="url">url being navigated to</param>
        /// <returns>action created (mainly for testing)</returns>
        public ActionNavigate AddNavigate(string windowName, string url)
        {
            var action = new ActionNavigate(_browsers[windowName]) { Url = url };
            AddAction(action);
            return action;
        }

        /// <summary>
        /// *INSERTS* an alert handler in the action list, just before the last action
        /// </summary>
        /// <param name="windowName">window where it was performed</param>
        /// <returns>action created (mainly for testing)</returns>
        public ActionAlertHandler AddAlertHandler(string windowName)
        {
            var action = new ActionAlertHandler(_browsers[windowName])
                             {WrapAction = _actionList[_actionList.Count - 1]};
            AddAction(action, _actionList.Count - 1);
            return action;
        }

        /// <summary>
        /// Adds a backward navigation to the action list
        /// </summary>
        /// <param name="windowName">window where it was performed</param>
        /// <returns>action created (mainly for testing)</returns>
        public ActionBackward AddBackward(string windowName)
        {
            var action = new ActionBackward(_browsers[windowName]);
            AddAction(action);
            return action;
        }

        /// <summary>
        /// Adds a forward navigation to the action list
        /// </summary>
        /// <param name="windowName">window where it was performed</param>
        /// <returns>action created (mainly for testing)</returns>
        public ActionForward AddForward(string windowName)
        {
            var action = new ActionForward(_browsers[windowName]);
            AddAction(action);
            return action;
        }

        /// <summary>
        /// Adds a refresh to the action list
        /// </summary>
        /// <param name="windowName">window where it was performed</param>
        /// <returns>action created (mainly for testing)</returns>
        public ActionRefresh AddRefresh(string windowName)
        {
            var action = new ActionRefresh(_browsers[windowName]);
            AddAction(action);
            return action;
        }

        /// <summary>
        /// Adds sleep to the action list
        /// </summary>
        /// <param name="miliseconds">number of seconds to sleep</param>
        /// <returns>action created (mainly for testing)</returns>
        public ActionSleep AddSleep(int miliseconds)
        {
            var action = new ActionSleep(null) { Miliseconds = miliseconds };
            AddAction(action);
            return action;
        }

        /// <summary>
        /// Adds a browser window to the internal dictionary list
        /// </summary>
        /// <param name="internalWindowName">internal name of the window, used as the hash</param>
        /// <param name="url">url when the window opens</param>
        /// <param name="windowTitle">title of the window</param>
        public void AddBrowserWindow(string internalWindowName, string url="", string windowTitle="")
        {
            _browsers.Add(internalWindowName, new BrowserWindow(internalWindowName, url, windowTitle));
        }

        /// <summary>
        /// Adds a refresh to the action list
        /// </summary>
        /// <param name="windowName">window where it was performed</param>
        /// <returns>action created (mainly for testing)</returns>
        public ActionBase AddCloseWindow(string windowName)
        {
            var action = new ActionCloseWindow(_browsers[windowName]);
            AddAction(action);
            return action;
        }

        /// <summary>
        /// Removes an action from the list, also removing any connected action
        /// </summary>
        /// <param name="action">action to remove</param>
        public void RemoveAction(ActionBase action)
        {
            if (action == null) return;
            int index = _actionList.IndexOf(action);
            _actionList.Remove(action);
            ActionBase connectedAction = _actionList.Find(a => a.WrapAction == action);
            if (connectedAction != null)
                RemoveAction(connectedAction);

            OnActionRemoved(action, index);
        }

        /// <summary>
        /// Adds an action to the list, also adding any connected action
        /// </summary>
        /// <param name="action">action to add</param>
        /// <param name="index">index to add at</param>
        public void AddAction(ActionBase action, int index=-1)
        {
            if (index==-1)
            {
                _actionList.Add(action);
                index = _actionList.Count-1;
            }
            else _actionList.Insert(index, action);

            if (action.WrapAction != null)
                _actionList.Insert(index, action.WrapAction);

            OnActionAdded(action, index);
        }

        /// <summary>
        /// Removes all the actions from the list
        /// </summary>
        public void ClearActions()
        {
            while (_actionList.Count > 0)
            {
                RemoveAction(_actionList[0]);
            }
        }

        /// <summary>
        /// move an action up on the list
        /// </summary>
        /// <param name="action">action to move</param>
        /// <returns>new index position</returns>
        public int MoveUp(ActionBase action)
        {
            int currentIndex = _actionList.IndexOf(action);
            RemoveAction(action);
            AddAction(action, currentIndex-1);
            return currentIndex - 1;
        }

        /// <summary>
        /// move an action down on the list
        /// </summary>
        /// <param name="action">action to move</param>
        public int MoveDown(ActionBase action)
        {
            int currentIndex = _actionList.IndexOf(action);
            RemoveAction(action);
            AddAction(action, currentIndex + 1);
            return currentIndex + 1;
        }

        /// <summary>
        /// updates the browser window title, just for cleanliness
        /// </summary>
        /// <param name="name">unique name of the window</param>
        /// <param name="title">title to set</param>
        public void UpdateBrowserWindow(string name, string title)
        {
            _browsers[name].WindowTitle = title;
        }
    }
}
