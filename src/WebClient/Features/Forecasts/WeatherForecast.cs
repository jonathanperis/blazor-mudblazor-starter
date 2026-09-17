using System.ComponentModel.DataAnnotations;

namespace WebClient.Features.Forecasts;

public sealed class WeatherForecast : IValidatableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public DateTime? Date { get; set; } = DateTime.Today;

    [Required, Range(-100, 100, ErrorMessage = "Use a temperature between -100 and 100 °C for this lab.")]
    public int? TemperatureC { get; set; } = 20;

    [Required, StringLength(120)]
    public string Summary { get; set; } = "Mild";

    public int? TemperatureF => TemperatureC is { } c ? (int)Math.Round(c * 9d / 5 + 32) : null;

    public WeatherForecast Copy() => new() { Id = Id, Date = Date, TemperatureC = TemperatureC, Summary = Summary };

    public void Apply(WeatherForecast draft)
    {
        Validator.ValidateObject(draft, new ValidationContext(draft), validateAllProperties: true);
        Date = draft.Date;
        TemperatureC = draft.TemperatureC;
        Summary = draft.Summary.Trim();
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Id == Guid.Empty)
            yield return new ValidationResult("A record needs an identity.", [nameof(Id)]);
        if (Date == DateTime.MinValue)
            yield return new ValidationResult("Choose a date.", [nameof(Date)]);
    }
}
