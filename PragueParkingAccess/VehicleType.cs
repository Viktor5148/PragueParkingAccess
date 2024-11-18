namespace PragueParkingAccess
{
    public class VehicleType
    {
        public string Type { get; init; }
        public int Size { get; init; }

        public override string ToString()
        {
            return $"{Type} (Size: {Size})";
        }
    }
}
