import type { Metadata, Viewport } from "next";
import { Manrope, Space_Grotesk } from "next/font/google";
import { cookies } from "next/headers";
import { Providers } from "./providers";
import { THEME_STORAGE_KEY, type ThemeMode } from "./theme.constants";
import "./globals.css";

const bodyFont = Manrope({
  variable: "--font-body",
  subsets: ["latin"],
});

const displayFont = Space_Grotesk({
  variable: "--font-display",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Domio",
  description: "Платформа объявлений по недвижимости",
  icons: {
    icon: "/new_icon.png",
    shortcut: "/new_icon.png",
    apple: "/new_icon.png",
  },
};

export const viewport: Viewport = {
  width: "device-width",
  initialScale: 1,
  maximumScale: 1,
  viewportFit: "cover",
};

export default async function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const cookieStore = await cookies();
  const themeCookie = cookieStore.get(THEME_STORAGE_KEY)?.value;
  const initialTheme: ThemeMode = themeCookie === "dark" ? "dark" : "light";

  return (
    <html lang="ru" data-theme={initialTheme} suppressHydrationWarning>
      <head>
        <script
          dangerouslySetInnerHTML={{
            __html: `(function(){try{var url=new URL(window.location.href);var forced=url.searchParams.get("__mobile_theme");var isMobileClient=url.searchParams.get("__mobile_client")==="1";var rootTheme=document.documentElement.dataset.theme;var stored=window.localStorage.getItem(${JSON.stringify(THEME_STORAGE_KEY)});var theme=forced==="dark"||forced==="light"?forced:(stored==="dark"||stored==="light"?stored:(rootTheme==="dark"||rootTheme==="light"?rootTheme:(window.matchMedia("(prefers-color-scheme: dark)").matches?"dark":"light")));if(isMobileClient){document.documentElement.dataset.mobileClientShell="true";}document.documentElement.dataset.theme=theme;document.documentElement.style.colorScheme=theme;window.localStorage.setItem(${JSON.stringify(THEME_STORAGE_KEY)},theme);document.cookie=${JSON.stringify(`${THEME_STORAGE_KEY}=`)}+encodeURIComponent(theme)+"; path=/; max-age=31536000; SameSite=Lax";if(forced==="dark"||forced==="light"){url.searchParams.delete("__mobile_theme");}if(isMobileClient){url.searchParams.delete("__mobile_client");}if(forced==="dark"||forced==="light"||isMobileClient){window.history.replaceState(window.history.state,"",url.toString());}}catch(e){}})();`,
          }}
        />
      </head>
      <body className={`${bodyFont.variable} ${displayFont.variable}`}>
        <Providers initialTheme={initialTheme}>{children}</Providers>
      </body>
    </html>
  );
}
