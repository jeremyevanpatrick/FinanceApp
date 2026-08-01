import { Component, inject, input, output, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthClient } from '../../../services/auth-client';
import { ERROR_MESSAGES } from '../../../shared/constants/error-messages';
import { RegisterRequest } from '../../../models/requests/register-request';
import { passwordsMatchValidator } from '../../../shared/validators/passwords-match';
import { Error } from '../../error/error';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-register-form',
  imports: [ReactiveFormsModule, Error],
  templateUrl: './register-form.html',
  styleUrl: './register-form.scss',
})
export class RegisterForm {
  private readonly auth = inject(AuthClient);
  private readonly formBuilder = inject(NonNullableFormBuilder);

  readonly isOpen = input(false);
  readonly toggle = output<void>();

  readonly errorMessage = signal<string | undefined>(undefined);
  readonly isPendingEmailConfirmation = signal(false);
  readonly btnSpinner = signal(false);

  readonly form = this.formBuilder.group(
    {
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]],
      passwordRepeat: ['', [Validators.required]],
    },
    {
      validators: passwordsMatchValidator,
    },
  );

  onSignUp() {
    if (this.form.invalid) return;

    this.btnSpinner.set(true);

    const { email, password } = this.form.getRawValue();
    const request: RegisterRequest = {
      email,
      password,
    };

    this.auth
      .register(request)
      .pipe(
        finalize(() => {
          this.btnSpinner.set(false);
        }),
      )
      .subscribe({
        next: (response) => {
          this.isPendingEmailConfirmation.set(true);
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
