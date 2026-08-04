import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChangeEmailForm } from './change-email-form';
import { Observable, of, throwError } from 'rxjs';
import { AuthClient } from '../../../services/auth-client';
import { ChangeEmailRequest } from '../../../models/requests/change-email-request';

describe('ChangeEmailForm', () => {
  let component: ChangeEmailForm;
  let fixture: ComponentFixture<ChangeEmailForm>;

  //mocking the API calls for maintainability and fast feedback
  let authClientMock: {
    changeEmail: ReturnType<typeof vi.fn<(request: ChangeEmailRequest) => Observable<void>>>;
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
      changeEmail: vi.fn().mockImplementation((request: ChangeEmailRequest) => {
        return of(undefined);
      }),
    };

    await TestBed.configureTestingModule({
      imports: [ChangeEmailForm],
      providers: [{ provide: AuthClient, useValue: authClientMock }],
    }).compileComponents();

    fixture = TestBed.createComponent(ChangeEmailForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should show form when page loads', () => {
    //Verify form is displayed when page loads
    expect(element('#changeEmailBtn')).toBeTruthy();

    //Verify success message is NOT displayed by default
    expect(element('#changeEmailSuccessMessage')).toBeFalsy();

    //Verify error message is NOT set by default
    expect(component.errorMessage()).toBeFalsy();
  });

  it('should send successful change email request', () => {
    const emailInput = 'test@email.com';
    const passwordInput = 'test';

    //set required form fields
    const emailElement = element('#changeEmailNewEmail');
    emailElement.value = emailInput;
    emailElement.dispatchEvent(new Event('input'));
    const passwordElement = element('#changeEmailPassword');
    passwordElement.value = passwordInput;
    passwordElement.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    //submit form
    click('#changeEmailBtn');
    fixture.detectChanges();

    //Verify request is sent
    expect(authClientMock.changeEmail).toHaveBeenCalledWith({
      newEmail: emailInput,
      password: passwordInput,
    });

    //Verify success message is displayed
    expect(element('#changeEmailSuccessMessage')).toBeTruthy();
  });

  //Should show errors
  it('should display error when request fails', () => {
    authClientMock.changeEmail = vi.fn().mockImplementation((request: ChangeEmailRequest) => {
      return throwError(() => ({
        error: {
          detail: undefined,
        },
        status: 500,
      }));
    });
    fixture.detectChanges();

    //set required form fields
    const emailElement = element('#changeEmailNewEmail');
    emailElement.value = 'test@email.com';
    emailElement.dispatchEvent(new Event('input'));
    const passwordElement = element('#changeEmailPassword');
    passwordElement.value = 'test';
    passwordElement.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    //submit form
    click('#changeEmailBtn');
    fixture.detectChanges();

    //Verify error message is set
    expect(component.errorMessage()).toBeTruthy();
  });
});
