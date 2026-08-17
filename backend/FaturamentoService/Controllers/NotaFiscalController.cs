using FaturamentoService.Data;
using FaturamentoService.Models;
using FaturamentoService.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

namespace FaturamentoService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotaFiscalController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        public NotaFiscalController(AppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }


        //post pra criar a nota fiscal e fazer uma chamada http p o microsserviço de estoque pra atualizar o estoque

        [HttpPost]
        [EndpointSummary("Criar nova Nota Fiscal")]
        [EndpointDescription("Cadastra uma nova nota fiscal sequencial com status inicial 'Aberta'. O saldo dos produtos no estoque não é abatido nesta etapa.")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task <ActionResult<NotaFiscalModel>> CriarNotaFiscal([FromBody] CriarNotaFiscalDTO dto)
        {
            if (dto.Itens == null || !dto.Itens.Any())
            {
                return BadRequest(new { mensagem = "A nota fiscal deve conter pelo menos um item."});
            }

            var totalNotasExistentes = await _context.NotaFiscais.CountAsync();
            var proximoNumero = totalNotasExistentes + 1;
            var numeroNotaSequencial = $"NF-{proximoNumero:D4}";


            var novaNota = new NotaFiscalModel
            {
                NumeroNota = numeroNotaSequencial,
                Cliente = dto.Cliente,
                DataCriacao = DateTime.UtcNow,
                Status = StatusNotaFiscal.Aberta,
                ValorTotal = dto.Itens.Sum(i => i.Quantidade * i.PrecoUnitario),
                Itens = dto.Itens.Select(i => new ItemNotaFiscalModel
                {
                    ProdutoId = i.ProdutoId,
                    DescricaoProduto = i.DescricaoProduto,
                    Quantidade = i.Quantidade,
                    PrecoUnitario = i.PrecoUnitario
                }).ToList(),
            };

            _context.NotaFiscais.Add(novaNota);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(BuscarNotaPorId), new { id = novaNota.Id }, novaNota);
        }

        //get p retornar todos
        [HttpGet]
        [EndpointSummary("Listar todas as Notas Fiscais")]
        [EndpointDescription("Retorna todas as notas fiscais cadastradas no sistema com seus respectivos itens vinculados e status.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<NotaFiscalModel>>> ObterTodas()
        {
            var notas = await _context.NotaFiscais
                .Include(n => n.Itens)
                .AsNoTracking()
                .ToListAsync();

            return Ok(notas);
        }

        //get p retorna um com ID
        [HttpGet]
        [Route("{id}")]
        [EndpointSummary("Buscar Nota Fiscal por ID")]
        [EndpointDescription("Busca os detalhes completos de uma nota fiscal específica, incluindo a lista de itens, valores e status atual.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NotaFiscalModel>> BuscarNotaPorId(int id)
        {
            var notaID = await _context.NotaFiscais
                .Include(n => n.Itens) // chama o item associado a nota fiscal
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == id); // pega pelo id

            return Ok(notaID);


        }



        //post para cancelar a nota fiscal, que vai atualizar o status da nota fiscal e fazer uma chamada http p o microsserviço de estoque pra atualizar o estoque
        [HttpPut]
        [Route("{id}/cancelar")]
        [EndpointSummary("Cancelar Nota Fiscal")]
        [EndpointDescription("Cancela uma nota fiscal. Caso ela já esteja com status 'Fechada', realiza a requisição ao microsserviço de Estoque para estornar os produtos.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> CancelarNota(int id)
        {

            var nota = await _context.NotaFiscais
                .Include(n => n.Itens)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (nota == null)
            {
                return NotFound($"Nota Fiscal {id} não encontrada.");
            }


            if (nota.Status == StatusNotaFiscal.Cancelada)
            {
                return BadRequest("Esta nota fiscal já está cancelada.");
            }


            var client = _httpClientFactory.CreateClient("EstoqueService");
            var payload = new
            {
                itens = nota.Itens.Select(i => new
                {
                    produtoId = i.ProdutoId,
                    quantidade = i.Quantidade
                }).ToList()
            };

            var respostaEstoque = await client.PostAsJsonAsync("api/Produtos/adicionar-saldo", payload);

            if (!respostaEstoque.IsSuccessStatusCode)
            {
                return StatusCode((int)respostaEstoque.StatusCode, "Erro ao devolver itens ao estoque.");
            }


            nota.Status = (StatusNotaFiscal)3;
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = $"Nota Fiscal {nota.NumeroNota} cancelada e estoque estornado com sucesso." });
        }


        //endpoint para imprimir a nota fiscal, que vai retornar um PDF no angular com os dados da nota fiscal

        [HttpPost]
        [Route("{id}/imprimir")]
        [EndpointSummary("Imprimir e Fechar Nota Fiscal")]
        [EndpointDescription("Valida se a nota está com status 'Aberta', faz a requisição síncrona ao microsserviço de Estoque para dar baixa física nos produtos e atualiza o status da nota para 'Fechada'.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> ImprimirNota(int id)
        {
           
            var nota = await _context.NotaFiscais
                .Include(n => n.Itens)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (nota == null)
            {
                return NotFound($"Nota fiscal com ID {id} não encontrada.");
            }

            
            if (nota.Status != StatusNotaFiscal.Aberta)
            {
                return BadRequest($"Apenas notas com status 'Aberta' podem ser impressas. Status atual: {nota.Status}");
            }

            
            var client = _httpClientFactory.CreateClient("EstoqueService");
            var payloadAbate = new
            {
                itens = nota.Itens.Select(i => new
                {
                    produtoId = i.ProdutoId,
                    quantidade = i.Quantidade
                }).ToList()
            };

            try
            {
                var respostaEstoque = await client.PostAsJsonAsync("api/Produtos/abater-saldo", payloadAbate);

                if (!respostaEstoque.IsSuccessStatusCode)
                {
                    var erroEstoque = await respostaEstoque.Content.ReadAsStringAsync();
                    return StatusCode((int)respostaEstoque.StatusCode, new
                    {
                        mensagem = "Não foi possível imprimir a nota devido a uma falha no estoque.",
                        detalhes = erroEstoque
                    });
                }
            }
            catch (HttpRequestException)
            {
                
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    mensagem = "Serviço de Estoque temporariamente indisponível. A nota permaneceu Aberta."
                });
            }

            
            nota.Status = StatusNotaFiscal.Fechada;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = $"Nota Fiscal {nota.NumeroNota} impressa e fechada com sucesso!",
                notaFiscal = nota
            });
        }

    }
}
