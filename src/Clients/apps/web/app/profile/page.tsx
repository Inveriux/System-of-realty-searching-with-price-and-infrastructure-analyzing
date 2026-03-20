import Link from "next/link";
import { AppShell } from "../components/AppShell";

export default function ProfilePage() {
  return (
    <AppShell>
      <section
        className="surface"
        style={{
          borderRadius: 28,
          padding: 28,
          display: "grid",
          gap: 18,
        }}
      >
        <div className="eyebrow">Profile</div>
        <h1 className="section-title">Профиль</h1>
        <p style={{ margin: 0, color: "var(--foreground-muted)", maxWidth: 680 }}>
          Здесь будет профиль пользователя. Пока что можно сразу перейти к созданию
          нового объявления.
        </p>
        <div style={{ display: "flex", gap: 12, flexWrap: "wrap" }}>
          <Link className="button-primary" href="/create-listing">
            Создать объявление
          </Link>
        </div>
      </section>
    </AppShell>
  );
}
