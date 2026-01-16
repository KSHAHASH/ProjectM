import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { FinancialInputComponent } from './features/financial-input/financial-input.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';

const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'financial-input', component: FinancialInputComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
