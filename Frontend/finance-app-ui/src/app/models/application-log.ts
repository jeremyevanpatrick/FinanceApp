export interface ApplicationLog {
  id: number;
  timestamp: string;
  level: string;
  errorCode?: string;
  message: string;
  messageTemplate?: string;
  exception?: string;
  correlationId?: string;
  serverName: string;
  applicationName: string;
}
