namespace JetFlight.WebApi.Helpers
{
    public static class SubTypesHelper
    {
        public static List<Type> GetSubtypes(this Type baseType)
        {
            var derivedTypes = baseType.Assembly
                .GetTypes()
                .Where(t => baseType.IsAssignableFrom(t) && t != baseType && !t.IsAbstract && !t.IsInterface)
                .ToList();

            return derivedTypes;
        } 
    }
}
