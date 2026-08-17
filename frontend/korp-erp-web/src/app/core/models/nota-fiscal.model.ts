export interface ItemNotaFiscal {
  id?: number;
  produtoId: number;
  descricaoProduto?: string;
  quantidade: number;
  precoUnitario: number;
  valorTotal?: number;
}

export interface NotaFiscal {
  id?: number;
  numeroNota?: string;
  cliente: string;
  valorTotal: number;
  status: number | string;
  criadoEm?: string | Date;
  dataCriacao?: string | Date;
  dataEmissao?: string | Date;
  itens?: ItemNotaFiscal[];
}

export interface CriarNotaFiscalDto {
  cliente: string;
  itens: {
    produtoId: number;
    descricaoProduto?: string;
    quantidade: number;
    precoUnitario: number;
  }[];
}

