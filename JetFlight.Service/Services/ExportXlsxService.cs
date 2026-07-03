using ClosedXML.Excel;
using JetFlight.Service.Extensions;
using JetFlight.Shared.Constants;
using JetFlight.Shared.Models.Export;
using Microsoft.EntityFrameworkCore;

namespace JetFlight.Service.Services
{
    public class ExportXlsxService : IExportService
    {
        public async Task<ExportFile> ExportAsync<T>(IQueryable<T> query, List<string> fields, Func<T, Dictionary<string, object>> toDictionary, string name)
        {
            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add(name);

            // Add headers
            for (var i = 0; i < fields.Count; i++)
            {
                worksheet.Cell(1, i + 1).Value = fields[i];
            }

            // Add data from collection
            var rowIndex = 2;

            var offset = 0;
            var fetchCount = 500;

            List<T> records;

            do
            {
                records = await query.Skip(offset).Take(fetchCount).ToListAsync();
                var dictionaryList = records.Select(toDictionary).ToList();

                foreach (var item in dictionaryList)
                {
                    for (var i = 0; i < fields.Count; i++)
                    {
                        worksheet.Cell(rowIndex, i + 1).Value = CreateCell(
                            item.TryGetValue(fields[i], out var value)
                            ? value
                            : null);
                    }

                    rowIndex++;
                }
            }
            while (records.Count == fetchCount);

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return new ExportFile
            {
                Stream = stream,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName = $"{name}-{DateTimeOffset.UtcNow.Ticks}.xlsx"
            };
        }

        private XLCellValue CreateCell(object? value)
        {
            if (value == null)
            {
                return Blank.Value;
            }

            var type = value.GetType();

            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            if (underlyingType != type)
            {
                value = Convert.ChangeType(value, underlyingType);
            }

            if (underlyingType == typeof(DateTime))
            {
                value = ((DateTime)value).FromUtcToTimezone(TimeZoneConstants.UATimezone);
            }

            if (underlyingType == typeof(Guid))
            {
                value = ((Guid)value).ToString();
            }

            return XLCellValue.FromObject(value);
        }
    }
}
