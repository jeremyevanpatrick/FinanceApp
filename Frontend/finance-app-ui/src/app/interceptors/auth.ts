import { HttpInterceptorFn, HttpRequest, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthClient } from '../services/auth-client';
import { Storage } from '../services/storage';
import { catchError, map, of, switchMap, throwError } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { JwtPayload } from '../models/jwt-payload';
import { AuthResponse } from '../models/responses/auth-response';
import { SKIP_AUTH_HEADER_NAME } from '../shared/constants/headers';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthClient);
  const storage = inject(Storage);

  //prevent attempting to validate/refresh the access token for unauthenticated requests
  if (req.headers.has(SKIP_AUTH_HEADER_NAME)) {
    const cleanReq = req.clone({ headers: req.headers.delete(SKIP_AUTH_HEADER_NAME) });
    return next(cleanReq);
  }

  const accessToken = storage.get('jwt_token') ?? '';

  let accessToken$ = of(accessToken);
  if (isTokenExpiredOrExpiring(accessToken)) {
    accessToken$ = auth.refreshAccessToken().pipe(map((response) => response.accessToken));
  }

  return accessToken$.pipe(
    switchMap((accessToken: string) => {
      const authReq = addAccessToken(req, accessToken);

      return next(authReq).pipe(
        catchError((error: HttpErrorResponse) => {
          if (error.status === 401) {
            return auth.refreshAccessToken().pipe(
              switchMap((authResponse: AuthResponse) => {
                const newToken = authResponse?.accessToken ?? '';
                const authReq2 = addAccessToken(req, newToken);

                return next(authReq2);
              }),
            );
          }
          return throwError(() => error);
        }),
      );
    }),
  );
};

function isTokenExpiredOrExpiring(jwtToken: string): boolean {
  if (!jwtToken) {
    return true;
  }

  try {
    const jwtPayload: JwtPayload = jwtDecode<JwtPayload>(jwtToken);
    const tokenExpirationDateMs = jwtPayload.exp * 1000;
    const twoMinutesFromNowMs = Date.now() + 2 * 60 * 1000;
    return tokenExpirationDateMs <= twoMinutesFromNowMs;
  } catch {
    return true;
  }
}

function addAccessToken(request: HttpRequest<any>, accessToken: string | null): HttpRequest<any> {
  if (accessToken) {
    return request.clone({
      setHeaders: {
        Authorization: `Bearer ${accessToken}`,
      },
    });
  }
  return request;
}
