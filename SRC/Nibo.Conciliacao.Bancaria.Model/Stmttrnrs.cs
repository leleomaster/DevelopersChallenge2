namespace Nibo.Conciliacao.Bancaria.Models
{
    public class Stmttrnrs
    {
        public int Trnuid { get; set; }
        public Status Status { get; set; }
        public Stmtrs Stmtrs { get; set; }
    }
}