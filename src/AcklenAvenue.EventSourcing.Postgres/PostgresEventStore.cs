using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Npgsql;

namespace AcklenAvenue.EventSourcing.Postgres
{
    public class PostgresEventStore : IEventStore
    {
        readonly string _connectionString;
        readonly string _tableName;

        public PostgresEventStore(string connectionString, string tableName)
        {
            _connectionString = connectionString;
            _tableName = tableName;
        }

        public async Task<IEnumerable<object>> GetStream(Guid aggregateId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                NpgsqlCommand command = connection.CreateCommand();
                command.CommandText = string.Format("SELECT * FROM \"{0}\" WHERE \"aggregateId\" = '{1}' ORDER BY date",
                    _tableName, aggregateId);
                var adapter = new NpgsqlDataAdapter {SelectCommand = command};
                await connection.OpenAsync();

                var dataSet = new DataSet();
                adapter.Fill(dataSet);

                var jsonEvents = dataSet.Tables[0].Rows.Cast<DataRow>().Select(GetJsonEvent).ToList();
                
                List<object> list = jsonEvents.Select(GetEvent).ToList();

                connection.Close();
                return list;
            }
        }

        public async void Persist(Guid aggregateId, object @event)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                string insertCommandText = GetInsertCommandText(aggregateId, @event);
                var command = new NpgsqlCommand(insertCommandText, conn);

                try
                {
                    int linesAffected = await command.ExecuteNonQueryAsync();
                    Console.WriteLine("It was added {0} lines in table table1", linesAffected);
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        static JsonEvent GetJsonEvent(DataRow row)
        {
            DateTime dateTime = Convert.ToDateTime(row["date"]);
            string json = row["data"].ToString();
            string type = row["type"].ToString();
            return new JsonEvent(type, json, dateTime);
        }

        static object GetEvent(JsonEvent jsonEvent)
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
            return instance;
        }

        string GetInsertCommandText(Guid aggregateId, object @event)
        {
            string json = JsonConvert.SerializeObject(@event);

            string dateTimeFormattedForMySql = String.Format("{0:yyyy-M-d HH:mm:ss}", DateTime.Now);

            string insertCommandText = string.Format(
                "INSERT INTO \"{4}\" (\"aggregateId\", data, date, type) VALUES ('{0}','{1}','{2}','{3}')",
                aggregateId, json, dateTimeFormattedForMySql, @event.GetType().FullName, _tableName);

            return insertCommandText;
        }
    }
}