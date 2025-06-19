using HexagonalSkeleton.Application.Dto;

namespace HexagonalSkeleton.Application.Command
{
    /// <summary>
    /// Response DTO for SoftDeleteUserCommand operation.
    /// Indicates the success of the logical user deletion operation.
    /// </summary>
    public class SoftDeleteUserCommandResult : BaseResponseDto
    {        public SoftDeleteUserCommandResult() : base()
        {
        }

        public SoftDeleteUserCommandResult(IDictionary<string, string[]> errors) : base(errors)
        {
        }

        public SoftDeleteUserCommandResult(string error) : base(error)
        {
        }
    }
}
