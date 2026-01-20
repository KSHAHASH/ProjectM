import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface User {
  id: number;
  name: string;
  email: string;
  monthlyIncome: number;
}

export interface LoginResponse {
  message: string;
  user: User;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = 'http://localhost:5294/api';
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    // Check if user is already logged in on service initialization
    this.checkAuthentication();
  }

  /**
   * Check if user is authenticated by calling /api/auth/me
   */
  private checkAuthentication(): void {
    this.getCurrentUser().subscribe({
      next: (user) => {
        this.currentUserSubject.next(user);
      },
      error: (error) => {
        console.log("Authentication check failed", error.status);
        this.currentUserSubject.next(null);
      },
    });
  }

  /**
   * Login with username (email) and password
   */
  login(username: string, password: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(
        `${this.apiUrl}/auth/login`,
        { username, password },
        { withCredentials: true } // Essential for cookie-based auth
      )
      .pipe(
        tap((response) => {
          this.currentUserSubject.next(response.user);
        })
      );
  }

  /**
   * Register a new user
   */
  register(
    name: string,
    email: string,
    password: string,
    monthlyIncome: number
  ): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(
        `${this.apiUrl}/auth/register`,
        { name, email, password, monthlyIncome },
        { withCredentials: true }
      )
      .pipe(
        tap((response) => {
          this.currentUserSubject.next(response.user);
        })
      );
  }

  /**
   * Logout the current user
   */
  logout(): Observable<any> {
    return this.http
      .post(`${this.apiUrl}/auth/logout`, {}, { withCredentials: true })
      .pipe(
        tap(() => {
          this.currentUserSubject.next(null);
        })
      );
  }

  /**
   * Get current authenticated user
   */
  getCurrentUser(): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/auth/me`, {
      withCredentials: true,
    });
  }

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    return this.currentUserSubject.value !== null;
  }

  /**
   * Get the current user value (synchronous)
   */
  getCurrentUserValue(): User | null {
    return this.currentUserSubject.value;
  }
}
