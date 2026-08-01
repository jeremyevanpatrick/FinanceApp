import { Component, inject, signal } from '@angular/core';
import { Error } from '../../error/error';
import { AuthClient } from '../../../services/auth-client';
import { Router } from '@angular/router';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DeleteAccountRequest } from '../../../models/requests/delete-account-request';
import { ERROR_MESSAGES } from '../../../shared/constants/error-messages';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-delete-account-form',
  imports: [ReactiveFormsModule, Error],
  templateUrl: './delete-account-form.html',
  styleUrl: './delete-account-form.scss',
})
export class DeleteAccountForm {
  private auth = inject(AuthClient);
  private router = inject(Router);
  private formBuilder = inject(NonNullableFormBuilder);

  errorMessage = signal<string | undefined>(undefined);
  isModalVisible = signal(false);
  isButtonDisabled = signal(true);
  btnSpinner = signal(false);

  form = this.formBuilder.group({
    password: ['', [Validators.required]],
  });

  ngOnInit() {
    this.form.get('password')?.valueChanges.subscribe((value) => {
      const hasValue = !!value && value.trim().length > 0;
      this.isButtonDisabled.set(!hasValue);
    });
  }

  onDeleteAccount() {
    if (this.form.invalid) return;

    this.btnSpinner.set(true);

    const request: DeleteAccountRequest = this.form.getRawValue();

    this.auth
      .deleteAccount(request)
      .pipe(
        finalize(() => {
          this.btnSpinner.set(false);
        }),
      )
      .subscribe({
        next: (response) => {
          this.router.navigate(['/login']);
        },
        error: (err) => {
          this.errorMessage.set(err?.error?.detail ?? ERROR_MESSAGES.generic);
        },
      });
  }

  onShowDeleteModal() {
    this.isModalVisible.set(true);
  }

  onHideDeleteModal() {
    this.isModalVisible.set(false);
    this.errorMessage.set(undefined);
    this.form.reset();
  }
}
