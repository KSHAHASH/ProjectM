import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { FinancialInputComponent } from './features/financial-input/financial-input.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { UserProfileComponent } from './features/user-profile/user-profile.component';
import { AuthGuard } from './core/guards/auth.guard';
import { AnalysisComponent } from './features/analysis/analysis.component';
import { MonthlyAnalysisComponent } from './features/analysis/monthly-analysis/monthly-analysis.component';

const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [AuthGuard],
  },
  {
    path: 'financial-input',
    component: FinancialInputComponent,
    canActivate: [AuthGuard],
  },
  {
    path: 'user-profile',
    component: UserProfileComponent,
    canActivate: [AuthGuard],
  },
  {
    path: 'analysis',
    component: AnalysisComponent,
    canActivate: [AuthGuard],
  },
  {
    path: 'monthly-analysis',
    component: MonthlyAnalysisComponent,
    canActivate: [AuthGuard],
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
