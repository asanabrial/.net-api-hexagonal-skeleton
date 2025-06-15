namespace HexagonalSkeleton.Application.Dto
{
    public class ResultDto
    {
        public ResultDto(IDictionary<string, string[]> errors)
        {
            Errors = errors;
        }

        public ResultDto(object data)
        {
            Data = data;
        }

        public bool IsValid => !Errors.Any();
        public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
        public object? Data { get; set; }
    }
}
