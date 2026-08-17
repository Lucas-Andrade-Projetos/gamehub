import { HttpErrorResponse } from '@angular/common/http';

export function extractErrorMessage(error: HttpErrorResponse, fallback: string): string {
  if (typeof error.error === 'string') return error.error;

  const validationErrors = error.error?.errors;
  if (validationErrors && typeof validationErrors === 'object') {
    const firstMessage = Object.values(validationErrors).flat()[0];
    if (typeof firstMessage === 'string') return firstMessage;
  }

  return fallback;
}
