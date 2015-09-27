using Minimum.Synchronizer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Minimum.Synchronizer
{
    public interface ISynchronizer
    {
        string ErrorMessage { get; }

        IList<T> GetRecords<T>(ref bool hasMore, params object[] parameters) where T : class;
        bool SetRecord<T>(T record, bool isResponse, params object[] parameters) where T : class;        
        Custom Action(Custom custom);
    }
}