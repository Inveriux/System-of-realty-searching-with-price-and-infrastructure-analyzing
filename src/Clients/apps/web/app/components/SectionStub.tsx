"use client";

export function SectionStub({
  title,
  description,
}: {
  title: string;
  description: string;
}) {
  return (
    <section
      className="surface"
      style={{
        borderRadius: 28,
        padding: 28,
      }}
    >
      <div className="eyebrow">Soon</div>
      <h1 className="section-title" style={{ marginTop: 10 }}>
        {title}
      </h1>
      <p style={{ color: "var(--foreground-muted)", maxWidth: 720 }}>{description}</p>
    </section>
  );
}
