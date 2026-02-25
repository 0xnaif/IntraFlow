using FluentAssertions;
using IntraFlow.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace IntraFlow.Tests.Integration;

public class AdminAuthorizationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AdminAuthorizationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Anonymous_user_accessing_admin_page_is_redirected_to_login()
    {
        await using var plainFactory = new WebApplicationFactory<Program>();
        var client = plainFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/Admin/Index");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);

        response.Headers.Location!.ToString().Should().Contain("/Identity/Account/Login");
    }

    [Fact]
    public async Task Admin_can_access_admin_page()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        //var loginPage = await client.GetAsync("/Identity/Account/Login");
        //loginPage.StatusCode.Should().Be(HttpStatusCode.OK);

        //var form = new Dictionary<string, string>
        //{
        //    ["Input.Email"] = "admin@intraflow.com",
        //    ["Input.Password"] = "Admin123!",
        //    ["Input.RememberMe"] = "false"
        //};

        //var loginResponse = await client.PostAsync("/Identity/Account/Login", new FormUrlEncodedContent(form));

        //loginResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);

        var adminResponse = await client.GetAsync("/Admin/Index");
        adminResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}