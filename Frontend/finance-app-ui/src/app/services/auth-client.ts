import { Service, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Storage } from '../services/storage';
import { finalize, shareReplay, tap } from 'rxjs/operators';
import { AuthResponse } from '../models/responses/auth-response';
import { LoginRequest } from '../models/requests/login-request';
import { RegisterRequest } from '../models/requests/register-request';
import { Observable } from 'rxjs';
import { SKIP_AUTH_HEADER } from '../shared/constants/headers';
import { DeleteAccountRequest } from '../models/requests/delete-account-request';
import { ChangeEmailRequest } from '../models/requests/change-email-request';
import { ChangePasswordRequest } from '../models/requests/change-password-request';
import { ResendConfirmationEmailRequest } from '../models/requests/resend-confirmation-email-request';
import { ConfirmEmailRequest } from '../models/requests/confirm-email-request';
import { ChangeEmailConfirmationRequest } from '../models/requests/change-email-confirmation-request';
import { ForgotPasswordRequest } from '../models/requests/forgot-password-request';
import { ResetPasswordRequest } from '../models/requests/reset-password-request';
import { APP_CONFIG } from '../models/app-config';

@Service()
export class AuthClient {
  private readonly http = inject(HttpClient);
  private readonly storage = inject(Storage);
  private config = inject(APP_CONFIG);

  private refreshInProgress$?: Observable<AuthResponse>;

  refreshAccessToken() {
    if (!this.refreshInProgress$) {
      this.refreshInProgress$ = this.http
        .post<AuthResponse>(
          `${this.config.authBaseUrl}/sessions/refresh`,
          {},
          { withCredentials: true, headers: SKIP_AUTH_HEADER.headers },
        )
        .pipe(
          tap({
            next: (response) => {
              this.storage.set('jwt_token', response.accessToken);
            },
            error: () => {
              this.storage.remove('jwt_token');
            },
          }),
          finalize(() => {
            this.refreshInProgress$ = undefined;
          }),
          shareReplay(1),
        );
    }

    return this.refreshInProgress$!;
  }

  login(request: LoginRequest) {
    return this.http
      .post<AuthResponse>(`${this.config.authBaseUrl}/sessions`, request, {
        withCredentials: true,
        headers: SKIP_AUTH_HEADER.headers,
      })
      .pipe(
        tap((response) => {
          this.storage.set('jwt_token', response.accessToken);
          this.storage.set('user_id', response.userId);
          this.storage.set('user_email', response.email);
        }),
      );
  }

  logout() {
    return this.http.delete(`${this.config.authBaseUrl}/sessions`, { withCredentials: true }).pipe(
      tap((response) => {
        this.storage.remove('jwt_token');
        this.storage.remove('user_id');
        this.storage.remove('user_email');
      }),
    );
  }

  register(request: RegisterRequest) {
    return this.http.post(`${this.config.authBaseUrl}/users`, request, SKIP_AUTH_HEADER);
  }

  resendConfirmationEmail(request: ResendConfirmationEmailRequest) {
    return this.http.post(
      `${this.config.authBaseUrl}/email-confirmation-requests/resend`,
      request,
      SKIP_AUTH_HEADER,
    );
  }

  confirmEmail(request: ConfirmEmailRequest) {
    return this.http.post(
      `${this.config.authBaseUrl}/email-confirmation-requests/confirm`,
      request,
      SKIP_AUTH_HEADER,
    );
  }

  changeEmail(request: ChangeEmailRequest) {
    return this.http.post(`${this.config.authBaseUrl}/email-change-requests`, request);
  }

  changeEmailConfirmation(request: ChangeEmailConfirmationRequest) {
    return this.http.post(`${this.config.authBaseUrl}/email-change-requests/confirm`, request);
  }

  forgotPassword(request: ForgotPasswordRequest) {
    return this.http.post(
      `${this.config.authBaseUrl}/password-reset-requests`,
      request,
      SKIP_AUTH_HEADER,
    );
  }

  resetPassword(request: ResetPasswordRequest) {
    return this.http.post(
      `${this.config.authBaseUrl}/password-reset-requests/confirm`,
      request,
      SKIP_AUTH_HEADER,
    );
  }

  deleteAccount(request: DeleteAccountRequest) {
    return this.http.delete(`${this.config.authBaseUrl}/users/me`, { body: request });
  }

  changePassword(request: ChangePasswordRequest) {
    return this.http.patch(`${this.config.authBaseUrl}/users/me/password`, request);
  }
}
