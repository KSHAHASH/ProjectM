import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { NgxChartsModule } from '@swimlane/ngx-charts';
import { HTTP_INTERCEPTORS } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { CoreModule } from './core/core.module';
// import { SharedModule } from './shared/shared.module';
import { FinancialInputComponent } from './features/financial-input/financial-input.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { UserProfileComponent } from './features/user-profile/user-profile.component';
import { AuthInterceptor } from './core/interceptors/auth.interceptor';
import { AnalysisComponent } from './features/analysis/analysis.component';
import { MonthlyAnalysisComponent } from './features/analysis/monthly-analysis/monthly-analysis.component';

@NgModule({
  declarations: [
    AppComponent,
    FinancialInputComponent,
    DashboardComponent,
    LoginComponent,
    RegisterComponent,
    UserProfileComponent,
    AnalysisComponent,
    MonthlyAnalysisComponent,
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    ReactiveFormsModule,
    FormsModule,
    NgxChartsModule,
    CoreModule,
    // SharedModule,
    AppRoutingModule,
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true,
    },
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
