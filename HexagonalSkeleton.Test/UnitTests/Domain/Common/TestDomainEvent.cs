using HexagonalSkeleton.Domain.Common;

namespace HexagonalSkeleton.Test.Unit.Domain.Common
{
    public class TestDomainEvent : DomainEvent
    {
        public string Message { get; }
        
        public TestDomainEvent(string message)
        {
            Message = message;
        }
    }
}
