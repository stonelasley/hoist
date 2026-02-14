# Quickstart: Authentication, Login & Registration

**Feature Branch**: `001-auth-login-registration`

## Prerequisites

- Docker running (for Aspire SQL Server container)
- .NET 10 SDK
- Node.js 18+ with npm
- Expo CLI (`npx expo`)
- Google Cloud project with OAuth 2.0 credentials (Client ID for iOS, Android, and Web)
- SendGrid account with API key

## Backend Setup

```bash
# Start the backend with Aspire (starts SQL Server container + Web API)
dotnet run --project src/AppHost

# Verify API is running
curl http://localhost:5001/api/specification.json
```

The Identity API endpoints are auto-mapped at `/api/users/*`.

## Mobile App Setup

```bash
cd src/App
npm install
npx expo start
```

Scan the QR code with Expo Go, or press `i` for iOS simulator / `a` for Android emulator.

## Environment Configuration

### Backend (User Secrets or appsettings.Development.json)

```json
{
  "SendGrid": {
    "ApiKey": "<your-sendgrid-api-key>",
    "FromEmail": "noreply@hoist.app",
    "FromName": "Hoist"
  },
  "Google": {
    "ClientId": "<your-google-client-id>"
  },
  "App": {
    "BaseUrl": "https://localhost:5001",
    "DeepLinkScheme": "hoist"
  }
}
```

### Mobile App (environment config)

The mobile app needs the API base URL and Google Client ID configured. These will be provided via Expo environment config or constants.

## Testing the Auth Flow

1. **Register**: Open the app → tap "Register" → fill in name, email, password → submit
2. **Check email**: A verification email arrives via SendGrid
3. **Verify**: Click the link in the email → app opens (or web fallback)
4. **Login**: Return to login screen → enter email/password → see "Landing Page"
5. **Google OAuth**: Tap "Sign in with Google" → complete Google auth → see "Landing Page"
6. **Forgot Password**: Tap "Forgot password?" → enter email → check email → click reset link → set new password

## Verification Checklist

- [ ] `POST /api/users/register` creates user and sends verification email
- [ ] `POST /api/users/login` returns 401 for unverified user with descriptive error
- [ ] `POST /api/users/login` returns bearer token for verified user
- [ ] `POST /api/users/verify-email` confirms email and enables login
- [ ] `POST /api/users/resend-verification` sends new verification email
- [ ] `POST /api/users/google-login` creates or links account from Google token
- [ ] `POST /api/users/forgot-password` sends reset email (200 for all inputs)
- [ ] `POST /api/users/reset-password` updates password with valid token
- [ ] Mobile login screen renders on iOS, Android, and web
- [ ] Deep links open app for verification and password reset
- [ ] Landing page displays "Landing Page" after successful auth
