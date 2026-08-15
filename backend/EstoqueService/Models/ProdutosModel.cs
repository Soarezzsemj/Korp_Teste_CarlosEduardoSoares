using System.ComponentModel.DataAnnotations;

namespace EstoqueService.Models
{
    public class ProdutosModel
    {
        public int Id { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        public int Saldo { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [Timestamp] // vai checar a concorrencia, para nao haver erros em relação a isso 
        public byte[] RowVersion { get; set; } = null!;



    }
}
