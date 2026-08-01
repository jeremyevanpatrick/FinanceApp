import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ForgotPassword } from './forgot-password';
import { provideRouter, Router } from '@angular/router';
import { NEVER, Observable, of, throwError } from 'rxjs';
import { AuthClient } from '../../services/auth-client';

describe('ForgotPassword', () => {
  let component: ForgotPassword;
  let fixture: ComponentFixture<ForgotPassword>;

  let router: Router;

  //mocking the API calls for maintainability and fast feedback
  let authClientMock: {
    forgotPassword: ReturnType<typeof vi.fn<() => Observable<void>>>;
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
      forgotPassword: vi.fn().mockImplementation((email: string) => {
        return of(undefined);
      }),
    };

    await TestBed.configureTestingModule({
      imports: [ForgotPassword],
      providers: [provideRouter([]), { provide: AuthClient, useValue: authClientMock }],
    }).compileComponents();

    fixture = TestBed.createComponent(ForgotPassword);
    component = fixture.componentInstance;

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
  });

  //Verify page states
  it('should show forgot password form when page loads', () => {
    fixture.detectChanges();

    //Verify form is displayed when page loads
    expect(element('#forgotPasswordBtn')).toBeTruthy();

    //Verify success message is NOT displayed by default
    expect(element('#forgotPasswordSuccessMessage')).toBeFalsy();

    //Verify error message is NOT set by default
    expect(component.errorMessage()).toBeFalsy();
  });

  it('should show success message when request completes', () => {
    fixture.detectChanges();
    const email = 'test@email.com';

    //set the email value
    const emailElement = element('#email');
    emailElement.value = email;
    emailElement.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    //submit form
    click('#forgotPasswordBtn');
    fixture.detectChanges();

    //Verify forgot password endpoint was called
    expect(authClientMock.forgotPassword).toHaveBeenCalledWith({
      email: email,
    });

    //Verify form is NOT displayed
    expect(element('#forgotPasswordBtn')).toBeFalsy();

    //Verify success message is displayed
    expect(element('#forgotPasswordSuccessMessage')).toBeTruthy();
  });

  it('should show error message when request fails', () => {
    //must override the mock BEFORE calling setPath()
    authClientMock.forgotPassword = vi.fn().mockImplementation((email: string) => {
      return throwError(() => ({
        error: {
          detail: undefined,
        },
        status: 500,
      }));
    });

    fixture.detectChanges();

    const email = 'test@email.com';

    //set the email value
    const emailElement = element('#email');
    emailElement.value = email;
    emailElement.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    //submit form
    click('#forgotPasswordBtn');
    fixture.detectChanges();

    //Verify error is displayed
    expect(component.errorMessage()).toBeTruthy();
  });

  it('should navigate to login page when Back button is clicked', () => {
    fixture.detectChanges();

    click('#backBtn');
    fixture.detectChanges();

    //Navigating to login page
    expect(router.navigate).toHaveBeenCalledWith([`/login`]);
  });
});
