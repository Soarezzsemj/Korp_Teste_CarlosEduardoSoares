export interface Produto {
  id?: number;
  codigo: string;
  descricao: string;
  saldo: number;
  dataCriacao?: string | Date;
}

export interface CriarProdutoDto {
  codigo: string;
  descricao: string;
  saldo: number;
}

export interface AdicionarProdutoDto {
  saldo: number;
}

export interface AbaterEstoqueItemDto {
  produtoId: number;
  quantidade: number;
}

export interface AbaterEstoqueRequestDto {
  itens: AbaterEstoqueItemDto[];
}