namespace CompanyName.Application.Common.Exceptions;

public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException()
        : base("The record was modified by another user.")
    {
    }
}