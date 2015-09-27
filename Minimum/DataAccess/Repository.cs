using Minimum.Connection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Minimum.DataAccess
{
    public class Repository
    {
        private const string NoKeyDefined = "The type {0} has no [Identity] attribute defined.";

        private IMapper _mapper;
        private IConnection _connection;

        public Repository(IConnection connection)
        {
            _connection = connection;
            _mapper = new AutoMapper();
        }

        public Repository(IConnection connection, IMapper mapper)
        {
            _connection = connection;
            _mapper = mapper;
        }

        public long Count<T>() where T : class
        {
            Query<T> query = new Query<T>(_connection, _mapper.Map(typeof(T)));

            return query.Count();
        }

        public long Count<T>(params Criteria[] criterias) where T : class
        {
            Query<T> query = new Query<T>(_connection, _mapper.Map(typeof(T)));

            return query.Where(criterias).Count();
        }

        public long Count<T>(Expression<Func<T, bool>> expression) where T : class
        {
            Query<T> query = new Query<T>(_connection, _mapper.Map(typeof(T)));

            return query.Where(expression).Count();
        }

        public T Select<T>(int entityID) where T : class
        {
            Query<T> query = new Query<T>(_connection, _mapper.Map(typeof(T)));

            Property identity = query.Map.Properties.FirstOrDefault(p => p.IsIdentity);
            if (identity == null) { throw new InvalidOperationException(String.Format(NoKeyDefined, typeof(T).Name)); }

            return query.Where(Criteria.EqualTo(identity.PropertyInfo.Name, entityID)).Select().FirstOrDefault();
        }

        public IList<T> Select<T>() where T : class
        {
            Query<T> query = new Query<T>(_connection, _mapper.Map(typeof(T)));

            return query.Select();
        }

        public IList<T> Select<T>(params Criteria[] criterias) where T : class
        {
            Query<T> query = new Query<T>(_connection, _mapper.Map(typeof(T)));

            return query.Where(criterias).Select();
        }

        public IList<T> Select<T>(Expression<Func<T, bool>> expression) where T : class
        {
            Query<T> query = new Query<T>(_connection, _mapper.Map(typeof(T)));

            return query.Where(expression).Select();
        }

        public T Update<T>(T entity) where T : class
        {
            Query<T> query = new Query<T>(_connection, _mapper.Map(typeof(T)));            

            Property identity = query.Map.Properties.FirstOrDefault(p => p.IsIdentity);
            if (identity == null) { throw new InvalidOperationException(String.Format(NoKeyDefined, typeof(T).Name)); }

            return query.Where(Criteria.EqualTo(identity.PropertyInfo.Name, identity.PropertyInfo.GetValue(entity))).Update(entity);
        }

        public T Update<T>(T entity, params string[] properties) where T : class 
        {
            Query<T> query = new Query<T>(_connection, _mapper.Map(typeof(T), properties));

            Property identity = query.Map.Properties.FirstOrDefault(p => p.IsIdentity);
            if (identity == null) { throw new InvalidOperationException(String.Format(NoKeyDefined, typeof(T).Name)); }

            return query.Where(Criteria.EqualTo(identity.PropertyInfo.Name, identity.PropertyInfo.GetValue(entity))).Update(entity);
        }

        public T Update<T>(T entity, params Expression<Func<T, object>>[] properties) where T : class
        {
            Query<T> query = new Query<T>(_connection, _mapper.Map(typeof(T), MapExpression.MapProperties(properties)));

            Property identity = query.Map.Properties.FirstOrDefault(p => p.IsIdentity);
            if (identity == null) { throw new InvalidOperationException(String.Format(NoKeyDefined, typeof(T).Name)); }

            return query.Where(Criteria.EqualTo(identity.PropertyInfo.Name, identity.PropertyInfo.GetValue(entity))).Update(entity);
        }

        public T Insert<T>(T entity) where T : class
        {
            //Query<T> query = new Query<T>(_connection, _mapper.Map(typeof(T)));
            Query<T> query = new Query<T>(_connection, _mapper.Map(typeof(T)));
            
            return query.Insert(entity);
        }

        public T Delete<T>(T entity) where T : class
        {
            Query<T> query = new Query<T>(_connection, _mapper.Map(typeof(T)));

            Property identity = query.Map.Properties.FirstOrDefault(p => p.IsIdentity);
            if (identity == null) { throw new InvalidOperationException(String.Format(NoKeyDefined, typeof(T).Name)); }

            return query.Where(Criteria.EqualTo(identity.PropertyInfo.Name, identity.PropertyInfo.GetValue(entity))).Delete(entity);
        }

        public IList<T> Execute<T>(string query) where T : class
        {
            Procedure<T> procedure = new Procedure<T>(_connection, _mapper.Map(typeof(T)));

            return procedure.Execute(query);
        }

        public int Execute(string query)
        {
            int rows = 0;

            using (DbConnection connection = _connection.NewConnection())
            {
                connection.Open();

                DbCommand command = connection.CreateCommand();
                command.CommandText = query;

                object result = command.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    rows = (int)result;
                }
                
                connection.Close();
            }

            return rows;
        }

        public bool Create<T>() where T : class
        {
            Query<T> query = new Query<T>(_connection, _mapper.Map(typeof(T)));
            
            query.Create();

            return true;
        }

        public bool Drop<T>() where T : class
        {
            Query<T> query = new Query<T>(_connection, _mapper.Map(typeof(T)));

            query.Drop();

            return true;
        }

        public ConnectionTest Test()
        {
            return _connection.TestConnection();
        }
    }
}