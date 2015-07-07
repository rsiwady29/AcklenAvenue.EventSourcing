using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AcklenAvenue.EventSourcing.Serializer.JsonNet;
using Newtonsoft.Json;

using Npgsql;

namespace AcklenAvenue.EventSourcing.Postgres
{
    public abstract class PostgresEventStore<TId> : IEventStore<TId>
    {
        readonly string _connectionString;

        readonly string _tableName;

        protected PostgresEventStore(string connectionString, string tableName)
        {
            _connectionString = connectionString;
            _tableName = tableName;            
        }

        public async Task<IEnumerable<object>> GetStream(TId aggregateId = default(TId))
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                List<object> list;
                using (NpgsqlCommand command = connection.CreateCommand())
                {
                    string where = Equals(aggregateId, default(TId))
                                       ? ""
                                       : string.Format("WHERE \"aggregateId\" = '{0}'", aggregateId);

                    command.CommandText = string.Format("SELECT * FROM \"{0}\" {1} ORDER BY date", _tableName, @where);
                    using (var adapter = new NpgsqlDataAdapter { SelectCommand = command })
                    {
                        await connection.OpenAsync();

                        list = ConvertDatasetToListOfEvents(adapter);
                    }
                }

                connection.Close();

                return list;
            }
        }

        public async Task Persist(TId aggregateId, object @event)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                string insertCommandText = GetInsertCommandText(aggregateId, @event);
                using (var command = new NpgsqlCommand(insertCommandText, conn))
                {
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
        }

        public async Task PersistInBatch(IEnumerable<InBatchEvent<TId>> batchEvents)
        {
            await Task.Factory
                .StartNew(() =>
                          {
                              using (var connection = new NpgsqlConnection(_connectionString))
                              {
                                  connection.Open();
                                  using (NpgsqlCommand command = connection.CreateCommand())
                                  {
                                      command.CommandText = string.Format(
                                          "COPY \"{0}\" (\"aggregateId\", data, date, type) FROM STDIN;", _tableName);
                                      var serializer = new NpgsqlCopySerializer(connection);
                                      var copyIn = new NpgsqlCopyIn(command, connection, serializer.ToStream);
                                      try
                                      {
                                          copyIn.Start();

                                          foreach (var @event in batchEvents)
                                          {
                                              serializer.AddString(string.Format("{0}", @event.AggregateId));
                                              serializer.AddString(SerializeEvent(@event.Event));
                                              serializer.AddDateTime(DateTime.Now);
                                              serializer.AddString(@event.Event.GetType().FullName);
                                              serializer.EndRow();
                                              serializer.Flush();
                                          }

                                          copyIn.End();
                                          serializer.Close();
                                      }
                                      catch (Exception e)
                                      {
                                          copyIn.Cancel("Undo copy on exception.");

                                          throw e;
                                      }
                                  }
                              }
                          });
        }

        public List<object> ConvertDatasetToListOfEvents(NpgsqlDataAdapter adapter)
        {
            List<JsonEvent> jsonEvents;
            using (var dataSet = new DataSet())
            {
                adapter.Fill(dataSet);

                jsonEvents = dataSet.Tables[0].Rows.Cast<DataRow>().Select(GetJsonEvent).ToList();
            }

            List<object> list = jsonEvents.Select(DeserializeEvent).ToList();
            return list;
        }

        public abstract object DeserializeEvent(JsonEvent eventJson);

        static JsonEvent GetJsonEvent(DataRow row)
        {
            DateTime dateTime = Convert.ToDateTime(row["date"]);
            string json = row["data"].ToString();
            string type = row["type"].ToString();
            return new JsonEvent(type, json, dateTime);
        }

        string GetInsertCommandText(TId aggregateId, object @event)
        {
            string json = SerializeEvent(@event);

            string dateTimeFormattedForMySql = DateTimeFormattedForMySql();

            string insertCommandText =
                string.Format(
                    "INSERT INTO \"{4}\" (\"aggregateId\", data, date, type) VALUES ('{0}','{1}','{2}','{3}')",
                    aggregateId,
                    json,
                    dateTimeFormattedForMySql,
                    @event.GetType().FullName,
                    _tableName);

            return insertCommandText;
        }

        static string DateTimeFormattedForMySql()
        {
            string dateTimeFormattedForMySql = String.Format("{0:yyyy-M-d HH:mm:ss}", DateTime.Now);
            return dateTimeFormattedForMySql;
        }

        static string SerializeEvent(object @event)
        {
            string json = JsonConvert.SerializeObject(@event);
            return json;
        }
    }
}