namespace Modules.Identity.Core.Constants;

public static class IdentityConstants
{
    public const string PatientRole = "Patient";
    public const string DoctorRole = "Doctor";
    
    public const string JwtIssuer = "HealMe";
    public const string JwtAudience = "HealMe-Users";
    
    public const int DefaultTokenExpirationMinutes = 60;
    public const int RefreshTokenExpirationDays = 7;
}