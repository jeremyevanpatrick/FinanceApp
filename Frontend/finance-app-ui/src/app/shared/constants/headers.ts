import { HttpHeaders } from '@angular/common/http';

export const SKIP_AUTH_HEADER_NAME = 'X-Skip-Auth';
export const SKIP_AUTH_HEADER = {
  headers: new HttpHeaders().set(SKIP_AUTH_HEADER_NAME, 'true'),
};
export const SKIP_ERROR_HEADER_NAME = 'X-Skip-Error';
export const SKIP_AUTH_AND_ERROR_HEADER = {
  headers: new HttpHeaders().set(SKIP_AUTH_HEADER_NAME, 'true').set(SKIP_ERROR_HEADER_NAME, 'true'),
};

export const CORRELATION_ID_HEADER_NAME = 'X-Correlation-ID';
