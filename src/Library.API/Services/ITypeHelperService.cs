namespace Library.API.Services
{
    public interface ITypeHelperService
    {
        bool TypeHasPropertires<T>(string fields);
    }
}