import { Component, inject, input, output, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthClient } from '../../../services/auth-client';
import { LoginRequest } from '../../../models/requests/login-request';
import { ERROR_MESSAGES } from '../../../shared/constants/error-messages';
import { Error } from '../../error/error';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-login-form',
  imports: [ReactiveFormsModule, Error, RouterLink],
  templateUrl: './login-form.html',
  styleUrl: './login-form.scss',
})
export class LoginForm {
  private readonly auth = inject(AuthClient);
  private readonly router = inject(Router);
  private readonly formBuilder = inject(NonNullableFormBuilder);

  readonly isOpen = input(false);
  readonly toggle = output<void>();

  readonly errorMessage = signal<string | undefined>(undefined);
  readonly btnSpinner = signal(false);

  readonly form = this.formBuilder.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  onSignIn() {
    if (this.form.invalid) return;

    this.btnSpinner.set(true);

    const request: LoginRequest = this.form.getRawValue();

    this.auth
      .login(request)
      .pipe(
        finalize(() => {
          this.btnSpinner.set(false);
        }),
      )
      .subscribe({
        next: (response) => {
          this.router.navigate(['/budgets']);
        },
        error: (err) => {
          this.errorMessage.set(err?.error?.detail ?? ERROR_MESSAGES.generic);
        },
      });
  }

  onToggleSignIn() {
    this.toggle.emit();
  }
}
