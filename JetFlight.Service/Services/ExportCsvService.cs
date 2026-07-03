using CsvHelper;
using CsvHelper.Configuration;
using JetFlight.Service.Extensions;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models.Export;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace JetFlight.Service.Services
{
    public class ExportCsvService : IExportService
    {
        public ExportCsvService()
        {
        }

        public async Task<ExportFile> ExportAsync<T>(IQueryable<T> query, List<string> fields, Func<T, Dictionary<string, object>> toDictionary, string name)
        {
            var stream = new MemoryStream();

            using var streamWriter = new StreamWriter(stream, encoding: Encoding.UTF8, leaveOpen: true);
            using var csv = new CsvWriter(streamWriter, new CsvConfiguration(CultureInfo.InvariantCulture), true);

            foreach (var field in fields)
            {
                csv.WriteField(field);
            }
            csv.NextRecord();

            var offset = 0;
            var fetchCount = 500;

            List<T> records;

            do
            {
                records = await query.Skip(offset).Take(fetchCount).ToListAsync();
                var items = records.Select(toDictionary).ToList();

                foreach (var item in items)
                {
                    foreach (var field in fields)
                    {
                        item.TryGetValue(field, out var value);

                        if (value != null)
                        {
                            var type = value.GetType();

                            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

                            if (underlyingType == typeof(DateTime))
                            {
                                var dateTime = underlyingType == type ? (DateTime)value : ((DateTime?)value).Value;
                                value = dateTime.FromUtcToTimezone(TimeZoneConstants.UATimezone);
                            }
                        }

                        csv.WriteField(value);
                    }
                    csv.NextRecord();
                }
            }
            while (records.Count == fetchCount);

            csv.Flush();

            stream.Seek(0, SeekOrigin.Begin);

            return new ExportFile
            {
                Stream = stream,
                ContentType = "text/csv",
                FileName = $"{name}-{DateTimeOffset.UtcNow.Ticks}.csv"
            };
        }
    }
}
