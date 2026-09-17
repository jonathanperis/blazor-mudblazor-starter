using Bunit;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;
using WebClient.Components.Pages;
using WebClient.Components.Learning;
using WebClient.Components.Weather;
using WebClient.Features.Forecasts;
using WebClient.Features.Learning;
using WebClient.Features.Notebook;

namespace WebClient.Tests;

public sealed class ComponentTests : BunitContext, IAsyncLifetime
{
    public Task InitializeAsync() => Task.CompletedTask;
    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    public ComponentTests()
    {
        Services.AddMudServices();
        Services.AddScoped<CircuitCounter>();
        JSInterop.Mode = JSRuntimeMode.Loose;
        Render<MudPopoverProvider>();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Editor_keeps_original_unchanged_until_parent_commits(bool confirm)
    {
        var record = new WeatherForecast();
        var provider = Render<MudDialogProvider>();
        var dialog = await provider.InvokeAsync(() => Services.GetRequiredService<IDialogService>()
            .ShowAsync<EditWeather>("Edit", new DialogParameters<EditWeather> { { x => x.Item, record } }));
        provider.Find("input[maxlength='120']").Change("Edited summary");
        Assert.Equal("Mild", record.Summary);
        if (confirm) provider.Find("form").Submit();
        else provider.FindAll("button").Single(button => button.TextContent.Trim() == "Cancel").Click();
        var result = await dialog.Result!;
        Assert.NotNull(result);
        Assert.Equal(!confirm, result.Canceled);
        Assert.Equal("Mild", record.Summary);
        if (confirm)
        {
            record.Apply(Assert.IsType<WeatherForecast>(result.Data));
            Assert.Equal("Edited summary", record.Summary);
        }
    }

    [Fact]
    public async Task Required_summary_blocks_dialog_submission()
    {
        var provider = Render<MudDialogProvider>();
        var dialog = await provider.InvokeAsync(() => Services.GetRequiredService<IDialogService>().ShowAsync<AddWeather>("Add"));
        provider.Find("input[maxlength='120']").Change("");
        provider.Find("form").Submit();
        provider.WaitForAssertion(() => Assert.NotEmpty(provider.FindAll(".validation-errors li")));
        Assert.False(dialog.Result!.IsCompleted);
        provider.Find("input[maxlength='120']").Change("Valid summary");
        provider.Find("form").Submit();
        var result = await dialog.Result;
        Assert.False(result!.Canceled);
    }

    [Fact]
    public void Counter_callback_and_reset_update_the_owned_values()
    {
        var counter = Render<Counter>();
        counter.FindAll("button").Single(button => button.TextContent.Contains("Increment component")).Click();
        Assert.Equal("1", counter.Find("[data-testid=component-count]").TextContent);
        counter.FindAll("button").Single(button => button.TextContent.Contains("Reset experiment")).Click();
        Assert.Equal("0", counter.Find("[data-testid=component-count]").TextContent);
    }

    [Fact]
    public async Task Preferences_load_and_await_both_write_paths()
    {
        JSInterop.Setup<PreferenceSnapshot>("learningPreferences.read").SetResult(new(true, false, true));
        JSInterop.Setup<bool>("learningPreferences.write", _ => true).SetResult(true);
        var preferences = new UiPreferences(Services.GetRequiredService<IJSRuntime>());
        await preferences.LoadAsync();
        Assert.True(preferences.IsDarkMode);
        Assert.False(preferences.DrawerOpen);
        await preferences.SetThemeAsync(false);
        await preferences.SetDrawerAsync(true);
        Assert.False(preferences.IsDarkMode);
        Assert.True(preferences.DrawerOpen);
        Assert.Equal(2, JSInterop.Invocations["learningPreferences.write"].Count);
    }

    [Fact]
    public async Task Api_reset_ignores_a_late_response()
    {
        await using var context = new BunitContext();
        context.Services.AddMudServices();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        using var handler = new DelayedResponse();
        using var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        context.Services.AddSingleton(new ForecastApiClient(http));
        context.Render<MudPopoverProvider>();
        var page = context.Render<WebClient.Components.Pages.Labs.Api>();
        var loading = page.FindAll("button").Single(button => button.TextContent.Trim() == "Load page").ClickAsync(new MouseEventArgs());
        page.WaitForAssertion(() => Assert.NotNull(page.Find("[aria-label='Loading API page']")));
        page.FindAll("button").Single(button => button.TextContent.Contains("Reset experiment")).Click();
        handler.Response.SetResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new ForecastPage(ForecastData.Generate(1), 1)) });
        await loading;
        Assert.Contains("Experiment reset. Load a new page.", page.Markup);
        Assert.Empty(page.FindAll("table"));
    }

    [Fact]
    public async Task Reset_cancels_processing_without_overwriting_reset_status()
    {
        var page = Render<WebClient.Components.Pages.Labs.Files>();
        var processing = page.FindAll("button").Single(button => button.TextContent.Trim() == "Start processing").ClickAsync(new MouseEventArgs());
        page.WaitForAssertion(() => Assert.Contains("Running", page.Markup));
        page.FindAll("button").Single(button => button.TextContent.Contains("Reset experiment")).Click();
        await processing;
        Assert.Contains("Idle", page.Markup);
        Assert.DoesNotContain("Canceled", page.Markup);
    }

    [Fact]
    public async Task Root_initializes_circuit_workspace_without_a_live_HttpContext()
    {
        await using var context = new BunitContext();
        context.Services.AddMudServices();
        context.Services.AddScoped<LearnerWorkspace>();
        context.Services.AddScoped<UiPreferences>();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.JSInterop.Setup<PreferenceSnapshot>("learningPreferences.read").SetResult(new(false, true, true));
        var id = Guid.NewGuid().ToString("N");
        context.Render<WebClient.Components.Routes>(parameters => parameters.Add(component => component.WorkspaceId, id));
        Assert.Equal(id, context.Services.GetRequiredService<LearnerWorkspace>().Id);
    }

    [Fact]
    public void Error_recovery_resets_parent_state_before_retrying_child_content()
    {
        var fail = true;
        var frame = Render<LabFrame>(parameters => parameters
            .Add(component => component.Slug, "state")
            .Add(component => component.OnReset, () => fail = false)
            .Add(component => component.ChildContent, builder =>
            {
                builder.OpenComponent<RecoverableContent>(0);
                builder.AddAttribute(1, nameof(RecoverableContent.Fail), fail);
                builder.CloseComponent();
            }));
        frame.FindAll("button").Single(button => button.TextContent.Contains("Reset and retry")).Click();
        Assert.Contains("Recovered content", frame.Markup);
    }

    private sealed class DelayedResponse : HttpMessageHandler
    {
        public TaskCompletionSource<HttpResponseMessage> Response { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Response.Task;
    }

    public sealed class RecoverableContent : ComponentBase
    {
        [Parameter] public bool Fail { get; set; }
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (Fail) throw new InvalidOperationException("Simulated experiment error.");
            builder.AddContent(0, "Recovered content");
        }
    }
}
