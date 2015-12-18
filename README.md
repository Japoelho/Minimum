# Minimum
A collection of utils and tools for easier and faster development of my daily work. It contains:

1. A Repository (SCRUD) ready to use, supports T-SQL (2008 and 2012 syntax) and SQLite.
2. A synchronization framework to synchronize records between different systems.
3. Proxy to intercept virtual functions.
4. A Validation helper with some extra functions.
5. Some text and util general functions.
6. A javascript file with a few commonly used functions.
7. WPF controls, a notification popup system, auto-complete textbox and an image-drop box (still working).

# Version 1.0
- First useable version. Trying it out on various projects, will be adding more features as I figure out the needs and problems.
- Working on the Javascript Common Library/Tools.
- Thinking on more useful tools/stuff that should be in here.

# Version 1.1
- Query optimizations for the DataAccess.
- SQLite support added.
- Improvements to the Query: managing sub-selects, sub-updates and sub-inserts.

# Version 1.2
- More optimizations to the generated queries.
- Improved some of the conventions used by the AutoMapper.
- Moved the SQLite to another project so the required files don't clutter your project if you don't need them.
- Added a WPF controls project.
- Removed updates and inserts to child elements of a class, things could get bad and I think it was hurting performance needlessly checking for updates/inserts all the time.
- Removed the FileFormats, they weren't used and for most situations I they weren't helpful (OpenXML and iTextSharp) so I need to look for something with a broader range or implement my own.
- Added a big synchronization helper, though I think it needs some clarification to make it easier to use/implement.
