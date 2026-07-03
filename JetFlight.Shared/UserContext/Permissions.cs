using System.Runtime.Serialization;

namespace JetFlight.Shared.UserContext;

public enum Permission
{
    [EnumMember(Value = "Контент")]
    Content,

    [EnumMember(Value = "Аналітика")]
    Analytics,

    [EnumMember(Value = "Програма лояльності")]
    LoyaltyProgram,

    [EnumMember(Value = "Товари")]
    Products,

    [EnumMember(Value = "Користувачі")]
    Users,

    [EnumMember(Value = "Акції")]
    Promotions,

    [EnumMember(Value = "Заявки")]
    Applications,

    [EnumMember(Value = "Сповіщення")]
    Notifications,

    [EnumMember(Value = "Магазини")]
    Stores,

    [EnumMember(Value = "Таргет")]
    Target,
}

public enum PermissionLevel
{
    [EnumMember(Value = "Переглядати")]
    Read = 1,
    [EnumMember(Value = "Редагувати")]
    Modify,
    [EnumMember(Value = "Видалення")]
    Delete
}
