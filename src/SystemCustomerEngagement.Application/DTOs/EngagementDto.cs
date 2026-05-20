using SystemCustomerEngagement.Domain.Entities;

namespace SystemCustomerEngagement.Application.DTOs;

public sealed record EngagementDto(
    Guid Id,
    Guid CustomerId,
    EngagementChannel Channel,
    EngagementStatus Status,
    string Message,
    DateTime CreatedAt,
    DateTime? ProcessedAt);
