using Npgsql.Expirements.TypeHandling;
using Npgsql.Expirements.TypeHandling.Handlers;
using System;
using System.IO;

namespace Npgsql.Expirements
{
    class Program
    {
        static void Main(string[] args)
        {
            var stream = new MemoryStream();
            var reader = new NpgsqlBufferReader(stream);
            var writer = new NpgsqlBufferWriter();

            var factory = new TextHandlerFactory();
            var handler = factory.CreateHandler(typeof(string), null!, new NpgsqlConnection());
            if (handler is NpgsqlTypeHandler<string> handlerTyped)
            {
                handlerTyped.WriteValue(writer, "Oh, yeah, baby! It works!");

                writer.FlushAsync(stream, default).GetAwaiter().GetResult();
                stream.Position = 0;

                var value = handlerTyped.ReadAsync(reader, default).GetAwaiter().GetResult();
            }

            Console.WriteLine("Hello World!");
        }
    }
}
