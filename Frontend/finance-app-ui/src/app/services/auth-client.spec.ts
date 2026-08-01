import { TestBed } from '@angular/core/testing';

import { AuthClient } from './auth-client';

describe('AuthClient', () => {
  let service: AuthClient;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AuthClient);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
