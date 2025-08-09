namespace HexagonalSkeleton.Domain.Common;

/// <summary>
/// Utility class for age-related calculations
/// Centralizes age calculation logic to follow DRY principle
/// </summary>
public static class AgeCalculator
{
    /// <summary>
    /// Calculates the age based on birthdate
    /// Uses the standard calculation that accounts for whether the birthday has occurred this year
    /// </summary>
    /// <param name="birthdate">The date of birth</param>
    /// <param name="referenceDate">The reference date to calculate age from (defaults to today)</param>
    /// <returns>The calculated age in years</returns>
    public static int CalculateAge(DateTime birthdate, DateTime? referenceDate = null)
    {
        var reference = referenceDate ?? DateTime.Today;
        var age = reference.Year - birthdate.Year;
        
        // Subtract one year if the birthday hasn't occurred yet this year
        if (birthdate.Date > reference.AddYears(-age))
            age--;
            
        return age;
    }

    /// <summary>
    /// Validates if a person is at least the specified minimum age
    /// </summary>
    /// <param name="birthdate">The date of birth</param>
    /// <param name="minimumAge">The minimum required age</param>
    /// <param name="referenceDate">The reference date to calculate age from (defaults to today)</param>
    /// <returns>True if the person is at least the minimum age</returns>
    public static bool IsAtLeastAge(DateTime birthdate, int minimumAge, DateTime? referenceDate = null)
    {
        return CalculateAge(birthdate, referenceDate) >= minimumAge;
    }

    /// <summary>
    /// Validates if a person is within the specified age range
    /// </summary>
    /// <param name="birthdate">The date of birth</param>
    /// <param name="minimumAge">The minimum age (inclusive)</param>
    /// <param name="maximumAge">The maximum age (inclusive)</param>
    /// <param name="referenceDate">The reference date to calculate age from (defaults to today)</param>
    /// <returns>True if the person is within the age range</returns>
    public static bool IsWithinAgeRange(DateTime birthdate, int minimumAge, int maximumAge, DateTime? referenceDate = null)
    {
        var age = CalculateAge(birthdate, referenceDate);
        return age >= minimumAge && age <= maximumAge;
    }
}
