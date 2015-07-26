using DocumentFormat.OpenXml.Packaging;
using System;
using System.IO;

namespace Minimum.Files
{
    public class Word : IDisposable
    {
        #region [ Fields ]
        private MemoryStream _memoryStream = null;
        private WordprocessingDocument _document = null;
        private string _fileName = null;
        private bool _isOpen = false;
        #endregion

        #region [ Properties ]
        public bool IsOpen { get { return _isOpen; } }
        public string ErrorMessage { get; private set; }
        //application/vnd.openxmlformats-officedocument.wordprocessingml.document
        public string ContentType { get { return "application/vnd.ms-word.document"; } }
        #endregion

        #region [ IDisposable ]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
        }
        #endregion

        #region [ Constructor ]
        public Word()
        {
            _isOpen = false;
        }

        public Word(string filePath)
        {
            _isOpen = false;
            Open(filePath);
        }
        #endregion

        #region [ Public ]
        public void Dummy()
        {
            Type t = typeof(WordprocessingDocument);
        }

        public bool Open(string filePath)
        {
            Close();

            try
            {
                _fileName = filePath;

                byte[] byteArray = File.ReadAllBytes(filePath);
                _memoryStream = new MemoryStream(byteArray);

                _document = WordprocessingDocument.Open(_memoryStream, true);
                //_document = WordprocessingDocument.Open(filePath, true, new OpenSettings() { AutoSave = false });
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

            if (_memoryStream != null)
            {
                _memoryStream.Dispose();
            }
        }

        public void Save(bool overwriteOriginal = false)
        {
            if (overwriteOriginal)
            {                
                using (WordprocessingDocument document = WordprocessingDocument.Open(_fileName, true, new OpenSettings() { AutoSave = false }))
                { _document.MainDocumentPart.Document.Save(document.MainDocumentPart); }
            }

            _document.MainDocumentPart.Document.Save();
        }

        public void Replace(string text, string replace)
        {
            string docText = null;
            using (StreamReader sReader = new StreamReader(_document.MainDocumentPart.GetStream()))
            {
                docText = sReader.ReadToEnd();
            }

            docText = docText.Replace(text, replace);

            using (StreamWriter sWriter = new StreamWriter(_document.MainDocumentPart.GetStream(FileMode.Create)))
            {
                sWriter.Write(docText);
            }
        }
        
        public void CopyToStream(Stream target)
        {
            long pos = _memoryStream.Position;
            
            _memoryStream.Position = 0;
            _memoryStream.CopyTo(target);
            _memoryStream.Position = pos;
        }
        #endregion
    }
}
