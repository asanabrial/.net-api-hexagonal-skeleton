using MongoDB.Driver.GeoJsonObjectModel;
using HexagonalSkeleton.Domain.ValueObjects;

namespace HexagonalSkeleton.Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for location-related operations
    /// </summary>
    public static class LocationExtensions
    {
        /// <summary>
        /// Extension method to get latitude from a GeoJsonPoint
        /// </summary>
        public static double Latitude(this GeoJsonPoint<GeoJson2DGeographicCoordinates> point)
        {
            return point?.Coordinates?.Latitude ?? 0;
        }

        /// <summary>
        /// Extension method to get longitude from a GeoJsonPoint
        /// </summary>
        public static double Longitude(this GeoJsonPoint<GeoJson2DGeographicCoordinates> point)
        {
            return point?.Coordinates?.Longitude ?? 0;
        }

        /// <summary>
        /// Creates a GeoJsonPoint from latitude and longitude
        /// </summary>
        public static GeoJsonPoint<GeoJson2DGeographicCoordinates> CreateGeoJsonPoint(double latitude, double longitude)
        {
            return new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
                new GeoJson2DGeographicCoordinates(longitude, latitude));
        }
        
        /// <summary>
        /// Creates a GeoJsonPoint from a domain Location
        /// </summary>
        public static GeoJsonPoint<GeoJson2DGeographicCoordinates> ToGeoJsonPoint(this Location location)
        {
            if (location == null)
                return CreateGeoJsonPoint(0, 0);
                
            return new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
                new GeoJson2DGeographicCoordinates(location.Longitude, location.Latitude));
        }
    }
}