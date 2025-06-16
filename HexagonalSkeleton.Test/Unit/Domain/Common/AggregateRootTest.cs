using Xunit;
using HexagonalSkeleton.Domain.Common;
using HexagonalSkeleton.CommonCore.Event;

namespace HexagonalSkeleton.Test.Unit.Domain.Common;

// Test implementation of AggregateRoot for testing
public class TestAggregate : AggregateRoot
{
    public TestAggregate()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public void AddTestEvent(DomainEvent domainEvent)
    {
        AddDomainEvent(domainEvent);
    }

    public void RemoveTestEvent(DomainEvent domainEvent)
    {
        RemoveDomainEvent(domainEvent);
    }

    public void SetUpdated()
    {
        MarkAsUpdated();
    }
}

public class TestDomainEvent : DomainEvent
{
    public string Message { get; }
    
    public TestDomainEvent(string message)
    {
        Message = message;
    }
}

public class AggregateRootTest
{
    [Fact]
    public void NewAggregate_ShouldHaveDefaultValues()
    {
        // Act
        var aggregate = new TestAggregate();

        // Assert
        Assert.Equal(0, aggregate.Id);
        Assert.False(aggregate.IsDeleted);
        Assert.Null(aggregate.DeletedAt);
        Assert.Null(aggregate.UpdatedAt);
        Assert.Empty(aggregate.DomainEvents);
        Assert.True(aggregate.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void AddDomainEvent_ShouldAddEventToCollection()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var domainEvent = new TestDomainEvent("Test event");

        // Act
        aggregate.AddTestEvent(domainEvent);

        // Assert
        Assert.Single(aggregate.DomainEvents);
        Assert.Contains(domainEvent, aggregate.DomainEvents);
    }

    [Fact]
    public void AddMultipleDomainEvents_ShouldAddAllEvents()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var event1 = new TestDomainEvent("Event 1");
        var event2 = new TestDomainEvent("Event 2");

        // Act
        aggregate.AddTestEvent(event1);
        aggregate.AddTestEvent(event2);

        // Assert
        Assert.Equal(2, aggregate.DomainEvents.Count);
        Assert.Contains(event1, aggregate.DomainEvents);
        Assert.Contains(event2, aggregate.DomainEvents);
    }

    [Fact]
    public void RemoveDomainEvent_ShouldRemoveEventFromCollection()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var event1 = new TestDomainEvent("Event 1");
        var event2 = new TestDomainEvent("Event 2");
        aggregate.AddTestEvent(event1);
        aggregate.AddTestEvent(event2);

        // Act
        aggregate.RemoveTestEvent(event1);

        // Assert
        Assert.Single(aggregate.DomainEvents);
        Assert.DoesNotContain(event1, aggregate.DomainEvents);
        Assert.Contains(event2, aggregate.DomainEvents);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var event1 = new TestDomainEvent("Event 1");
        var event2 = new TestDomainEvent("Event 2");
        aggregate.AddTestEvent(event1);
        aggregate.AddTestEvent(event2);

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void Delete_ShouldMarkAsDeletedAndSetDeletedAt()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var beforeDelete = DateTime.UtcNow;

        // Act
        aggregate.Delete();

        // Assert
        Assert.True(aggregate.IsDeleted);
        Assert.NotNull(aggregate.DeletedAt);
        Assert.True(aggregate.DeletedAt >= beforeDelete);
        Assert.True(aggregate.DeletedAt <= DateTime.UtcNow);
        Assert.NotNull(aggregate.UpdatedAt);
    }

    [Fact]
    public void Restore_ShouldUnmarkAsDeletedAndClearDeletedAt()
    {
        // Arrange
        var aggregate = new TestAggregate();
        aggregate.Delete();
        var beforeRestore = DateTime.UtcNow;

        // Act
        aggregate.Restore();

        // Assert
        Assert.False(aggregate.IsDeleted);
        Assert.Null(aggregate.DeletedAt);
        Assert.NotNull(aggregate.UpdatedAt);
        Assert.True(aggregate.UpdatedAt >= beforeRestore);
    }

    [Fact]
    public void MarkAsUpdated_ShouldSetUpdatedAt()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var beforeUpdate = DateTime.UtcNow;

        // Act
        aggregate.SetUpdated();

        // Assert
        Assert.NotNull(aggregate.UpdatedAt);
        Assert.True(aggregate.UpdatedAt >= beforeUpdate);
        Assert.True(aggregate.UpdatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void DomainEvents_ShouldBeReadOnly()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var domainEvent = new TestDomainEvent("Test event");
        aggregate.AddTestEvent(domainEvent);        // Act
        var events = aggregate.DomainEvents;

        // Assert
        Assert.IsAssignableFrom<IReadOnlyCollection<DomainEvent>>(events);
        // Cannot modify the collection directly - try to cast to mutable collection and add
        if (events is ICollection<DomainEvent> mutableEvents)
        {
            Assert.Throws<NotSupportedException>(() => mutableEvents.Add(domainEvent));
        }
        else
        {
            // If we can't cast to ICollection, it means it's truly read-only, which is what we want
            Assert.True(true); // Collection is properly read-only
        }
    }
}
