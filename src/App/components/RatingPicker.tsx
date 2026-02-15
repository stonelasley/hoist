import { View, Text, StyleSheet, Pressable, useColorScheme } from 'react-native';
import { Colors } from '@/constants/theme';

type RatingPickerProps = {
  value: number | null;
  onChange: (value: number | null) => void;
};

export function RatingPicker({ value, onChange }: RatingPickerProps) {
  const colorScheme = useColorScheme();
  const theme = Colors[colorScheme ?? 'light'];

  const handlePress = (rating: number) => {
    if (value === rating) {
      onChange(null);
    } else {
      onChange(rating);
    }
  };

  return (
    <View style={styles.container}>
      {[1, 2, 3, 4, 5].map((rating) => (
        <Pressable
          key={rating}
          onPress={() => handlePress(rating)}
          style={styles.star}
        >
          <Text
            style={[
              styles.starText,
              { color: value && rating <= value ? theme.tint : theme.icon },
            ]}
          >
            {value && rating <= value ? '★' : '☆'}
          </Text>
        </Pressable>
      ))}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    gap: 8,
  },
  star: {
    padding: 4,
  },
  starText: {
    fontSize: 32,
  },
});
