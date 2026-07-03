namespace JetFlight.Shared.Models.Analytic
{
    /// <summary>
    /// Відповідь GetGeneralRegistrationAnalytic з діагностичними лічильниками (щоб порівняти з SQL).
    /// </summary>
    public class GeneralRegistrationAnalyticDebugDto
    {
        public decimal AverageRegistrationTime { get; set; }
        public int CountLeftOvers { get; set; }

        /// <summary>Кількість CustomerSettings у діапазоні дат (як у запиті 1).</summary>
        public int DebugSettingsInRange { get; set; }

        /// <summary>Кількість унікальних CustomerId з цих налаштувань (як у запиті 2).</summary>
        public int DebugCustomersWithSettingsInRange { get; set; }

        /// <summary>З них скільки мають Customer.CreatedAt NOT NULL (як у запиті 3).</summary>
        public int DebugCustomersWithCreatedAt { get; set; }

        /// <summary>Left overs за тим самим правилом (як у запиті 4).</summary>
        public int DebugLeftOvers { get; set; }
    }
}
