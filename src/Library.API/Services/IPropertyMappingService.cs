using System.Collections.Generic;

namespace Library.API.Services
{
    // Entire shorting implementation needs to revist
    public interface IPropertyMappingService
    {
        bool ValidMappingExistsFor<TSource, TDestination>(string fields);

        Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>();
    }
}