import { Component, inject, signal } from '@angular/core';
import { Error } from '../../error/error';
import { AuthClient } from '../../../services/auth-client';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ChangePasswordRequest } from '../../../models/requests/change-password-request';
import { ERROR_MESSAGES } from '../../../shared/constants/error-messages';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-change-password-form',
  imports: [Error, ReactiveFormsModule],
  templateUrl: './change-password-form.html',
  styleUrl: './change-password-form.scss',
})
export class ChangePasswordForm {
  private auth = inject(AuthClient);
  private formBuilder = inject(NonNullableFormBuilder);

  errorMessage = signal<string | undefined>(undefined);
  isNewPasswordSaved = signal(false);
  btnSpinner = signal(false);

  form = this.formBuilder.group({
    existingPassword: ['', [Validators.required]],
    newPassword: ['', [Validators.required]],
  });

  onChangePassword() {
    if (this.form.invalid) return;

    this.btnSpinner.set(true);

    const request: ChangePasswordRequest = this.form.getRawValue();

    this.auth
      .changePassword(request)
      .pipe(
        finalize(() => {
          this.btnSpinner.set(false);
        }),
      )
      .subscribe({
        next: (response) => {
          this.isNewPasswordSaved.set(true);
          this.errorMessage.set(undefined);
        },
        error: (err) => {
          this.errorMessage.set(err?.error?.detail ?? ERROR_MESSAGES.generic);
        },
      });
  }
}
