# Korp_Teste_CarlosEduardoSoares

# Korp ERP — Sistema de Faturamento & Controle de Estoque

> **Projeto de Teste Técnico**
> Este repositório foi desenvolvido como teste técnico para processo seletivo.
> Autor: **Carlos Eduardo Soares Souza Santos**

Solução corporativa distribuída construída sobre arquitetura de **Microsserviços** utilizando **.NET 9**, **Entity Framework Core**, **SQL Server** e **Angular 19** com **Tailwind CSS**. O sistema orquestra fluxos de controle de estoque e faturamento/emissão de Notas Fiscais com controle de concorrência e resiliência entre serviços.

---

## Arquitetura do Sistema

```
                              ┌───────────────────────────────┐
                              │           Frontend             │
                              │    Angular · localhost:4200    │
                              └────────────────┬────────────────┘
                                                │
                                                │ HTTP / REST
                                                │
                    ┌───────────────────────────┴───────────────────────────┐
                    │                                                       │
                    ▼                                                       ▼
     ┌─────────────────────────────────┐                    ┌─────────────────────────────────┐
     │        FaturamentoService        │   HTTP síncrono   │          EstoqueService           │
     │      localhost:5169  (Scalar)    │ ──────────────────►│      localhost:5190  (Scalar)     │
     │                                   │                    │                                   │
     │  Emissão, fechamento e            │                    │  Catálogo de produtos e            │
     │  cancelamento de Notas Fiscais    │                    │  controle de saldo em estoque      │
     └────────────────┬──────────────────┘                    └────────────────┬──────────────────┘
                       │                                                        │
                       └───────────────────────┬────────────────────────────────┘
                                                 │
                                                 ▼
                              ┌───────────────────────────────┐
                              │           SQL Server           │
                              │       Docker · porta 1433      │
                              └───────────────────────────────┘
```

**Fluxo resumido:** o Frontend consome os dois microsserviços via REST; o `FaturamentoService` chama o `EstoqueService` de forma síncrona para validar e baixar saldo no fechamento da nota; ambos os serviços persistem seus dados em instâncias próprias no mesmo SQL Server.

---

## Como Executar com Docker Compose (Recomendado)

O projeto está totalmente configurado para inicialização autônoma via contêineres.

### Pré-requisitos
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado e em execução.

### Passo Único de Execução
Na raiz do repositório, execute:

```bash
docker compose up --build -d
```

### URLs de Acesso

| Aplicação / Serviço | URL | Descrição |
|---|---|---|
| Frontend Web | http://localhost:4200 | Interface SPA em Angular |
| Faturamento API | http://localhost:5169/scalar/v1 | Documentação Interativa Scalar / OpenAPI |
| Estoque API | http://localhost:5190/scalar/v1 | Documentação Interativa Scalar / OpenAPI |
| SQL Server | localhost:1433 | Banco de Dados Relacional |

### Acesso ao Banco de Dados

Caso queira inspecionar as tabelas via DBeaver, SSMS ou VS Code:

- **Host:** `localhost`
- **Porta:** `1433`
- **Usuário:** `sa`
- **Senha:** `KorpErp2026!`
- **Bancos criados:** `Korp_Estoque_DB` e `Korp_Faturamento_DB`
- **Connection String rápida:**
  `Server=localhost,1433;Database=master;User Id=sa;Password=KorpErp2026!;TrustServerCertificate=True;`

---

## Principais Fluxos e Regras de Negócio Implementadas

* **Validação de Duplicidade:** bloqueio de cadastro de produtos com mesma descrição ou código no `EstoqueService`.
* **Emissão com Trava de Saldo:** o `FaturamentoService` valida se a quantidade demandada existe no `EstoqueService` antes de permitir a criação da nota.
* **Fechamento e Baixa Física:** o fechamento da NF aciona a baixa de estoque via chamada HTTP síncrona entre microsserviços.
* **Cancelamento e Estorno:** cancelar uma NF com status Fechada devolve automaticamente o saldo para o estoque.
* **Controle de Concorrência:** implementação de `RowVersion` no estoque para mitigar problemas de race condition em operações simultâneas.
* **Espelho de DANFE / PDF:** geração de documento de impressão formatado nativamente no frontend.

---

## Tecnologias Utilizadas

* **Backend:** .NET 9, C#, Entity Framework Core, Scalar API Reference.
* **Frontend:** Angular (Standalone Components), TypeScript, Tailwind CSS.
* **Banco de Dados:** Microsoft SQL Server 2022.
* **DevOps:** Docker, Docker Compose, Nginx.

---

## Autor

Desenvolvido por **Carlos Eduardo Soares Souza Santos** como parte de um teste técnico para processo seletivo.
