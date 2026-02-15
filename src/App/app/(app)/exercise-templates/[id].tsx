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
import { Image } from 'expo-image';
import * as ImagePicker from 'expo-image-picker';
import { router, useFocusEffect, useLocalSearchParams } from 'expo-router';
import { useApi } from '@/hooks/useApi';
import { useAuth } from '@/hooks/useAuth';
import { API_BASE_URL } from '@/constants/api';
import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';
import {
  getExerciseTemplate,
  updateExerciseTemplate,
  deleteExerciseTemplate,
  uploadExerciseTemplateImage,
  deleteExerciseTemplateImage,
  IMPLEMENT_TYPES,
  EXERCISE_TYPES,
} from '@/services/exercise-templates';
import LocationPickerModal from '@/components/LocationPickerModal';

export default function ExerciseTemplateDetailScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const colorScheme = useColorScheme();
  const colors = Colors[colorScheme ?? 'light'];
  const api = useApi();
  const { accessToken } = useAuth();

  const [name, setName] = useState('');
  const [implementTypeIndex, setImplementTypeIndex] = useState<number>(0);
  const [exerciseTypeIndex, setExerciseTypeIndex] = useState<number>(0);
  const [model, setModel] = useState('');
  const [locationId, setLocationId] = useState<number | null>(null);
  const [locationName, setLocationName] = useState<string | null>(null);
  const [showLocationPicker, setShowLocationPicker] = useState(false);
  const [imagePath, setImagePath] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [isUploadingImage, setIsUploadingImage] = useState(false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const loadTemplate = useCallback(async () => {
    if (!id) return;
    try {
      setIsLoading(true);
      const template = await getExerciseTemplate(api, parseInt(id, 10));
      setName(template.name);
      setModel(template.model ?? '');
      setImagePath(template.imagePath);
      setLocationId(template.locationId ?? null);
      setLocationName(template.locationName ?? null);

      const implIdx = IMPLEMENT_TYPES.indexOf(
        template.implementType as (typeof IMPLEMENT_TYPES)[number],
      );
      setImplementTypeIndex(implIdx >= 0 ? implIdx : 0);

      const exIdx = EXERCISE_TYPES.indexOf(
        template.exerciseType as (typeof EXERCISE_TYPES)[number],
      );
      setExerciseTypeIndex(exIdx >= 0 ? exIdx : 0);
    } catch (err) {
      console.error('Failed to load exercise template:', err);
      Alert.alert('Error', 'Failed to load exercise template.');
    } finally {
      setIsLoading(false);
    }
  }, [api, id]);

  useFocusEffect(
    useCallback(() => {
      loadTemplate();
    }, [loadTemplate]),
  );

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};
    if (!name.trim()) {
      newErrors.name = 'Name is required.';
    }
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSave = async () => {
    if (!validate()) return;

    setIsSaving(true);
    try {
      const templateId = parseInt(id!, 10);
      await updateExerciseTemplate(api, templateId, {
        id: templateId,
        name: name.trim(),
        implementType: implementTypeIndex,
        exerciseType: exerciseTypeIndex,
        model: model.trim() || undefined,
        locationId: locationId ?? undefined,
      });
      router.back();
    } catch (err: unknown) {
      const message =
        err instanceof Error ? err.message : 'Failed to save exercise template.';
      Alert.alert('Error', message);
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = () => {
    Alert.alert(
      'Delete Exercise',
      'Are you sure you want to delete this exercise template?',
      [
        { text: 'Cancel', style: 'cancel' },
        {
          text: 'Delete',
          style: 'destructive',
          onPress: async () => {
            try {
              await deleteExerciseTemplate(api, parseInt(id!, 10));
              router.back();
            } catch (err: unknown) {
              const message =
                err instanceof Error ? err.message : 'Failed to delete exercise template.';
              Alert.alert('Error', message);
            }
          },
        },
      ],
    );
  };

  const handlePickImage = async () => {
    const permissionResult = await ImagePicker.requestMediaLibraryPermissionsAsync();
    if (!permissionResult.granted) {
      Alert.alert('Permission Required', 'Camera roll access is needed to upload images.');
      return;
    }

    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ['images'],
      allowsEditing: true,
      aspect: [1, 1],
      quality: 0.8,
    });

    if (result.canceled || !result.assets[0]) return;

    const asset = result.assets[0];
    const uri = asset.uri;
    const fileName = asset.fileName ?? 'image.jpg';
    const mimeType = asset.mimeType ?? 'image/jpeg';

    setIsUploadingImage(true);
    try {
      await uploadExerciseTemplateImage(
        API_BASE_URL,
        accessToken,
        parseInt(id!, 10),
        uri,
        fileName,
        mimeType,
      );
      // Reload to get the new image path
      await loadTemplate();
    } catch (err: unknown) {
      const message =
        err instanceof Error ? err.message : 'Failed to upload image.';
      Alert.alert('Error', message);
    } finally {
      setIsUploadingImage(false);
    }
  };

  const handleDeleteImage = () => {
    Alert.alert('Remove Image', 'Are you sure you want to remove this image?', [
      { text: 'Cancel', style: 'cancel' },
      {
        text: 'Remove',
        style: 'destructive',
        onPress: async () => {
          try {
            await deleteExerciseTemplateImage(api, parseInt(id!, 10));
            setImagePath(null);
          } catch (err: unknown) {
            const message =
              err instanceof Error ? err.message : 'Failed to remove image.';
            Alert.alert('Error', message);
          }
        },
      },
    ]);
  };

  const renderSelector = (
    label: string,
    options: readonly string[],
    selectedIndex: number,
    onSelect: (index: number) => void,
  ) => (
    <View style={styles.selectorSection}>
      <Text style={[styles.label, { color: colors.text }]}>{label} *</Text>
      <View style={styles.optionsGrid}>
        {options.map((option, index) => {
          const isSelected = selectedIndex === index;
          return (
            <TouchableOpacity
              key={option}
              style={[
                styles.optionChip,
                isSelected
                  ? { backgroundColor: colors.tint, borderColor: colors.tint }
                  : { backgroundColor: colors.icon + '10', borderColor: colors.icon + '30' },
              ]}
              onPress={() => onSelect(index)}
              disabled={isSaving}
            >
              <Text
                style={[
                  styles.optionChipText,
                  { color: isSelected ? '#fff' : colors.text },
                ]}
              >
                {option}
              </Text>
            </TouchableOpacity>
          );
        })}
      </View>
    </View>
  );

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
        <Text style={[styles.title, { color: colors.text }]}>Edit Exercise</Text>

        {/* Image Section */}
        <View style={styles.imageSection}>
          {imagePath ? (
            <View style={styles.imageWrapper}>
              <Image
                source={{ uri: `${API_BASE_URL}${imagePath}` }}
                style={styles.image}
                contentFit="cover"
              />
              <View style={styles.imageActions}>
                <TouchableOpacity
                  style={[styles.imageActionButton, { backgroundColor: colors.tint }]}
                  onPress={handlePickImage}
                  disabled={isUploadingImage}
                >
                  <Text style={styles.imageActionText}>Change</Text>
                </TouchableOpacity>
                <TouchableOpacity
                  style={[styles.imageActionButton, { backgroundColor: '#ef4444' }]}
                  onPress={handleDeleteImage}
                  disabled={isUploadingImage}
                >
                  <Text style={styles.imageActionText}>Remove</Text>
                </TouchableOpacity>
              </View>
            </View>
          ) : (
            <TouchableOpacity
              style={[styles.uploadPlaceholder, { borderColor: colors.icon + '40' }]}
              onPress={handlePickImage}
              disabled={isUploadingImage}
            >
              {isUploadingImage ? (
                <ActivityIndicator color={colors.tint} />
              ) : (
                <>
                  <Text style={[styles.uploadIcon, { color: colors.icon }]}>+</Text>
                  <Text style={[styles.uploadText, { color: colors.icon }]}>Add Image</Text>
                </>
              )}
            </TouchableOpacity>
          )}
        </View>

        <Text style={[styles.label, { color: colors.text }]}>Name *</Text>
        <TextInput
          style={[styles.input, { color: colors.text, borderColor: colors.icon + '40' }]}
          placeholder="Exercise name"
          placeholderTextColor={colors.icon}
          value={name}
          onChangeText={setName}
          autoCapitalize="words"
          editable={!isSaving}
        />
        {errors.name && <Text style={styles.errorText}>{errors.name}</Text>}

        {renderSelector(
          'Equipment Type',
          IMPLEMENT_TYPES,
          implementTypeIndex,
          setImplementTypeIndex,
        )}

        {renderSelector(
          'Exercise Type',
          EXERCISE_TYPES,
          exerciseTypeIndex,
          setExerciseTypeIndex,
        )}

        <Text style={[styles.label, { color: colors.text }]}>Model</Text>
        <TextInput
          style={[styles.input, { color: colors.text, borderColor: colors.icon + '40' }]}
          placeholder="Optional model or variation"
          placeholderTextColor={colors.icon}
          value={model}
          onChangeText={setModel}
          autoCapitalize="words"
          editable={!isSaving}
        />

        <Text style={[styles.label, { color: colors.text }]}>Location</Text>
        <TouchableOpacity
          style={[styles.input, styles.pickerField, { borderColor: colors.icon + '40' }]}
          onPress={() => setShowLocationPicker(true)}
          disabled={isSaving}
        >
          <Text style={[styles.pickerFieldText, { color: locationName ? colors.text : colors.icon }]}>
            {locationName ?? 'Select a location (optional)'}
          </Text>
        </TouchableOpacity>

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
          <Text style={styles.deleteButtonText}>Delete Exercise</Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={styles.cancelButton}
          onPress={() => router.back()}
          disabled={isSaving}
        >
          <Text style={[styles.cancelButtonText, { color: colors.icon }]}>Cancel</Text>
        </TouchableOpacity>
      </ScrollView>

      <LocationPickerModal
        visible={showLocationPicker}
        onClose={() => setShowLocationPicker(false)}
        onSelect={(id, name) => {
          setLocationId(id);
          setLocationName(name);
          setShowLocationPicker(false);
        }}
      />
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
  imageSection: {
    marginBottom: 20,
    alignItems: 'center',
  },
  imageWrapper: {
    alignItems: 'center',
  },
  image: {
    width: 160,
    height: 160,
    borderRadius: 12,
    marginBottom: 10,
  },
  imageActions: {
    flexDirection: 'row',
    gap: 10,
  },
  imageActionButton: {
    paddingHorizontal: 16,
    paddingVertical: 8,
    borderRadius: 6,
  },
  imageActionText: {
    color: '#fff',
    fontSize: 14,
    fontWeight: '500',
  },
  uploadPlaceholder: {
    width: 160,
    height: 160,
    borderRadius: 12,
    borderWidth: 2,
    borderStyle: 'dashed',
    justifyContent: 'center',
    alignItems: 'center',
  },
  uploadIcon: {
    fontSize: 32,
    fontWeight: '300',
    marginBottom: 4,
  },
  uploadText: {
    fontSize: 14,
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
  selectorSection: {
    marginBottom: 16,
  },
  optionsGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
  },
  optionChip: {
    paddingHorizontal: 14,
    paddingVertical: 10,
    borderRadius: 8,
    borderWidth: 1,
  },
  optionChipText: {
    fontSize: 14,
    fontWeight: '500',
  },
  errorText: {
    color: '#ef4444',
    fontSize: 13,
    marginTop: -12,
    marginBottom: 8,
  },
  saveButton: {
    height: 48,
    borderRadius: 8,
    justifyContent: 'center',
    alignItems: 'center',
    marginTop: 8,
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
  pickerField: {
    justifyContent: 'center',
  },
  pickerFieldText: {
    fontSize: 16,
  },
});
