using System;
using DocumentFormat.OpenXml.Packaging;

namespace Minimum.Files
{
    public class Excel
    {
        private SpreadsheetDocument _document = null;
        private bool _isOpen = false;

        public bool IsOpen { get { return _isOpen; } }
        public string ErrorMessage { get; private set; }

        public Excel()
        {
            _isOpen = false;
        }

        public Excel(string filePath)
        {
            _isOpen = false;
            Open(filePath);
        }

        public bool Open(string filePath)
        {
            Close();

            try
            {
                _document = SpreadsheetDocument.Open(filePath, true);
                _isOpen = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                _isOpen = false;
            }

            return _isOpen;
        }

        public void Close()
        {
            if(_isOpen == true)
            {
                _document.Dispose();
                _document = null;
                _isOpen = false;
            }
        }
    }
}