# Microsserviços Backend (.NET 9)

Camada de serviços distribuídos responsável pelo processamento de faturamento e gestão de estoque.

---

## Serviços

### 1. EstoqueService (Porta: 5190)
Gerencia o catálogo de produtos e seus saldos em estoque.

* `GET /api/Produtos` — Listagem geral de itens e saldos.
* `GET /api/Produtos/{id}` — Consulta de detalhes e saldo por ID.
* `POST /api/Produtos` — Cadastro de produto (com bloqueio para duplicidade de código e descrição).
* `POST /api/Produtos/abater-saldo` — Abatimento atômico com validação de concorrência (`RowVersion`).
* `POST /api/Produtos/adicionar-saldo` — Devolução/estorno de itens ao estoque.
* `PUT /api/Produtos/{id}` — Atualização cadastral e acréscimo de saldo.

### 2. FaturamentoService (Porta: 5169)
Responsável pelo ciclo de vida das Notas Fiscais.

* `GET /api/NotaFiscal` — Lista todas as notas com itens vinculados.
* `GET /api/NotaFiscal/{id}` — Detalha nota fiscal específica.
* `POST /api/NotaFiscal` — Emite nova nota com numeração sequencial (`NF-0001`) e validação prévia de saldo.
* `POST /api/NotaFiscal/{id}/imprimir` — Fecha a nota e aciona a baixa física no microsserviço de Estoque.
* `PUT /api/NotaFiscal/{id}/cancelar` — Cancela a nota e estorna o saldo no EstoqueService.

---

## Execução Local sem Docker

### Pré-requisitos
* [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
* SQL Server Local / LocalDB em execução

### 1. Configurar as Connection Strings
Verifique os arquivos `appsettings.json` de cada projeto para apontar para a sua instância do SQL Server:
* `EstoqueService`: `Korp_Estoque_DB`
* `FaturamentoService`: `Korp_Faturamento_DB`

### 2. Executar as Migrações
No terminal, dentro de cada pasta:

```bash
# No EstoqueService:
dotnet ef database update

# No FaturamentoService:
dotnet ef database update
```

### 3. Rodar os Serviços
Abra dois terminais separados:

```bash
# Terminal 1:
cd EstoqueService
dotnet run

# Terminal 2:
cd FaturamentoService
dotnet run
```