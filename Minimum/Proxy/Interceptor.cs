using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Minimum.Proxy
{
    public class Interceptor
    {        
        private IList<Execute> _functions;

        public Interceptor()
        {
            _functions = new List<Execute>();            
        }

        internal void Add(Execute execute)
        {
            _functions.Add(execute);
        }

        public void InterceptBefore(string methodName, object target, object[] args)
        {
            IList<Execute> functions = _functions.Where(f => f.MethodName == methodName && f.When == When.Before).ToList();

            for (int i = 0; i < functions.Count; i++) { functions[i].Before.Invoke(target, args); }
            for (int i = functions.Count - 1; i >= 0; i--) { if (functions[i].Run == Run.Once) { _functions.Remove(functions[i]); } }
        }

        public void InterceptAfter(string methodName, object target, object[] args, ref object result)
        {
            IList<Execute> functions = _functions.Where(f => f.MethodName == methodName && f.When == When.After).ToList();

            for (int i = 0; i < functions.Count; i++) { result = functions[i].After.Invoke(target, args, result); }
            for (int i = functions.Count - 1; i >= 0; i--) { if (functions[i].Run == Run.Once) { _functions.Remove(functions[i]); } }
        }
    }

    internal class Execute
    {
        public string MethodName { get; set; }
        public Func<object, object[], object> Before { get; set; }
        public Func<object, object[], object, object> After { get; set; }
        public Run Run { get; set; }
        public When When { get; set; }
    }

    public enum Run
    { Once, Always }

    public enum When
    { Before, After }    
}
