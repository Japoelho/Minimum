using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Minimum
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class MinimumItems : ValidationAttribute
    {
        private readonly int _minimumElements;

        public MinimumItems(int minimumElements)
        {
            _minimumElements = minimumElements;
        }

        public override bool IsValid(object value)
        {
            IList list = value as IList;
            if (list != null)
            {
                return list.Count >= _minimumElements;
            }

            return false;
        }
    }
}
