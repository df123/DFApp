using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace DFApp.ElectricVehicle
{
    public class GasolinePrice : AuditedAggregateRoot<Guid>
    {
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
    }
}
