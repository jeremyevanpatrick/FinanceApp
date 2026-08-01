import { TestBed } from '@angular/core/testing';

import { FormHelper } from './form-helper';

describe('FormHelper', () => {
  let service: FormHelper;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(FormHelper);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
