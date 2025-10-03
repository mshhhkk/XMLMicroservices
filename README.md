# XMLMicroservices
Два .NET 8 worker-сервиса, обменивающиеся сообщениями через RabbitMQ:  
**FileParserService** парсит входные XML, случайно мутирует `ModuleState` и публикует JSON-события;  
**DataProcessorService** подписывается и сохраняет состояния модулей в SQLite.


## Технологии
- .NET 8 Worker Services (Generic Host)
- RabbitMQ (topic exchange)
- SQLite (`Microsoft.Data.Sqlite`, репозиторий поверх ADO/EF)
- LINQ to XML (парсинг вложенного/экранированного XML)
- Docker & Docker Compose

---
## Требования и установка
### Вариант A: Windows + Docker Desktop (без WSL)
1. Скачайте и установите **Docker Desktop for Windows**.

2. Во время установки оставьте опцию Use WSL 2 based engine включённой (по умолчанию).

3. Откройте Docker Desktop и дождитесь статуса Running

> Дополнительных настроек WSL не требуется — все команды ниже выполняются в PowerShell. SQLite-CLI ставить не обязательно — в примерах ниже используется одноразовый контейнер nouchka/sqlite3.

### Вариант B: WSL/Linux
  ```bash
   sudo apt-get update
   sudo apt-get install -y docker.io docker-compose-plugin
   # (опционально) чтобы не писать sudo
   sudo usermod -aG docker $USER && newgrp docker
 ```
> ⚠️ Если не добавить пользователя в группу докер, запускать котейнер можно только через суперюзера под sudo
  
---
## Быстрый старт (Docker)
# PowerShell (Windows)
Создаём каталоги для входных файлов и БД, собираем и запускаем контейнеры.
1. Перейти в корень репозитория
   ```powershell
   cd ..\XMLMicroservice
   ```
2. Подготовить папки
   ```powershell
   md parser-in, parser-bad, db
   ```
3. Соберите и поднимите сервисы:
   ```powershell
   docker compose down -v
   docker compose build --no-cache
   docker compose up -d --force-recreate
   ```
4. Проверьте состояние:
   ```powershell
   docker compose ps
   ```
   В колонке `STATUS` для `rabbit`, `xmlmicroservices-fileparser-1`, `xmlmicroservices-dataprocessor-1` должно быть `Up`.

5. Откройте UI RabbitMQ: **http://localhost:15672** (логин/пароль: `guest/guest`).

# Bash (WSL/Linux)
1. Перейти в корень репозитория
   ```bash
   cd ..\XMLMicroservice
   ```
2. Подготовить папки
    ```bash
    mkdir -p parser-in parser-bad db
    ```
3. Соберите и поднимите сервисы:
   ```bash
   docker compose down -v
   docker compose build --no-cache
   docker compose up -d --force-recreate
   ```
4. Проверьте состояние:
   ```bash
   docker compose ps
   ```
Откройте UI RabbitMQ: **http://localhost:15672** (логин/пароль: `guest/guest`).

> ⚠️ Конфигурация берётся **только из переменных окружения** в `docker-compose.yml`. `appsettings*.json` не читаются.

### Как это связано в Compose (фрагмент)
```yaml
services:
  rabbit:
    image: rabbitmq:3-management
    ports: ["5672:5672", "15672:15672"]
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest

  fileparser:
    build:
      context: .
      dockerfile: FileParserService/Dockerfile
    depends_on: [rabbit]
    environment:
      RabbitMQ__HostName: rabbit
      RabbitMQ__Port: "5672"
      RabbitMQ__UserName: guest
      RabbitMQ__Password: guest
      RabbitMQ__Exchange: modules.topic
      RabbitMQ__RoutingKey: modules.update
      RabbitMQ__Queue: modules.db
      Watch__IncomeFolder: "./in"
      Watch__FailedFolder: "./fail"
      Watch__IntervalMs: "1000"
      Watch__MaxParallel: "4"
    volumes:
      - ./parser-in:/app/in
      - ./parser-bad:/app/fail

  dataprocessor:
    build:
      context: .
      dockerfile: DataProcessorService/Dockerfile
    depends_on: [rabbit]
    environment:
      RabbitMQ__HostName: rabbit
      RabbitMQ__Port: "5672"
      RabbitMQ__UserName: guest
      RabbitMQ__Password: guest
      RabbitMQ__Exchange: modules.topic
      RabbitMQ__RoutingKey: modules.update
      RabbitMQ__Queue: modules.db
      RabbitMQ__Prefetch: "50"
      SQLite__DbPath: "/app/db/data.sqlite"
    volumes:
      - ./db:/app/db
```

---

## Использование

1. Скопируйте тестовый XML в папку `parser-in` на хосте — она смонтирована в контейнер `fileparser` как `/app/in`.
2. Сервис **FileParserService**:
   - читает XML;
   - парсит вложенный/экранированный XML;
   - **случайно** меняет `ModuleState` (`Online | Run | NotReady | Offline`);
   - публикует JSON в `RabbitMQ.Exchange=modules.topic`, `RoutingKey=modules.update`.
3. **DataProcessorService** потребляет сообщения из очереди `modules.db` и делает upsert в SQLite.

Проверка логов:
```powershell
docker compose logs --no-color --tail=200 fileparser
docker compose logs --no-color --tail=200 dataprocessor
```

Проверка переменных окружения внутри контейнера:
```bash
docker compose exec fileparser    sh -lc 'printenv | grep "^RabbitMQ__\|^Watch__" | sort'
docker compose exec dataprocessor sh -lc 'printenv | grep "^RabbitMQ__\|^SQLite__" | sort'
```

Проверка БД:
```powershell
docker run --rm -v ${PWD}/db:/db nouchka/sqlite3 `
  sqlite3 /db/data.sqlite "SELECT * FROM ModuleStates;"
```

---

## Deploy и CI/CD
- Каждый сервис упакован своим `Dockerfile`, оркестрация — `docker-compose.yml`.
- Рекомендуемый пайплайн (например, GitHub Actions):
  1) checkout  
  2) setup .NET  
  3) `dotnet restore/build/test`  
  4) сборка образов и push в реестр  
  5) деплой и `docker compose up -d` на сервере.
- Конфигурация окружения — только через ENV (секреты — через секреты раннера/реестра).

---

## FAQ

**Почему не используем `appsettings.json`?**  
Чтобы исключить рассинхронизацию окружений — конфиг только из ENV (особенно удобно в контейнерах/CI).

**`Connection refused` при подключении к RabbitMQ**  
Проверьте, что сервис `rabbit` в `docker compose ps` — `Up`, и в ENV `RabbitMQ__HostName=rabbit` (не `localhost`).

**Где лежит база?**  
Файл `./db/data.sqlite` на хосте, смонтирован в контейнер по пути `/app/db/data.sqlite`.

---

