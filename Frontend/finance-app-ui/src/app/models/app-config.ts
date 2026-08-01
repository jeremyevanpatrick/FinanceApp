import { InjectionToken } from '@angular/core';

export interface AppConfig {
  production: boolean;
  apiBaseUrl: string;
  authBaseUrl: string;
  remoteLoggingUrl: string;
  appName: string;
  enableRunningLogs: boolean;
}

export const APP_CONFIG = new InjectionToken<AppConfig>('APP_CONFIG');
