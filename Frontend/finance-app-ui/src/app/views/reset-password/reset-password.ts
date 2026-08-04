import { Component, inject, signal } from '@angular/core';
import { Error } from '../error/error';
import { ERROR_MESSAGES } from '../../shared/constants/error-messages';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ResetPasswordRequest } from '../../models/requests/reset-password-request';
import { AuthClient } from '../../services/auth-client';
import { ActivatedRoute } from '@angular/router';
import { LoginHeader } from '../login-header/login-header';
import { finalize } from 'rxjs';
import { passwordsMatchValidator } from '../../shared/validators/passwords-match';

@Component({
  selector: 'app-reset-password',
  imports: [Error, ReactiveFormsModule, LoginHeader],
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.scss',
})
export class ResetPassword {
  constructor(private route: ActivatedRoute) {}

  private auth = inject(AuthClient);
  private formBuilder = inject(NonNullableFormBuilder);

  private email = '';
  private token = '';

  ngOnInit(): void {
    this.email = this.route.snapshot.queryParamMap.get('email') ?? '';
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';
  }

  errorMessage = signal<string | undefined>(undefined);
  isEmailSent = signal(false);
  btnSpinner = signal(false);

  form = this.formBuilder.group(
    {
      password: ['', [Validators.required]],
      passwordRepeat: ['', [Validators.required]],
    },
    {
      validators: passwordsMatchValidator,
    },
  );

  onResetPassword() {
    if (this.form.invalid) return;

    this.btnSpinner.set(true);

    const formValue = this.form.getRawValue();

    const request: ResetPasswordRequest = {
      newPassword: formValue.password,
      email: this.email,
      resetCode: this.token,
    };

    this.auth
      .resetPassword(request)
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
