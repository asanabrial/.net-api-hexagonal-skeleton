namespace HexagonalSkeleton.Domain.ValueObjects
{
    public record Location
    {
        public double Latitude { get; }
        public double Longitude { get; }

        public Location(double latitude, double longitude)
        {
            if (latitude < -90 || latitude > 90)
                throw new ArgumentException("Latitude must be between -90 and 90", nameof(latitude));

            if (longitude < -180 || longitude > 180)
                throw new ArgumentException("Longitude must be between -180 and 180", nameof(longitude));

            Latitude = latitude;
            Longitude = longitude;
        }        public double CalculateDistanceTo(Location other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            const double earthRadius = 6371; // km

            var lat1Rad = Latitude * Math.PI / 180;
            var lat2Rad = other.Latitude * Math.PI / 180;
            var deltaLat = (other.Latitude - Latitude) * Math.PI / 180;
            var deltaLon = (other.Longitude - Longitude) * Math.PI / 180;

            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                    Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                    Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadius * c;
        }

        public override string ToString() => $"({Latitude.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)}, {Longitude.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)})";
    }
}
