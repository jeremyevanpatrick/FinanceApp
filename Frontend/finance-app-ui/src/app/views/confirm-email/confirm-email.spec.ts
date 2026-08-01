import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConfirmEmail } from './confirm-email';
import { ActivatedRoute, convertToParamMap, ParamMap } from '@angular/router';
import { NEVER, Observable, of, throwError } from 'rxjs';
import { ConfirmEmailRequest } from '../../models/requests/confirm-email-request';
import { ResendConfirmationEmailRequest } from '../../models/requests/resend-confirmation-email-request';
import { AuthClient } from '../../services/auth-client';

describe('ConfirmEmail', () => {
  let component: ConfirmEmail;
  let fixture: ComponentFixture<ConfirmEmail>;

  let routeMock: {
    queryParamMap: Observable<ParamMap>;
    snapshot: {
      queryParamMap: ParamMap;
    };
  };

  //mocking the API calls for maintainability and fast feedback
  let authClientMock: {
    confirmEmail: ReturnType<typeof vi.fn<(request: ConfirmEmailRequest) => Observable<void>>>;
    resendConfirmationEmail: ReturnType<
      typeof vi.fn<(request: ResendConfirmationEmailRequest) => Observable<void>>
    >;
  };

  function setPath(userId: string, token: string) {
    const params = convertToParamMap({
      userid: userId,
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
      confirmEmail: vi.fn().mockImplementation((request: ConfirmEmailRequest) => {
        return of(undefined);
      }),
      resendConfirmationEmail: vi
        .fn()
        .mockImplementation((request: ResendConfirmationEmailRequest) => {
          return of(undefined);
        }),
    };

    await TestBed.configureTestingModule({
      imports: [ConfirmEmail],
      providers: [
        { provide: AuthClient, useValue: authClientMock },
        {
          provide: ActivatedRoute,
          useValue: routeMock,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ConfirmEmail);
    component = fixture.componentInstance;
  });

  //Verify required parameters
  it('should show error when userid is missing', () => {
    setPath('', 'testtoken');
    expect(component.errorMessage()).toBeTruthy();
  });

  it('should show error when token is missing', () => {
    setPath('testuserid', '');
    expect(component.errorMessage()).toBeTruthy();
  });

  //Verify page states
  it('should show in-progress message when page loads', () => {
    authClientMock.confirmEmail.mockReturnValue(NEVER);
    setPath('testuserid', 'testtoken');

    //Verify in-progress confirming message is displayed when page loads
    expect(element('#confirmingMessage')).toBeTruthy();

    //Verify success message is NOT displayed by default
    expect(element('#confirmEmailSuccessMessage')).toBeFalsy();

    //Verify error message is NOT set by default
    expect(component.errorMessage()).toBeFalsy();
  });

  it('should show success message when initial request completes', () => {
    const userId = 'testuserid';
    const token = 'testtoken';
    setPath(userId, token);

    //Verify in-progress confirming message is hidden
    expect(element('#confirmingMessage')).toBeFalsy();

    //Verify success message is displayed
    expect(element('#confirmEmailSuccessMessage')).toBeTruthy();

    //Verify error message is NOT set
    expect(component.errorMessage()).toBeFalsy();

    //Verify confirm email endpoint was called
    expect(authClientMock.confirmEmail).toHaveBeenCalledWith({
      userId: userId,
      token: token,
    });
  });

  it('should show resend form when initial request fails', () => {
    //must override the mock BEFORE calling setPath()
    authClientMock.confirmEmail = vi.fn().mockImplementation((userId: string, token: string) => {
      return throwError(() => ({
        error: {
          detail: undefined,
        },
        status: 500,
      }));
    });

    const userId = 'testuserid';
    const token = 'testtoken';
    setPath(userId, token);

    //Verify resend form is displayed
    expect(element('#resendEmailConfirmationBtn')).toBeTruthy();
  });

  it('should show success message when resend request succeeds', () => {
    //must override the mock BEFORE calling setPath()
    authClientMock.confirmEmail = vi.fn().mockImplementation((userId: string, token: string) => {
      return throwError(() => ({
        error: {
          detail: undefined,
        },
        status: 500,
      }));
    });

    const userId = 'testuserid';
    const token = 'testtoken';
    const email = 'test@email.com';
    setPath(userId, token);

    //change the editable value
    const emailElement = element('#email');
    emailElement.value = email;
    emailElement.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    //send request
    click('#resendEmailConfirmationBtn');
    fixture.detectChanges();

    //Verify resend email confirmation endpoint was called
    expect(authClientMock.resendConfirmationEmail).toHaveBeenCalledWith({
      email: email,
    });

    //Verify resend success message is displayed
    expect(element('#resendEmailConfirmationSuccessMessage')).toBeTruthy();
  });

  it('should show error when resend request fails', () => {
    //must override the mock BEFORE calling setPath()
    authClientMock.confirmEmail = vi.fn().mockImplementation((userId: string, token: string) => {
      return throwError(() => ({
        error: {
          detail: undefined,
        },
        status: 500,
      }));
    });
    authClientMock.resendConfirmationEmail = vi.fn().mockImplementation((email: string) => {
      return throwError(() => ({
        error: {
          detail: undefined,
        },
        status: 500,
      }));
    });

    const userId = 'testuserid';
    const token = 'testtoken';
    const email = 'test@email.com';
    setPath(userId, token);

    //change the editable value
    const emailElement = element('#email');
    emailElement.value = email;
    emailElement.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    //send request
    click('#resendEmailConfirmationBtn');
    fixture.detectChanges();

    //Verify error message is displayed
    expect(component.errorMessage()).toBeTruthy();
  });
});
