import { useState, useEffect } from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  KeyboardAvoidingView,
  Platform,
  ActivityIndicator,
  ScrollView,
} from 'react-native';
import { Link, router } from 'expo-router';
import * as AuthSession from 'expo-auth-session';
import * as WebBrowser from 'expo-web-browser';
import { useAuth } from '@/hooks/useAuth';
import * as authService from '@/services/auth';
import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';
import { GOOGLE_CLIENT_ID } from '@/constants/api';

WebBrowser.maybeCompleteAuthSession();

export default function LoginScreen() {
  const colorScheme = useColorScheme();
  const colors = Colors[colorScheme ?? 'light'];
  const { setTokens } = useAuth();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [showResendVerification, setShowResendVerification] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [resendSuccess, setResendSuccess] = useState(false);

  const discovery = AuthSession.useAutoDiscovery('https://accounts.google.com');

  const redirectUri = AuthSession.makeRedirectUri();

  const [googleRequest, googleResponse, googlePromptAsync] = AuthSession.useAuthRequest(
    {
      clientId: GOOGLE_CLIENT_ID,
      redirectUri,
      scopes: ['openid', 'profile', 'email'],
      responseType: AuthSession.ResponseType.IdToken,
    },
    discovery
  );

  useEffect(() => {
    if (googleResponse?.type === 'success') {
      const { id_token } = googleResponse.params;
      handleGoogleLogin(id_token);
    }
  }, [googleResponse]);

  const handleLogin = async () => {
    setError(null);
    setShowResendVerification(false);
    setResendSuccess(false);
    setIsLoading(true);

    try {
      const response = await authService.login(email.trim(), password);
      await setTokens(response.accessToken, response.refreshToken);
      router.replace('/(app)');
    } catch (err: any) {
      if (err.message === 'email_not_verified' || err.detail === 'email_not_verified') {
        setError('Please verify your email before logging in.');
        setShowResendVerification(true);
      } else {
        setError('Invalid email or password.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleResendVerification = async () => {
    try {
      await authService.resendVerification(email.trim());
      setResendSuccess(true);
    } catch {
      // Silently fail — already showing success to prevent enumeration
      setResendSuccess(true);
    }
  };

  const handleGoogleLogin = async (idToken: string) => {
    setError(null);
    setShowResendVerification(false);
    setResendSuccess(false);
    setIsLoading(true);

    try {
      const response = await authService.googleLogin(idToken);
      await setTokens(response.accessToken, response.refreshToken);
      router.replace('/(app)');
    } catch {
      setError('Google sign-in failed. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <KeyboardAvoidingView
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
      style={[styles.container, { backgroundColor: colors.background }]}
    >
      <ScrollView
        contentContainerStyle={styles.scrollContent}
        keyboardShouldPersistTaps="handled"
      >
        <View style={styles.content}>
          <Text style={[styles.title, { color: colors.text }]}>Hoist</Text>
          <Text style={[styles.subtitle, { color: colors.icon }]}>Welcome back</Text>

          <View style={styles.spacer} />

          <TextInput
            style={[
              styles.input,
              { color: colors.text, borderColor: colors.icon + '40' },
            ]}
            placeholder="Email"
            placeholderTextColor={colors.icon}
            value={email}
            onChangeText={setEmail}
            keyboardType="email-address"
            autoCapitalize="none"
            autoComplete="email"
            editable={!isLoading}
          />

          <TextInput
            style={[
              styles.input,
              { color: colors.text, borderColor: colors.icon + '40' },
            ]}
            placeholder="Password"
            placeholderTextColor={colors.icon}
            value={password}
            onChangeText={setPassword}
            secureTextEntry
            autoComplete="password"
            editable={!isLoading}
          />

          {error && <Text style={styles.errorText}>{error}</Text>}

          {showResendVerification && (
            <TouchableOpacity
              onPress={handleResendVerification}
              disabled={isLoading}
              style={styles.resendButton}
            >
              <Text style={[styles.resendText, { color: colors.tint }]}>
                Resend verification email
              </Text>
            </TouchableOpacity>
          )}

          {resendSuccess && (
            <Text style={styles.successText}>Verification email sent!</Text>
          )}

          <TouchableOpacity
            style={[styles.primaryButton, { backgroundColor: colors.tint }]}
            onPress={handleLogin}
            disabled={isLoading}
          >
            {isLoading ? (
              <ActivityIndicator color="#fff" />
            ) : (
              <Text style={styles.primaryButtonText}>Log In</Text>
            )}
          </TouchableOpacity>

          <TouchableOpacity
            style={[
              styles.secondaryButton,
              { borderColor: colors.icon + '60' },
            ]}
            onPress={() => googlePromptAsync()}
            disabled={isLoading || !googleRequest}
          >
            <Text style={[styles.secondaryButtonText, { color: colors.text }]}>
              Sign in with Google
            </Text>
          </TouchableOpacity>

          <View style={styles.spacer} />

          <Link href="/(auth)/forgot-password" asChild>
            <TouchableOpacity disabled={isLoading}>
              <Text style={[styles.linkText, { color: colors.tint }]}>
                Forgot password?
              </Text>
            </TouchableOpacity>
          </Link>

          <Link href="/(auth)/register" asChild>
            <TouchableOpacity disabled={isLoading} style={styles.registerLink}>
              <Text style={[styles.linkText, { color: colors.icon }]}>
                Don't have an account?{' '}
                <Text style={{ color: colors.tint }}>Register</Text>
              </Text>
            </TouchableOpacity>
          </Link>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  scrollContent: {
    flexGrow: 1,
  },
  content: {
    flex: 1,
    justifyContent: 'center',
    paddingHorizontal: 24,
    paddingVertical: 32,
  },
  title: {
    fontSize: 48,
    fontWeight: 'bold',
    textAlign: 'center',
    marginBottom: 8,
  },
  subtitle: {
    fontSize: 18,
    textAlign: 'center',
    marginBottom: 24,
  },
  spacer: {
    height: 24,
  },
  input: {
    height: 48,
    borderWidth: 1,
    borderRadius: 8,
    paddingHorizontal: 16,
    fontSize: 16,
    marginBottom: 12,
  },
  errorText: {
    color: '#ef4444',
    fontSize: 14,
    marginBottom: 12,
  },
  successText: {
    color: '#22c55e',
    fontSize: 14,
    marginBottom: 12,
  },
  resendButton: {
    marginBottom: 12,
  },
  resendText: {
    fontSize: 14,
    textDecorationLine: 'underline',
  },
  primaryButton: {
    height: 48,
    borderRadius: 8,
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 12,
  },
  primaryButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: '600',
  },
  secondaryButton: {
    height: 48,
    borderRadius: 8,
    borderWidth: 1,
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 12,
  },
  secondaryButtonText: {
    fontSize: 16,
    fontWeight: '600',
  },
  linkText: {
    fontSize: 14,
    textAlign: 'center',
  },
  registerLink: {
    marginTop: 12,
  },
});
