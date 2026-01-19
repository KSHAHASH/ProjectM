import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MonthlyAnalysis } from '../models/monthly-analysis.model';

@Injectable({
  providedIn: 'root',
})
export class MonthlyAnalysisService {
  private apiUrl = 'http://localhost:5294/api';

  constructor(private http: HttpClient) {}

  /**
   * Get monthly analysis for a specific month
   * @param year Year (e.g., 2024)
   * @param month Month (1-12)
   */
  getMonthlyAnalysis(year: number, month: number): Observable<MonthlyAnalysis> {
    return this.http.get<MonthlyAnalysis>(
      `${this.apiUrl}/analysis/monthly?year=${year}&month=${month}`,
      { withCredentials: true }
    );
  }
}
