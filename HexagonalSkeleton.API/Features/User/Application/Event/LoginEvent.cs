﻿using HexagonalSkeleton.CommonCore.Event;

namespace HexagonalSkeleton.API.Features.User.Application.Event
{
    public class LoginEvent(int userId) : DomainEvent
    {
        public int UserId { get; } = userId;
    }
}
