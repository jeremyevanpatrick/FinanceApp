import { TestBed } from '@angular/core/testing';

import { AuthClient } from './auth-client';
import { APP_CONFIG } from '../models/app-config';

describe('AuthClient', () => {
  let service: AuthClient;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        {
          provide: APP_CONFIG,
          useValue: {
            authBaseUrl: '',
          },
        },
      ],
    });
    service = TestBed.inject(AuthClient);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
