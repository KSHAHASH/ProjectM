import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { AuthService, User } from './core/services/auth.service';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit {
  title = 'budget-planner-frontend';
  totalBalance = 25650;
  currentUser: User | null = null;
  showSidebar = true;

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit(): void {
    // Subscribe to current user
    this.authService.currentUser$.subscribe((user) => {
      this.currentUser = user;
    });

    // Check initial route
    this.checkRoute(this.router.url);

    // Subscribe to route changes
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe((event: any) => {
        this.checkRoute(event.url);
      });
  }

  checkRoute(url: string): void {
    // Hide sidebar on auth pages (login, register, forgot password)
    this.showSidebar =
      !url.includes('/login') &&
      !url.includes('/register') &&
      !url.includes('/forgot-password');
  }
}
