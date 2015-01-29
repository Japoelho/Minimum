using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimum.DataAccess.Mapping
{
    public interface IMapper
    {
        QueryMap Map(Type type);
    }
}
