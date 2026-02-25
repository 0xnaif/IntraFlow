using IntraFlow.Application.Abstractions;

namespace IntraFlow.Tests.Application.Fakes;

public sealed class FakeEmailSender : IEmailSender
{
    public record SentEmail(string To, string Subject, string Body);

    public List<SentEmail> Sent { get; } = new();

    public Task SendAsync(string toEmail, string subject, string body, CancellationToken ct = default)
    {
        Sent.Add(new SentEmail(toEmail, subject, body));
        return Task.CompletedTask;
    }
}