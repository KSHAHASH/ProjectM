import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { CoreModule } from './core/core.module';
import { SharedModule } from './shared/shared.module';
import { FinancialInputComponent } from './features/financial-input/financial-input.component';
import { BudgetAnalysisComponent } from './features/budget-analysis/budget-analysis.component';

@NgModule({
  declarations: [
    AppComponent,
    FinancialInputComponent,
    BudgetAnalysisComponent,
  ],
  imports: [
    BrowserModule,
    ReactiveFormsModule,
    CoreModule,
    SharedModule,
    AppRoutingModule,
  ],
  providers: [],
  bootstrap: [AppComponent],
})
export class AppModule {}
