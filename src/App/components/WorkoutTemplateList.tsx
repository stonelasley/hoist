import { FlatList, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { Colors } from '@/constants/theme';
import { useColorScheme } from '@/hooks/use-color-scheme';
import type { WorkoutTemplateBriefDto } from '@/services/workout-templates';

type Props = {
  templates: WorkoutTemplateBriefDto[];
  onPress: (id: number) => void;
  onCreatePress: () => void;
};

function WorkoutTemplateItem({
  item,
  onPress,
  colors,
}: {
  item: WorkoutTemplateBriefDto;
  onPress: (id: number) => void;
  colors: (typeof Colors)['light'];
}) {
  return (
    <TouchableOpacity
      style={[styles.card, { backgroundColor: colors.background, borderColor: colors.icon + '30' }]}
      onPress={() => onPress(item.id)}
      activeOpacity={0.7}
    >
      <View style={styles.cardContent}>
        <View style={styles.cardTextSection}>
          <Text style={[styles.cardTitle, { color: colors.text }]} numberOfLines={1}>
            {item.name}
          </Text>
          {item.notes ? (
            <Text style={[styles.cardSubtitle, { color: colors.icon }]} numberOfLines={1}>
              {item.notes}
            </Text>
          ) : null}
          {item.locationName ? (
            <Text style={[styles.cardDetail, { color: colors.icon }]} numberOfLines={1}>
              {item.locationName}
            </Text>
          ) : null}
        </View>
        <View style={[styles.badge, { backgroundColor: colors.tint + '20' }]}>
          <Text style={[styles.badgeText, { color: colors.tint }]}>
            {item.exerciseCount} {item.exerciseCount === 1 ? 'exercise' : 'exercises'}
          </Text>
        </View>
      </View>
    </TouchableOpacity>
  );
}

export default function WorkoutTemplateList({ templates, onPress, onCreatePress }: Props) {
  const colorScheme = useColorScheme();
  const colors = Colors[colorScheme ?? 'light'];

  if (templates.length === 0) {
    return (
      <View style={styles.emptyContainer}>
        <Text style={[styles.emptyTitle, { color: colors.text }]}>No Workouts Yet</Text>
        <Text style={[styles.emptySubtitle, { color: colors.icon }]}>
          Create your first workout template to get started.
        </Text>
        <TouchableOpacity
          style={[styles.createButton, { backgroundColor: colors.tint }]}
          onPress={onCreatePress}
        >
          <Text style={styles.createButtonText}>Create Workout</Text>
        </TouchableOpacity>
      </View>
    );
  }

  return (
    <FlatList
      data={templates}
      keyExtractor={(item) => item.id.toString()}
      renderItem={({ item }) => (
        <WorkoutTemplateItem item={item} onPress={onPress} colors={colors} />
      )}
      scrollEnabled={false}
      contentContainerStyle={styles.listContent}
    />
  );
}

const styles = StyleSheet.create({
  listContent: {
    gap: 8,
  },
  card: {
    borderWidth: 1,
    borderRadius: 10,
    padding: 14,
  },
  cardContent: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  cardTextSection: {
    flex: 1,
    marginRight: 12,
  },
  cardTitle: {
    fontSize: 16,
    fontWeight: '600',
    marginBottom: 2,
  },
  cardSubtitle: {
    fontSize: 13,
    marginBottom: 2,
  },
  cardDetail: {
    fontSize: 12,
  },
  badge: {
    paddingHorizontal: 10,
    paddingVertical: 4,
    borderRadius: 12,
  },
  badgeText: {
    fontSize: 12,
    fontWeight: '600',
  },
  emptyContainer: {
    alignItems: 'center',
    paddingVertical: 32,
    paddingHorizontal: 24,
  },
  emptyTitle: {
    fontSize: 18,
    fontWeight: '600',
    marginBottom: 8,
  },
  emptySubtitle: {
    fontSize: 14,
    textAlign: 'center',
    marginBottom: 20,
  },
  createButton: {
    paddingHorizontal: 24,
    paddingVertical: 12,
    borderRadius: 8,
  },
  createButtonText: {
    color: '#fff',
    fontSize: 15,
    fontWeight: '600',
  },
});
