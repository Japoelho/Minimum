using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using Minimum.DataAccess.Mapping;
using Minimum.Connection.Interfaces;

namespace Minimum.DataAccess
{
    public class DataAccess
    {
        private const string NoKeyDefined = "The type {0} has no [Key] attribute defined.";

        private IMapper _mapper;
        private IConnectionInfo _connectionInfo;

        public DataAccess(IConnectionInfo connectionInfo)
        {
            _connectionInfo = connectionInfo;
            _mapper = new Mapper();
        }

        public DataAccess(IConnectionInfo connectionInfo, IMapper mapper)
        {
            _connectionInfo = connectionInfo;
            _mapper = mapper;
        }

        public Query Query<T>() where T : class, new()
        {
            Query query = new Query(_connectionInfo, _mapper.Map(typeof(T)));            
            
            return query;
        }

        public T GetByID<T>(int elementID) where T : class, new()
        {
            Query iQuery = new Query(_connectionInfo, _mapper.Map(typeof(T)));

            Column identity = iQuery.Columns.FirstOrDefault(p => p.IsKey);
            if (identity == null) { throw new InvalidOperationException(String.Format(NoKeyDefined, typeof(T).Name)); }

            return iQuery.Where(Criteria.EqualTo(identity.Property.Name, elementID)).Select<T>().FirstOrDefault();
        }

        public IList<T> Get<T>() where T : class, new()
        {
            Query iQuery = new Query(_connectionInfo, _mapper.Map(typeof(T)));

            return iQuery.Select<T>();
        }

        public IList<T> GetBy<T>(params Criteria[] criterias) where T : class, new()
        {
            Query iQuery = new Query(_connectionInfo, _mapper.Map(typeof(T)));

            return iQuery.Where(criterias).Select<T>();
        }

        public IList<T> GetBy<T>(Expression<Func<T, bool>> expression) where T : class, new()
        {
            Query iQuery = new Query(_connectionInfo, _mapper.Map(typeof(T)));

            return iQuery.Where<T>(expression).Select<T>();
        }

        public T Set<T>(T element) where T : class, new()
        {
            Query iQuery = new Query(_connectionInfo, _mapper.Map(typeof(T)));

            Column identity = iQuery.Columns.FirstOrDefault(p => p.IsKey);
            if (identity == null) { throw new InvalidOperationException(String.Format(NoKeyDefined, typeof(T).Name)); }

            return iQuery.Where(Criteria.EqualTo(identity.Property.Name, identity.Property.GetValue(element))).Update<T>(element);
        }

        public T Add<T>(T element) where T : class, new()
        {
            Query iQuery = new Query(_connectionInfo, _mapper.Map(typeof(T)));

            return iQuery.Insert<T>(element);
        }

        public T Del<T>(T element) where T : class, new()
        {
            Query iQuery = new Query(_connectionInfo, _mapper.Map(typeof(T)));

            Column identity = iQuery.Columns.FirstOrDefault(p => p.IsKey);
            if (identity == null) { throw new InvalidOperationException(String.Format(NoKeyDefined, typeof(T).Name)); }

            return iQuery.Where(Criteria.EqualTo(identity.Property.Name, identity.Property.GetValue(element))).Delete<T>(element);
        }
    }        
}
