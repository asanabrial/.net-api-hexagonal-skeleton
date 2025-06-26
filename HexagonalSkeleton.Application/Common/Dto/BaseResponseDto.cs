namespace HexagonalSkeleton.Application.Common.Dto
{
    /// <summary>
    /// Base class for all response DTOs in the application.
    /// Provides consistent error handling across all operations.
    /// </summary>
    public abstract class BaseResponseDto
    {
        protected BaseResponseDto()
        {
            Errors = new Dictionary<string, string[]>();
        }

        protected BaseResponseDto(IDictionary<string, string[]> errors)
        {
            Errors = errors ?? new Dictionary<string, string[]>();
        }

        protected BaseResponseDto(string error)
        {
            Errors = new Dictionary<string, string[]>
            {
                { "Error", new[] { error } }
            };
        }

        /// <summary>
        /// Indicates whether the operation was successful (no errors).
        /// </summary>
        public bool IsValid => !Errors.Any();

        /// <summary>
        /// Collection of validation errors keyed by field name.
        /// </summary>
        public IDictionary<string, string[]> Errors { get; set; }

        /// <summary>
        /// Creates a validation failure result for the specified type.
        /// </summary>
        public static T ValidationFailure<T>(IDictionary<string, string[]> errors) where T : BaseResponseDto, new()
        {
            return new T { Errors = errors ?? new Dictionary<string, string[]>() };
        }

        /// <summary>
        /// Creates an error result for the specified type.
        /// </summary>
        public static T Error<T>(string errorMessage) where T : BaseResponseDto, new()
        {
            return new T
            {
                Errors = new Dictionary<string, string[]>
                {
                    { "Error", new[] { errorMessage } }
                }
            };
        }
    }
}
