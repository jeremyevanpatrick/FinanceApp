import { Component, inject, signal } from '@angular/core';
import { Error } from '../../error/error';
import { AuthClient } from '../../../services/auth-client';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ChangeEmailRequest } from '../../../models/requests/change-email-request';
import { ERROR_MESSAGES } from '../../../shared/constants/error-messages';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-change-email-form',
  imports: [Error, ReactiveFormsModule],
  templateUrl: './change-email-form.html',
  styleUrl: './change-email-form.scss',
})
export class ChangeEmailForm {
  private auth = inject(AuthClient);
  private formBuilder = inject(NonNullableFormBuilder);

  errorMessage = signal<string | undefined>(undefined);
  isNewEmailSaved = signal(false);
  btnSpinner = signal(false);

  form = this.formBuilder.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  onChangeEmail() {
    if (this.form.invalid) return;

    this.btnSpinner.set(true);

    const request: ChangeEmailRequest = this.form.getRawValue();

    this.auth
      .changeEmail(request)
      .pipe(
        finalize(() => {
          this.btnSpinner.set(false);
        }),
      )
      .subscribe({
        next: (response) => {
          this.isNewEmailSaved.set(true);
          this.errorMessage.set(undefined);
        },
        error: (err) => {
          this.errorMessage.set(err?.error?.detail ?? ERROR_MESSAGES.generic);
        },
      });
  }
}
