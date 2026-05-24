using Modules.AI.Core.DTOs;
using Modules.AI.Core.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Modules.AI.Infrastructure.Services;

public class AnamnesisPdfService : IAnamnesisPdfService
{
    public byte[] GenerateDoctorPdf(AnamnesisPdfData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(c => ComposeDoctorHeader(c, data));
                page.Content().Element(c => ComposeDoctorContent(c, data));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GeneratePatientPdf(AnamnesisPdfData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(c => ComposePatientHeader(c, data));
                page.Content().Element(c => ComposePatientContent(c, data));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    // ── Doctor PDF ──────────────────────────────────────────────

    private void ComposeDoctorHeader(IContainer container, AnamnesisPdfData data)
    {
        container.Column(column =>
        {
            column.Item().BorderBottom(2).BorderColor(Colors.Blue.Darken3).PaddingBottom(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("АНАМНЕЗ ПАЦІЄНТА").FontSize(22).Bold().FontColor(Colors.Blue.Darken3);
                    col.Item().Text("HealMe Medical Platform").FontSize(10).FontColor(Colors.Grey.Darken1);
                });

                row.ConstantItem(150).AlignRight().Column(col =>
                {
                    col.Item().Text($"Дата: {data.SessionDate:dd.MM.yyyy}").FontSize(9).AlignRight();
                    col.Item().Text($"Час: {data.SessionDate:HH:mm}").FontSize(9).AlignRight();
                });
            });

            // Patient info block
            column.Item().PaddingTop(15).Background(Colors.Grey.Lighten4).Padding(12).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("ДАНІ ПАЦІЄНТА").FontSize(10).Bold().FontColor(Colors.Blue.Darken3);
                    col.Item().PaddingTop(5).Text($"ПІБ: {data.PatientFullName}").FontSize(10);
                    if (data.DateOfBirth.HasValue)
                        col.Item().Text($"Дата народження: {data.DateOfBirth.Value:dd.MM.yyyy}").FontSize(10);
                    col.Item().Text($"Стать: {data.Gender}").FontSize(10);
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("КОНТАКТИ").FontSize(10).Bold().FontColor(Colors.Blue.Darken3);
                    col.Item().PaddingTop(5).Text($"Email: {data.PatientEmail}").FontSize(10);
                    col.Item().Text($"Телефон: {data.PatientPhone}").FontSize(10);
                });
            });

            column.Item().PaddingTop(10);
        });
    }

    private void ComposeDoctorContent(IContainer container, AnamnesisPdfData data)
    {
        container.PaddingTop(5).Column(column =>
        {
            foreach (var section in data.Sections)
            {
                // Section header
                column.Item().PaddingTop(8).BorderBottom(1).BorderColor(Colors.Blue.Lighten3)
                    .PaddingBottom(3)
                    .Text(section.Title).FontSize(13).Bold().FontColor(Colors.Blue.Darken3);

                // Section items
                foreach (var item in section.Items)
                {
                    column.Item().PaddingTop(3).PaddingLeft(10).Text($"• {item}").FontSize(10);
                }

                column.Item().PaddingTop(5);
            }
        });
    }
    

    private void ComposePatientHeader(IContainer container, AnamnesisPdfData data)
    {
        container.Column(column =>
        {
            column.Item().BorderBottom(2).BorderColor(Colors.Teal.Darken1).PaddingBottom(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Підсумок консультації").FontSize(20).Bold().FontColor(Colors.Teal.Darken1);
                    col.Item().Text("HealMe").FontSize(10).FontColor(Colors.Grey.Darken1);
                });

                row.ConstantItem(150).AlignRight().Column(col =>
                {
                    col.Item().Text($"{data.SessionDate:dd MMMM yyyy}").FontSize(10).AlignRight();
                });
            });

            column.Item().PaddingTop(10).Text(text =>
            {
                text.Span($"{data.PatientFullName}").FontSize(11);
                text.Span(", нижче наведено підсумок вашої останньої консультації з медичним асистентом HealMe.")
                    .FontSize(10).FontColor(Colors.Grey.Darken2);
            });

            column.Item().PaddingTop(10);
        });
    }

    private void ComposePatientContent(IContainer container, AnamnesisPdfData data)
    {
        container.PaddingTop(5).Column(column =>
        {
            foreach (var section in data.Sections)
            {
                column.Item().PaddingTop(10).Background(Colors.Teal.Lighten5).Padding(10).Column(block =>
                {
                    block.Item().Text(section.Title).FontSize(12).Bold().FontColor(Colors.Teal.Darken2);

                    foreach (var item in section.Items)
                    {
                        block.Item().PaddingTop(4).PaddingLeft(5).Text($"• {item}").FontSize(10);
                    }
                });
            }

            // Disclaimer
            column.Item().PaddingTop(20).BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(10)
                .Text("Цей документ згенеровано автоматично системою HealMe. " +
                      "Він не є медичним діагнозом. Для отримання медичної допомоги зверніться до кваліфікованого лікаря.")
                .FontSize(8).Italic().FontColor(Colors.Grey.Darken1);
        });
    }

    // ── Footer ──────────────────────────────────────────────────

    private void ComposeFooter(IContainer container)
    {
        container.BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(5).Row(row =>
        {
            row.RelativeItem().Text("HealMe Medical Platform").FontSize(8).FontColor(Colors.Grey.Medium);
            row.RelativeItem().AlignRight().Text(text =>
            {
                text.Span("Сторінка ").FontSize(8).FontColor(Colors.Grey.Medium);
                text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                text.Span(" з ").FontSize(8).FontColor(Colors.Grey.Medium);
                text.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });
    }
}
