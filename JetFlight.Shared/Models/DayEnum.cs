using System.ComponentModel;

namespace JetFlight.Shared.Models
{
    public enum Day
    {
        [Description("Понеділок")]
        Monday,

        [Description("Вівторок")]
        Tuesday,

        [Description("Середа")]
        Wednesday,

        [Description("Четвер")]
        Thursday,

        [Description("П'ятниця")]
        Friday,

        [Description("Субота")]
        Saturday,

        [Description("Неділя")]
        Sunday
    }
}
