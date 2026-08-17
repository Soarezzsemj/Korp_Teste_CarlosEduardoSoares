using EstoqueService.Data;
using EstoqueService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop.Infrastructure;

namespace EstoqueService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {

        private readonly AppDbContext _context;

        public ProdutosController(AppDbContext context)
        {
            _context = context;
        }


        // busca todos os produtos 
        [HttpGet]
        [EndpointSummary("Listar todos os produtos")]
        [EndpointDescription("Retorna a lista completa de produtos cadastrados com seus respectivos saldos em estoque.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<List<ProdutosModel>> BuscarProdutos()
        {
            var produtos = _context.Produtos.ToList();
            return Ok(produtos);
        }


        // busca os produtos pela id dele 
        [HttpGet]
        [Route("{id}")]
        [EndpointSummary("Obter produto por ID")]
        [EndpointDescription("Busca os detalhes e o saldo disponível de um produto específico através do seu identificador único.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ProdutosModel> BuscarProdutosPorId(int id)
        {
            var produto = _context.Produtos.Find(id);  

            if(produto == null)
            {
                return NotFound();
            }

            return Ok(produto);
        }

        // cria um novo produto por post

        [HttpPost]
        [EndpointSummary("Cadastrar novo produto")]
        [EndpointDescription("Cadastra um produto com código, descrição e saldo inicial disponível para comercialização.")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<ProdutosModel> CriarProduto([FromBody] CriarProdutoDto dto)
        {
           if(dto == null)
            {
                return BadRequest("Ocorreu um erro na solicitação");
            }

            var novoProduto = new ProdutosModel
            {
                Codigo = dto.Codigo,
                Descricao = dto.Descricao,
                Saldo = dto.Saldo,
                DataCriacao = DateTime.Now
            };



           _context.Produtos.Add(novoProduto);
            _context.SaveChanges();
    
                return CreatedAtAction(nameof(BuscarProdutosPorId), new { id = novoProduto.Id }, novoProduto);
        }


        // abate o saldo do produto com o post, passando o id do produto e a quantidade a ser abatida
        [HttpPost]
        [Route("abater-saldo")]
        [EndpointSummary("Abater saldo de produtos")]
        [EndpointDescription("Valida o saldo disponível e abate as quantidades solicitadas. Possui controle de concorrência com RowVersion para evitar saldo negativo.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AbaterSaldo([FromBody] AbaterEstoqueRequestDto dto)
        {
            if (dto == null || !dto.Itens.Any())
            {
                return BadRequest("Ocorreu um erro na solicitação");
            }   

            foreach(var item in dto.Itens)
            {
                var produto = await _context.Produtos.FindAsync(item.ProdutoId);
                if(produto == null)
                {
                    return NotFound($"Produto com ID {item.ProdutoId} não encontrado.");
                }

                if(produto.Saldo < item.Quantidade)
                {
                    return BadRequest($"Saldo insuficiente para o produto com ID {item.ProdutoId}. Saldo atual: {produto.Saldo}, quantidade solicitada: {item.Quantidade}");
                }
            }


            foreach(var item in dto.Itens)
            {
                var produto = await _context.Produtos.FindAsync(item.ProdutoId);
                produto.Saldo -= item.Quantidade;
            }



            try{
                await _context.SaveChangesAsync();

                return Ok("Saldo abatido com sucesso.");
            }
            catch (DbUpdateConcurrencyException)
            {

                //se caso der conflito de concorrencia com o rowversion
                return Conflict(new { mensagem = "O estoque deste produto foi modificado por outra operação simultânea. Tente novamente." });

            }

            
        }


        // devolver saldo quando a nota for cancelado
        [HttpPost]
        [Route("adicionar-saldo")]
        [EndpointSummary("Estornar/Adicionar saldo ao estoque")]
        [EndpointDescription("Devolve quantidades de produtos ao estoque físico (usado no cancelamento de notas fiscais).")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AdicionarSaldo([FromBody] AbaterEstoqueRequestDto dto)
        {
            if (dto == null || dto.Itens == null || !dto.Itens.Any())
            {
                return BadRequest("A lista de itens não pode estar vazia.");
            }

            foreach (var item in dto.Itens)
            {
                var produto = await _context.Produtos.FindAsync(item.ProdutoId);
                if (produto != null)
                {
                    produto.Saldo += item.Quantidade;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { mensagem = "Saldo devolvido ao estoque com sucesso." });
        }


        // atualizar estoque 

        [HttpPut]
        [Route("{id}")]
        [EndpointSummary("Atualizar produto")]
        [EndpointDescription("Atualiza a descrição e/ou o saldo físico de um produto previamente cadastrado no estoque.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ProdutosModel> EditarProduto([FromBody] AdicionarProdutoDto dto, int id)
        {

            if (dto == null || dto.Saldo <= 0)
            {
                return BadRequest("Ocorreu um erro na solicitação");
            }   


            var produto = _context.Produtos.Find(id);

            if(produto == null)
            {
                return NotFound("Registro nao encontrado");
            }



            produto.Saldo += dto.Saldo;
            _context.SaveChanges();

            return Ok(new { mensagem = "Saldo atualizado com sucesso!", saldoAtual = produto.Saldo });

        }
    }
}
