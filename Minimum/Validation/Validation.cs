using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Minimum
{
    public class Validation
    {
        #region [ Privates ]
        private IList<ValidationResult> _resultList = null;
        #endregion

        #region [ Constructor ]
        private Validation()
        {
            _resultList = new List<ValidationResult>();
        }
        #endregion

        #region [ Functions ]
        public static Validation Validate(object objectToValidate, string property)
        {
            ValidationContext context = new ValidationContext(objectToValidate, serviceProvider: null, items: null);

            context.MemberName = property;

            Validation result = new Validation();
            result.IsValid = Validator.TryValidateProperty(objectToValidate.GetType().GetProperty(property).GetValue(objectToValidate), context, result._resultList);

            return result;
        }

        public static Validation Validate(object objectToValidate, bool recursive = false)
        {
            Validation result = new Validation();

            if (recursive)
            {
                result.IsValid = Validate(result, objectToValidate);
            }
            else
            {
                ValidationContext context = new ValidationContext(objectToValidate, serviceProvider: null, items: null);

                result.IsValid = Validator.TryValidateObject(objectToValidate, context, result._resultList, true);
            }

            return result;
        }

        private static bool Validate(Validation validation, object objectToValidate)
        {
            ValidationContext context = new ValidationContext(objectToValidate, serviceProvider: null, items: null);

            bool status = Validator.TryValidateObject(objectToValidate, context, validation._resultList, true);

            PropertyInfo[] properties = objectToValidate.GetType().GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                if (Attribute.GetCustomAttribute(properties[i], typeof(IgnoreValidation)) != null) { continue; }

                if (properties[i].PropertyType.IsGenericType && properties[i].PropertyType.GetGenericTypeDefinition().Equals(typeof(IList<>)) || typeof(IList).IsAssignableFrom(properties[i].PropertyType))
                {
                    object list = properties[i].GetValue(objectToValidate);
                    if (list != null)
                    {
                        for (int j = 0; j < (list as IList).Count; j++)
                        {
                            if (status == true) { status = Validate(validation, (list as IList)[j]); }
                            else { Validate(validation, (list as IList)[j]); }
                        }
                    }
                }
                else if (properties[i].PropertyType.IsClass && !properties[i].PropertyType.Equals(typeof(System.String)) && !properties[i].PropertyType.Equals(typeof(System.Object)))
                {
                    object child = properties[i].GetValue(objectToValidate);
                    if (child != null)
                    {
                        if (status == true) { status = Validate(validation, child); }
                        else { Validate(validation, child); }
                    }
                }
            }

            return status;
        }

        public string GetConcatenatedMessage()
        {
            string error = null;
            for (int i = 0; i < _resultList.Count; i++)
            {
                error += error == null ? _resultList[i].ErrorMessage : "\n" + _resultList[i].ErrorMessage;
            }

            return error;
        }

        public string[] GetErrors()
        {
            if (_resultList.Count == 0) { return null; }

            string[] errors = new string[_resultList.Count];
            for (int i = 0; i < _resultList.Count; i++)
            {
                errors[i] = _resultList[i].ErrorMessage;
            }

            return errors;
        }
        #endregion

        #region [ Properties ]
        public bool IsValid { get; private set; }
        public int Count { get { return _resultList.Count; } }
        public string this[int index] { get { return _resultList[index].ErrorMessage; } }
        #endregion
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class IgnoreValidation : Attribute { }
}