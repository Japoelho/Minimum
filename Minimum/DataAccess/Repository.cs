using Minimum.Connection;
using Minimum.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
        
        public T Cascade<T>(T entity) where T : class
        {
            IMap map = _mapper.Map(typeof(T));

            Property identity = map.Properties.FirstOrDefault(p => p.IsIdentity);
            if (identity == null) { throw new InvalidOperationException(String.Format(NoKeyDefined, typeof(T).Name)); }

            T original = Select<T>((int)identity.PropertyInfo.GetValue(entity));

            return Compare<T>(entity, original, map);
        }

        private T Compare<T>(T update, T original, IMap map) where T : class
        {
            if (original == null)
            {
                Insert<T>(update);
            }
            else if (update == null)
            {
                Delete<T>(original);
            }
            else
            {
                Update<T>(update);
            }

            if (map == null) { map = _mapper.Map(typeof(T)); }
            
            for (int i = 0; i < map.Relations.Count; i++)
            {
                if (!map.Relations[i].IsCascade) { continue; }

                if (map.Relations[i].IsCollection)
                {
                    IMap oMap = _mapper.Map(map.Relations[i].Type);                    
                    Property identity = oMap.Properties.FirstOrDefault(p => p.IsIdentity);
                    if (identity == null) { throw new InvalidOperationException(String.Format(NoKeyDefined, oMap.Type.Name)); }

                    MethodInfo compare = typeof(Repository).GetMethod("Compare", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(map.Relations[i].Type);

                    IList listA = update != null ? map.Relations[i].PropertyInfo.GetValue(update) as IList : null;
                    IList listB = original != null ? map.Relations[i].PropertyInfo.GetValue(original) as IList : null;

                    if (listA != null)
                    {
                        for (int j = 0; j < listA.Count; j++)
                        {
                            object objectA = listA[j];
                            object objectB = null;

                            object aID = identity.PropertyInfo.GetValue(listA[j]);
                            for (int h = 0; h < listB.Count; h++)
                            {
                                object oID = identity.PropertyInfo.GetValue(listB[h]);
                                if (aID.Equals(oID))
                                {
                                    objectB = listB[h];
                                    listB.Remove(objectB);
                                    break;
                                }
                            }

                            for (int h = 0; h < map.Relations[i].On.Length; h++)
                            {
                                Property parent = map.Properties.FirstOrDefault(p => p.ColumnName == map.Relations[i].On[h].ForeignKey);
                                object joinValue = parent.PropertyInfo.GetValue(update);

                                Property join = map.Relations[i].JoinMap.Properties.FirstOrDefault(p => p.ColumnName == map.Relations[i].On[h].PrimaryKey);
                                join.PropertyInfo.SetValue(objectA, joinValue);
                            }

                            compare.Invoke(this, new object[] { objectA, objectB, oMap });
                        }
                    }

                    if (listB != null)
                    {
                        for (int j = 0; j < listB.Count; j++)
                        {
                            object objectA = null;
                            object objectB = listB[j];

                            compare.Invoke(this, new object[] { objectA, objectB, oMap });
                        }
                    }
                }
                else if (!map.Relations[i].IsInheritance)
                {
                    object objectA = update != null ? map.Relations[i].PropertyInfo.GetValue(update) : null;
                    object objectB = original != null ? map.Relations[i].PropertyInfo.GetValue(original) : null;

                    MethodInfo compare = typeof(Repository).GetMethod("Compare", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(map.Relations[i].Type);
                    compare.Invoke(this, new object[] { objectA, objectB, null });
                }
            }

            return update;
        }

        public IList<T> Execute<T>(string query) where T : class
        {
            Procedure<T> procedure = new Procedure<T>(_connection, _mapper.Map(typeof(T)));

            return procedure.Execute(query);
        }

        public int Execute(string query)
        {
            Procedure<object> procedure = new Procedure<object>(_connection, null);

            return procedure.ExecuteScalar(query);
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