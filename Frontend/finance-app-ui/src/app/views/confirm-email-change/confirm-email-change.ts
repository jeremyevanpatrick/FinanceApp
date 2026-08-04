import { Component, inject, signal } from '@angular/core';
import { AuthClient } from '../../services/auth-client';
import { ActivatedRoute } from '@angular/router';
import { Error } from '../error/error';
import { ChangeEmailConfirmationRequest } from '../../models/requests/change-email-confirmation-request';
import { finalize } from 'rxjs';
import { ERROR_MESSAGES } from '../../shared/constants/error-messages';
import { LoginHeader } from '../login-header/login-header';

@Component({
  selector: 'app-confirm-email-change',
  imports: [Error, LoginHeader],
  templateUrl: './confirm-email-change.html',
  styleUrl: './confirm-email-change.scss',
})
export class ConfirmEmailChange {
  constructor(private route: ActivatedRoute) {}

  private auth = inject(AuthClient);

  private userId = '';
  private email = '';
  private token = '';

  ngOnInit(): void {
    this.userId = this.route.snapshot.queryParamMap.get('userid') ?? '';
    this.email = this.route.snapshot.queryParamMap.get('email') ?? '';
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';

    if (!this.userId || !this.email || !this.token) {
      this.errorMessage.set('Error loading page');
      this.isLoading.set(false);
      return;
    }

    const request: ChangeEmailConfirmationRequest = {
      userId: this.userId,
      newEmail: this.email,
      token: this.token,
    };

    this.auth
      .changeEmailConfirmation(request)
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
}
