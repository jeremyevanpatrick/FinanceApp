import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConfirmEmailChange } from './confirm-email-change';
import { ActivatedRoute, convertToParamMap, ParamMap } from '@angular/router';
import { NEVER, Observable, of, throwError } from 'rxjs';
import { ChangeEmailConfirmationRequest } from '../../models/requests/change-email-confirmation-request';
import { AuthClient } from '../../services/auth-client';

describe('ConfirmEmailChange', () => {
  let component: ConfirmEmailChange;
  let fixture: ComponentFixture<ConfirmEmailChange>;

  let routeMock: {
    queryParamMap: Observable<ParamMap>;
    snapshot: {
      queryParamMap: ParamMap;
    };
  };

  //mocking the API calls for maintainability and fast feedback
  let authClientMock: {
    confirmEmail: ReturnType<
      typeof vi.fn<(request: ChangeEmailConfirmationRequest) => Observable<void>>
    >;
  };

  function setPath(userId: string, email: string, token: string) {
    const params = convertToParamMap({
      userId: userId,
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
      confirmEmail: vi.fn().mockImplementation((request: ChangeEmailConfirmationRequest) => {
        return of(undefined);
      }),
    };

    await TestBed.configureTestingModule({
      imports: [ConfirmEmailChange],
      providers: [
        { provide: AuthClient, useValue: authClientMock },
        {
          provide: ActivatedRoute,
          useValue: routeMock,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ConfirmEmailChange);
    component = fixture.componentInstance;
  });

  //Verify required parameters
  it('should show error when userid is missing', () => {
    setPath('', 'testemail', 'testtoken');
    expect(component.errorMessage()).toBeTruthy();
  });

  it('should show error when email is missing', () => {
    setPath('testuserid', '', 'testtoken');
    expect(component.errorMessage()).toBeTruthy();
  });

  it('should show error when token is missing', () => {
    setPath('testuserid', 'testemail', '');
    expect(component.errorMessage()).toBeTruthy();
  });

  //Verify page states
  it('should show in-progress message when page loads', () => {
    authClientMock.confirmEmail.mockReturnValue(NEVER);
    setPath('testuserid', 'testemail', 'testtoken');

    //Verify in-progress confirming message is displayed when page loads
    expect(element('#confirmingMessage')).toBeTruthy();

    //Verify success message is NOT displayed by default
    expect(element('#confirmEmailChangeSuccessMessage')).toBeFalsy();

    //Verify error message is NOT set by default
    expect(component.errorMessage()).toBeFalsy();
  });

  it('should show success message when initial request completes', () => {
    const userId = 'testuserid';
    const email = 'test@email.com';
    const token = 'testtoken';
    setPath(userId, email, token);

    //Verify in-progress confirming message is hidden
    expect(element('#confirmingMessage')).toBeFalsy();

    //Verify success message is displayed
    expect(element('#confirmEmailChangeSuccessMessage')).toBeTruthy();

    //Verify error message is NOT set
    expect(component.errorMessage()).toBeFalsy();

    //Verify confirm email endpoint was called
    expect(authClientMock.confirmEmail).toHaveBeenCalledWith({
      userId: userId,
      newEmail: email,
      token: token,
    });
  });

  it('should show error message when initial request fails', () => {
    //must override the mock BEFORE calling setPath()
    authClientMock.confirmEmail = vi
      .fn()
      .mockImplementation((userId: string, email: string, token: string) => {
        return throwError(() => ({
          error: {
            detail: undefined,
          },
          status: 500,
        }));
      });

    const userId = 'testuserid';
    const email = 'test@email.com';
    const token = 'testtoken';
    setPath(userId, email, token);

    //Verify error message is displayed
    expect(element('#confirmEmailChangeErrorMessage')).toBeTruthy();

    //Verify error is displayed
    expect(component.errorMessage()).toBeTruthy();
  });
});
