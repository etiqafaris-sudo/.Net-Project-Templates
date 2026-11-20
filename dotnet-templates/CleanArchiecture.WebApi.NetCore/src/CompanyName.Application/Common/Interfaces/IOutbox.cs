namespace CompanyName.Application.Common.Interfaces;

public interface IOutbox
{
    void Enqueue<T>(T message)
        where T : class;
}