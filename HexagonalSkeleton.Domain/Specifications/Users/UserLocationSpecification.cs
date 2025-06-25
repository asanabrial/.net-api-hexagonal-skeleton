using System.Linq.Expressions;
using HexagonalSkeleton.Domain.Specifications;

namespace HexagonalSkeleton.Domain.Specifications.Users
{
    /// <summary>
    /// Specification for filtering users by geographical location within a radius
    /// Useful for proximity-based searches and location-aware features
    /// </summary>
    public class UserLocationSpecification : Specification<User>
    {
        private readonly double _centerLatitude;
        private readonly double _centerLongitude;
        private readonly double _radiusInKm;

        public UserLocationSpecification(double centerLatitude, double centerLongitude, double radiusInKm)
        {
            if (radiusInKm <= 0)
                throw new ArgumentException("Radius must be positive", nameof(radiusInKm));

            _centerLatitude = centerLatitude;
            _centerLongitude = centerLongitude;
            _radiusInKm = radiusInKm;
        }

        public override Expression<Func<User, bool>> ToExpression()
        {
            // Note: This is a simplified distance calculation
            // For production, consider using more accurate geographic distance formulas
            // or database-specific geographic functions
            
            return user => 
                Math.Sqrt(
                    Math.Pow(user.Location.Latitude - _centerLatitude, 2) + 
                    Math.Pow(user.Location.Longitude - _centerLongitude, 2)
                ) * 111 <= _radiusInKm; // Rough conversion to km (1 degree ≈ 111 km)
        }
    }
}
