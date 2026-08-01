import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LoginForm } from './login-form';
import { provideRouter, Router } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';
import { LoginRequest } from '../../../models/requests/login-request';
import { AuthClient } from '../../../services/auth-client';

describe('LoginForm', () => {
  let component: LoginForm;
  let fixture: ComponentFixture<LoginForm>;

  let router: Router;

  //mocking the API calls for maintainability and fast feedback
  let authClientMock: {
    login: ReturnType<typeof vi.fn<(request: LoginRequest) => Observable<void>>>;
  };

  function click(id: string) {
    fixture.nativeElement.querySelector(id).click();
  }

  function text(id: string) {
    return fixture.nativeElement.querySelector(id).textContent;
  }

  function value(id: string) {
    return fixture.nativeElement.querySelector(id).value;
  }

  function element(id: string) {
    return fixture.nativeElement.querySelector(id);
  }

  beforeEach(async () => {
    authClientMock = {
      login: vi.fn().mockImplementation((request: LoginRequest) => {
        return of(undefined);
      }),
    };

    await TestBed.configureTestingModule({
      imports: [LoginForm],
      providers: [provideRouter([]), { provide: AuthClient, useValue: authClientMock }],
    }).compileComponents();

    fixture = TestBed.createComponent(LoginForm);
    fixture.componentRef.setInput('isOpen', true);
    component = fixture.componentInstance;

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
  });

  //Verify page states
  it('should show login form when page loads', () => {
    fixture.detectChanges();

    //Verify form is displayed
    expect(element('#signInBtn')).toBeTruthy();

    //Verify error message is NOT set by default
    expect(component.errorMessage()).toBeFalsy();
  });

  it('should show success message when request completes', () => {
    fixture.detectChanges();
    const email = 'test@email.com';
    const password = 'testpassword';

    //set the form values
    const emailLoginElement = element('#emailLogin');
    emailLoginElement.value = email;
    emailLoginElement.dispatchEvent(new Event('input'));
    const passwordLoginElement = element('#passwordLogin');
    passwordLoginElement.value = password;
    passwordLoginElement.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    //submit form
    click('#signInBtn');
    fixture.detectChanges();

    //Verify reset password endpoint was called
    expect(authClientMock.login).toHaveBeenCalledWith({
      email: email,
      password: password,
    });

    //Verify navigation
    expect(router.navigate).toHaveBeenCalledWith(['/budgets']);
  });

  it('should show error message when request fails', () => {
    //must override the mock BEFORE calling setPath()
    authClientMock.login = vi.fn().mockImplementation((request: LoginRequest) => {
      return throwError(() => ({
        error: {
          detail: undefined,
        },
        status: 500,
      }));
    });

    fixture.detectChanges();
    const email = 'test@email.com';
    const password = 'testpassword';

    //set the form values
    const emailLoginElement = element('#emailLogin');
    emailLoginElement.value = email;
    emailLoginElement.dispatchEvent(new Event('input'));
    const passwordLoginElement = element('#passwordLogin');
    passwordLoginElement.value = password;
    passwordLoginElement.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    //submit form
    click('#signInBtn');
    fixture.detectChanges();

    //Verify error is displayed
    expect(component.errorMessage()).toBeTruthy();
  });

  it('should toggle login/signup when button is clicked', () => {
    const emitSpy = vi.spyOn(component.toggle, 'emit');
    fixture.detectChanges();

    click('#toggleRegisterBtn');
    fixture.detectChanges();

    expect(emitSpy).toHaveBeenCalled();
  });

  it('should have link to forgot password page', () => {
    fixture.detectChanges();

    const link = element('#forgotPasswordBtn') as HTMLAnchorElement;

    expect(link.getAttribute('href')).toBe('/forgotpassword');
  });
});
