namespace FaturamentoService.Models.DTOs
{
    public class ProdutoConsultaDto
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public int Saldo { get; set; }

    }
}
