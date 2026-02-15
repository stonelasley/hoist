import { useState, useEffect } from 'react';
import { StyleSheet, TextInput } from 'react-native';
import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';

type Props = {
  value: number | null;
  onChange: (seconds: number | null) => void;
  placeholder?: string;
};

export default function DurationPicker({ value, onChange, placeholder = 'mm:ss' }: Props) {
  const colorScheme = useColorScheme();
  const colors = Colors[colorScheme ?? 'light'];

  const formatDuration = (seconds: number | null): string => {
    if (seconds === null || seconds === 0) return '';
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  const parseDuration = (text: string): number | null => {
    const cleaned = text.replace(/[^\d:]/g, '');
    if (!cleaned) return null;

    const parts = cleaned.split(':');
    if (parts.length === 1) {
      const secs = parseInt(parts[0], 10);
      return isNaN(secs) ? null : secs;
    }

    const mins = parseInt(parts[0], 10) || 0;
    const secs = parseInt(parts[1], 10) || 0;
    return mins * 60 + secs;
  };

  const [text, setText] = useState(formatDuration(value));

  useEffect(() => {
    setText(formatDuration(value));
  }, [value]);

  const handleChangeText = (newText: string) => {
    setText(newText);
  };

  const handleBlur = () => {
    const parsed = parseDuration(text);
    onChange(parsed);
    setText(formatDuration(parsed));
  };

  return (
    <TextInput
      style={[styles.input, { color: colors.text, borderColor: colors.icon + '40' }]}
      placeholder={placeholder}
      placeholderTextColor={colors.icon}
      value={text}
      onChangeText={handleChangeText}
      onBlur={handleBlur}
      keyboardType="numbers-and-punctuation"
    />
  );
}

const styles = StyleSheet.create({
  input: {
    height: 40,
    borderWidth: 1,
    borderRadius: 6,
    paddingHorizontal: 12,
    fontSize: 15,
    minWidth: 80,
  },
});
