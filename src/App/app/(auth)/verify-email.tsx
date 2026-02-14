import { useEffect, useState } from 'react';
import { View, Text, StyleSheet, ActivityIndicator, TouchableOpacity } from 'react-native';
import { useLocalSearchParams, router } from 'expo-router';
import * as authService from '@/services/auth';
import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';

type VerificationStatus = 'loading' | 'success' | 'already_verified' | 'error';

export default function VerifyEmailScreen() {
  const colorScheme = useColorScheme();
  const theme = Colors[colorScheme ?? 'light'];
  const params = useLocalSearchParams();

  const [status, setStatus] = useState<VerificationStatus>('loading');
  const [errorMessage, setErrorMessage] = useState('');

  useEffect(() => {
    const verifyEmail = async () => {
      const userId = params.userId as string;
      const token = params.token as string;

      if (!userId || !token) {
        setStatus('error');
        setErrorMessage('Invalid verification link. Missing required parameters.');
        return;
      }

      try {
        await authService.verifyEmail(userId, token);
        setStatus('success');
      } catch (err: any) {
        if (err.message?.toLowerCase().includes('already verified')) {
          setStatus('already_verified');
        } else {
          setStatus('error');
          setErrorMessage(
            err.message || 'Verification failed. The link may have expired.'
          );
        }
      }
    };

    verifyEmail();
  }, [params.userId, params.token]);

  const handleGoToLogin = () => {
    router.replace('/(auth)/login');
  };

  if (status === 'loading') {
    return (
      <View style={[styles.container, { backgroundColor: theme.background }]}>
        <ActivityIndicator size="large" color={theme.tint} />
        <Text style={[styles.message, { color: theme.text }]}>
          Verifying your email...
        </Text>
      </View>
    );
  }

  if (status === 'success') {
    return (
      <View style={[styles.container, { backgroundColor: theme.background }]}>
        <Text style={[styles.title, { color: theme.text }]}>✓</Text>
        <Text style={[styles.message, styles.successText]}>
          Your email has been verified!
        </Text>
        <TouchableOpacity
          style={[styles.primaryButton, { backgroundColor: theme.tint }]}
          onPress={handleGoToLogin}
        >
          <Text style={styles.buttonText}>Go to Login</Text>
        </TouchableOpacity>
      </View>
    );
  }

  if (status === 'already_verified') {
    return (
      <View style={[styles.container, { backgroundColor: theme.background }]}>
        <Text style={[styles.title, { color: theme.text }]}>✓</Text>
        <Text style={[styles.message, { color: theme.text }]}>
          Your email is already verified.
        </Text>
        <TouchableOpacity
          style={[styles.primaryButton, { backgroundColor: theme.tint }]}
          onPress={handleGoToLogin}
        >
          <Text style={styles.buttonText}>Go to Login</Text>
        </TouchableOpacity>
      </View>
    );
  }

  // Error state
  return (
    <View style={[styles.container, { backgroundColor: theme.background }]}>
      <Text style={[styles.title, styles.errorTitle]}>✕</Text>
      <Text style={[styles.message, styles.errorText]}>Verification failed</Text>
      <Text style={[styles.errorDetail, { color: theme.text }]}>
        {errorMessage}
      </Text>
      <TouchableOpacity
        style={[styles.primaryButton, { backgroundColor: theme.tint }]}
        onPress={handleGoToLogin}
      >
        <Text style={styles.buttonText}>Go to Login</Text>
      </TouchableOpacity>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    paddingHorizontal: 24,
    justifyContent: 'center',
    alignItems: 'center',
  },
  title: {
    fontSize: 64,
    fontWeight: '700',
    marginBottom: 24,
  },
  message: {
    fontSize: 20,
    fontWeight: '600',
    textAlign: 'center',
    marginBottom: 32,
  },
  successText: {
    color: '#27ae60',
  },
  errorTitle: {
    color: '#e74c3c',
  },
  errorText: {
    color: '#e74c3c',
  },
  errorDetail: {
    fontSize: 14,
    textAlign: 'center',
    marginBottom: 32,
    paddingHorizontal: 16,
  },
  primaryButton: {
    height: 48,
    borderRadius: 8,
    justifyContent: 'center',
    alignItems: 'center',
    paddingHorizontal: 32,
    minWidth: 200,
  },
  buttonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: '600',
  },
});
