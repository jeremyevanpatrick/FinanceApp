import { TestBed } from '@angular/core/testing';

import { BudgetClient } from './budget-client';
import { APP_CONFIG } from '../models/app-config';

describe('BudgetClient', () => {
  let service: BudgetClient;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        {
          provide: APP_CONFIG,
          useValue: {
            apiBaseUrl: '',
          },
        },
      ],
    });
    service = TestBed.inject(BudgetClient);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
