namespace Modules.Doctors.Core;

public class Specialization
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    
    public ICollection<DoctorProfile> Doctors { get; set; }
}