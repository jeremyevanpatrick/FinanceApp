import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RegisterForm } from './register-form';
import { RegisterRequest } from '../../../models/requests/register-request';
import { Observable, of, throwError } from 'rxjs';
import { AuthClient } from '../../../services/auth-client';
import { provideRouter, Router } from '@angular/router';

describe('RegisterForm', () => {
  let component: RegisterForm;
  let fixture: ComponentFixture<RegisterForm>;

  let router: Router;

  //mocking the API calls for maintainability and fast feedback
  let authClientMock: {
    register: ReturnType<typeof vi.fn<(request: RegisterRequest) => Observable<void>>>;
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
      register: vi.fn().mockImplementation((request: RegisterRequest) => {
        return of(undefined);
      }),
    };

    await TestBed.configureTestingModule({
      imports: [RegisterForm],
      providers: [provideRouter([]), { provide: AuthClient, useValue: authClientMock }],
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterForm);
    fixture.componentRef.setInput('isOpen', true);
    component = fixture.componentInstance;

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
  });

  //Verify page states
  it('should show register form when page loads', () => {
    fixture.detectChanges();

    //Verify form is displayed
    expect(element('#signUpBtn')).toBeTruthy();

    //Verify error message is NOT set by default
    expect(component.errorMessage()).toBeFalsy();
  });

  it('should show success message when request completes', () => {
    fixture.detectChanges();
    const email = 'test@email.com';
    const password = 'testpassword';

    //set the form values
    const emailRegisterElement = element('#emailRegister');
    emailRegisterElement.value = email;
    emailRegisterElement.dispatchEvent(new Event('input'));
    const passwordRegisterElement = element('#passwordRegister');
    passwordRegisterElement.value = password;
    passwordRegisterElement.dispatchEvent(new Event('input'));
    const passwordRepeatRegisterElement = element('#passwordRepeatRegister');
    passwordRepeatRegisterElement.value = password;
    passwordRepeatRegisterElement.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    //submit form
    click('#signUpBtn');
    fixture.detectChanges();

    //Verify register endpoint was called
    expect(authClientMock.register).toHaveBeenCalledWith({
      email: email,
      password: password,
    });

    //Verify success message is displayed
    expect(element('#registerSuccessMessage')).toBeTruthy();
  });

  it('should show error message when request fails', () => {
    authClientMock.register = vi.fn().mockImplementation((request: RegisterRequest) => {
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
    const emailRegisterElement = element('#emailRegister');
    emailRegisterElement.value = email;
    emailRegisterElement.dispatchEvent(new Event('input'));
    const passwordRegisterElement = element('#passwordRegister');
    passwordRegisterElement.value = password;
    passwordRegisterElement.dispatchEvent(new Event('input'));
    const passwordRepeatRegisterElement = element('#passwordRepeatRegister');
    passwordRepeatRegisterElement.value = password;
    passwordRepeatRegisterElement.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    //submit form
    click('#signUpBtn');
    fixture.detectChanges();

    //Verify error is displayed
    expect(component.errorMessage()).toBeTruthy();
  });

  it('should toggle login/signup when button is clicked', () => {
    const emitSpy = vi.spyOn(component.toggle, 'emit');
    fixture.detectChanges();

    click('#toggleLoginBtn');
    fixture.detectChanges();

    expect(emitSpy).toHaveBeenCalled();
  });
});
