# Backend Testing

В backend теперь есть два канонических тестовых проекта:

- `src/Backend/tests/Backend.UnitTests`
- `src/Backend/tests/Backend.IntegrationTests`

Smoke-проверки вынесены не в отдельный проект, а в отдельную категорию `Smoke` внутри `Backend.IntegrationTests`. Это уменьшает дублирование инфраструктуры `WebApplicationFactory`, но сохраняет явное разделение при запуске и в отчетах.

## Что покрыто

### Unit

`Backend.UnitTests` покрывает:

- валидацию search-запросов;
- валидацию команды создания объявления;
- базовую проверку owner/admin authorization handler;
- обновление собственного профиля пользователя;
- расчет price history timeline для объявлений;
- ранжирование search-результатов;
- инфраструктурный scoring;
- базовую ценовую аналитику через справочник городов.

Всего в проекте сейчас 15 unit-тестов.

### Integration

`Backend.IntegrationTests` поднимает минимальный integration layer для ключевых API через `WebApplicationFactory` и изолированный SQLite:

- `UserService`
  - регистрация;
  - подтверждение email;
  - логин;
  - доступ к защищенному маршруту с токеном и без токена.
- `ListingService.Presentation`
  - старт приложения;
  - базовый GET списка объявлений;
  - пагинация списка объявлений;
  - 401 на защищенном маршруте без токена;
  - создание объявления с корректными данными;
  - отказ при невалидных данных;
  - получение объявления по id.
- `PricingAnalyticsService`
  - базовый аналитический сценарий `POST /api/pricing-analytics/estimate`.

Всего в проекте сейчас 12 integration-тестов.

### Smoke

Smoke-тесты помечены `Trait("Category", "Smoke")`. Сейчас к smoke относятся:

- старт `UserService`;
- старт `ListingService`;
- базовый GET списка объявлений;
- 401 на создании объявления без авторизации;
- успешный happy-path создания объявления.

## Ограничения integration-тестов

- Полный end-to-end слой всех микросервисов не поднимается. Для диплома сделан реалистичный минимальный integration layer на `UserService`, `ListingService.Presentation` и `PricingAnalyticsService`.
- `SearchService` и часть аналитических запросов в production завязаны на PostgreSQL-специфичные конструкции (`ILike`, `jsonb`, runtime schema upgrade). В текущем наборе они не поднимаются полноценно в SQLite-интеграции.
- Для внешних зависимостей (`ImageService`, `InfrastructureService`, `SearchService`, currency-rate client, email verification`) в integration-тестах используются контролируемые test doubles.

## Запуск из терминала

Из корня репозитория:

```bash
./scripts/test-unit.sh
./scripts/test-integration.sh
./scripts/test-smoke.sh
./scripts/test-all.sh
./scripts/generate-test-report.sh
```

PowerShell:

```powershell
./scripts/test-unit.ps1
./scripts/test-integration.ps1
./scripts/test-smoke.ps1
./scripts/test-all.ps1
./scripts/generate-test-report.ps1
```

Прямой запуск `dotnet test`:

```bash
dotnet test src/Backend/tests/Backend.UnitTests/Backend.UnitTests.csproj --filter "Category=Unit"
dotnet test src/Backend/tests/Backend.IntegrationTests/Backend.IntegrationTests.csproj --filter "Category=Integration&Category!=Smoke"
dotnet test src/Backend/tests/Backend.IntegrationTests/Backend.IntegrationTests.csproj --filter "Category=Smoke"
```

## Запуск из VS Code

- Открой папку репозитория.
- Используй Test Explorer или C# Dev Kit test runner.
- Основные проекты для запуска:
  - `Backend.UnitTests`
  - `Backend.IntegrationTests`
- Для smoke удобно запускать через терминал со stable filter `Category=Smoke`.

## Где лежат артефакты

После запуска скриптов:

- `Tests/test-results/unit`
- `Tests/test-results/integration`
- `Tests/test-results/smoke`
- `Tests/coverage/unit`
- `Tests/coverage/integration`
- `Tests/coverage/smoke`
- `Tests/coverage/combined`
- `Tests/coverage-report/unit`
- `Tests/coverage-report/integration`
- `Tests/coverage-report/smoke`
- `Tests/coverage-report/combined`
- `Tests/logs`
- `Tests/index.html`

### Имена файлов

Основные результаты:

- `backend-unit-tests.trx`
- `backend-integration-tests.trx`
- `backend-smoke-tests.trx`

Покрытие копируется в стабильные файлы:

- `Tests/coverage/unit/backend-unit-tests.cobertura.xml`
- `Tests/coverage/integration/backend-integration-tests.cobertura.xml`
- `Tests/coverage/smoke/backend-smoke-tests.cobertura.xml`

## Что можно вставить в диплом

Подходящие артефакты:

- скриншот успешного `./scripts/test-all.sh`;
- `.trx`-результаты по unit/integration/smoke;
- `coverage.cobertura.xml` как доказательство сбора покрытия;
- `Tests/index.html` как сводный HTML-дашборд;
- `Tests/coverage-report/combined/index.html` как удобный HTML coverage report;
- структура тестовых проектов `Backend.UnitTests` / `Backend.IntegrationTests`;
- этот README как описание тестовой стратегии и ограничений.

## HTML-отчеты

Локально подключен `ReportGenerator` как dotnet local tool через `dotnet-tools.json`.

Команда:

```bash
./scripts/generate-test-report.sh
```

или:

```powershell
./scripts/generate-test-report.ps1
```

Что генерируется:

- HTML coverage report по unit;
- HTML coverage report по integration;
- HTML coverage report по smoke;
- combined HTML coverage report;
- `Tests/index.html` со сводкой по `.trx` и ссылками на отчеты.
