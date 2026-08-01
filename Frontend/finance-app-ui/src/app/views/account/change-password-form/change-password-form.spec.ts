import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChangePasswordForm } from './change-password-form';
import { Observable, of, throwError } from 'rxjs';
import { ChangePasswordRequest } from '../../../models/requests/change-password-request';
import { AuthClient } from '../../../services/auth-client';

describe('ChangePasswordForm', () => {
  let component: ChangePasswordForm;
  let fixture: ComponentFixture<ChangePasswordForm>;

  //mocking the API calls for maintainability and fast feedback
  let authClientMock: {
    changePassword: ReturnType<typeof vi.fn<(request: ChangePasswordRequest) => Observable<void>>>;
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
      changePassword: vi.fn().mockImplementation((request: ChangePasswordRequest) => {
        return of(undefined);
      }),
    };

    await TestBed.configureTestingModule({
      imports: [ChangePasswordForm],
      providers: [{ provide: AuthClient, useValue: authClientMock }],
    }).compileComponents();

    fixture = TestBed.createComponent(ChangePasswordForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should show form when page loads', () => {
    //Verify form is displayed when page loads
    expect(element('#changePasswordBtn')).toBeTruthy();

    //Verify success message is NOT displayed by default
    expect(element('#changePasswordSuccessMessage')).toBeFalsy();

    //Verify error message is NOT set by default
    expect(component.errorMessage()).toBeFalsy();
  });

  it('should send successful change password request', () => {
    const existingPasswordInput = 'test_existing';
    const newPasswordInput = 'test_new';

    //set required form fields
    const existingPasswordElement = element('#changePasswordExistingPassword');
    existingPasswordElement.value = existingPasswordInput;
    existingPasswordElement.dispatchEvent(new Event('input'));
    const newPasswordElement = element('#changePasswordNewPassword');
    newPasswordElement.value = newPasswordInput;
    newPasswordElement.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    //submit form
    click('#changePasswordBtn');
    fixture.detectChanges();

    //Verify request is sent
    expect(authClientMock.changePassword).toHaveBeenCalledWith({
      existingPassword: existingPasswordInput,
      newPassword: newPasswordInput,
    });

    //Verify success message is displayed
    expect(element('#changePasswordSuccessMessage')).toBeTruthy();
  });

  //Should show errors
  it('should display error when request fails', () => {
    const existingPasswordInput = 'test_existing';
    const newPasswordInput = 'test_new';

    authClientMock.changePassword = vi.fn().mockImplementation((request: ChangePasswordRequest) => {
      return throwError(() => ({
        error: {
          detail: undefined,
        },
        status: 500,
      }));
    });
    fixture.detectChanges();

    //set required form fields
    const existingPasswordElement = element('#changePasswordExistingPassword');
    existingPasswordElement.value = existingPasswordInput;
    existingPasswordElement.dispatchEvent(new Event('input'));
    const newPasswordElement = element('#changePasswordNewPassword');
    newPasswordElement.value = newPasswordInput;
    newPasswordElement.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    //submit form
    click('#changePasswordBtn');
    fixture.detectChanges();

    //Verify error message is set
    expect(component.errorMessage()).toBeTruthy();
  });
});
