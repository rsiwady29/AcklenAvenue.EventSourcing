using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AcklenAvenue.EventSourcing.Serializer.JsonNet;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace AcklenAvenue.EventSourcing.MySql
{
    public class MySqlEventStore<TId> : IEventStore<TId>
    {
        readonly string _connectionString;
        readonly string _tableName;

        public MySqlEventStore(string connectionString, string tableName)
        {
            _connectionString = connectionString;
            _tableName = tableName;
        }

        public async Task<IEnumerable<object>> GetStream(TId aggregateId)
        {
            using (var mySqlConnection = new MySqlConnection(_connectionString))
            {
                MySqlCommand command = mySqlConnection.CreateCommand();
                command.CommandText = string.Format("SELECT * FROM {0} WHERE Id = '{1}' ORDER BY Time",
                    _tableName, aggregateId);
                var adapter = new MySqlDataAdapter {SelectCommand = command};
                await mySqlConnection.OpenAsync();

                var dataSet = new DataSet();
                adapter.Fill(dataSet);

                var jsonEvents = new List<JsonEvent>();
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    DateTime dateTime = Convert.ToDateTime(row["Time"]);
                    string json = row["Event"].ToString();
                    string type = row["Type"].ToString();

                    jsonEvents.Add(new JsonEvent(type, json, dateTime));
                }

                var list = new List<object>();
                foreach (JsonEvent jsonEvent in jsonEvents)
                {
                    Type type = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(x => x.GetTypes())
                        .FirstOrDefault(x => x.FullName.EndsWith(jsonEvent.Type));

                    var objectThatWasDeserialized =
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonEvent.Json);
                    ConstructorInfo constructorInfo = type.GetConstructors()[0];
                    ParameterInfo[] parameterInfos = constructorInfo.GetParameters();
                    object[] paramValues = parameterInfos.Select(x =>
                                                                 {
                                                                     KeyValuePair<string, object> item =
                                                                         objectThatWasDeserialized.FirstOrDefault(
                                                                             y => y.Key.ToLower() == x.Name.ToLower());

                                                                     TypeConverter typeConverter =
                                                                         TypeDescriptor.GetConverter(x.ParameterType);
                                                                     object fromString =
                                                                         typeConverter.ConvertFromString(
                                                                             item.Value.ToString());
                                                                     return fromString;
                                                                 }).ToArray();

                    object instance = constructorInfo.Invoke(paramValues);
                    list.Add(instance);
                }

                mySqlConnection.Close();
                return list;
            }
        }

        public async Task Persist(TId aggregateId, object @event)
        {
            using (var mySqlConnection = new MySqlConnection(_connectionString))
            {
                MySqlCommand command = mySqlConnection.CreateCommand();

                string json = JsonConvert.SerializeObject(@event);

                string dateTimeFormattedForMySql = String.Format("{0:yyyy-M-d HH:mm:ss}", DateTime.Now);

                command.CommandText =
                    string.Format(
                        "INSERT INTO {4} (Id, Event, Time, Type) VALUES ('{0}','{1}','{2}','{3}')",
                        aggregateId, json, dateTimeFormattedForMySql, @event.GetType().FullName, _tableName);

                mySqlConnection.Open();
                await command.ExecuteNonQueryAsync();
                mySqlConnection.Close();
            }
        }

        public async Task Persist(DateTime datetimestamp, TId aggregateId, object @event)
        {
            using (var mySqlConnection = new MySqlConnection(_connectionString))
            {
                MySqlCommand command = mySqlConnection.CreateCommand();

                string json = JsonConvert.SerializeObject(@event);

                string dateTimeFormattedForMySql = String.Format("{0:yyyy-M-d HH:mm:ss}", datetimestamp);

                command.CommandText =
                    string.Format(
                        "INSERT INTO {4} (Id, Event, Time, Type) VALUES ('{0}','{1}','{2}','{3}')",
                        aggregateId, json, dateTimeFormattedForMySql, @event.GetType().FullName, _tableName);

                mySqlConnection.Open();
                await command.ExecuteNonQueryAsync();
                mySqlConnection.Close();
            }
        }

        public Task PersistInBatch(IEnumerable<InBatchEvent<TId>> batchEvents)
        {
            throw new NotImplementedException();
        }

        public Task PersistInBatch(DateTime datetimestamp, IEnumerable<InBatchEvent<TId>> batchEvents)
        {
            throw new NotImplementedException();
        }
    }
}