import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  isLoading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Check if already logged in
    this.authService.getCurrentUser().subscribe({
      next: () => {
        // User is already logged in, redirect to dashboard
        this.router.navigate(['/dashboard']);
      },
      error: () => {
        // User is not logged in, stay on login page
      },
    });

    this.loginForm = this.fb.group({
      username: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const { username, password } = this.loginForm.value;

    this.authService.login(username, password).subscribe({
      next: (response: any) => {
        console.log('Login successful:', response);
        this.router.navigate(['/dashboard']);
      },
      error: (error: any) => {
        console.error('Login failed:', error);
        this.errorMessage =
          error.error?.message || 'Invalid username or password';
        this.isLoading = false;
      },
    });
  }

  get username() {
    return this.loginForm.get('username');
  }

  get password() {
    return this.loginForm.get('password');
  }
}
