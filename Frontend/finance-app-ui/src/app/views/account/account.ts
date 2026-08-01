import { Component, inject, signal } from '@angular/core';
import { Error } from '../error/error';
import { AuthClient } from '../../services/auth-client';
import { Storage } from '../../services/storage';
import { Router } from '@angular/router';
import { ERROR_MESSAGES } from '../../shared/constants/error-messages';
import { ChangeEmailForm } from './change-email-form/change-email-form';
import { ChangePasswordForm } from './change-password-form/change-password-form';
import { DeleteAccountForm } from './delete-account-form/delete-account-form';

@Component({
  selector: 'app-account',
  imports: [Error, ChangeEmailForm, ChangePasswordForm, DeleteAccountForm, DeleteAccountForm],
  templateUrl: './account.html',
  styleUrl: './account.scss',
})
export class Account {
  private auth = inject(AuthClient);
  private router = inject(Router);
  private storage = inject(Storage);

  errorMessage = signal<string | undefined>(undefined);
  userEmail: string = '';

  ngOnInit() {
    this.userEmail = this.storage.get('user_email') ?? '';
  }

  onLogout() {
    this.auth.logout().subscribe({
      next: (response) => {
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.errorMessage.set(err?.error?.detail ?? ERROR_MESSAGES.generic);
      },
    });
  }

  onBack() {
    this.router.navigate(['/budgets']);
  }
}
