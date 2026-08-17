import { Routes } from '@angular/router';
import { ProdutosComponent } from './pages/produtos/produtos';
import { NotasComponent } from './pages/notas/notas';
import { NotaDetalheComponent } from './pages/nota-detalhe/nota-detalhe';

export const routes: Routes = [
  { path: '', redirectTo: 'notas', pathMatch: 'full' },
  { path: 'produtos', component: ProdutosComponent },
  { path: 'notas', component: NotasComponent },
  { path: 'notas/:id', component: NotaDetalheComponent },
  { path: '**', redirectTo: 'notas' }
];