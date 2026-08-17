import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { NotaFiscalService } from '../../core/services/nota-fiscal.service';
import { ProdutoService } from '../../core/services/produto.service';
import { NotaFiscal, CriarNotaFiscalDto } from '../../core/models/nota-fiscal.model';
import { Produto } from '../../core/models/produto.model';
import { Router ,RouterLink } from '@angular/router';

@Component({
  selector: 'app-notas',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink],
  templateUrl: './notas.html',
  styleUrl: './notas.css'
})
export class NotasComponent implements OnInit {
  notas: NotaFiscal[] = [];
  produtosDisponiveis: Produto[] = [];
  
  notaForm: FormGroup;
  exibirModalNovaNota: boolean = false;
  
  carregando: boolean = false;
  salvando: boolean = false;
  loadingImpressoes: { [id: number]: boolean } = {};
  
  mensagemSucesso: string = '';
  mensagemErro: string = '';

  obterStatusInfo(status: any): { texto: string; classe: string } {
  const s = String(status).toLowerCase();
  
  if (s === '1' || s === 'aberta') {
    return {
      texto: 'Aberta',
      classe: 'bg-amber-50 text-amber-700 border-amber-200'
    };
  }
  
  if (s === '2' || s === 'fechada') {
    return {
      texto: 'Fechada',
      classe: 'bg-emerald-50 text-emerald-700 border-emerald-200'
    };
  }
  
  if (s === '3' || s === 'cancelada') {
    return {
      texto: 'Cancelada',
      classe: 'bg-red-50 text-red-700 border-red-200'
    };
  }

  return {
    texto: String(status),
    classe: 'bg-slate-100 text-slate-600 border-slate-200'
  };
}

formularioValidoComEstoque(): boolean {
  if (this.notaForm.invalid || this.itens.length === 0) return false;
  
  for (let i = 0; i < this.itens.length; i++) {
    if (!this.validarSaldoDisponivel(i)) {
      return false;
    }
  }
  return true;
}

  constructor(
    private fb: FormBuilder,
    private notaService: NotaFiscalService,
    private produtoService: ProdutoService,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {
    this.notaForm = this.fb.group({
      cliente: ['', [Validators.required, Validators.minLength(3)]],
      itens: this.fb.array([])
    });
  }

  verDetalhe(nota: NotaFiscal): void {
    if (nota.id) {
      this.router.navigate(['/notas', nota.id]);
    }
  }

  obterStatusTexto(status: any): string {
    if (status === 1 || status === '1' || status === 'Aberta') return 'Aberta';
    if (status === 2 || status === '2' || status === 'Fechada') return 'Fechada';
    if (status === 3 || status === '3' || status === 'Cancelada') return 'Cancelada';
    return String(status);
  }

  get itens(): FormArray {
    return this.notaForm.get('itens') as FormArray;
  }

  ngOnInit(): void {
    this.carregarNotas();
    this.carregarProdutos();
  }

  carregarNotas(): void {
    this.carregando = true;
    this.mensagemErro = '';
    this.cdr.markForCheck();

    this.notaService.listar()
      .pipe(
        finalize(() => {
          this.carregando = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: (dados) => {
          this.notas = dados;
          this.cdr.markForCheck();
        },
        error: () => {
          this.mensagemErro = 'Não foi possível carregar a lista de notas fiscais.';
        }
      });
  }

  carregarProdutos(): void {
    this.produtoService.listar().subscribe({
      next: (dados) => {
        this.produtosDisponiveis = dados;
        this.cdr.markForCheck();
      }
    });
  }

  abrirModal(): void {
    this.notaForm.reset();
    this.itens.clear();
    this.adicionarItem();
    this.exibirModalNovaNota = true;
    this.mensagemErro = '';
    this.mensagemSucesso = '';
    this.carregarProdutos();
  }

  fecharModal(): void {
    this.exibirModalNovaNota = false;
  }

  adicionarItem(): void {
    const itemGroup = this.fb.group({
      produtoId: [null, [Validators.required]],
      quantidade: [1, [Validators.required, Validators.min(1)]],
      precoUnitario: [0, [Validators.required, Validators.min(0.01)]]
    });
    this.itens.push(itemGroup);
  }

  removerItem(index: number): void {
    if (this.itens.length > 1) {
      this.itens.removeAt(index);
    }
  }

  calcularTotalNota(): number {
    return this.itens.controls.reduce((total, control) => {
      const qtd = control.get('quantidade')?.value || 0;
      const preco = control.get('precoUnitario')?.value || 0;
      return total + (qtd * preco);
    }, 0);
  }

  onSubmit(): void {
    if (this.notaForm.invalid || this.itens.length === 0) {
      this.notaForm.markAllAsTouched();
      return;
    }

    this.salvando = true;
    this.mensagemErro = '';
    this.cdr.markForCheck();

    const formVal = this.notaForm.value;
    const dto: CriarNotaFiscalDto = {
      cliente: formVal.cliente,
      itens: formVal.itens.map((item: any) => {
        const prod = this.produtosDisponiveis.find(p => p.id === Number(item.produtoId));
        return {
          produtoId: Number(item.produtoId),
          descricaoProduto: prod ? prod.descricao : '',
          quantidade: Number(item.quantidade),
          precoUnitario: Number(item.precoUnitario)
        };
      })
    };

    this.notaService.criar(dto)
      .pipe(
        finalize(() => {
          this.salvando = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: () => {
          this.mensagemSucesso = 'Nota fiscal emitida com sucesso!';
          this.fecharModal();
          this.carregarNotas();
        },
        error: (err) => {
          this.mensagemErro = err.error?.mensagem || 'Erro ao emitir nota fiscal.';
        }
      });

  }


  validarSaldoDisponivel(index: number): boolean {
  const itemControl = this.itens.at(index);
  const produtoId = Number(itemControl.get('produtoId')?.value);
  const quantidade = Number(itemControl.get('quantidade')?.value);

  const produto = this.produtosDisponiveis.find(p => p.id === produtoId);
  if (produto && quantidade > produto.saldo) {
    return false; // Quantidade maior que o estoque
  }
  return true;
}

  imprimirNota(nota: NotaFiscal): void {
    if (!nota.id || nota.status !== 'Aberta') return;

    this.loadingImpressoes[nota.id] = true;
    this.mensagemErro = '';
    this.mensagemSucesso = '';
    this.cdr.markForCheck();

    this.notaService.imprimir(nota.id)
      .pipe(
        finalize(() => {
          this.loadingImpressoes[nota.id!] = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: () => {
          this.mensagemSucesso = `Nota ${nota.numeroNota || nota.id} impressa e estoque baixado com sucesso!`;
          this.carregarNotas();
        },
        error: (err) => {
          this.mensagemErro = err.error?.mensagem || 'Falha ao imprimir nota (verifique saldo em estoque ou concorrência).';
        }
      });
  }
}