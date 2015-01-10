using System;
using Minimum.DataAccess.V08.Mapping;

namespace Minimum.DataAccess.V08.Statement
{
    internal interface IStatement
    {
        string Select(Type type);
        string Update(object element, Type type);
        string Insert(object element, Type type);
        string Delete(object element, Type type);
        IStatement AddCriteria(params Criteria[] criterias);
        AliasMap AliasMap();
    }
}
