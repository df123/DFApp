using System;

namespace DFApp.ElectricVehicle
{
    public class GasolinePriceDto
    {
        public Guid Id { get; set; }
        public string Province { get; set; }
        public DateTime Date { get; set; }
        public decimal? Price0H { get; set; }
        public decimal? Price89H { get; set; }
        public decimal? Price90H { get; set; }
        public decimal? Price92H { get; set; }
        public decimal? Price93H { get; set; }
        public decimal? Price95H { get; set; }
        public decimal? Price97H { get; set; }
        public decimal? Price98H { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
