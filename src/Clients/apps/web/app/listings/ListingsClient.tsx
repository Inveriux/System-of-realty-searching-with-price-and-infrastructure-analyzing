"use client";

import { useListingsQuery } from "@diploma/features";
import Image from "next/image";
import { AppShell } from "../components/AppShell";
import { useAuth } from "../providers";

export function ListingsClient() {
  const { api } = useAuth();
  const { data, isLoading, error } = useListingsQuery(api);
  const placeholderImage =
    "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 900 700'%3E%3Crect width='900' height='700' fill='%23ece5ff'/%3E%3Ctext x='50%25' y='50%25' dominant-baseline='middle' text-anchor='middle' fill='%235326d4' font-family='Arial' font-size='54'%3EDiploma Listing%3C/text%3E%3C/svg%3E";

  return (
    <AppShell>
      <main style={{ display: "grid", gap: 20 }}>
        <section
          className="surface"
          style={{
            borderRadius: 28,
            padding: 28,
            overflow: "hidden",
            position: "relative",
          }}
        >
          <div
            style={{
              position: "absolute",
              inset: "auto -60px -80px auto",
              width: 220,
              height: 220,
              borderRadius: "50%",
              background: "radial-gradient(circle, var(--color-accent) 0%, transparent 68%)",
              opacity: 0.32,
              pointerEvents: "none",
            }}
          />
          <p
            style={{
              maxWidth: 720,
              color: "var(--foreground-muted)",
              fontSize: "1rem",
              marginBottom: 0,
            }}
          >
            Лента объявлений с фотографиями, ценой и кратким описанием.
          </p>
        </section>

        <section style={{ display: "grid", gap: 16 }}>
          <div
            style={{
              display: "flex",
              justifyContent: "space-between",
              gap: 12,
              alignItems: "end",
              flexWrap: "wrap",
            }}
          >
            <div>
              <h2 className="section-title" style={{ marginTop: 8 }}>
                Объявлений найдено: {data?.totalCount ?? 0}
              </h2>
            </div>
          </div>

          {isLoading ? <div className="surface" style={{ borderRadius: 28, padding: 24 }}>Загрузка объявлений...</div> : null}
          {error ? (
            <div className="surface" style={{ borderRadius: 28, padding: 24, color: "#d04b73" }}>
              Ошибка: {String(error.message)}
            </div>
          ) : null}

          <div
            style={{
              display: "grid",
              gridTemplateColumns: "repeat(auto-fit, minmax(280px, 1fr))",
              gap: 16,
            }}
          >
            {(data?.items ?? []).map((listing) => (
              <article
                className="surface"
                key={listing.id}
                style={{
                  borderRadius: 28,
                  overflow: "hidden",
                }}
              >
                <div style={{ background: "var(--background-muted)", aspectRatio: "4 / 3" }}>
                  <Image
                    src={listing.mainMedia ?? placeholderImage}
                    alt={listing.title}
                    width={900}
                    height={700}
                    unoptimized
                    style={{
                      width: "100%",
                      height: "100%",
                      objectFit: "cover",
                    }}
                  />
                </div>

                <div style={{ padding: 20, display: "grid", gap: 10 }}>
                  <div
                    style={{
                      display: "flex",
                      justifyContent: "space-between",
                      gap: 12,
                      alignItems: "start",
                    }}
                  >
                    <h3
                      style={{
                        margin: 0,
                        fontSize: "1.1rem",
                        lineHeight: 1.15,
                      }}
                    >
                      {listing.title}
                    </h3>
                    <span
                      style={{
                        padding: "8px 10px",
                        borderRadius: 999,
                        background: "var(--color-primary-soft)",
                        color: "var(--color-primary-strong)",
                        whiteSpace: "nowrap",
                        fontWeight: 700,
                      }}
                    >
                      {listing.priceAmount
                        ? `${listing.priceAmount.toLocaleString()} ${listing.priceCurrency ?? ""}`
                        : "Цена скрыта"}
                    </span>
                  </div>

                  <p style={{ margin: 0, color: "var(--foreground-muted)" }}>
                    {listing.description ?? "Описание пока не заполнено."}
                  </p>
                </div>
              </article>
            ))}
          </div>
        </section>
      </main>
    </AppShell>
  );
}
