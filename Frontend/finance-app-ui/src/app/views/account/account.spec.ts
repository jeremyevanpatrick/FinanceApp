import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Account } from './account';
import { provideRouter, Router } from '@angular/router';
import { AuthClient } from '../../services/auth-client';
import { Observable, of, throwError } from 'rxjs';

describe('Account', () => {
  let component: Account;
  let fixture: ComponentFixture<Account>;

  let router: Router;

  //mocking the API calls for maintainability and fast feedback
  let authClientMock: {
    logout: ReturnType<typeof vi.fn<() => Observable<void>>>;
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
      logout: vi.fn().mockImplementation(() => {
        return of(undefined);
      }),
    };

    await TestBed.configureTestingModule({
      imports: [Account],
      providers: [provideRouter([]), { provide: AuthClient, useValue: authClientMock }],
    }).compileComponents();

    fixture = TestBed.createComponent(Account);
    component = fixture.componentInstance;

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
  });

  //Navigation should work
  it('should send logout request and navigate to login page when Logout button is clicked', () => {
    click('#logoutLink');

    //Verify the logout endpoint is called
    expect(authClientMock.logout).toHaveBeenCalled();

    //Verify redirect to login page
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should navigate to budgets page when Back button is clicked', () => {
    click('#backBtn');

    //Verify navigation to budgets page
    expect(router.navigate).toHaveBeenCalledWith(['/budgets']);
  });

  //Should show errors
  it('should display error when request fails', () => {
    authClientMock.logout = vi.fn().mockImplementation(() => {
      return throwError(() => ({
        error: {
          detail: undefined,
        },
        status: 500,
      }));
    });
    fixture.detectChanges();

    click('#logoutLink');

    //Verify error message is set
    expect(component.errorMessage()).toBeTruthy();
  });
});
