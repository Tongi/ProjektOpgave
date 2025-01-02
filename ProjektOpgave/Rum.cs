namespace TemperaturValidering
{
    public class Rum
    {
        public string Navn { get; set; }
        public double Temperatur { get; set; }
        public string Type { get; set; }
        public string Person { get; set; }
        public bool LysTændt { get; set; }
        public double Energiforbrug { get; set; }
        public double? FastTemperatur { get; set; }
    }
}
