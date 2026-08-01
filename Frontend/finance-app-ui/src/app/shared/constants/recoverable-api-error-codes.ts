import { ApiErrorCodes } from './api-error-codes';

export const RecoverableErrorCodes: string[] = [
  ApiErrorCodes.INVALID_CREDENTIALS,
  ApiErrorCodes.INVALID_REQUEST_PARAMETERS,
  ApiErrorCodes.TOKEN_INVALID_OR_EXPIRED,
  ApiErrorCodes.PASSWORD_DOES_NOT_MEET_REQUIREMENTS,
  ApiErrorCodes.EMAIL_ADDRESS_ALREADY_IN_USE,
  ApiErrorCodes.ACCOUNT_LOCKED,
  ApiErrorCodes.AUTH_NO_LONGER_VALID,
  ApiErrorCodes.UNAUTHORIZED,
  ApiErrorCodes.FORBIDDEN,
  ApiErrorCodes.TOOMANYREQUESTS,
] as const;
