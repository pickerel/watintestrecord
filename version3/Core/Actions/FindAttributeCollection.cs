using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using IfacesEnumsStructsClasses;

namespace TestRecorder.Core.Actions
{
    public class FindAttributeCollection
    {
        /// <summary>
        /// gets list of attributes and should only be used for testing
        /// </summary>
        public List<FindAttribute> AttributeList
        {
            get { return _attributes; }
            set { _attributes = value; }
        }

        internal IHTMLElement ActionElement;

        public string ActionUrl { get; set; }

        public string TagName
        {
            get { return ActionElement == null ? _tagName : GetTagName(ActionElement); }
            set { SetTagName(value); }
        }
        private string _tagName;

        /// <summary>
        /// private list of attributes
        /// </summary>
        private List<FindAttribute> _attributes = new List<FindAttribute>();

        /// <summary>
        /// private ordered list of attributes to find
        /// </summary>
        private List<string> _localPattern;

        /// <summary>
        /// constructor to populate find mechanism
        /// </summary>
        /// <param name="element">element to describe</param>
        /// <param name="findPattern">optional ordered attribute list. should be for testing only</param>
        /// <param name="url">url this is to be found on</param>
        public FindAttributeCollection(IHTMLElement element=null, List<string> findPattern=null, string url=null)
        {
            ActionUrl = url;
            if (element == null) return;
            GetFindAttributes(element, findPattern);
        }

        /// <summary>
        /// method to get find attributes. This both populates the instance list 
        /// and returns the instance list when used to find the frame
        /// </summary>
        /// <param name="element">element to find parent of</param>
        /// <param name="findPattern">optional ordered attribute list. should be for testing only</param>
        /// <returns>instance list of attributes</returns>
        public List<FindAttribute> GetFindAttributes(IHTMLElement element = null, List<string> findPattern = null)
        {
            if (element==null) return null;

            ActionElement = element;

            _localPattern = findPattern ?? Settings.GetFindPattern();

            // get the entire available list
            var availableAttributes = GetAvailableAttributes(element);

            // evaluate in order
            foreach (var patternAttribute in _localPattern)
            {
                if (string.IsNullOrEmpty(availableAttributes[patternAttribute])) continue;
                var attribute = new FindAttribute(patternAttribute, availableAttributes[patternAttribute]);
                _attributes.Add(attribute);
            }

            var table = CreateAttributeTable((IHTMLDocument3)element.document, element.tagName, availableAttributes);

            // add items until there is only 1            
            for (int includeIdx = 1; includeIdx<table.Columns.Count-1; includeIdx++)
            {
                if (IsolateAttribute(includeIdx, availableAttributes, table)) break;
            }

            return _attributes;
        }

        /// <summary>
        /// private method to create a DataTable populated with matching tags to make it easier to find attributes in common
        /// </summary>
        /// <param name="containingDocument">document to search for elements</param>
        /// <param name="tagName">tag name to find</param>
        /// <param name="availableAttributes">attributes available for comparison</param>
        /// <returns>DataTable with a list of attributes</returns>
        private DataTable CreateAttributeTable(IHTMLDocument3 containingDocument, string tagName, NameValueCollection availableAttributes)
        {
            var table = new DataTable();
            _attributes.ForEach(attr => table.Columns.Add(attr.FindName));
            var possibleList = (IHTMLElementCollection) containingDocument.getElementsByTagName(tagName);
            foreach (IHTMLElement possibleElement in possibleList)
            {
                DataRow row = table.NewRow();
                foreach (string attributeKey in availableAttributes.Keys)
                {
                    object objValue = possibleElement.getAttribute(attributeKey, 0);
                    if (objValue != null
                        && table.Columns[attributeKey] != null
                        && !string.IsNullOrEmpty(objValue.ToString()))
                        row[attributeKey] = objValue.ToString();
                }
                table.Rows.Add(row);
            }

            // remove columns where all values are the same
            var deleteColumns = (from DataColumn column in table.Columns
                                 let query = column.ColumnName + "='" + table.Rows[0][column.ColumnName] + "'"
                                 let arrRows = table.Select(query)
                                 where arrRows.Length == table.Rows.Count
                                 select column.ColumnName).ToList();

            // if the deletion would remove all columns, return what we have
            if (table.Columns.Count == deleteColumns.Count)
                return table;

            foreach (string deleteColumn in deleteColumns)
            {
                table.Columns.Remove(deleteColumn);
                _attributes.Remove(_attributes.Find(attr => attr.FindName == deleteColumn));
            }

            return table;
        }

