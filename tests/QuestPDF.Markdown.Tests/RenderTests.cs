using System.Reflection;
using NUnit.Framework;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;

namespace QuestPDF.Markdown.Tests;

[Explicit("These tests are disabled for automated workflows because they open a QuestPDF previewer window")]
public class RenderTests
{
    private string _markdown = string.Empty;
    
    [SetUp]
    public void Setup()
    {
        Settings.License = LicenseType.Community;
        var assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "QuestPDF.Markdown.Tests.test.md";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream!);
        _markdown = reader.ReadToEnd();
    }

    [Test]
    public async Task RenderToFile()
    {
        var markdown = ParsedMarkdownDocument.FromText(_markdown);
        await markdown.DownloadImages().ConfigureAwait(false);
        
        var document = GenerateDocument(item => item.Markdown(markdown));
        document.GeneratePdf(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "test.pdf"));
    }
    
    [Test]
    public async Task Render()
    {
        var markdown = ParsedMarkdownDocument.FromText(_markdown);
        await markdown.DownloadImages().ConfigureAwait(false);
        
        var document = GenerateDocument(item => item.Markdown(markdown));
        
        try
        {
            await document.ShowInPreviewerAsync().ConfigureAwait(true);
        }
        catch(OperationCanceledException)
        {
            // Ignore
        }
    }
    
    [Test]
    public async Task RenderDebug()
    {
        var options = new MarkdownRendererOptions { Debug = true };
        
        var markdown = ParsedMarkdownDocument.FromText(_markdown);
        await markdown.DownloadImages().ConfigureAwait(false);
        
        var document = GenerateDocument(item => item.Markdown(markdown, options));
        
        try
        {
            await document.ShowInPreviewerAsync().ConfigureAwait(true);    
        }
        catch(OperationCanceledException)
        {
            // Ignore
        }
    }

    [Test]
    public async Task RenderEquations()
    {
        var options = new MarkdownRendererOptions { Debug = true };

        var singleMd = @"If $u = u(x)$ and $du = u'(x)dx$, while $v=v(x)$ and
        $dv = v'(x)\:dx$, then integration by parts states that:

$$
        \begin{array}{rcl}
            \int^b_au(x)v'(x)\:dx &=& \left[ u(x)v(x) \right]^b_a - \int^b_a u'(x)v(x)\:dx \\
                                  &=& u(b)v(b) - u(a)v(a) - \int^b_a u'(x)v(x)\:dx
        \end{array}
$$

or more compactly: $[\int u\:dv = uv - \int v \:du.]$


If $u = u(x)$ and $du = u'(x)dx$, while $v=v(x)$ and
        $dv = v'(x)\:dx$, then integration by parts states that:

$$
        \begin{array}{rcl}
            \int^b_au(x)v'(x)\:dx &=& \left[ u(x)v(x) \right]^b_a - \int^b_a u'(x)v(x)\:dx \\
                                  &=& u(b)v(b) - u(a)v(a) - \int^b_a u'(x)v(x)\:dx
        \end{array}
$$

or more compactly: $[\int u\:dv = uv - \int v \:du.]$";
        var markdown = ParsedMarkdownDocument.FromText(singleMd);

        var document = GenerateDocument(item => item.Markdown(markdown, options));
        await document.ShowInPreviewerAsync().ConfigureAwait(true);

    }

    private static Document GenerateDocument(Action<IContainer> body)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(text =>
                {
                    text.FontFamily(Fonts.Arial);
                    text.LineHeight(1.5f);
                    return text;
                });

                page.Content()
                    .PaddingVertical(20)
                    .Column(main =>
                    {
                        main.Spacing(20);
                        body(main.Item());
                    });
            });
        }).WithMetadata(new DocumentMetadata
        {
            Author = "QuestPDF.Markdown",
            Title = "QuestPDF.Markdown",
            Subject = "QuestPDF.Markdown",
            Keywords = "questpdf, markdown, pdf",
            CreationDate = new DateTime(2023, 11, 15, 12, 00, 00),
            ModifiedDate = new DateTime(2023, 11, 15, 12, 00, 00),
        });
    }
}