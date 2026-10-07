using MudBlazor;

namespace WebClient.Shared.Features.Learning;

/// <summary>
/// The sandbox's visual identity: a printed lab manual. Warm paper, ink, one vermilion accent, a serif for
/// headings, and IBM Plex for text and code. Fonts are self-hosted in wwwroot/fonts.
/// </summary>
public static class LearningTheme
{
    private static readonly string[] Sans = ["IBM Plex Sans", "ui-sans-serif", "system-ui", "-apple-system", "Segoe UI", "sans-serif"];
    private static readonly string[] Serif = ["Fraunces", "ui-serif", "Georgia", "Cambria", "serif"];
    private static readonly string[] Mono = ["IBM Plex Mono", "ui-monospace", "SFMono-Regular", "Menlo", "Consolas", "monospace"];

    public static MudTheme Theme { get; } = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#B4441F", PrimaryContrastText = "#FFF8F2",
            Secondary = "#1F5F5B", SecondaryContrastText = "#F2FBF9",
            Tertiary = "#7A5C14", TertiaryContrastText = "#FFF9EC",
            Info = "#2B5C8A", Success = "#2F6B3A", Warning = "#9A5B0B", Error = "#A8261B", Dark = "#2A2824",
            Black = "#1C1B19",
            Background = "#F4F1EA", BackgroundGray = "#ECE8DF", Surface = "#FBFAF6",
            AppbarBackground = "#1C1B19", AppbarText = "#F4F1EA",
            DrawerBackground = "#EFEBE2", DrawerText = "#2A2824", DrawerIcon = "#5E5A52",
            TextPrimary = "#1C1B19", TextSecondary = "#5E5A52", TextDisabled = "#9C978C",
            ActionDefault = "#5E5A52", ActionDisabled = "#B3AEA3", ActionDisabledBackground = "#E4DFD5",
            LinesDefault = "#DDD7CB", LinesInputs = "#8E877A", TableLines = "#E4DFD5", TableStriped = "#F1EDE5", TableHover = "#ECE7DC",
            Divider = "#DDD7CB", DividerLight = "#E9E4DA", Skeleton = "#E6E1D7",
            HoverOpacity = 0.06
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#FF8A5C", PrimaryContrastText = "#1A0D07",
            Secondary = "#6FC3B8", SecondaryContrastText = "#06201D",
            Tertiary = "#E0B85C", TertiaryContrastText = "#231A04",
            Info = "#7EB3E6", Success = "#7CC489", Warning = "#E8AA55", Error = "#F28B7F", Dark = "#D8D2C6", DarkContrastText = "#141311",
            InfoContrastText = "#0B1A2A", SuccessContrastText = "#0C1F10", WarningContrastText = "#231503", ErrorContrastText = "#2A0A06",
            Black = "#0E0D0C",
            Background = "#141311", BackgroundGray = "#1A1916", Surface = "#1C1B18",
            AppbarBackground = "#0E0D0C", AppbarText = "#ECE7DD",
            DrawerBackground = "#171614", DrawerText = "#D8D2C6", DrawerIcon = "#A9A397",
            TextPrimary = "#ECE7DD", TextSecondary = "#A9A397", TextDisabled = "#6E695F",
            ActionDefault = "#A9A397", ActionDisabled = "#5A564E", ActionDisabledBackground = "#2A2824",
            LinesDefault = "#34312C", LinesInputs = "#6E685D", TableLines = "#2E2B27", TableStriped = "#1F1E1B", TableHover = "#25231F",
            Divider = "#34312C", DividerLight = "#2A2824", Skeleton = "#2A2824",
            HoverOpacity = 0.08
        },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = Sans, FontSize = ".9375rem", LineHeight = "1.6", LetterSpacing = "normal" },
            H1 = new H1Typography { FontFamily = Serif, FontWeight = "500", FontSize = "3.5rem", LineHeight = "1.05", LetterSpacing = "-.02em" },
            H2 = new H2Typography { FontFamily = Serif, FontWeight = "500", FontSize = "2.75rem", LineHeight = "1.08", LetterSpacing = "-.02em" },
            H3 = new H3Typography { FontFamily = Serif, FontWeight = "500", FontSize = "2.25rem", LineHeight = "1.1", LetterSpacing = "-.015em" },
            H4 = new H4Typography { FontFamily = Serif, FontWeight = "500", FontSize = "1.75rem", LineHeight = "1.15", LetterSpacing = "-.01em" },
            H5 = new H5Typography { FontFamily = Serif, FontWeight = "500", FontSize = "1.375rem", LineHeight = "1.25", LetterSpacing = "-.005em" },
            H6 = new H6Typography { FontFamily = Sans, FontWeight = "600", FontSize = "1.0625rem", LineHeight = "1.35", LetterSpacing = "normal" },
            Subtitle1 = new Subtitle1Typography { FontFamily = Sans, FontWeight = "500", FontSize = "1rem", LineHeight = "1.5" },
            Subtitle2 = new Subtitle2Typography { FontFamily = Sans, FontWeight = "600", FontSize = ".875rem", LineHeight = "1.5" },
            Body1 = new Body1Typography { FontFamily = Sans, FontSize = ".9375rem", LineHeight = "1.6" },
            Body2 = new Body2Typography { FontFamily = Sans, FontSize = ".875rem", LineHeight = "1.55" },
            // Sentence-case buttons read as words, not shouting.
            Button = new ButtonTypography { FontFamily = Sans, FontWeight = "500", FontSize = ".875rem", LineHeight = "1.6", LetterSpacing = ".01em", TextTransform = "none" },
            Caption = new CaptionTypography { FontFamily = Sans, FontSize = ".78rem", LineHeight = "1.5", LetterSpacing = ".01em" },
            Overline = new OverlineTypography { FontFamily = Mono, FontWeight = "500", FontSize = ".72rem", LineHeight = "1.8", LetterSpacing = ".08em", TextTransform = "uppercase" }
        },
        LayoutProperties = new LayoutProperties { DefaultBorderRadius = "6px", AppbarHeight = "56px", DrawerWidthLeft = "264px" }
    };
}
