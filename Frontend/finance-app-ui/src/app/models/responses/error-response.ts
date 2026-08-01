export interface ErrorResponse {
  type?: string;
  title?: string;
  status: number;
  detail: string;
  errorCode: string;
  errors?: ResponseErrorItem[];
  instance?: string;
}

export interface ResponseErrorItem {
  field?: string;
  messages?: string[];
}
