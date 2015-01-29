using System;

namespace Minimum.DataAccess.Statement
{
    internal interface IStatement
    {
        string Select(Query query);
        string Update(Query query, object element);
        string Insert(Query query, object element);
        string Delete(Query query, object element);
        object FormatReadValue(object value, Type type);
        string FormatWriteValue(object value, Type type);
    }
}
