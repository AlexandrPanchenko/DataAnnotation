using JetFlight.Shared.Models;
using JetFlight.Shared.Models.LogHistory;

namespace JetFlight.Shared
{
    public static class WorkingHoursComparer
    {
        public static List<LogHistoryField> CompareWorkingHours(WorkingHoursLogHistoryDTO? oldHours, WorkingHoursLogHistoryDTO? newHours, string entity)
        {
            var result = new List<LogHistoryField>();

            if (oldHours == null && newHours == null)
            {
                return result;
            }

            // Compare working hours for specific days
            if (oldHours?.Day != null && newHours?.Day != null && oldHours.Day == newHours.Day)
            {
                string dayOfWeek = string.Empty;

                switch (newHours.Day.Value)
                {
                    case Day.Monday:
                        dayOfWeek = "Понеділок";
                        break;
                    case Day.Tuesday:
                        dayOfWeek = "Вівторок";
                        break;
                    case Day.Wednesday:
                        dayOfWeek = "Середа";
                        break;
                    case Day.Thursday:
                        dayOfWeek = "Четвер";
                        break;
                    case Day.Friday:
                        dayOfWeek = "П'ятниця";
                        break;
                    case Day.Saturday:
                        dayOfWeek = "Субота";
                        break;
                    case Day.Sunday:
                        dayOfWeek = "Неділя";
                        break;
                }

                if (oldHours.OpeningTime != newHours.OpeningTime || oldHours.ClosingTime != newHours.ClosingTime)
                {
                    result.Add(
                        new LogHistoryField(
                            $"оновив(ла) робочі години в {dayOfWeek} {entity}",
                            $"{oldHours.OpeningTime} - {oldHours.ClosingTime}",
                            $"{newHours.OpeningTime} - {newHours.ClosingTime}"));
                }

                if (oldHours.Note != newHours.Note)
                {
                    result.Add(
                        new LogHistoryField(
                            $"оновив(ла) примітку в {dayOfWeek} {entity}",
                            $"{oldHours.Note}",
                            $"{newHours.Note}"));
                }
            }

            // Compare custom dates
            if (oldHours?.Date != null && newHours?.Date != null && oldHours.Date == newHours.Date)
            {
                if (oldHours.OpeningTime != newHours.OpeningTime || oldHours.ClosingTime != newHours.ClosingTime)
                {
                    result.Add(
                        new LogHistoryField(
                            $"оновив(ла) робочі години в окрему дату {newHours.Date.Value.ToShortDateString()} {entity}",
                            $"{oldHours.OpeningTime} - {oldHours.ClosingTime}",
                            $"{newHours.OpeningTime} - {newHours.ClosingTime}"));
                }

                if (oldHours.Note != newHours.Note)
                {
                    result.Add(
                        new LogHistoryField(
                            $"оновив(ла) примітку в окрему дату {newHours.Date.Value.ToShortDateString()} {entity}",
                            $"{oldHours.Note}",
                            $"{newHours.Note}"));
                }
            }
            else if (newHours?.Date != null && (oldHours == null || oldHours?.Date == null))
            {
                result.Add(
                    new LogHistoryField(
                        $"додав(ла) робочі години в окрему дату {newHours.Date.Value.ToShortDateString()} {entity}",
                        string.Empty,
                        $"{newHours.OpeningTime} - {newHours.ClosingTime}"));

                if (newHours.Note != null)
                {
                    result.Add(
                        new LogHistoryField(
                            $"додав(ла) примітку в окрему дату {newHours.Date.Value.ToShortDateString()} {entity}",
                            string.Empty,
                            $"{newHours.Note}"));
                }
            }
            else if (oldHours?.Date != null)
            {
                if (newHours == null || newHours?.Date == null)
                {
                    result.Add(
                    new LogHistoryField(
                        $"видалив(ла) окрему дату {oldHours.Date.Value.ToShortDateString()} {entity}",
                        $"{oldHours.OpeningTime} - {oldHours.ClosingTime}",
                        string.Empty));
                }
                else if (oldHours.Note != null && newHours?.Note == null)
                {
                    result.Add(
                        new LogHistoryField(
                            $"видалив(ла) примітку в окрему дату {oldHours.Date.Value.ToShortDateString()} {entity}",
                            $"{oldHours.Note}",
                            string.Empty));
                }
            }

            return result;
        }
    }
}