        /// <summary>
        /// Creates a name-value collection of available attributes
        /// </summary>
        /// <param name="element">element to evaluate</param>
        /// <returns>NameValueCollection of available attributes actually listed in the HTML</returns>
        public NameValueCollection GetAvailableAttributes(IHTMLElement element)
        {
            var nvcAvailableAttributes = new NameValueCollection();
            IHTMLAttributeCollection elementAttributes = ((IHTMLDOMNode)element).attributes;
            foreach (IHTMLDOMAttribute attribute in elementAttributes)
            {
                // only get the attributes that are really part of the HTML
                if (!element.outerHTML.Contains(attribute.nodeName + "=")) continue;
                string value = attribute.nodeValue != null ? attribute.nodeValue.ToString() : null;
                if (string.IsNullOrEmpty(value)) continue;
                nvcAvailableAttributes.Add(attribute.nodeName, value);
                if (!_localPattern.Contains(attribute.nodeName)
                    && !attribute.nodeName.Contains("-"))
                    _localPattern.Add(attribute.nodeName);
            }

            if (element.innerText != null)
            {
                _localPattern.Add("Text");
                nvcAvailableAttributes.Add("Text", element.innerText);
            }

            return nvcAvailableAttributes;
        }

        /// <summary>
        /// Evaluates the Attribute for it's ability to exclude other tag records in the table
        /// </summary>
        /// <param name="includeIdx"></param>
        /// <param name="availableAttributes"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        private bool IsolateAttribute(int includeIdx, NameValueCollection availableAttributes, DataTable table)
        {
            var sbQuery = new StringBuilder();
            var isolatedList = new List<FindAttribute>();

            // build a string to query with the remaining items
            for (int queryidx = 0; queryidx < includeIdx; queryidx++)
            {
                string columnName = table.Columns[queryidx].ColumnName;
                sbQuery.Append(" and " + columnName + "='" + availableAttributes[columnName] + "'");
                isolatedList.Add(new FindAttribute(columnName, availableAttributes[columnName]));
            }
            sbQuery = sbQuery.Remove(0, 5);
            DataRow[] arrRows = table.Select(sbQuery.ToString());

            // if we have only one, then our work is done
            if (arrRows.Length == 1)
            {
                _attributes = isolatedList;
                return true;
            }
            return false;
        }

        private string GetTagName(IHTMLElement element)
        {
            string tagName = element.tagName.ToLower();
            if (tagName == "input")
            {
                object typeobject = element.getAttribute("type", 0);
                tagName = typeobject != null ? typeobject.ToString() : "TextField";
            }
            tagName = UppercaseFirstLetter(tagName);
            return tagName;
        }

        private string UppercaseFirstLetter(string value)
        {
            return char.ToUpper(value[0]) + value.Substring(1).ToLower();
        }

        private void SetTagName(string tagName)
        {
            _tagName = tagName;
        }

        public string FriendlyName
        {
            get
            {
                if (string.IsNullOrEmpty(_friendlyName))
                    _friendlyName = GetFriendlyName();
                return _friendlyName;
            }
            set { _friendlyName = value; }
        }
        private string _friendlyName;

        /// <summary>
        /// creates a friendly (albeit silly) name for the element
        /// </summary>
        /// <returns></returns>
        private string GetFriendlyName()
        {
            var builder = new StringBuilder(TagName.ToLower());

            if (_attributes.Count > 0)
            {
                int strLength = _attributes[0].FindValue.Length > 15 ? 15 : _attributes[0].FindValue.Length;
                builder.Append(UppercaseFirstLetter(_attributes[0].FindValue.Substring(0,strLength)));
            }

            return builder.ToString();
        }

        /// <summary>
        /// creates a friendly description for this collection
        /// </summary>
        /// <returns>string description of the collection</returns>
        public string GetDescription()
        {
            var builder = new StringBuilder();
            foreach (var attribute in _attributes)
            {
                builder.Append(attribute.FindName + "=" + attribute.FindValue + ", ");                
            }
            builder = builder.Remove(builder.Length - 2, 2);
            return builder.ToString();
        }

        /// <summary>
        /// clears the attribute list and takes the parameter as the new list
        /// </summary>
        /// <param name="nvcAttributes">new list of attributes to set</param>
        public void SetAttributes(NameValueCollection nvcAttributes)
        {
            _attributes.Clear();
            foreach (string attributekey in nvcAttributes)
            {
                _attributes.Add(new FindAttribute(attributekey, nvcAttributes[attributekey]));
            }
        }

        public bool KeyExists(string key)
        {
            return _attributes.Exists(attribute => attribute.FindName == key);
        }
    }
}
