using LoadCbrData.Data.Models;
using Microsoft.Data.SqlClient;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using ServiceReference1;
using System.Data.Common;

namespace LoadCbrData;
internal class Program
{
    private static async Task Main(string[] args)
    {
        //
        // TODO: Настройки подключения необходимо вынести в конфигурационный файл
        SqlConnectionStringBuilder sqlConnectionStringBuilder = new()
        {
            DataSource = "localhost",
            InitialCatalog = "cbrf",
            IntegratedSecurity = true,
        };

        NpgsqlConnectionStringBuilder npgConnectionStringBuilder = new()
        {
            Host = "srv-sam5-006.corp.efive.ru",
            Database = "cbrdb",
            Username = "postgres",
            Password = "postgres"
        };

        //
        // создаем контекст для Postgres
        using CbrDbContext context = CreateDbContext(npgConnectionStringBuilder);

        await InitializeAsync(context);

        await LoadDataAsync(context);
    }

    /// <summary>
    /// Создает экземпляр DbContext (для Sql Server либо для Postgres)
    /// </summary>
    /// <param name="sqlServer">определяет тип конекста, если true то для Sql Server</param>
    /// <returns></returns>
    private static CbrDbContext CreateDbContext(DbConnectionStringBuilder connectionStringBuilder)
    {
        ArgumentNullException.ThrowIfNull(connectionStringBuilder, nameof(connectionStringBuilder));

        if (connectionStringBuilder is SqlConnectionStringBuilder)
        {
            DbContextOptionsBuilder<CbrSqlDbContext> builder = new();

            DbContextOptions<CbrSqlDbContext> options = builder.UseSqlServer(connectionStringBuilder.ToString()).Options;

            return new CbrSqlDbContext(options);
        }
        else if(connectionStringBuilder is NpgsqlConnectionStringBuilder)
        {
            DbContextOptionsBuilder<CbrPostgresDbContext> builder = new();

            DbContextOptions<CbrPostgresDbContext> options = builder.UseNpgsql(connectionStringBuilder.ToString()).Options;

            return new CbrPostgresDbContext(options);
        }
        else
        {
            throw new ArgumentException($"unsupported server type: {connectionStringBuilder.GetType().Name}");
        }
    }

    /// <summary>
    /// Загружает те записи, которые есть в БД Банка России, но еще не загружены в
    /// локальную БД
    /// </summary>
    private static async Task LoadDataAsync(CbrDbContext context)
    {
        //
        // сервис находится http://www.cbr.ru/ecbwebserv/rcbws.asmx
        using var client = new rcbwsSoapClient(rcbwsSoapClient.EndpointConfiguration.rcbwsSoap12);

        ArrayOfUnsignedLong array = new();
        for (int count = 0; ;)
        {
            List<long> ids = await (from id in context.RecordId
                                    join r in context.Record on id.Id equals r.Id into gr
                                    from r in gr.DefaultIfEmpty()
                                    where r == null
                                    orderby id.Id
                                    select id.Id).Take(20).ToListAsync();
            if (ids.Count == 0)
            {
                break;
            }
            array.AddRange(ids.Select(t => (ulong)t));
            GetDataRangeResponse response = await client.GetDataRangeAsync(array);
            array.Clear();
            foreach (DataItem item in response.Body.GetDataRangeResult)
            {
                context.Record.Add(new Record
                {
                    Id = (long)item.ID,
                    Code = item.Code,
                    Inn = item.INN == 0 ? default : item.INN.ToString(),
                    Ogrn = item.OGRN == 0 ? default : item.OGRN.ToString(),
                    Name = item.Name,
                    ShortName = item.SName,
                    Data = item.Data,
                    ModifiedDate = item.LastUpdate
                });
                count++;
            }
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            Console.WriteLine($"{count} records written");
        }
    }

    /// <summary>
    /// Инициализация БД. Загружаются все идентификаторы записей, которые есть
    /// в Реестре эмиссионных ценных бумаг Банка России.
    /// </summary>
    private static async Task InitializeAsync(CbrDbContext context)
    {
        if (!await context.RecordId.AnyAsync())
        {
            //
            // сервис находится http://www.cbr.ru/ecbwebserv/rcbws.asmx
            using var client = new rcbwsSoapClient(rcbwsSoapClient.EndpointConfiguration.rcbwsSoap12);

            int total = 0;
            HashSet<long> all = new();
            List<RecordId> records = new();
            DateTime initialDate = new (1990, 1, 1);
            for (int page = 0; ; ++page)
            {
                var listOfIds = await client.GetIDsAfterAsync(initialDate, page);
                var ids = listOfIds.Body.GetIDsAfterResult;
                if (ids.Count > 0)
                {
                    foreach (var id in ids)
                    {
                        if (all.Add((long)id))
                        {
                            records.Add(new RecordId { Id = (long)id });
                        }
                    }
                    if (records.Count > 0)
                    {
                        context.RecordId.AddRange(records);
                        await context.SaveChangesAsync();
                        context.ChangeTracker.Clear();
                        total += records.Count;
                        Console.WriteLine($"{page,6}: {total} {ids.Min()} {ids.Max()}");
                        records.Clear();
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }
}
