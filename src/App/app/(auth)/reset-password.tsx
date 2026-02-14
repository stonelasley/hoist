import { useState } from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
} from 'react-native';
import { useLocalSearchParams, router } from 'expo-router';
import * as authService from '@/services/auth';
import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';

export default function ResetPasswordScreen() {
  const colorScheme = useColorScheme();
  const theme = Colors[colorScheme ?? 'light'];
  const params = useLocalSearchParams();

  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [success, setSuccess] = useState(false);

  const handleResetPassword = async () => {
    if (newPassword !== confirmPassword) {
      setError('Passwords do not match.');
      return;
    }

    const email = params.email as string;
    const token = params.token as string;

    if (!email || !token) {
      setError('Invalid reset link. Missing required parameters.');
      return;
    }

    setError(null);
    setIsLoading(true);
    try {
      await authService.resetPassword(email, token, newPassword);
      setSuccess(true);
    } catch (err: any) {
      if (err.errors) {
        const allErrors = Object.values(err.errors).flat();
        setError((allErrors as string[])[0] || 'Reset failed.');
      } else {
        setError(err.message || 'Reset failed. The link may have expired.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  if (success) {
    return (
      <View style={[styles.container, { backgroundColor: theme.background }]}>
        <Text style={[styles.title, { color: theme.text }]}>Password Reset!</Text>
        <Text style={styles.successText}>Password reset successfully!</Text>
        <TouchableOpacity
          style={[styles.primaryButton, { backgroundColor: theme.tint }]}
          onPress={() => router.replace('/(auth)/login')}
        >
          <Text style={styles.buttonText}>Go to Login</Text>
        </TouchableOpacity>
      </View>
    );
  }

  return (
    <View style={[styles.container, { backgroundColor: theme.background }]}>
      <Text style={[styles.title, { color: theme.text }]}>Reset Password</Text>
      <Text style={[styles.subtitle, { color: theme.icon }]}>
        Enter your new password
      </Text>

      <TextInput
        style={[styles.input, { color: theme.text, borderColor: theme.icon }]}
        placeholder="New Password"
        placeholderTextColor={theme.icon}
        value={newPassword}
        onChangeText={setNewPassword}
        secureTextEntry
        autoCapitalize="none"
        editable={!isLoading}
      />

      <TextInput
        style={[styles.input, { color: theme.text, borderColor: theme.icon }]}
        placeholder="Confirm Password"
        placeholderTextColor={theme.icon}
        value={confirmPassword}
        onChangeText={setConfirmPassword}
        secureTextEntry
        autoCapitalize="none"
        editable={!isLoading}
      />

      <Text style={[styles.passwordHint, { color: theme.icon }]}>
        Min 8 characters with uppercase, lowercase, digit, and special character
      </Text>

      {error && <Text style={styles.errorText}>{error}</Text>}

      <TouchableOpacity
        style={[
          styles.primaryButton,
          { backgroundColor: theme.tint },
          isLoading && styles.disabledButton,
        ]}
        onPress={handleResetPassword}
        disabled={isLoading}
      >
        {isLoading ? (
          <ActivityIndicator color="#fff" />
        ) : (
          <Text style={styles.buttonText}>Reset Password</Text>
        )}
      </TouchableOpacity>

      <TouchableOpacity onPress={() => router.push('/(auth)/login')}>
        <Text style={[styles.link, { color: theme.tint }]}>Back to Login</Text>
      </TouchableOpacity>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    paddingHorizontal: 24,
    justifyContent: 'center',
  },
  title: {
    fontSize: 32,
    fontWeight: '700',
    marginBottom: 8,
  },
  subtitle: {
    fontSize: 16,
    marginBottom: 32,
  },
  input: {
    height: 48,
    borderWidth: 1,
    borderRadius: 8,
    paddingHorizontal: 16,
    fontSize: 16,
    marginBottom: 12,
  },
  passwordHint: {
    fontSize: 12,
    marginBottom: 16,
    paddingHorizontal: 4,
  },
  primaryButton: {
    height: 48,
    borderRadius: 8,
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 16,
  },
  disabledButton: {
    opacity: 0.6,
  },
  buttonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: '600',
  },
  link: {
    textAlign: 'center',
    fontSize: 14,
  },
  errorText: {
    color: '#e74c3c',
    textAlign: 'center',
    marginBottom: 16,
    fontSize: 14,
  },
  successText: {
    color: '#27ae60',
    textAlign: 'center',
    marginBottom: 32,
    fontSize: 16,
  },
});
