import { useState } from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  ActivityIndicator,
  KeyboardAvoidingView,
  ScrollView,
  Platform,
} from 'react-native';
import { router } from 'expo-router';
import * as authService from '@/services/auth';
import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';

function FieldError({ errors, field }: { errors: Record<string, string[]>; field: string }) {
  // Check for field name variations (camelCase from server)
  const fieldErrors = errors[field] || errors[field.charAt(0).toUpperCase() + field.slice(1)] || [];
  if (fieldErrors.length === 0) return null;
  return <Text style={styles.fieldError}>{fieldErrors[0]}</Text>;
}

export default function RegisterScreen() {
  const colorScheme = useColorScheme();
  const theme = Colors[colorScheme ?? 'light'];

  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [age, setAge] = useState('');
  const [errors, setErrors] = useState<Record<string, string[]>>({});
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [success, setSuccess] = useState(false);

  const handleRegister = async () => {
    setErrors({});
    setError(null);
    setIsLoading(true);
    try {
      const parsedAge = age ? parseInt(age, 10) : undefined;
      await authService.register(
        firstName.trim(),
        lastName.trim(),
        email.trim(),
        password,
        parsedAge && !isNaN(parsedAge) ? parsedAge : undefined
      );
      setSuccess(true);
    } catch (err: any) {
      if (err.errors) {
        // ValidationProblem from server
        setErrors(err.errors);
      } else {
        setError(err.message || 'Registration failed. Please try again.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  if (success) {
    return (
      <View style={[styles.container, { backgroundColor: theme.background }]}>
        <Text style={[styles.title, { color: theme.text }]}>Registration Successful!</Text>
        <Text style={styles.successText}>
          Check your email for a verification link.
        </Text>
        <TouchableOpacity
          style={[styles.primaryButton, { backgroundColor: theme.tint }]}
          onPress={() => router.push('/(auth)/login')}
        >
          <Text style={styles.buttonText}>Go to Login</Text>
        </TouchableOpacity>
      </View>
    );
  }

  return (
    <KeyboardAvoidingView
      style={{ flex: 1 }}
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
      keyboardVerticalOffset={Platform.OS === 'ios' ? 0 : 20}
    >
      <ScrollView
        contentContainerStyle={{ flexGrow: 1 }}
        keyboardShouldPersistTaps="handled"
      >
        <View style={[styles.container, { backgroundColor: theme.background }]}>
          <Text style={[styles.title, { color: theme.text }]}>Create Account</Text>
          <Text style={[styles.subtitle, { color: theme.icon }]}>Join Hoist today</Text>

          <TextInput
            style={[styles.input, { color: theme.text, borderColor: theme.icon }]}
            placeholder="First Name"
            placeholderTextColor={theme.icon}
            value={firstName}
            onChangeText={setFirstName}
            autoCapitalize="words"
            editable={!isLoading}
          />
          <FieldError errors={errors} field="firstName" />

          <TextInput
            style={[styles.input, { color: theme.text, borderColor: theme.icon }]}
            placeholder="Last Name"
            placeholderTextColor={theme.icon}
            value={lastName}
            onChangeText={setLastName}
            autoCapitalize="words"
            editable={!isLoading}
          />
          <FieldError errors={errors} field="lastName" />

          <TextInput
            style={[styles.input, { color: theme.text, borderColor: theme.icon }]}
            placeholder="Email"
            placeholderTextColor={theme.icon}
            value={email}
            onChangeText={setEmail}
            keyboardType="email-address"
            autoCapitalize="none"
            autoComplete="email"
            editable={!isLoading}
          />
          <FieldError errors={errors} field="email" />

          <TextInput
            style={[styles.input, { color: theme.text, borderColor: theme.icon }]}
            placeholder="Password"
            placeholderTextColor={theme.icon}
            value={password}
            onChangeText={setPassword}
            secureTextEntry
            autoCapitalize="none"
            editable={!isLoading}
          />
          <Text style={[styles.passwordHint, { color: theme.icon }]}>
            Min 8 characters with uppercase, lowercase, digit, and special character
          </Text>
          <FieldError errors={errors} field="password" />

          <TextInput
            style={[styles.input, { color: theme.text, borderColor: theme.icon }]}
            placeholder="Age (optional)"
            placeholderTextColor={theme.icon}
            value={age}
            onChangeText={setAge}
            keyboardType="number-pad"
            editable={!isLoading}
          />
          <FieldError errors={errors} field="age" />

          {error && <Text style={styles.errorText}>{error}</Text>}

          <TouchableOpacity
            style={[
              styles.primaryButton,
              { backgroundColor: theme.tint },
              isLoading && styles.disabledButton,
            ]}
            onPress={handleRegister}
            disabled={isLoading}
          >
            {isLoading ? (
              <ActivityIndicator color="#fff" />
            ) : (
              <Text style={styles.buttonText}>Register</Text>
            )}
          </TouchableOpacity>

          <TouchableOpacity onPress={() => router.push('/(auth)/login')}>
            <Text style={[styles.link, { color: theme.tint }]}>
              Already have an account? Log in
            </Text>
          </TouchableOpacity>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
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
    marginTop: -8,
    marginBottom: 8,
    paddingHorizontal: 4,
  },
  primaryButton: {
    height: 48,
    borderRadius: 8,
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 12,
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
    marginTop: 16,
    fontSize: 14,
  },
  fieldError: {
    color: '#e74c3c',
    fontSize: 12,
    marginTop: -8,
    marginBottom: 8,
    paddingHorizontal: 4,
  },
  errorText: {
    color: '#e74c3c',
    textAlign: 'center',
    marginBottom: 12,
    fontSize: 14,
  },
  successText: {
    color: '#27ae60',
    textAlign: 'center',
    marginBottom: 32,
    fontSize: 16,
  },
});
