import { Injectable } from '@angular/core';
import {
  CanActivate,
  Router,
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
  UrlTree,
} from '@angular/router';
import { Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root',
})
export class AuthGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ):
    | Observable<boolean | UrlTree>
    | Promise<boolean | UrlTree>
    | boolean
    | UrlTree {
    // Check authentication by calling the API
    return this.authService.getCurrentUser().pipe(
      take(1),
      map((user) => {
        if (user) {
          // User is authenticated
          return true;
        } else {
          // User is not authenticated, redirect to login
          console.log('Not authenticated, redirecting to login');
          return this.router.createUrlTree(['/login'], {
            queryParams: { returnUrl: state.url },
          });
        }
      })
    );
  }
}
