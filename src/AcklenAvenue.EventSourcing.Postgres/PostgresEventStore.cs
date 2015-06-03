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
        readonly JsonEventConverter _jsonEventConverter;

        public PostgresEventStore(string connectionString, string tableName)
        {
            _connectionString = connectionString;
            _tableName = tableName;
            _jsonEventConverter = new JsonEventConverter();
        }

        public async Task<IEnumerable<object>> GetStream(Guid aggregateId = default(Guid))
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                NpgsqlCommand command = connection.CreateCommand();
                
                var where = aggregateId == default(Guid)
                    ? ""
                    : string.Format("WHERE \"aggregateId\" = '{0}'", aggregateId);

                command.CommandText = string.Format("SELECT * FROM \"{0}\" {1} ORDER BY date",
                    _tableName, where);
                var adapter = new NpgsqlDataAdapter {SelectCommand = command};
                await connection.OpenAsync();

                var list = ConvertDatasetToListOfEvents(adapter);

                connection.Close();

                return list;
            }
        }

        public List<object> ConvertDatasetToListOfEvents(NpgsqlDataAdapter adapter)
        {
            var dataSet = new DataSet();
            adapter.Fill(dataSet);

            var jsonEvents = dataSet.Tables[0].Rows.Cast<DataRow>().Select(GetJsonEvent).ToList();

            List<object> list = jsonEvents.Select(_jsonEventConverter.GetEvent).ToList();
            return list;
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