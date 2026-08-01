import { Component, inject, signal } from '@angular/core';
import { Error } from '../error/error';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ResendConfirmationEmailRequest } from '../../models/requests/resend-confirmation-email-request';
import { AuthClient } from '../../services/auth-client';
import { ERROR_MESSAGES } from '../../shared/constants/error-messages';
import { ActivatedRoute } from '@angular/router';
import { finalize } from 'rxjs';
import { ConfirmEmailRequest } from '../../models/requests/confirm-email-request';
import { LoginHeader } from '../login-header/login-header';

@Component({
  selector: 'app-confirm-email',
  imports: [Error, ReactiveFormsModule, LoginHeader],
  templateUrl: './confirm-email.html',
  styleUrl: './confirm-email.scss',
})
export class ConfirmEmail {
  constructor(private route: ActivatedRoute) {}

  private auth = inject(AuthClient);
  private formBuilder = inject(NonNullableFormBuilder);

  private userId = '';
  private token = '';

  ngOnInit(): void {
    this.userId = this.route.snapshot.queryParamMap.get('userid') ?? '';
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';

    if (!this.userId || !this.token) {
      this.errorMessage.set('Error loading page');
      this.isLoading.set(false);
      return;
    }

    const request: ConfirmEmailRequest = {
      userId: this.userId,
      token: this.token,
    };

    this.auth
      .confirmEmail(request)
      .pipe(
        finalize(() => {
          this.isLoading.set(false);
        }),
      )
      .subscribe({
        next: (response) => {
          this.isConfirmationSuccess.set(true);
          this.errorMessage.set(undefined);
        },
        error: (err) => {
          this.isConfirmationSuccess.set(false);
          this.errorMessage.set(err?.error?.detail ?? ERROR_MESSAGES.generic);
        },
      });
  }

  errorMessage = signal<string | undefined>(undefined);
  isLoading = signal(true);
  isConfirmationSuccess = signal(false);
  isEmailResent = signal(false);
  btnSpinner = signal(false);

  form = this.formBuilder.group({
    email: ['', [Validators.required, Validators.email]],
  });

  onResendEmailConfirmation() {
    if (this.form.invalid) return;

    this.btnSpinner.set(true);

    const request: ResendConfirmationEmailRequest = this.form.getRawValue();

    this.auth
      .resendConfirmationEmail(request)
      .pipe(
        finalize(() => {
          this.btnSpinner.set(false);
        }),
      )
      .subscribe({
        next: (response) => {
          this.isEmailResent.set(true);
          this.errorMessage.set(undefined);
        },
        error: (err) => {
          this.errorMessage.set(err?.error?.detail ?? ERROR_MESSAGES.generic);
        },
      });
  }
}
