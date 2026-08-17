import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Produto, CriarProdutoDto, AdicionarProdutoDto } from '../models/produto.model';

@Injectable({
  providedIn: 'root'
})
export class ProdutoService {
  private readonly apiUrl = `${environment.apiUrlEstoque}/Produtos`;

  constructor(private http: HttpClient) {}

  listar(): Observable<Produto[]> {
    return this.http.get<Produto[]>(this.apiUrl);
  }

  obterPorId(id: number): Observable<Produto> {
    return this.http.get<Produto>(`${this.apiUrl}/${id}`);
  }

  criar(dto: CriarProdutoDto): Observable<Produto> {
    return this.http.post<Produto>(this.apiUrl, dto);
  }

  atualizarEstoque(id: number, dto: AdicionarProdutoDto): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, dto);
  }
}