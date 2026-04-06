# Mobile Shell

Мобильное приложение сведено к одной оболочке `WebView`, которая открывает веб-версию проекта.

Используется адрес:

```env
EXPO_PUBLIC_WEB_APP_URL=https://diploma.com
```

Если нужен другой адрес для локальной разработки, задайте его перед запуском:

```bash
cd /home/Admin/Projects/Diploma/src/Clients/apps/mobile
EXPO_PUBLIC_WEB_APP_URL=https://your-host pnpm start
```

Приложение больше не содержит отдельного мобильного каталога, профиля, форм объявления, карт и mobile-specific бизнес-логики.
