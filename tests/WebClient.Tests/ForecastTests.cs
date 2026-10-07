using System.ComponentModel.DataAnnotations;
using WebClient.Shared.Features.Forecasts;
using WebClient.Shared.Features.Learning;

namespace WebClient.Tests;

public sealed class ForecastTests
{
    [Fact]
    public void Drafts_preserve_identity_and_commit_only_valid_values()
    {
        var original = new WeatherForecast();
        Assert.NotEqual(Guid.Empty, original.Id);
        Assert.NotNull(original.Date);
        var draft = original.Copy();
        draft.TemperatureC = 100;
        draft.Summary = "";
        Assert.Throws<ValidationException>(() => original.Apply(draft));
        Assert.Equal(20, original.TemperatureC);
        draft.Summary = "Hot";
        original.Apply(draft);
        Assert.Equal(draft.Id, original.Id);
        Assert.Equal(212, original.TemperatureF);
    }

    [Fact]
    public void Seeded_generation_and_paging_are_repeatable()
    {
        var one = ForecastData.Generate(10, 42);
        var two = ForecastData.Generate(10, 42);
        Assert.Equal(one.Select(r => r.Id), two.Select(r => r.Id));
        Assert.NotEqual(one[0].Id, ForecastData.Generate(10, 43)[0].Id);
        var catalog = new ForecastCatalog();
        var page = catalog.Read(new ForecastQuery { Count = 10, PageSize = 3, Page = 1 });
        Assert.Equal(10, page.Total);
        Assert.Equal(one.Skip(3).Take(3).Select(r => r.Id), page.Items.Select(r => r.Id));
        Assert.Empty(catalog.Read(new ForecastQuery { Search = "absent phrase" }).Items);
        Assert.Throws<ArgumentOutOfRangeException>(() => ForecastData.Generate(ForecastData.MaxCount + 1));
    }

    [Fact]
    public void Csv_roundtrips_quotes_newlines_and_culture_independent_values()
    {
        var rows = ForecastData.Generate(2);
        rows[0].Summary = "Rain, \"clouds\"\ntonight";
        var imported = ForecastCsv.Parse(ForecastCsv.Export(rows));
        Assert.Equal(rows.Select(r => (r.Id, r.Date, r.TemperatureC, r.Summary)), imported.Select(r => (r.Id, r.Date, r.TemperatureC, r.Summary)));
        rows[0].Summary = "=1+1";
        Assert.Contains("\"'=1+1\"", ForecastCsv.Export(rows));
    }

    [Theory]
    [InlineData("=1+1")]
    [InlineData("-5 and windy")]
    [InlineData("'quoted'")]
    [InlineData("''")]
    [InlineData("Mild")]
    public void Csv_escaping_survives_a_round_trip_at_the_length_limit(string summary)
    {
        var rows = ForecastData.Generate(2);
        rows[0].Summary = summary;
        rows[1].Summary = summary[0] + new string('x', 119);
        var imported = ForecastCsv.Parse(ForecastCsv.Export(rows));
        Assert.Equal(rows.Select(r => r.Summary), imported.Select(r => r.Summary));
    }

    [Fact]
    public void Csv_ignores_blank_lines()
    {
        var rows = ForecastData.Generate(2);
        var text = ForecastCsv.Export(rows).Replace("\n", "\r\n\r\n", StringComparison.Ordinal) + "\n";
        Assert.Equal(rows.Select(r => r.Id), ForecastCsv.Parse(text).Select(r => r.Id));
    }

    [Theory]
    [InlineData("en-US", "1/2/2026")]
    [InlineData("pt-BR", "02/01/2026")]
    [InlineData("pt-BR", "2026-01-02")]
    public void Api_search_uses_the_callers_culture(string culture, string search)
    {
        var page = new ForecastCatalog().Read(new ForecastQuery { Count = 100, Search = search, Culture = culture });
        Assert.Equal(new DateTime(2026, 1, 2), Assert.Single(page.Items).Date);
    }

    [Fact]
    public void Api_rejects_unsupported_cultures_and_orders_by_direction()
    {
        Assert.Contains("culture", new ForecastQuery { Culture = "fr-FR" }.Validate().Keys);
        var catalog = new ForecastCatalog();
        var newest = catalog.Read(new ForecastQuery { Count = 10, Descending = true, PageSize = 10 });
        Assert.Equal(ForecastData.Generate(10).Select(r => r.Id).Reverse(), newest.Items.Select(r => r.Id));
    }

    [Theory]
    [InlineData("Id,Date,TemperatureC,Summary\nnot-a-guid,2026-01-01,20,Mild")]
    [InlineData("Id,Date,TemperatureC,Summary\n00000000-0000-0000-0000-000000000000,2026-01-01,20,Mild")]
    [InlineData("Id,Date,TemperatureC,Summary\n\"unclosed")]
    [InlineData("Id,Date,TemperatureC,Summary\n\"id\"garbage,2026-01-01,20,Mild")]
    public void Csv_rejects_invalid_records_before_returning_a_dataset(string text) => Assert.Throws<FormatException>(() => ForecastCsv.Parse(text));

    [Fact]
    public void Csv_enforces_size_row_and_identity_limits()
    {
        Assert.Throws<FormatException>(() => ForecastCsv.Parse(new string('x', ForecastCsv.MaxBytes + 1)));
        Assert.Throws<FormatException>(() => ForecastCsv.Parse(ForecastCsv.Export(ForecastData.Generate(ForecastCsv.MaxRows + 1))));
        var row = ForecastData.Generate(1)[0];
        Assert.Throws<FormatException>(() => ForecastCsv.Parse(ForecastCsv.Export([row, row])));
    }

    [Fact]
    public async Task Work_reports_completion_and_stops_after_cancellation()
    {
        var progress = new List<int>();
        await BatchExperiment.RunAsync(2, value => { progress.Add(value); return Task.CompletedTask; }, CancellationToken.None);
        Assert.Equal([50, 100], progress);
        using var cancellation = new CancellationTokenSource();
        progress.Clear();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => BatchExperiment.RunAsync(100, value =>
        {
            progress.Add(value); cancellation.Cancel(); return Task.CompletedTask;
        }, cancellation.Token));
        Assert.Single(progress);
    }
}
