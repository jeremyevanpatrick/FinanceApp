import { Component, inject, signal } from '@angular/core';
import { Error } from '../error/error';
import { AuthClient } from '../../services/auth-client';
import { Router } from '@angular/router';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ForgotPasswordRequest } from '../../models/requests/forgot-password-request';
import { ERROR_MESSAGES } from '../../shared/constants/error-messages';
import { LoginHeader } from '../login-header/login-header';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-forgot-password',
  imports: [Error, ReactiveFormsModule, LoginHeader],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.scss',
})
export class ForgotPassword {
  private auth = inject(AuthClient);
  private router = inject(Router);
  private formBuilder = inject(NonNullableFormBuilder);

  errorMessage = signal<string | undefined>(undefined);
  isEmailSent = signal(false);
  btnSpinner = signal(false);

  form = this.formBuilder.group({
    email: ['', [Validators.required, Validators.email]],
  });

  onLoginNavigation() {
    this.router.navigate(['/login']);
  }

  onForgotPassword() {
    if (this.form.invalid) return;

    this.btnSpinner.set(true);

    const request: ForgotPasswordRequest = this.form.getRawValue();

    this.auth
      .forgotPassword(request)
      .pipe(
        finalize(() => {
          this.btnSpinner.set(false);
        }),
      )
      .subscribe({
        next: (response) => {
          this.isEmailSent.set(true);
          this.errorMessage.set(undefined);
        },
        error: (err) => {
          this.errorMessage.set(err?.error?.detail ?? ERROR_MESSAGES.generic);
        },
      });
  }
}
