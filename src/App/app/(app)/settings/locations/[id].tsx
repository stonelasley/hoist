import { useCallback, useState } from 'react';
import {
  ActivityIndicator,
  Alert,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from 'react-native';
import { router, useFocusEffect, useLocalSearchParams } from 'expo-router';
import { useApi } from '@/hooks/useApi';
import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';
import { getLocation, updateLocation, deleteLocation } from '@/services/locations';

export default function EditLocationScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const colorScheme = useColorScheme();
  const colors = Colors[colorScheme ?? 'light'];
  const api = useApi();

  const [name, setName] = useState('');
  const [instagramHandle, setInstagramHandle] = useState('');
  const [latitude, setLatitude] = useState('');
  const [longitude, setLongitude] = useState('');
  const [notes, setNotes] = useState('');
  const [address, setAddress] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const loadLocation = useCallback(async () => {
    if (!id) return;
    try {
      setIsLoading(true);
      const location = await getLocation(api, parseInt(id, 10));
      setName(location.name);
      setInstagramHandle(location.instagramHandle ?? '');
      setLatitude(location.latitude !== null ? location.latitude.toString() : '');
      setLongitude(location.longitude !== null ? location.longitude.toString() : '');
      setNotes(location.notes ?? '');
      setAddress(location.address ?? '');
    } catch (err) {
      console.error('Failed to load location:', err);
      Alert.alert('Error', 'Failed to load location.');
    } finally {
      setIsLoading(false);
    }
  }, [api, id]);

  useFocusEffect(
    useCallback(() => {
      loadLocation();
    }, [loadLocation]),
  );

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    const trimmedName = name.trim();
    if (!trimmedName) {
      newErrors.name = 'Name is required.';
    }

    const trimmedLat = latitude.trim();
    if (trimmedLat) {
      const latValue = parseFloat(trimmedLat);
      if (isNaN(latValue) || latValue < -90 || latValue > 90) {
        newErrors.latitude = 'Latitude must be between -90 and 90.';
      }
    }

    const trimmedLon = longitude.trim();
    if (trimmedLon) {
      const lonValue = parseFloat(trimmedLon);
      if (isNaN(lonValue) || lonValue < -180 || lonValue > 180) {
        newErrors.longitude = 'Longitude must be between -180 and 180.';
      }
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSave = async () => {
    if (!validate()) return;

    setIsSaving(true);
    try {
      const locationId = parseInt(id!, 10);
      const trimmedHandle = instagramHandle.trim().replace(/^@/, '');
      const latValue = latitude.trim() ? parseFloat(latitude.trim()) : undefined;
      const lonValue = longitude.trim() ? parseFloat(longitude.trim()) : undefined;

      await updateLocation(api, locationId, {
        id: locationId,
        name: name.trim(),
        instagramHandle: trimmedHandle || undefined,
        latitude: latValue,
        longitude: lonValue,
        notes: notes.trim() || undefined,
        address: address.trim() || undefined,
      });
      router.back();
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Failed to save location.';
      Alert.alert('Error', message);
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = () => {
    Alert.alert(
      'Delete Location',
      'Are you sure you want to delete this location? This cannot be undone.',
      [
        { text: 'Cancel', style: 'cancel' },
        {
          text: 'Delete',
          style: 'destructive',
          onPress: async () => {
            try {
              await deleteLocation(api, parseInt(id!, 10));
              router.back();
            } catch (err: unknown) {
              const message =
                err instanceof Error ? err.message : 'Failed to delete location.';
              Alert.alert('Error', message);
            }
          },
        },
      ],
    );
  };

  if (isLoading) {
    return (
      <View style={[styles.loadingContainer, { backgroundColor: colors.background }]}>
        <ActivityIndicator size="large" color={colors.tint} />
      </View>
    );
  }

  return (
    <KeyboardAvoidingView
      style={[styles.container, { backgroundColor: colors.background }]}
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
    >
      <ScrollView
        contentContainerStyle={styles.scrollContent}
        keyboardShouldPersistTaps="handled"
      >
        <Text style={[styles.title, { color: colors.text }]}>Edit Location</Text>

        <Text style={[styles.label, { color: colors.text }]}>Name *</Text>
        <TextInput
          style={[styles.input, { color: colors.text, borderColor: colors.icon + '40' }]}
          placeholder="Location name"
          placeholderTextColor={colors.icon}
          value={name}
          onChangeText={setName}
          autoCapitalize="words"
          editable={!isSaving}
        />
        {errors.name && <Text style={styles.errorText}>{errors.name}</Text>}

        <Text style={[styles.label, { color: colors.text }]}>Instagram Handle</Text>
        <TextInput
          style={[styles.input, { color: colors.text, borderColor: colors.icon + '40' }]}
          placeholder="e.g. @mygym or mygym"
          placeholderTextColor={colors.icon}
          value={instagramHandle}
          onChangeText={setInstagramHandle}
          autoCapitalize="none"
          editable={!isSaving}
        />

        <Text style={[styles.label, { color: colors.text }]}>Latitude</Text>
        <TextInput
          style={[styles.input, { color: colors.text, borderColor: colors.icon + '40' }]}
          placeholder="e.g. 40.7128"
          placeholderTextColor={colors.icon}
          value={latitude}
          onChangeText={setLatitude}
          keyboardType="decimal-pad"
          editable={!isSaving}
        />
        {errors.latitude && <Text style={styles.errorText}>{errors.latitude}</Text>}

        <Text style={[styles.label, { color: colors.text }]}>Longitude</Text>
        <TextInput
          style={[styles.input, { color: colors.text, borderColor: colors.icon + '40' }]}
          placeholder="e.g. -74.0060"
          placeholderTextColor={colors.icon}
          value={longitude}
          onChangeText={setLongitude}
          keyboardType="decimal-pad"
          editable={!isSaving}
        />
        {errors.longitude && <Text style={styles.errorText}>{errors.longitude}</Text>}

        <Text style={[styles.label, { color: colors.text }]}>Address</Text>
        <TextInput
          style={[styles.input, { color: colors.text, borderColor: colors.icon + '40' }]}
          placeholder="e.g. 123 Main St, New York, NY 10001"
          placeholderTextColor={colors.icon}
          value={address}
          onChangeText={setAddress}
          autoCapitalize="words"
          editable={!isSaving}
        />

        <Text style={[styles.label, { color: colors.text }]}>Notes</Text>
        <TextInput
          style={[
            styles.input,
            styles.textArea,
            { color: colors.text, borderColor: colors.icon + '40' },
          ]}
          placeholder="Optional notes"
          placeholderTextColor={colors.icon}
          value={notes}
          onChangeText={setNotes}
          multiline
          numberOfLines={3}
          textAlignVertical="top"
          editable={!isSaving}
        />

        <TouchableOpacity
          style={[
            styles.saveButton,
            { backgroundColor: colors.tint },
            isSaving && styles.disabledButton,
          ]}
          onPress={handleSave}
          disabled={isSaving}
        >
          {isSaving ? (
            <ActivityIndicator color="#fff" />
          ) : (
            <Text style={styles.saveButtonText}>Save Changes</Text>
          )}
        </TouchableOpacity>

        <TouchableOpacity style={styles.deleteButton} onPress={handleDelete} disabled={isSaving}>
          <Text style={styles.deleteButtonText}>Delete Location</Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={styles.cancelButton}
          onPress={() => router.back()}
          disabled={isSaving}
        >
          <Text style={[styles.cancelButtonText, { color: colors.icon }]}>Cancel</Text>
        </TouchableOpacity>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  scrollContent: {
    paddingHorizontal: 24,
    paddingTop: 60,
    paddingBottom: 32,
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  title: {
    fontSize: 28,
    fontWeight: '700',
    marginBottom: 24,
  },
  label: {
    fontSize: 14,
    fontWeight: '600',
    marginBottom: 6,
  },
  input: {
    height: 48,
    borderWidth: 1,
    borderRadius: 8,
    paddingHorizontal: 16,
    fontSize: 16,
    marginBottom: 16,
  },
  textArea: {
    height: 100,
    paddingTop: 12,
  },
  errorText: {
    color: '#ef4444',
    fontSize: 13,
    marginTop: -12,
    marginBottom: 12,
  },
  saveButton: {
    height: 48,
    borderRadius: 8,
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 12,
  },
  disabledButton: {
    opacity: 0.6,
  },
  saveButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: '600',
  },
  deleteButton: {
    height: 48,
    borderRadius: 8,
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 12,
    borderWidth: 1,
    borderColor: '#ef4444',
  },
  deleteButtonText: {
    color: '#ef4444',
    fontSize: 16,
    fontWeight: '600',
  },
  cancelButton: {
    height: 48,
    justifyContent: 'center',
    alignItems: 'center',
  },
  cancelButtonText: {
    fontSize: 16,
  },
});
