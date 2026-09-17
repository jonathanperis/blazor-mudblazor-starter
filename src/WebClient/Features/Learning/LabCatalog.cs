namespace WebClient.Features.Learning;

public sealed record Lab(string Slug, string Title, string Route, string Level, int Minutes, string Prerequisites,
    string Objective, string Explanation, string Pitfall, string Exercise, string Source);

public static class LabCatalog
{
    public static readonly IReadOnlyList<Lab> All =
    [
        new("state", "State and lifecycle", "/counter", "Beginner", 10, "Run the app",
            "Compare component state, parameters, event callbacks, and circuit-scoped state.",
            "The child raises an EventCallback; the parent owns the value. A scoped service survives route navigation within a Blazor Server circuit. A fresh page load creates a new circuit.",
            "Scoped means circuit lifetime for interactive server components, not one HTTP request. Prerendering and interactivity use different component instances.",
            "Increment both values, visit another lab, and return. Predict which value survives. Then reload the page.", "Counter.razor"),
        new("forms", "Forms and transactional dialogs", "/labs/forms", "Beginner", 15, "State and lifecycle",
            "Validate input and commit a draft only when a dialog is confirmed.",
            "EditForm validates data annotations. MudBlazor For expressions connect field feedback to the model. Editing a copy keeps Cancel meaningful.",
            "Binding a dialog directly to the live object mutates it before the user confirms.",
            "Clear Summary and try saving. Then change a value and cancel. The displayed record should stay unchanged.", "Labs/Forms.razor"),
        new("grid", "DataGrid experiments", "/weather", "Intermediate", 20, "Forms and dialogs",
            "Explore typed filtering, sorting, selection, virtualization, and CRUD at different dataset sizes.",
            "This grid holds its dataset in circuit memory. Virtualization reduces rendered rows, not allocation or filtering work. Compare it with the API lab's paged data.",
            "A formatted string property gives a date column text filters. Bind the date value and use Format for presentation.",
            "Compare 100 and 69,420 rows with the same seed. Search, select, edit, copy, and reset. Measure on your own machine.", "Weather.razor"),
        new("api", "API and server paging", "/labs/api", "Intermediate", 20, "DataGrid experiments",
            "Use a typed HttpClient, server-side paging, and explicit loading, empty, failure and cancellation states.",
            "The API owns a deterministic read-only dataset. Only a bounded page crosses HTTP. Each request accepts a cancellation token and validates query bounds.",
            "A late response can overwrite a newer search unless the previous request is canceled and its result is ignored.",
            "Set latency, load and cancel; then simulate a failure. Search for an absent summary and inspect the empty state.", "Labs/Api.razor"),
        new("persistence", "SQLite notebook", "/labs/persistence", "Intermediate", 25, "Forms and API",
            "Persist learner-owned notes using EF Core, migrations, and optimistic concurrency.",
            "Each operation creates a short-lived DbContext. A protected workspace cookie scopes queries. A version token detects edits from an older snapshot.",
            "A long-lived circuit should not hold a DbContext. Two tabs can read the same version and attempt conflicting saves.",
            "Create a note, open this lab in a second tab, and edit the same note in both. Save one, then the other. Reload after the conflict.", "Labs/Persistence.razor"),
        new("auth", "Authentication and policies", "/labs/auth", "Intermediate", 20, "HTTP forms and cookies",
            "Compare anonymous, student, and instructor access with server-enforced policies.",
            "This local lab issues demo cookies for fixed personas. Antiforgery protects form posts. The instructor endpoint requires a policy, regardless of what the UI displays.",
            "Hiding a button is not authorization. Demo personas are a teaching tool; real accounts require an identity provider.",
            "Try the instructor endpoint anonymously, as a student, and as an instructor. Inspect the 401, 403, and 200 responses.", "Labs/Auth.razor"),
        new("localization", "Localization and accessibility", "/labs/localization", "Beginner", 15, "State and lifecycle",
            "Switch English and Portuguese, compare culture formatting, and practice keyboard-friendly UI.",
            "Request localization reads a culture cookie. A full reload applies the chosen culture to a new circuit. MudBlazor translations and this example use that culture.",
            "Invariant globalization prevents useful culture experiments. An icon or color alone is not an accessible name.",
            "Switch language, compare date/number output, toggle the theme, and complete the form using only the keyboard.", "Labs/Localization.razor"),
        new("files", "Files and cancellable work", "/labs/files", "Intermediate", 25, "Forms and API",
            "Import bounded CSV atomically, export escaped fields, and cancel asynchronous server work with progress.",
            "The entire import is parsed and validated before replacing the sample. Processing belongs to this component and is canceled when it is disposed; it is not a durable job queue.",
            "Splitting CSV on commas breaks quoted fields. Unbounded uploads and work that outlives its owner waste server resources.",
            "Export, edit a quoted summary, and import it. Try an invalid row. Start processing and cancel halfway through.", "Labs/Files.razor"),
        new("observability", "Observability and deployment", "/labs/observability", "Intermediate", 20, "API and Docker",
            "Connect request IDs, structured logs, health checks, and an optional Azure deployment.",
            "The diagnostics endpoint logs a named event and returns its trace ID. Liveness checks the host; readiness checks SQLite. Application Insights is optional.",
            "A healthy process does not prove that a UI interaction works. Mutable image tags make deployments hard to reproduce.",
            "Send a diagnostic request, locate its trace ID in console logs, and follow the Docker and immutable-image deployment guide.", "Labs/Observability.razor")
    ];

    public static Lab Get(string slug) => All.Single(lab => lab.Slug == slug);
}
