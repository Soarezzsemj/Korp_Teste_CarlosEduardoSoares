import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { ProdutoService } from '../../core/services/produto.service';
import { Produto } from '../../core/models/produto.model';
import { Router ,RouterLink } from '@angular/router';

@Component({
  selector: 'app-produtos',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink],
  templateUrl: './produtos.html',
  styleUrl: './produtos.css'
})
export class ProdutosComponent implements OnInit {
  produtos: Produto[] = [];
  produtoForm: FormGroup;
  
  carregando: boolean = false;
  salvando: boolean = false;
  mensagemSucesso: string = '';
  mensagemErro: string = '';

  constructor(
    private fb: FormBuilder,
    private produtoService: ProdutoService,
    private cdr: ChangeDetectorRef
  ) {
    this.produtoForm = this.fb.group({
      codigo: ['', [Validators.required]],
      descricao: ['', [Validators.required, Validators.minLength(3)]],
      saldo: [0, [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    this.carregarProdutos();
  }

  carregarProdutos(): void {
    this.carregando = true;
    this.mensagemErro = '';
    this.cdr.markForCheck();

    this.produtoService.listar()
      .pipe(
        finalize(() => {
          this.carregando = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: (dados) => {
          this.produtos = dados;
          this.cdr.markForCheck();
        },
        error: () => {
          this.mensagemErro = 'Não foi possível atualizar a lista de produtos do estoque.';
        }
      });
  }

  onSubmit(): void {
    if (this.produtoForm.invalid) {
      this.produtoForm.markAllAsTouched();
      return;
    }

    this.salvando = true;
    this.mensagemSucesso = '';
    this.mensagemErro = '';
    this.cdr.markForCheck();

    const novoProduto: Produto = this.produtoForm.value;

    this.produtoService.criar(novoProduto)
      .pipe(
        finalize(() => {
          this.salvando = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: () => {
          this.mensagemSucesso = 'Produto cadastrado com sucesso!';
          this.produtoForm.reset({ saldo: 0 });
          this.carregarProdutos();
        },
        error: (err) => {
          this.mensagemErro = err.error?.mensagem || 'Erro ao cadastrar produto no microsserviço de Estoque.';
        }
      });
  }
}