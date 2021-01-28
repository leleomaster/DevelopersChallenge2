using System.Collections.Generic;

namespace Nibo.Conciliacao.Bancaria.Models
{
    public class Banktranlist
    {
        public string Dtstart { get; set; }
        public string Dtend { get; set; }
        public List<Stmttrn> Stmttrns { get; set; }
    }
}