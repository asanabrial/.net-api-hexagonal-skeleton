using System;

namespace HexagonalSkeleton.Test.TestData
{
    /// <summary>
    /// Default test data for user entities
    /// Contains standard test user information for consistent testing
    /// </summary>
    public static class DefaultUserTestData
    {
        public static class User
        {
            public const string PhoneNumber = "+1234567890";
            public const double Latitude = 40.7128;
            public const double Longitude = -74.006;
            public const string DefaultPassword = "TestPassword123!";
            
            public static DateTime DefaultBirthdate => new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }
        
        public static class Location
        {
            public static class NewYork
            {
                public const double Latitude = 40.7128;
                public const double Longitude = -74.006;
            }
            
            public static class London
            {
                public const double Latitude = 51.5074;
                public const double Longitude = -0.1278;
            }
            
            public static class LosAngeles
            {
                public const double Latitude = 34.0522;
                public const double Longitude = -118.2437;
            }
        }
    }
}
