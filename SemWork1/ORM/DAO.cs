using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemWork1.ORM
{
    internal class DAO
    {
        private MyORM _orm { get; }

        public DAO(string dbName)
        {
            _orm = new MyORM(dbName);
        }

        public List<T> Select<T>() => _orm.Select<T>();

        public T? SelectById<T>(int id) => _orm.Select<T>(id);

        public void Insert<T>(T item) => _orm.Insert(item);

        public void Delete<T>() => _orm.Delete<T>();

        public void DeleteById<T>(int id) => _orm.Delete<T>(id);

        public void Update<T>(int id, string columnName, object value) => _orm.Update<T>(id, columnName, value);
    }
}
