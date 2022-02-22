using System;
using System.Collections.Generic;

namespace LoadCbrData.Data.Models
{
    public partial class Record
    {
        public long Id { get; set; }
        public string? Code { get; set; }
        public string? Inn { get; set; }
        public string? Ogrn { get; set; }
        public string? Name { get; set; }
        public string? ShortName { get; set; }
        public string? Data { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
