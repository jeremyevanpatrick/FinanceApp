import { HttpInterceptorFn, HttpErrorResponse, HttpClient } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, EMPTY, throwError } from 'rxjs';
import { ErrorResponse } from '../models/responses/error-response';
import { RecoverableErrorCodes } from '../shared/constants/recoverable-api-error-codes';
import { Router } from '@angular/router';
import { ApiErrorCodes } from '../shared/constants/api-error-codes';
import { ERROR_MESSAGES } from '../shared/constants/error-messages';
import {
  CORRELATION_ID_HEADER_NAME,
  SKIP_AUTH_AND_ERROR_HEADER,
  SKIP_ERROR_HEADER_NAME,
} from '../shared/constants/headers';
import { ApplicationLog } from '../models/application-log';
import { getDateTimeNow } from '../shared/helpers/dates';
import { APP_CONFIG } from '../models/app-config';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const http = inject(HttpClient);
  const config = inject(APP_CONFIG);

  //prevent attempting to handle/log failures when calling the log service
  if (req.headers.has(SKIP_ERROR_HEADER_NAME)) {
    const cleanReq = req.clone({ headers: req.headers.delete(SKIP_ERROR_HEADER_NAME) });
    return next(cleanReq);
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const errorResponse = error.error as ErrorResponse;
      const errorCode = errorResponse?.errorCode ?? '';
      const errorDetail = errorResponse?.detail ?? '';

      //check for recoverable error
      if (error.status >= 400 && error.status < 500) {
        if (RecoverableErrorCodes.includes(errorCode) && errorDetail) {
          if (errorCode === ApiErrorCodes.UNAUTHORIZED) {
            router.navigate(['/login']);
            return EMPTY;
          }
          return throwError(() => error);
        }
      }

      //log unexpected error
      const request: ApplicationLog[] = [
        {
          id: 0,
          level: 'Error',
          serverName: '',
          applicationName: config.appName,
          errorCode: errorCode,
          message: errorDetail,
          correlationId: error.headers.get(CORRELATION_ID_HEADER_NAME) ?? '',
          timestamp: getDateTimeNow(),
        },
      ];

      http.post(`${config.remoteLoggingUrl}`, request, SKIP_AUTH_AND_ERROR_HEADER).subscribe({
        error: (err) => {
          console.error('Failed to send log:', err);
        },
      });

      return throwError(
        () =>
          new HttpErrorResponse({
            headers: error.headers,
            status: error.status,
            url: error.url ?? undefined,
            error: {
              ...error.error,
              detail: ERROR_MESSAGES.generic,
            },
          }),
      );
    }),
  );
};
