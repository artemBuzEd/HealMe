namespace Modules.Doctors.Infrastructure;

public class NotFoundException : Exception
{
    public NotFoundException(string id, string entity, string message = "") : base($"Entity [{entity}] not found for id: {id}, with message: [{message}]")
    {
        
    }
}