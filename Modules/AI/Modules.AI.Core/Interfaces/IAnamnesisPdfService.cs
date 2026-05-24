using Modules.AI.Core.DTOs;

namespace Modules.AI.Core.Interfaces;

public interface IAnamnesisPdfService
{
    byte[] GenerateDoctorPdf(AnamnesisPdfData data);
    byte[] GeneratePatientPdf(AnamnesisPdfData data);
}
