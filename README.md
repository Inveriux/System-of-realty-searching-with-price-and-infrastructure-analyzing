# Diploma

Репозиторий дипломного проекта со следующими частями:

- `src/Backend` - backend-сервисы на .NET
- `src/Clients/apps/web` - web-клиент на Next.js
- `src/Clients/apps/mobile` - mobile shell на Expo/React Native
- `src/K8S` - локальные k8s-манифесты и сертификаты
- `scripts` - скрипты запуска, тестов и подготовки локальной среды

## Что уже лежит в репозитории

Для локального запуска в репозиторий сознательно включены:

- локальные `.env.local` для web и mobile;
- `appsettings.Development.json` для backend-сервисов;
- локальные сертификаты backend/web;
- prebuild Android-проект для mobile.

Это сделано специально для воспроизводимого запуска в рамках диплома.

## Базовые зависимости

Нужно установить локально:

- `pnpm`
- `Node.js`
- `.NET SDK`
- `Docker`
- для mobile Android: `Android SDK`, `adb`
- для локального k8s-сценария: `kind`, `kubectl`

## Web

Рабочая папка:

```bash
cd /home/Admin/Projects/Diploma/src/Clients
```

Запуск:

```bash
pnpm install
pnpm dev:web
```

Альтернатива без HTTPS dev cert:

```bash
pnpm --filter @diploma/web run dev:http
```

## Mobile

Рабочая папка:

```bash
cd /home/Admin/Projects/Diploma/src/Clients
```

Запуск Expo:

```bash
pnpm install
pnpm dev:mobile
```

Запуск Android dev client по USB:

```bash
EXPO_PUBLIC_WEB_APP_URL=http://127.0.0.1:3000 /home/Admin/Projects/Diploma/scripts/mobile-android-dev-usb.sh
```

## Backend

Простой локальный compose-сценарий:

```bash
cd /home/Admin/Projects/Diploma/src/Backend
docker compose up --build
```

Локальный kind/k8s-сценарий:

```bash
cd /home/Admin/Projects/Diploma
./scripts/recreate-local-diploma-kind.sh
```

## Тесты

Из корня репозитория:

```bash
./scripts/test-unit.sh
./scripts/test-integration.sh
./scripts/test-smoke.sh
./scripts/test-all.sh
```
