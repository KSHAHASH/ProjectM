import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService, User } from '../../core/services/auth.service';

@Component({
  selector: 'app-user-profile',
  templateUrl: './user-profile.component.html',
  styleUrls: ['./user-profile.component.css'],
})
export class UserProfileComponent implements OnInit {
  currentUser: User | null = null;
  isLoggingOut = false;

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe((user: User | null) => {
      this.currentUser = user;
    });
  }

  logout(): void {
    if (confirm('Are you sure you want to logout?')) {
      this.isLoggingOut = true;
      this.authService.logout().subscribe({
        next: () => {
          this.router.navigate(['/login']);
        },
        error: (error: any) => {
          console.error('Logout failed:', error);
          this.isLoggingOut = false;
          // Even if logout fails, redirect to login
          this.router.navigate(['/login']);
        },
      });
    }
  }

  getInitials(name: string): string {
    if (!name) return 'U';
    const parts = name.split(' ');
    if (parts.length >= 2) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }
    return name.substring(0, 2).toUpperCase();
  }
}
