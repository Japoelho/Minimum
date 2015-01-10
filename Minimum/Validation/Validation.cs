using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Minimum
{
    public class Validation
    {
        #region [ Privates ]
        private IList<ValidationResult> _resultList = null;
        #endregion

        #region [ Constructor ]
        public Validation()
        {
            _resultList = new List<ValidationResult>();
        }
        #endregion

        #region [ Functions ]
        public bool IsValid(object objectToValidate)
        {
            ValidationContext context = new ValidationContext(objectToValidate, serviceProvider: null, items: null);
            _resultList.Clear();

            return Validator.TryValidateObject(objectToValidate, context, _resultList, true); ;
        }
        
        public string GetMessage(int index) { return _resultList[index].ErrorMessage; }
        #endregion

        #region [ Properties ]
        public int Count { get { return _resultList.Count; } }
        public string this[int index] { get { return _resultList[index].ErrorMessage; } }
        #endregion
    }
}