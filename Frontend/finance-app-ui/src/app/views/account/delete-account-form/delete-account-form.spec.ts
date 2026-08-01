import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DeleteAccountForm } from './delete-account-form';
import { Observable, of, throwError } from 'rxjs';
import { AuthClient } from '../../../services/auth-client';
import { DeleteAccountRequest } from '../../../models/requests/delete-account-request';
import { Router } from '@angular/router';

describe('DeleteAccountForm', () => {
  let component: DeleteAccountForm;
  let fixture: ComponentFixture<DeleteAccountForm>;

  let router: Router;

  //mocking the API calls for maintainability and fast feedback
  let authClientMock: {
    deleteAccount: ReturnType<typeof vi.fn<(request: DeleteAccountRequest) => Observable<void>>>;
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
      deleteAccount: vi.fn().mockImplementation((request: DeleteAccountRequest) => {
        return of(undefined);
      }),
    };

    await TestBed.configureTestingModule({
      imports: [DeleteAccountForm],
      providers: [{ provide: AuthClient, useValue: authClientMock }],
    }).compileComponents();

    fixture = TestBed.createComponent(DeleteAccountForm);
    component = fixture.componentInstance;
    fixture.detectChanges();

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
  });

  it('should show button when page loads', () => {
    //Verify button is displayed when page loads
    expect(element('#showDeleteModalBtn')).toBeTruthy();

    //Verify modal is NOT displayed when page loads
    expect(element('#deleteAccountBtn')).toBeFalsy();
  });

  it('should show modal with form when Delete button is clicked', () => {
    //show delete modal
    click('#showDeleteModalBtn');
    fixture.detectChanges();

    //Verify button is displayed when modal loads
    expect(element('#deleteAccountBtn')).toBeTruthy();

    //Verify success message is NOT displayed by default
    expect(element('#changePasswordSuccessMessage')).toBeFalsy();

    //Verify error message is NOT set by default
    expect(component.errorMessage()).toBeFalsy();
  });

  it('should hide modal when Cancel button is clicked', () => {
    //show delete modal
    click('#showDeleteModalBtn');
    fixture.detectChanges();

    //cancel delete
    click('#cancelDeleteBtn');
    fixture.detectChanges();

    //Verify modal is NOT displayed when cancel button is clicked
    expect(element('#deleteAccountBtn')).toBeFalsy();
  });

  it('should send successful change delete account request', () => {
    const passwordInput = 'test';

    //show delete modal
    click('#showDeleteModalBtn');
    fixture.detectChanges();

    //set required form fields
    const passwordElement = element('#deleteAccountPassword');
    passwordElement.value = passwordInput;
    passwordElement.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    //submit form
    click('#deleteAccountBtn');
    fixture.detectChanges();

    //Verify request is sent
    expect(authClientMock.deleteAccount).toHaveBeenCalledWith({
      password: passwordInput,
    });

    //Verify redirect to login page
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  //Should show errors
  it('should display error when request fails', () => {
    const passwordInput = 'test';

    authClientMock.deleteAccount = vi.fn().mockImplementation((request: DeleteAccountRequest) => {
      return throwError(() => ({
        error: {
          detail: undefined,
        },
        status: 500,
      }));
    });
    fixture.detectChanges();

    //show delete modal
    click('#showDeleteModalBtn');
    fixture.detectChanges();

    //set required form fields
    const passwordElement = element('#deleteAccountPassword');
    passwordElement.value = passwordInput;
    passwordElement.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    //submit form
    click('#deleteAccountBtn');
    fixture.detectChanges();

    //Verify error message is set
    expect(component.errorMessage()).toBeTruthy();
  });
});
