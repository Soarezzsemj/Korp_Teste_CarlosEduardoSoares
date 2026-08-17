using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FaturamentoService.Models
{
    public class ItemNotaFiscalModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int NotaFiscalId { get; set; }

        // ID do produto vindo do microsserviço de Estoque
        [Required]
        public int ProdutoId { get; set; }

        [Required]
        [MaxLength(200)]
        public string DescricaoProduto { get; set; } = string.Empty;

        [Required]
        public int Quantidade { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecoUnitario { get; set; }

        // o notmapped é pra nao criar no banco, p calcular no codigo mesmo sabe
        [NotMapped]
        public decimal Subtotal => Quantidade * PrecoUnitario;

        [JsonIgnore]
        public NotaFiscalModel? NotaFiscal { get; set; }
    }
}