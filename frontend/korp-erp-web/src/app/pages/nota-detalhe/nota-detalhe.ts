import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { NotaFiscalService } from '../../core/services/nota-fiscal.service';
import { NotaFiscal } from '../../core/models/nota-fiscal.model';

@Component({
  selector: 'app-nota-detalhe',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './nota-detalhe.html',
  styleUrl: './nota-detalhe.css'
})
export class NotaDetalheComponent implements OnInit {
  notaId!: number;
  nota: NotaFiscal | null = null;
  carregando: boolean = false;
  fechando: boolean = false;
  cancelando: boolean = false;
  mensagemErro: string = '';
  mensagemSucesso: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private notaService: NotaFiscalService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.notaId = Number(this.route.snapshot.paramMap.get('id'));
    if (this.notaId) {
      this.carregarNota();
    }
  }

  carregarNota(): void {
    this.carregando = true;
    this.cdr.markForCheck();

    this.notaService.obterPorId(this.notaId)
      .pipe(
        finalize(() => {
          this.carregando = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: (dados) => {
          this.nota = dados;
          this.cdr.markForCheck();
        },
        error: () => {
          this.mensagemErro = 'Não foi possível carregar os detalhes da nota fiscal.';
        }
      });
  }

  isAberta(): boolean {
    if (!this.nota) return false;
    const s = String(this.nota.status).toLowerCase();
    return s === '1' || s === 'aberta';
  }

  isCancelada(): boolean {
    if (!this.nota) return false;
    const s = String(this.nota.status).toLowerCase();
    return s === '3' || s === 'cancelada';
  }

  obterStatusTexto(): string {
    if (!this.nota) return '';
    const s = String(this.nota.status).toLowerCase();
    if (s === '1' || s === 'aberta') return 'Aberta';
    if (s === '2' || s === 'fechada') return 'Fechada';
    if (s === '3' || s === 'cancelada') return 'Cancelada';
    return String(this.nota.status);
  }

  fecharNota(): void {
    if (!this.notaId) return;

    this.fechando = true;
    this.mensagemErro = '';
    this.mensagemSucesso = '';
    this.cdr.markForCheck();

    this.notaService.imprimir(this.notaId)
      .pipe(
        finalize(() => {
          this.fechando = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: () => {
          this.mensagemSucesso = 'Nota fiscal fechada e estoque baixado com sucesso!';
          this.carregarNota();
        },
        error: (err) => {
          this.mensagemErro = err.error?.mensagem || 'Erro ao fechar a nota fiscal.';
        }
      });
  }

  cancelarNota(): void {
    if (!this.notaId) return;

    const confirmar = confirm('Tem certeza que deseja cancelar esta nota fiscal?');
    if (!confirmar) return;

    this.cancelando = true;
    this.mensagemErro = '';
    this.mensagemSucesso = '';
    this.cdr.markForCheck();

    this.notaService.cancelar(this.notaId)
      .pipe(
        finalize(() => {
          this.cancelando = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: () => {
          this.mensagemSucesso = 'Nota fiscal cancelada com sucesso!';
          this.carregarNota();
        },
        error: (err) => {
          this.mensagemErro = err.error?.mensagem || 'Erro ao cancelar a nota fiscal.';
        }
      });
  }

  gerarPdf(): void {
    window.print();
  }
}