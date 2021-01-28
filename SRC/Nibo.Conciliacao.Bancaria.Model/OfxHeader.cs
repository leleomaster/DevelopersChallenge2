using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nibo.Conciliacao.Bancaria.Models
{
    public class OfxHeader
    {
        public int Ofxheader { get; set; }
        public string  Data { get; set; }
        public string  Version { get; set; }
        public string  Security { get; set; }
        public string  Encoding { get; set; }
        public string  Charset { get; set; }
        public string  Compression { get; set; }
        public string  Oldfileuid { get; set; }
        public string Newfileuid { get; set; }

        public OfxBody OfxBody { get; set; }
}
}