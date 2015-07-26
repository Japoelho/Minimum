using System;
using System.Collections.Generic;

namespace Minimum.DataAccess.Statement
{
    public interface IStatement
    {
        string Select(IMap map, IList<Criteria> criterias);
        string Update(IMap map, IList<Criteria> criterias, object element);
        string Insert(IMap map, object element);
        string Delete(IMap map, IList<Criteria> criterias, object element);
        string Count(IMap map, IList<Criteria> criterias);

        string Create(IMap map);
        string Drop(IMap map);
        string GetInsertedID();
        object FormatReadValue(object value, Type type);
        string FormatWriteValue(object value, Type type);
    }
}
