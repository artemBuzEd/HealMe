namespace Modules.AI.Core.DTOs;

public class AnamnesisPdfData
{
    public string PatientFullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string PatientEmail { get; set; } = string.Empty;
    public string PatientPhone { get; set; } = string.Empty;
    public string SessionTitle { get; set; } = string.Empty;
    public DateTime SessionDate { get; set; }
    public string RawAnamnesis { get; set; } = string.Empty;
    public List<AnamnesisSection> Sections { get; set; } = new();
}

public class AnamnesisSection
{
    public string Title { get; set; } = string.Empty;
    public List<string> Items { get; set; } = new();
}
