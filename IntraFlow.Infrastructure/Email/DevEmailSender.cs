using IntraFlow.Application.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Infrastructure.Email;

public class DevEmailSender : IEmailSender
{
    private readonly ILogger<DevEmailSender> _logger;

    public DevEmailSender(ILogger<DevEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string toEmail, string subject, string body, CancellationToken ct = default)
    {
        _logger.LogInformation("DEV EMAIL => To:{To} Subject:{Subject} Body:{Body}", toEmail, subject, body);
        return Task.CompletedTask;
    }
}
