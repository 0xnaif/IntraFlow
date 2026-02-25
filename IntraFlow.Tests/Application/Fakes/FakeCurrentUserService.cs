using IntraFlow.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Tests.Application.Fakes;

public class FakeCurrentUserService : ICurrentUserService
{
    public string UserId { get; set; } = "user-1";
    public HashSet<string> Roles { get; } = new(StringComparer.OrdinalIgnoreCase);

    public bool IsInRole(string role) => Roles.Contains(role);
}
