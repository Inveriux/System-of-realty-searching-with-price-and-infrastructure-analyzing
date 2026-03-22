import { AppShell } from "../components/AppShell";

export default function FavoritesPage() {
  return (
    <AppShell>
      <section
        className="surface"
        style={{
          borderRadius: 28,
          padding: 28,
        }}
      >
        <h1 className="section-title" style={{ marginTop: 0 }}>
          Избранное
        </h1>
      </section>
    </AppShell>
  );
}
