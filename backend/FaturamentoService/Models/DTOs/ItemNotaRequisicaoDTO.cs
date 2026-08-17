namespace FaturamentoService.Models.DTOs
{
    public class ItemNotaRequisicaoDTO
    {
        public int ProdutoId { get; set; }

        public string DescricaoProduto { get; set; } = string.Empty;

        public int Quantidade { get; set; }

        public decimal PrecoUnitario { get; set; }

    }
}
