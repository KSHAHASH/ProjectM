import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { FinancialInputComponent } from './features/financial-input/financial-input.component';
import { BudgetAnalysisComponent } from './features/budget-analysis/budget-analysis.component';

const routes: Routes = [
  { path: '', redirectTo: '/financial-input', pathMatch: 'full' },
  { path: 'financial-input', component: FinancialInputComponent },
  { path: 'budget-analysis', component: BudgetAnalysisComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
