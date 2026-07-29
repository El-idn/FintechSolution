# PaymentService Documentation

## Overview
PaymentService handles payment creation, processing, status updates, and retrieval. It enforces robust business rules to ensure payment integrity and compliance.

## Business Rules
1. **Reference Uniqueness & Idempotency**: Prevents duplicate payments with the same reference for the same account.
2. **Payment Status Transitions**: Only allows valid status changes (e.g., Pending → Succeeded/Failed/Expired).
3. **Payment Amount Validation**: Ensures the amount is positive and within a reasonable range.
4. **Retry Logic for Failed Payments**: Allows up to 3 retries for failed payments, resets expiry on retry.
5. **Audit Logging**: Logs all status changes and important actions.
6. **Payment Expiry**: Sets expiry on creation and marks as expired if not processed in time.
7. **Custom Payment Descriptions**: Enforces max length (200 chars) and checks for forbidden words (e.g., 'fraud', 'illegal', 'scam').
8. **Consistency & Error Handling**: All logic is robust, with clear error messages and logging.

## Entity Fields
- `RetryCount` (int): Number of times a failed payment has been retried (max 3).
- `ExpiresAt` (DateTime?): When the payment expires if not processed.
- `Status` (enum): Now includes `Expired` in addition to `Pending`, `Succeeded`, and `Failed`.

## Endpoints (Sample)
- **Create Payment**: Validates reference, amount, and description. Returns existing payment if duplicate reference.
- **Process Payment**: Handles retries, expiry, and status transitions.
- **Get Payment**: Returns payment status, marks as expired if overdue.
- **Update Payment Status**: Enforces valid status transitions.

## Error Handling
- Returns clear error messages for invalid input, duplicate references, invalid transitions, max retries, and expired payments.

## Testing
- Comprehensive unit tests cover all business rules and edge cases using in-memory database and mocks.

---
For more details, see the source code and tests in this service. 