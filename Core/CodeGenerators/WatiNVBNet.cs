using System.Text;

namespace TestRecorder.Core.CodeGenerators
{
    public class WatiNVBNet : WatiNBase
    {
        public WatiNVBNet(CodeTemplate template) : base(template) { }

        public override string ClassCreateToString(string pageClass, string classVariable, string browserClass, params object[] constructorParameters)
        {
            var builder = new StringBuilder();
            foreach (object constructorParameter in constructorParameters)
            {
                builder.Append(",\"" + constructorParameter + "\"");
            }
            builder.Remove(0, 1);
            string cmd = string.Format("Dim {1} as {0}(new {2}({3}));", pageClass, classVariable, browserClass, builder);
            return cmd;
        }

        public override string SetElementToString(string elementVariable, string elementValue)
        {
            return elementVariable + " = " + elementValue;
        }

        public override string AlertDialog()
        {
            return @"UseDialogOnce(new AlertHandler()) begin";
        }
    }
}
