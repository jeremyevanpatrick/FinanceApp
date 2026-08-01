import { TestBed } from '@angular/core/testing';

import { BudgetClient } from './budget-client';

describe('BudgetClient', () => {
  let service: BudgetClient;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BudgetClient);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
