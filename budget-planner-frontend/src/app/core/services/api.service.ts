import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl: string = 'http://localhost:5293/api';

  constructor(private http: HttpClient) {}

  get<T>(endpoint: string, params?: any): Observable<T> {
    let httpParams = new HttpParams();
    
    if (params) {
      //keys and values from params object {income: 1000, expenses: '...'} keys are 'income' and 'expenses'
      Object.keys(params).forEach(key => {
        if (params[key] !== null && params[key] !== undefined) {
          httpParams = httpParams.set(key, params[key].toString()); // produces ? income=1000
        }
      });
    }

    return this.http.get<T>(`${this.baseUrl}/${endpoint}`, { params: httpParams });
  }

  postFinancialInput<T>(endpoint: string, body: any, params?: any): Observable<T> {
    let httpParams = new HttpParams();
    
    if (params) {
      Object.keys(params).forEach(key => {
        if (params[key] !== null && params[key] !== undefined) {
          httpParams = httpParams.set(key, params[key].toString());
        }
      });
    }

    return this.http.post<T>(`${this.baseUrl}/${endpoint}`, body, { params: httpParams });
  }
}
