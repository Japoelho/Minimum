# Minimum
A collection of utils and tools for easier development.

- Connection

Contains a basic connection management class. Mostly used for DataAccess.

- DataAccess

Contains a basic ORM implementation. 
Currently only T-SQL query (although incomplete) is implemented.

- FileFormats

Nothing yet.
Will contain classes for easy manipulation of common file types (.doc, .xls, .pdf).

- Loaders

Various converters and loaders.
Currently it converts from base64strings to Image types and WSQ, JSONs and XML serializers.

- Proxy

Contains a proxy implementation.
Useful for injecting code, used by DataAccess for lazy loading.

- Text

Contains some common text functions.

- Validation

Contains a validation from DataAnnotations for a general way to validate objects.

# Version 0.9
- DataAccess ready for aggregates implementation, some performance optimizations done.
- Started AccessStatement (queries for Access).
- Added a few new Text functions.
- Working on adding javascripts and other web-content functions/libraries.
