import { StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';

type Props = {
  value: string | null;
  onChange: (color: string) => void;
};

const BAND_COLORS = [
  { name: 'White', hex: '#FFFFFF' },
  { name: 'Red', hex: '#EF4444' },
  { name: 'Orange', hex: '#F97316' },
  { name: 'Yellow', hex: '#EAB308' },
  { name: 'Green', hex: '#22C55E' },
  { name: 'Blue', hex: '#3B82F6' },
  { name: 'Purple', hex: '#A855F7' },
  { name: 'Grey', hex: '#6B7280' },
];

export default function BandColorPicker({ value, onChange }: Props) {
  const colorScheme = useColorScheme();
  const colors = Colors[colorScheme ?? 'light'];

  return (
    <View style={styles.container}>
      {BAND_COLORS.map((bandColor) => (
        <TouchableOpacity
          key={bandColor.name}
          style={[
            styles.colorSwatch,
            { backgroundColor: bandColor.hex },
            value === bandColor.name && [
              styles.selectedSwatch,
              { borderColor: colors.tint },
            ],
            bandColor.name === 'White' && styles.whiteSwatch,
          ]}
          onPress={() => onChange(bandColor.name)}
        >
          {value === bandColor.name && (
            <View style={[styles.checkmark, { backgroundColor: colors.tint }]}>
              <Text style={styles.checkmarkText}>✓</Text>
            </View>
          )}
        </TouchableOpacity>
      ))}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
  },
  colorSwatch: {
    width: 44,
    height: 44,
    borderRadius: 22,
    borderWidth: 2,
    borderColor: 'transparent',
    justifyContent: 'center',
    alignItems: 'center',
  },
  whiteSwatch: {
    borderColor: '#D1D5DB',
  },
  selectedSwatch: {
    borderWidth: 3,
  },
  checkmark: {
    width: 20,
    height: 20,
    borderRadius: 10,
    justifyContent: 'center',
    alignItems: 'center',
  },
  checkmarkText: {
    color: '#fff',
    fontSize: 12,
    fontWeight: '700',
  },
});
