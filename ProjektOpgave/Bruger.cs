namespace TemperaturValidering
{
    public class Bruger
    {
        public string Navn { get; set; }
        public bool ErAdmin { get; set; } // Kun admin-brugere (Far og Mor) kan indstille fast temperatur

        public Bruger(string navn, bool erAdmin)
        {
            Navn = navn;
            ErAdmin = erAdmin;
        }
    }
}
