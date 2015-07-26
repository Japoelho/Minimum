using System.Collections.Generic;

namespace Minimum.DataAccess
{
    public interface IQuery
    {
        IMap Map { get; }
        IList<Criteria> Criterias { get; }
        IQuery Where(IList<Criteria> criterias);
    }
}
