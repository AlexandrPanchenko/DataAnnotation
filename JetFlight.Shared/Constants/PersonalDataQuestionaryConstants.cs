namespace JetFlight.Shared.Constants
{
    public class PersonalDataQuestionaryConstants
    {
        public const string Name = "Персональні дані";
        public const string DateOfBirthField = "Дата народження";
        // Повертаємо технічну назву поля до варіанту з малою літерою,
        // щоб збігалася з назвою в опитувальнику та відповідях:
        // "Вид основної діяльності"
        public const string TypeOfActivityField = "Вид основної діяльності";
        public const string SexField = "Стать";
        public const string SexFieldManOption = "Чоловік";
        public const string SexFieldWomanOption = "Жінка";
        public const string WhereFindOutField = "Звідки дізнались про програму лояльності?";
        public const string CityField = "Місто";
        public const string HomeAirportField = "Home airport";
        public const string EmailField = "Email";
        public const string NumberOfChildrenField = "Кількість дітей";
        public const string QuestionaryImageName = "personal-questionary.png";
        public const string QuestionaryCatJetImageName = "personal-questionary-catjet.png";
    }

    public class PersonalDataSelectOptions
    {
        public Dictionary<string, string> Cities { get; set; }
        public Dictionary<string, string> AirportHubs { get; set; }
    }
}
