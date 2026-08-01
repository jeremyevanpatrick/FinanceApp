export interface ChangeEmailConfirmationRequest {
  userId: string;
  newEmail: string;
  token: string;
}
