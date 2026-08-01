import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Login } from './login';
import { provideRouter } from '@angular/router';

describe('Login', () => {
  let component: Login;
  let fixture: ComponentFixture<Login>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(Login);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  //Verify parent state used for subcomponent parameter
  it('should toggle login panel when event is emitted', () => {
    const startingValue = component.showLogin();

    //Toggle
    component.onToggleSignIn();

    const toggledValue = component.showLogin();

    //Verify value changed
    expect(startingValue).not.toBe(toggledValue);
  });

  it('should toggle back when event is emitted', () => {
    const startingValue = component.showLogin();

    //Toggle
    component.onToggleSignIn();
    //Toggle back
    component.onToggleSignIn();

    const endingValue = component.showLogin();

    //Verify value changed back to original value
    expect(startingValue).toBe(endingValue);
  });
});
