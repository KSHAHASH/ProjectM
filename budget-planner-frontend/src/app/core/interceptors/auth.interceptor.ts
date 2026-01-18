import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpErrorResponse,
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private router: Router) {}

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    // Clone the request and add withCredentials for all API calls
    const authReq = req.clone({
      withCredentials: true,
    });

    return next.handle(authReq).pipe(
      catchError((error: HttpErrorResponse) => {
        // If we get a 401 Unauthorized, redirect to login
        if (error.status === 401 && !req.url.includes('/auth/login')) {
          console.log('Received 401, redirecting to login');
          this.router.navigate(['/login']);
        }
        return throwError(() => error);
      })
    );
  }
}
