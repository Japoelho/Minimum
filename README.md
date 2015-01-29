# Minimum
A collection of utils and tools for easier and faster development of my daily work.

- Connection

Contains a basic connection management class. Connection Factory, connection services, etc

- DataAccess

An object relational mapping.
Supports some custom queries for reports (in progress as I find more needs), custom mapping for your conventions, and creates a basic SCRUD from your classes. Has a default mapper that uses [Attribute] annotations.
Currently only supports T-SQL syntax.

- FileFormats

Still in progress. Will use OpenXml and iTextSharp for manipulation of common file types as .doc, .xls and .pdf.

- Loaders

Converters, loaders and a serializator.

- Proxy

Ready to use proxy on your classes, intercept any virtual method/property with any custom function. Needs more work to permanently save the interceptor functions, and to save dynamic libraries of the proxies for faster loads on release versions.

- Text

Contains some common text functions.

- Validation

Contains a validation from DataAnnotations for a general way to validate objects.

# Version 1.0
- First useable version. Trying it out on various projects, will be adding more features as I figure out the needs and problems.
- Working on the Javascript Common Library/Tools.
- Thinking on more useful tools/stuff that should be in here.
