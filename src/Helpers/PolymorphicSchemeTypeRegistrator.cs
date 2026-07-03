using JetFlight.Shared.Models.Coupons;
using JetFlight.Shared.Models.Promotion;
using JetFlight.Shared.Models.Questionary;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JetFlight.WebApi.Helpers
{
    public static class PolymorphicSchemeTypeRegistrator
    {
        public static void RegisterPolymorphicTypes(this SwaggerGenOptions options)
        {
            var types = new Type[]
            {
                typeof(CouponActivatorDTO),
                typeof(CouponDetailsDTO),
                typeof(QuestionaryFieldAnswerDTO),
                typeof(QuestionaryRewardDTO),
                typeof(PromotionDisplayBaseRuleDTO),
            };

            options.UseOneOfForPolymorphism();
            options.SelectSubTypesUsing(type =>
            {
                if (types.Contains(type))
                {
                    return type.GetSubtypes();
                }
                else
                {
                    return Array.Empty<Type>();
                }
            });
            options.SelectDiscriminatorNameUsing(x => "$type");
            options.SelectDiscriminatorValueUsing(type => type.Name);
        }
    }
}
