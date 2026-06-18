import { Injectable } from '@angular/core';

/**
 * Error Notification Service
 * 
 * Centralized service for displaying user notifications (errors, warnings, success messages).
 * This provides a single point of control for all user-facing feedback across the application.
 * 
 * Benefits:
 * - Consistent messaging across all components
 * - Easy to customize notification UI globally
 * - Simplifies component code (no inline error handling)
 * - Enables future integration with toast/snackbar libraries
 * 
 * Future Enhancement: Replace console.log with actual toast UI component
 * Example: Material Snackbar, Angular Toastr, or custom toast component
 */
@Injectable({
  providedIn: 'root'
})
export class ErrorNotificationService {
  private readonly DEFAULT_DURATION_MS = 5000; // 5 seconds

  constructor() {}

  /**
   * Display error message to user
   * 
   * @param message - Error message to display
   * @param durationMs - Duration in milliseconds (default: 5000ms)
   * 
   * Example:
   * this.errorNotificationService.showError('Failed to load profile');
   * this.errorNotificationService.showError('Invalid credentials', 3000);
   */
  showError(message: string, durationMs?: number): void {
    const duration = durationMs || this.DEFAULT_DURATION_MS;
    console.error(`[ERROR Notification] ${message} (${duration}ms)`);
    // TODO: Replace with actual toast/snackbar component
    // this.snackBar.open(message, 'Dismiss', { duration, panelClass: 'error-notification' });
  }

  /**
   * Display warning message to user
   * 
   * @param message - Warning message to display
   * @param durationMs - Duration in milliseconds (default: 5000ms)
   * 
   * Example:
   * this.errorNotificationService.showWarning('Token expiring soon');
   */
  showWarning(message: string, durationMs?: number): void {
    const duration = durationMs || this.DEFAULT_DURATION_MS;
    console.warn(`[WARNING Notification] ${message} (${duration}ms)`);
    // TODO: Replace with actual toast/snackbar component
    // this.snackBar.open(message, 'Dismiss', { duration, panelClass: 'warning-notification' });
  }

  /**
   * Display success message to user
   * 
   * @param message - Success message to display
   * @param durationMs - Duration in milliseconds (default: 3000ms for success)
   * 
   * Example:
   * this.errorNotificationService.showSuccess('Profile updated successfully');
   */
  showSuccess(message: string, durationMs?: number): void {
    const duration = durationMs || 3000; // Shorter duration for success messages
    console.log(`[SUCCESS Notification] ${message} (${duration}ms)`);
    // TODO: Replace with actual toast/snackbar component
    // this.snackBar.open(message, 'Dismiss', { duration, panelClass: 'success-notification' });
  }

  /**
   * Log error to console and potentially to external service
   * 
   * @param error - Error object or message
   * @param context - Optional context about where error occurred
   * 
   * Example:
   * this.errorNotificationService.logError(error, 'PersonnelService.searchProfiles()');
   * 
   * Note: Use this for errors that should be logged but not shown to user
   * (e.g., unexpected errors, debugging information)
   */
  logError(error: any, context?: string): void {
    const errorMessage = error?.message || String(error);
    const contextStr = context ? ` [Context: ${context}]` : '';
    console.error(`[ERROR Log]${contextStr} ${errorMessage}`, error);
    // TODO: Send to error tracking service (Sentry, App Insights, etc.)
  }

  /**
   * Clear all displayed notifications
   * 
   * Useful when navigating between pages or during logout
   * 
   * Example:
   * this.errorNotificationService.clearAll();
   */
  clearAll(): void {
    console.log('[Notifications] Clearing all notifications');
    // TODO: Implement in toast/snackbar component
  }
}
