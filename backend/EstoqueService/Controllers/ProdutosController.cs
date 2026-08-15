using EstoqueService.Data;
using EstoqueService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public ActionResult<List<ProdutosModel>> BuscarProdutos()
        {
            var produtos = _context.Produtos.ToList();
            return Ok(produtos);
        }


        // busca os produtos pela id dele 
        [HttpGet]
        [Route("{id}")]
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

            await _context.SaveChangesAsync();

            return Ok("Saldo abatido com sucesso.");
        }


        // atualizar estoque 

        [HttpPut]
        [Route("{id}")]
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
