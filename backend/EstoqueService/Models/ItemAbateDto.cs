using System.ComponentModel.DataAnnotations;

namespace EstoqueService.Models
{
    public class ItemAbateDto
    {
        [Required(ErrorMessage = "O ID do produto é obrigatório.")]
        public int ProdutoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A quantidade a ser abatida deve ser maior que zero.")]
        public int Quantidade { get; set; }
    }
   public class AbaterEstoqueRequestDto
    {
        [Required(ErrorMessage = "A lista de itens não pode estar vazia.")]
        [MinLength(1, ErrorMessage = "Informe ao menos um item para abate.")]
        public List<ItemAbateDto> Itens { get; set; } = new();
    }


}
