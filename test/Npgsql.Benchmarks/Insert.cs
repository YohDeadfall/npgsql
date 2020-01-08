using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Npgsql.Benchmarks
{
    public class Insert
    {
        NpgsqlConnection _connection = default!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _connection = new NpgsqlConnection(
                new NpgsqlConnectionStringBuilder(BenchmarkEnvironment.ConnectionString) { Pooling = false }.ToString());
            _connection.Open();

            using var command = new NpgsqlCommand(connection: _connection, cmdText:
                "CREATE TEMP TABLE data_table (id INTEGER NOT NULL, name TEXT NOT NULL) ON COMMIT DELETE ROWS");
            command.ExecuteNonQuery();
        }

        [GlobalCleanup]
        public void GlobalCleanup() => _connection.Close();

        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(Source))]
        public async Task InsertPlain(Data[] source)
        {
            var transaction = _connection.BeginTransaction();
            using var command = new NpgsqlCommand(connection: _connection, cmdText:
                "INSERT INTO data_table (id, name) VALUES (@i, @n)");

            var id = new NpgsqlParameter<int>("i", default(int));
            var name = new NpgsqlParameter<string>("n", string.Empty);

            command.Parameters.Add(id);
            command.Parameters.Add(name);

            foreach (var element in source)
            {
                id.TypedValue = element.Id;
                name.TypedValue = element.Name;

                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }

        [Benchmark]
        [ArgumentsSource(nameof(Source))]
        public async Task InsertPrepared(Data[] source)
        {
            var transaction = _connection.BeginTransaction();
            using var command = new NpgsqlCommand(connection: _connection, cmdText:
                "INSERT INTO data_table (id, name) VALUES (@i, @n)");

            await command.PrepareAsync();

            var id = new NpgsqlParameter<int>("i", default(int));
            var name = new NpgsqlParameter<string>("n", string.Empty);

            command.Parameters.Add(id);
            command.Parameters.Add(name);

            foreach (var element in source)
            {
                id.TypedValue = element.Id;
                name.TypedValue = element.Name;

                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }

        [Benchmark]
        [ArgumentsSource(nameof(Source))]
        public async Task InsertBatched(Data[] source)
        {
            using var command = new NpgsqlCommand(connection: _connection, cmdText: null);

            var sb = new StringBuilder("INSERT INTO data_table (id, name) VALUES ");
            for (var i = 0; i < source.Length; i++)
            {
                var pI = (i * 4).ToString();
                var pN = (i * 4 + 1).ToString();

                sb.Append("(@").Append(pI).Append(", @").Append(pN).Append(")");
                command.Parameters.Add(new NpgsqlParameter<int>(pI, source[i].Id));
                command.Parameters.Add(new NpgsqlParameter<int>(pN, source[i].Id));
            }

            await command.ExecuteNonQueryAsync();
        }

        [Benchmark]
        [ArgumentsSource(nameof(Source))]
        public async Task InsertUnnest(Data[] source)
        {
            using var command = new NpgsqlCommand(connection: _connection, cmdText:
                "INSERT INTO data_table (id, name) SELECT * FROM unnest(@i, @n) AS d");

            var id = new NpgsqlParameter<int[]>("i", source.Select(e => e.Id).ToArray());
            var name = new NpgsqlParameter<string[]>("n", source.Select(e => e.Name).ToArray());

            command.Parameters.Add(id);
            command.Parameters.Add(name);

            await command.ExecuteNonQueryAsync();
        }

        [Benchmark]
        [ArgumentsSource(nameof(Source))]
        public async Task Copy(Data[] source)
        {
            using var importer = _connection.BeginBinaryImport(
                "COPY data_table (id, name) FROM STDIN (FORMAT binary)");
    
            foreach (var element in source)
            {
                await importer.StartRowAsync();
                await importer.WriteAsync(element.Id);
                await importer.WriteAsync(element.Name);
            }

            await importer.CompleteAsync();
        }

        public static IEnumerable<object[]> Source()
        {
            for (int power = 1, count = 2; power < 6; power++)
            {
                yield return new object[]
                {
                    Enumerable
                        .Range(0, count)
                        .Select(i => new Data { Id = i, Name = $"My identifier is {i}" })
                        .ToArray()
                };

                count *= 10;
            }
        }

        public class Data
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}
