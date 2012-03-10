using System;
using System.Reflection;
using System.Globalization;
using IfacesEnumsStructsClasses;

namespace TestRecorder
{    
    public class HtmlEventProxy : IDisposable,IReflect
    {
        // Fields
        private readonly EventHandler _eventHandler;
        private readonly object _sender;
        private readonly IReflect _typeIReflectImplementation;
        private IHTMLElement2 _htmlElement;
        private readonly string _eventName;

        // private CTOR
        private HtmlEventProxy(string eventName, IHTMLElement2 htmlElement, EventHandler eventHandler)
        {
            _eventName = eventName;
            _htmlElement = htmlElement;
            _sender = this;
            _eventHandler = eventHandler;
            Type type = typeof(HtmlEventProxy);
            _typeIReflectImplementation = type;
        }

        public static HtmlEventProxy Create(string eventName,object htmlElement, EventHandler eventHandler)
        {
            var elem = (IHTMLElement2)htmlElement;

            var newProxy = new HtmlEventProxy(eventName,elem, eventHandler);
            elem.attachEvent(eventName, newProxy);
            return newProxy;            
        }

        /// <summary>
        /// detach only once (thread safe)
        /// </summary>
        public void Detach()
        {
            lock (this)
            {
                if (_htmlElement != null)
                {
                    var elem = _htmlElement;
                    elem.detachEvent(_eventName, this);
                    _htmlElement = null;
                }
            }
        }                

        /// <summary>
        /// HtmlElemet property  
        /// </summary>
        public IHTMLElement2 HtmlElement
        {
            get
            {
                return _htmlElement;
            }
        }

        #region IReflect

        FieldInfo IReflect.GetField(string name, BindingFlags bindingAttr)
        {
            return _typeIReflectImplementation.GetField(name, bindingAttr);
        }

        FieldInfo[] IReflect.GetFields(BindingFlags bindingAttr)
        {
            return _typeIReflectImplementation.GetFields(bindingAttr);
        }

        MemberInfo[] IReflect.GetMember(string name, BindingFlags bindingAttr)
        {
            return _typeIReflectImplementation.GetMember(name, bindingAttr);
        }

        MemberInfo[] IReflect.GetMembers(BindingFlags bindingAttr)
        {
            return _typeIReflectImplementation.GetMembers(bindingAttr);
        }

        MethodInfo IReflect.GetMethod(string name, BindingFlags bindingAttr)
        {
            return _typeIReflectImplementation.GetMethod(name, bindingAttr);
        }

        MethodInfo IReflect.GetMethod(string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
        {
            return _typeIReflectImplementation.GetMethod(name, bindingAttr, binder, types, modifiers);
        }

        MethodInfo[] IReflect.GetMethods(BindingFlags bindingAttr)
        {
            return _typeIReflectImplementation.GetMethods(bindingAttr);
        }

        PropertyInfo[] IReflect.GetProperties(BindingFlags bindingAttr)
        {
            return _typeIReflectImplementation.GetProperties(bindingAttr);
        }

        PropertyInfo IReflect.GetProperty(string name, BindingFlags bindingAttr)
        {
            return _typeIReflectImplementation.GetProperty(name, bindingAttr);
        }

        PropertyInfo IReflect.GetProperty(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            return _typeIReflectImplementation.GetProperty(name, bindingAttr, binder, returnType, types, modifiers);
        }

        object IReflect.InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            if (name == "[DISPID=0]")
            {
                if (_eventHandler != null)
                {
                    _eventHandler(_sender, EventArgs.Empty);
                }                
            }

            return null; 
        }



        Type IReflect.UnderlyingSystemType
        {
            get
            {
                return _typeIReflectImplementation.UnderlyingSystemType;
            }
        }

        #endregion
        
        #region IDisposable Members

        public void Dispose()
        {
            Detach();  
        }

        #endregion
    }
}
