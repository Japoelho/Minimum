using System;
using Minimum.DataAccess.Mapping;

namespace Minimum.DataAccess.Statement
{
    internal interface IStatement
    {
        string Select(Type type);
        string Update(object element, Type type);
        string Insert(object element, Type type);
        string Delete(object element, Type type);
        IStatement AddCriteria(params Criteria[] criterias);
        Alias Alias();
    }
}