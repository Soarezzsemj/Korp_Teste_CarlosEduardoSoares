namespace FaturamentoService.Models.DTOs
{
    public class CriarNotaFiscalDTO
    {
        public string Cliente { get; set; } = string.Empty;

        public List<ItemNotaRequisicaoDTO> Itens { get; set; } = new();
    }
}
