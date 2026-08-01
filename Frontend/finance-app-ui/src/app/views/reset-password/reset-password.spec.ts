import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ResetPassword } from './reset-password';
import { ActivatedRoute, convertToParamMap, ParamMap, provideRouter } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';
import { ResetPasswordRequest } from '../../models/requests/reset-password-request';
import { AuthClient } from '../../services/auth-client';

describe('ResetPassword', () => {
  let component: ResetPassword;
  let fixture: ComponentFixture<ResetPassword>;

  let routeMock: {
    queryParamMap: Observable<ParamMap>;
    snapshot: {
      queryParamMap: ParamMap;
    };
  };

  //mocking the API calls for maintainability and fast feedback
  let authClientMock: {
    resetPassword: ReturnType<typeof vi.fn<(request: ResetPasswordRequest) => Observable<void>>>;
  };

  function setPath(email: string, token: string) {
    const params = convertToParamMap({
      email: email,
      token: token,
    });

    routeMock.queryParamMap = of(params);
    routeMock.snapshot.queryParamMap = params;

    fixture.detectChanges();
  }

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
    routeMock = {
      queryParamMap: of(convertToParamMap({})),
      snapshot: {
        queryParamMap: convertToParamMap({}),
      },
    };

    authClientMock = {
      resetPassword: vi.fn().mockImplementation((request: ResetPasswordRequest) => {
        return of(undefined);
      }),
    };

    await TestBed.configureTestingModule({
      imports: [ResetPassword],
      providers: [
        { provide: AuthClient, useValue: authClientMock },
        {
          provide: ActivatedRoute,
          useValue: routeMock,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ResetPassword);
    component = fixture.componentInstance;
  });

  //Verify page states
  it('should show reset password form when page loads', () => {
    setPath('testemail', 'testtoken');

    //Verify form is displayed
    expect(element('#resetPasswordBtn')).toBeTruthy();

    //Verify success message is NOT displayed
    expect(element('#resetPasswordSuccessMessage')).toBeFalsy();

    //Verify error message is NOT set by default
    expect(component.errorMessage()).toBeFalsy();
  });

  it('should show success message when request completes', () => {
    const email = 'test@email.com';
    const token = 'testtoken';
    const password = 'testpassword';
    setPath(email, token);

    //set the password value
    const newPasswordElement = element('#newPassword');
    newPasswordElement.value = password;
    newPasswordElement.dispatchEvent(new Event('input'));
    const repeatNewPasswordElement = element('#repeatNewPassword');
    repeatNewPasswordElement.value = password;
    repeatNewPasswordElement.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    //submit form
    click('#resetPasswordBtn');
    fixture.detectChanges();

    //Verify reset password endpoint was called
    expect(authClientMock.resetPassword).toHaveBeenCalledWith({
      email: email,
      resetCode: token,
      newPassword: password,
    });

    //Verify form is hidden
    expect(element('#resetPasswordBtn')).toBeFalsy();

    //Verify success message is displayed
    expect(element('#resetPasswordSuccessMessage')).toBeTruthy();

    //Verify error message is NOT set
    expect(component.errorMessage()).toBeFalsy();
  });

  it('should show error message when request fails', () => {
    //must override the mock BEFORE calling setPath()
    authClientMock.resetPassword = vi.fn().mockImplementation((request: ResetPasswordRequest) => {
      return throwError(() => ({
        error: {
          detail: undefined,
        },
        status: 500,
      }));
    });

    const email = 'test@email.com';
    const token = 'testtoken';
    const password = 'testpassword';
    setPath(email, token);

    //set the password value
    const newPasswordElement = element('#newPassword');
    newPasswordElement.value = password;
    newPasswordElement.dispatchEvent(new Event('input'));
    const repeatNewPasswordElement = element('#repeatNewPassword');
    repeatNewPasswordElement.value = password;
    repeatNewPasswordElement.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    //submit form
    click('#resetPasswordBtn');
    fixture.detectChanges();

    //Verify error is displayed
    expect(component.errorMessage()).toBeTruthy();
  });
});
