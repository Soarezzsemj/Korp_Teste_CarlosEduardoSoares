# Korp ERP Web - Interface Frontend (Angular)

Interface Single Page Application (SPA) desenvolvida em **Angular** utilizando a arquitetura moderna de **Standalone Components** e estilizada com **Tailwind CSS**.

---

## Funcionalidades da Interface

* **Painel de Gestão de Estoque (`/produtos`):**
  * Cadastro de produtos com código, descrição e saldo inicial.
  * Validações de formulário reativas com feedback em tempo real.
  * Listagem com badges indicativos de disponibilidade de estoque.

* **Central de Faturamento (`/notas`):**
  * Modal dinâmico de emissão de NF com adição múltipla de itens.
  * Cálculo dinâmico do total da nota e verificação visual/lógica de saldo disponível.
  * Tabela com tags de status padronizadas (**Aberta**, **Fechada**, **Cancelada**).

* **Espelho e Detalhe da Nota Fiscal (`/notas/:id`):**
  * Layout DANFE pronto para impressão e exportação em PDF (`window.print()`).
  * Ação de **Fechar Nota**, disparando a baixa atômica de saldo no backend.
  * Ação de **Cancelar Nota**, com diálogo de confirmação e estorno automático.

---

## Execução Local para Desenvolvimento

### Pré-requisitos
* [Node.js](https://nodejs.org/) (versão 20 ou superior)
* [Angular CLI](https://angular.dev/) instalado globalmente: `npm install -g @angular/cli`

### Instalação e Execução

1. Instale as dependências:
   ```bash
   npm install
   ```

2. Inicie o servidor de desenvolvimento:
   ```bash
   npm start
   ```

3. Acesse no navegador:
   ```
   http://localhost:4200
   ```

---

## Build de Produção e Docker

O projeto utiliza um arquivo `Dockerfile` baseado em multi-stage build:

1. Compilação dos fontes via imagem `node:20-alpine`.
2. Servimento dos arquivos estáticos via servidor leve `nginx:alpine` com suporte a rotas de SPA (`nginx.conf`).