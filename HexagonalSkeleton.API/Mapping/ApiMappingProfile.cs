using AutoMapper;

namespace HexagonalSkeleton.API.Mapping
{
    /// <summary>
    /// Empty AutoMapper profile - all mappings are now automatic
    /// thanks to property name conventions and inheritance
    /// </summary>
    public class ApiMappingProfile : Profile
    {
        public ApiMappingProfile()
        {
            // No explicit mappings needed!
            // All DTOs now use either:
            // 1. Direct property matching (LoginApiResponse.AccessToken ← LoginCommandResult.AccessToken)
            // 2. Inheritance (UpdateUserApiResponse : UserApiDto)
        }
    }
}
