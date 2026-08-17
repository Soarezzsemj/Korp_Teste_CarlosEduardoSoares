using System.ComponentModel.DataAnnotations.Schema;

namespace FaturamentoService.Models
{

    public enum StatusNotaFiscal
    {
        Aberta = 1,
        Fechada = 2,
        Cancelada = 3
    }
    public class NotaFiscalModel
    {
        public int Id { get; set; }

        public string NumeroNota { get; set; } = string.Empty;

        public string Cliente { get; set; } = string.Empty;

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public StatusNotaFiscal Status { get; set; } = StatusNotaFiscal.Aberta;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorTotal { get; set; }

        public List<ItemNotaFiscalModel> Itens { get; set; } = new();

    }
}
