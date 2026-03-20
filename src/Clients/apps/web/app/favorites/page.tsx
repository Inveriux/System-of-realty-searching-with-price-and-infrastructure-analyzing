import { AppShell } from "../components/AppShell";
import { SectionStub } from "../components/SectionStub";

export default function FavoritesPage() {
  return (
    <AppShell>
      <SectionStub
        title="Избранное"
        description="Раздел уже выведен в верхнее меню. Дальше сюда можно докинуть сохраненные объявления, фильтры и синхронизацию с профилем."
      />
    </AppShell>
  );
}
