import { Component, signal } from '@angular/core';
import { LoginForm } from './login-form/login-form';
import { RegisterForm } from './register-form/register-form';
import { LoginHeader } from '../login-header/login-header';

@Component({
  selector: 'app-login',
  imports: [LoginForm, RegisterForm, LoginHeader],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  readonly showLogin = signal(true);

  onToggleSignIn() {
    this.showLogin.set(!this.showLogin());
  }
}
