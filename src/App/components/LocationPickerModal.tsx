import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  ActivityIndicator,
  FlatList,
  Modal,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from 'react-native';
import { router } from 'expo-router';
import { useApi } from '@/hooks/useApi';
import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';
import { getLocations } from '@/services/locations';
import type { LocationDto } from '@/services/locations';

type Props = {
  visible: boolean;
  onClose: () => void;
  onSelect: (locationId: number | null, locationName: string | null) => void;
};

export default function LocationPickerModal({ visible, onClose, onSelect }: Props) {
  const colorScheme = useColorScheme();
  const colors = Colors[colorScheme ?? 'light'];
  const api = useApi();

  const [locations, setLocations] = useState<LocationDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [searchText, setSearchText] = useState('');

  const loadLocations = useCallback(async () => {
    setIsLoading(true);
    try {
      const data = await getLocations(api);
      setLocations(data);
    } catch (err) {
      console.error('Failed to load locations:', err);
    } finally {
      setIsLoading(false);
    }
  }, [api]);

  useEffect(() => {
    if (visible) {
      loadLocations();
      setSearchText('');
    }
  }, [visible, loadLocations]);

  const filteredLocations = useMemo(() => {
    if (!searchText.trim()) {
      return locations;
    }
    const query = searchText.trim().toLowerCase();
    return locations.filter((loc) => loc.name.toLowerCase().includes(query));
  }, [locations, searchText]);

  const handleSelectNone = () => {
    onSelect(null, null);
    onClose();
  };

  const handleSelectLocation = (location: LocationDto) => {
    onSelect(location.id, location.name);
    onClose();
  };

  const handleCreateNew = () => {
    onClose();
    router.push('/(app)/settings/locations/create');
  };

  return (
    <Modal
      visible={visible}
      animationType="slide"
      presentationStyle="pageSheet"
      onRequestClose={onClose}
    >
      <View style={[styles.container, { backgroundColor: colors.background }]}>
        <View style={styles.header}>
          <TouchableOpacity onPress={onClose}>
            <Text style={[styles.headerAction, { color: colors.tint }]}>Cancel</Text>
          </TouchableOpacity>
          <Text style={[styles.headerTitle, { color: colors.text }]}>Select Location</Text>
          <View style={styles.headerSpacer} />
        </View>

        <View style={styles.searchSection}>
          <TextInput
            style={[styles.searchInput, { color: colors.text, borderColor: colors.icon + '40' }]}
            placeholder="Search locations..."
            placeholderTextColor={colors.icon}
            value={searchText}
            onChangeText={setSearchText}
            autoCapitalize="none"
            autoCorrect={false}
          />
        </View>

        {isLoading ? (
          <View style={styles.loadingContainer}>
            <ActivityIndicator size="large" color={colors.tint} />
          </View>
        ) : (
          <FlatList
            data={filteredLocations}
            keyExtractor={(item) => item.id.toString()}
            ListHeaderComponent={
              <TouchableOpacity
                style={[styles.locationRow, { borderColor: colors.icon + '20' }]}
                onPress={handleSelectNone}
                activeOpacity={0.7}
              >
                <Text style={[styles.locationName, { color: colors.icon }]}>None</Text>
              </TouchableOpacity>
            }
            renderItem={({ item }) => (
              <TouchableOpacity
                style={[styles.locationRow, { borderColor: colors.icon + '20' }]}
                onPress={() => handleSelectLocation(item)}
                activeOpacity={0.7}
              >
                <View style={styles.locationInfo}>
                  <Text style={[styles.locationName, { color: colors.text }]} numberOfLines={1}>
                    {item.name}
                  </Text>
                  {item.address ? (
                    <Text style={[styles.locationDetail, { color: colors.icon }]} numberOfLines={1}>
                      {item.address}
                    </Text>
                  ) : null}
                </View>
              </TouchableOpacity>
            )}
            contentContainerStyle={styles.listContent}
            ListEmptyComponent={
              <View style={styles.emptyContainer}>
                <Text style={[styles.emptyText, { color: colors.icon }]}>
                  {searchText ? 'No locations match your search.' : 'No locations available.'}
                </Text>
              </View>
            }
          />
        )}

        <TouchableOpacity
          style={[styles.createNewButton, { backgroundColor: colors.tint }]}
          onPress={handleCreateNew}
        >
          <Text style={styles.createNewButtonText}>Create New Location</Text>
        </TouchableOpacity>
      </View>
    </Modal>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: 16,
    paddingTop: 16,
    paddingBottom: 12,
  },
  headerAction: {
    fontSize: 16,
    fontWeight: '500',
  },
  headerTitle: {
    fontSize: 17,
    fontWeight: '600',
  },
  headerSpacer: {
    width: 50,
  },
  searchSection: {
    paddingHorizontal: 16,
    marginBottom: 8,
  },
  searchInput: {
    height: 44,
    borderWidth: 1,
    borderRadius: 8,
    paddingHorizontal: 14,
    fontSize: 15,
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  listContent: {
    paddingHorizontal: 16,
  },
  locationRow: {
    paddingVertical: 14,
    borderBottomWidth: 1,
  },
  locationInfo: {
    gap: 2,
  },
  locationName: {
    fontSize: 16,
    fontWeight: '500',
  },
  locationDetail: {
    fontSize: 13,
  },
  emptyContainer: {
    alignItems: 'center',
    paddingVertical: 40,
  },
  emptyText: {
    fontSize: 14,
  },
  createNewButton: {
    height: 48,
    marginHorizontal: 16,
    marginBottom: 32,
    marginTop: 8,
    borderRadius: 8,
    justifyContent: 'center',
    alignItems: 'center',
  },
  createNewButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: '600',
  },
});
