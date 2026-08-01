import { Routes } from '@angular/router';
import { Login } from './views/login/login';
import { ForgotPassword } from './views/forgot-password/forgot-password';
import { ResetPassword } from './views/reset-password/reset-password';
import { ConfirmEmailChange } from './views/confirm-email-change/confirm-email-change';
import { ConfirmEmail } from './views/confirm-email/confirm-email';
import { Budget } from './views/budget/budget';
import { Account } from './views/account/account';
import { authGuard } from './guards/auth';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'budgets',
    pathMatch: 'full',
  },
  {
    path: 'login',
    component: Login,
  },
  {
    path: 'forgotpassword',
    component: ForgotPassword,
  },
  {
    path: 'resetpassword',
    component: ResetPassword,
  },
  {
    path: 'confirmemailchange',
    component: ConfirmEmailChange,
  },
  {
    path: 'confirmemail',
    component: ConfirmEmail,
  },
  {
    path: 'budgets',
    component: Budget,
    canActivate: [authGuard],
  },
  {
    path: 'budgets/:year/:month',
    component: Budget,
    canActivate: [authGuard],
  },
  {
    path: 'account',
    component: Account,
    canActivate: [authGuard],
  },
];
