using System;
using System.Linq;
using System.Threading.Tasks;
using FitnessEf;
using FitnessEf.ExistingData;
using Microsoft.EntityFrameworkCore;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("==================================================");
Console.WriteLine("       ПРАКТИЧЕСКАЯ РАБОТА: FITNESS EF CORE       ");
Console.WriteLine("==================================================\n");

await RunTask2Async();
await RunTask3Async();
await RunTask4Async();
await RunTask5Async();
await RunTask6Async();
await RunTask7Async();
await RunTask8Async();
await RunTask9Async();
await RunTask10Async();
await RunTask11Async();

static async Task RunTask2Async()
{
    Console.WriteLine("--- [Задание 2] Модель Client и контекст FitnessContext ---");
    
    var sample = new Client
    {
        ClientId = 0,
        FirstName = "Тест",
        LastName = "Тестов",
        Email = "test@example.com"
    };
    Console.WriteLine($"[Модель Client]: Создан тестовый объект: {sample.FirstName} {sample.LastName}, статус: {sample.Status}, дата: {sample.RegistrationDate}");

    await using var db = new FitnessContext();
    var entityType = db.Model.FindEntityType(typeof(Client));
    Console.WriteLine($"[Fluent API сопоставление]: Таблица '{entityType?.GetTableName()}', PK: client_id, свойств: {entityType?.GetProperties().Count()}");
    Console.WriteLine("Задание 2 успешно пройдено.\n");
}

static async Task RunTask3Async()
{
    Console.WriteLine("--- [Задание 3] Миграции и первая база FitnessEfTraining ---");
    
    await using var db = new FitnessContext();
    var canConnect = await db.Database.CanConnectAsync();
    if (!canConnect)
    {
        Console.WriteLine("Ошибка: PostgreSQL недоступен.");
        return;
    }
    Console.WriteLine("Соединение с PostgreSQL (FitnessEfTraining) успешно установлено.");

    var migrations = await db.Database.GetAppliedMigrationsAsync();
    Console.WriteLine($"Применённые миграции: {string.Join(", ", migrations)}");
    var count = await db.Clients.CountAsync();
    Console.WriteLine($"Всего записей в таблице clients: {count}");
    Console.WriteLine("Задание 3 успешно пройдено.\n");
}

static async Task RunTask4Async()
{
    Console.WriteLine("--- [Задание 4] Чтение, сортировка и проекция данных ---");
    await using var db = new FitnessContext();

    Console.WriteLine("4.1. Список всех клиентов (сортировка по ClientId, AsNoTracking):");
    var allClients = await db.Clients
        .AsNoTracking()
        .OrderBy(c => c.ClientId)
        .ToListAsync();

    foreach (var client in allClients)
    {
        Console.WriteLine($"  {client.ClientId}: {client.FirstName} {client.LastName}; статус: {client.Status}");
    }

    Console.WriteLine("\n4.2. Проекция в анонимный тип (сортировка по фамилии, затем имени):");
    var projectedClients = await db.Clients
        .AsNoTracking()
        .OrderBy(c => c.LastName)
        .ThenBy(c => c.FirstName)
        .Select(c => new
        {
            c.ClientId,
            FullName = c.FirstName + " " + c.LastName,
            c.Phone
        })
        .ToListAsync();

    foreach (var c in projectedClients)
    {
        Console.WriteLine($"  {c.ClientId}: {c.FullName}; тел.: {c.Phone ?? "не указан"}");
    }

    Console.WriteLine("\n4.3. Демонстрация методов выборки одной записи:");
    var firstActive = await db.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Status == "active");
    Console.WriteLine($"  FirstOrDefault (первый активный): {firstActive?.FirstName} {firstActive?.LastName} (ID {firstActive?.ClientId})");

    var clientById = await db.Clients.FindAsync(2);
    Console.WriteLine($"  FindAsync(2) по PK: {clientById?.FirstName} {clientById?.LastName}");

    var singleClient = await db.Clients.AsNoTracking().SingleOrDefaultAsync(c => c.Email == "ivan@example.com");
    Console.WriteLine($"  SingleOrDefault(email == ivan@example.com): {singleClient?.FirstName} {singleClient?.LastName}");

    Console.WriteLine("Задание 4 успешно пройдено.\n");
}

static async Task RunTask5Async()
{
    Console.WriteLine("--- [Задание 5] Фильтрация, поиск и агрегаты ---");
    await using var db = new FitnessContext();

    Console.WriteLine("5.1. Фильтрация активных клиентов (Where c.Status == 'active'):");
    var activeClients = await db.Clients
        .AsNoTracking()
        .Where(c => c.Status == "active")
        .OrderBy(c => c.LastName)
        .ToListAsync();

    foreach (var c in activeClients)
    {
        Console.WriteLine($"  {c.ClientId}: {c.FirstName} {c.LastName} ({c.Status})");
    }

    string term = "Иван";
    Console.WriteLine($"\n5.2. Поиск по фрагменту фамилии '{term}' (Contains):");
    var foundByName = await db.Clients
        .AsNoTracking()
        .Where(c => c.LastName.Contains(term))
        .OrderBy(c => c.LastName)
        .ToListAsync();

    foreach (var c in foundByName)
    {
        Console.WriteLine($"  Найдено: {c.ClientId}: {c.FirstName} {c.LastName}");
    }

    int searchId = 1;
    Console.WriteLine($"\n5.3. Поиск клиента по ID={searchId} (FindAsync):");
    var foundById = await db.Clients.FindAsync(searchId);
    if (foundById is null)
        Console.WriteLine("  Клиент не найден.");
    else
        Console.WriteLine($"  Найден клиент: {foundById.ClientId}: {foundById.FirstName} {foundById.LastName}");

    Console.WriteLine("\n5.4. Агрегаты (CountAsync, AnyAsync):");
    int total = await db.Clients.CountAsync();
    int activeCount = await db.Clients.CountAsync(c => c.Status == "active");
    bool hasInactive = await db.Clients.AnyAsync(c => c.Status == "inactive");

    Console.WriteLine($"  Всего клиентов: {total}");
    Console.WriteLine($"  Активных клиентов: {activeCount}");
    Console.WriteLine(hasInactive ? "  Есть неактивные клиенты." : "  Неактивные клиенты отсутствуют.");

    Console.WriteLine("\n5.5. Постраничная загрузка (Пагинация: страница 1, размер 2):");
    int page = 1;
    int pageSize = 2;
    var pageOfClients = await db.Clients
        .AsNoTracking()
        .OrderBy(c => c.ClientId)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    foreach (var c in pageOfClients)
    {
        Console.WriteLine($"  [Стр {page}] {c.ClientId}: {c.FirstName} {c.LastName}");
    }

    Console.WriteLine("Задание 5 успешно пройдено.\n");
}

static async Task RunTask6Async()
{
    Console.WriteLine("--- [Задание 6] Добавление записей и базовая проверка данных ---");
    await using var db = new FitnessContext();

    string firstName = "Дмитрий";
    string lastName = "Соколов";

    if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
    {
        Console.WriteLine("Имя и фамилия обязательны.");
        return;
    }

    var existing = await db.Clients.FirstOrDefaultAsync(c => c.Email == "sokolov@example.com");
    if (existing != null)
    {
        db.Clients.Remove(existing);
        await db.SaveChangesAsync();
    }

    var newClient = new Client
    {
        FirstName = firstName,
        LastName = lastName,
        BirthDate = new DateOnly(2000, 5, 15),
        Phone = "79995554433",
        Email = "sokolov@example.com",
        RegistrationDate = DateOnly.FromDateTime(DateTime.Today),
        Status = "active"
    };

    try
    {
        db.Clients.Add(newClient);
        int saved = await db.SaveChangesAsync();

        Console.WriteLine($"Сохранено записей в БД: {saved}");
        Console.WriteLine($"ID добавленного клиента: {newClient.ClientId} ({newClient.FirstName} {newClient.LastName})");
    }
    catch (DbUpdateException ex)
    {
        Console.WriteLine("Не удалось сохранить клиента. Проверьте данные и ограничения БД.");
        Console.WriteLine(ex.GetBaseException().Message);
    }

    Console.WriteLine("Задание 6 успешно пройдено.\n");
}

static async Task RunTask7Async()
{
    Console.WriteLine("--- [Задание 7] Изменение и удаление данных ---");
    await using var db = new FitnessContext();

    int updateId = 1;
    var clientToUpdate = await db.Clients.FirstOrDefaultAsync(c => c.ClientId == updateId);
    if (clientToUpdate is not null)
    {
        string oldPhone = clientToUpdate.Phone ?? "не указан";
        clientToUpdate.Phone = "79990001122";
        await db.SaveChangesAsync();
        Console.WriteLine($"7.1. UPDATE: Клиент ID {updateId} ({clientToUpdate.FirstName} {clientToUpdate.LastName}), телефон изменён: {oldPhone} -> {clientToUpdate.Phone}");

        clientToUpdate.Phone = "79991112233";
        await db.SaveChangesAsync();
    }

    var tempClient = new Client
    {
        FirstName = "Временный",
        LastName = "ДляУдаления",
        RegistrationDate = DateOnly.FromDateTime(DateTime.Today),
        Status = "inactive"
    };
    db.Clients.Add(tempClient);
    await db.SaveChangesAsync();
    int tempId = tempClient.ClientId;
    Console.WriteLine($"\n7.2. DELETE: Создан временный клиент ID {tempId} для проверки удаления");

    var toDelete = await db.Clients.FindAsync(tempId);
    if (toDelete is not null)
    {
        db.Clients.Remove(toDelete);
        await db.SaveChangesAsync();
        Console.WriteLine($"Клиент ID {tempId} успешно удален через Remove + SaveChangesAsync.");
    }

    Console.WriteLine("\n7.4. Пакетная фиксация нескольких изменений в одной транзакции:");
    var targetClient = await db.Clients.FindAsync(2);
    if (targetClient is not null)
    {
        targetClient.Status = "active";
    }
    int batchSaved = await db.SaveChangesAsync();
    Console.WriteLine($"Успешно применена атомарная пачка изменений (обработано записей: {batchSaved}).");

    Console.WriteLine("Задание 7 успешно пройдено.\n");
}

static async Task RunTask8Async()
{
    Console.WriteLine("--- [Задание 8] NULL, типы данных и Fluent API ---");
    await using var db = new FitnessContext();

    Console.WriteLine("8.1. Выборка клиентов с отсутствующим email (Where c.Email == null -> IS NULL):");
    var clientsWithoutEmail = await db.Clients
        .AsNoTracking()
        .Where(c => c.Email == null)
        .OrderBy(c => c.ClientId)
        .ToListAsync();

    foreach (var c in clientsWithoutEmail)
    {
        Console.WriteLine($"  ID {c.ClientId}: {c.FirstName} {c.LastName} (Email: NULL, Тел: {c.Phone ?? "NULL"})");
    }

    Console.WriteLine("\n8.2. Выборка клиентов с заполненным телефоном (c.Phone != null -> IS NOT NULL):");
    var clientsWithPhone = await db.Clients
        .AsNoTracking()
        .Where(c => c.Phone != null)
        .OrderBy(c => c.ClientId)
        .ToListAsync();

    foreach (var c in clientsWithPhone)
    {
        Console.WriteLine($"  ID {c.ClientId}: {c.FirstName} {c.LastName} (Тел: {c.Phone})");
    }

    Console.WriteLine("\n8.4. Диагностика соответствия схеме (Fluent API правила):");
    var entity = db.Model.FindEntityType(typeof(Client));
    var phoneProp = entity?.FindProperty(nameof(Client.Phone));
    var emailProp = entity?.FindProperty(nameof(Client.Email));
    var statusProp = entity?.FindProperty(nameof(Client.Status));
    var dateProp = entity?.FindProperty(nameof(Client.RegistrationDate));

    Console.WriteLine($"  - Phone: Nullable={phoneProp?.IsNullable}, MaxLength={phoneProp?.GetMaxLength()}");
    Console.WriteLine($"  - Email: Nullable={emailProp?.IsNullable}, MaxLength={emailProp?.GetMaxLength()}");
    Console.WriteLine($"  - Status: Nullable={statusProp?.IsNullable}, MaxLength={statusProp?.GetMaxLength()}");
    Console.WriteLine($"  - RegistrationDate: ColumnType={dateProp?.GetColumnType()}");

    Console.WriteLine("Задание 8 успешно пройдено.\n");
}

static async Task RunTask9Async()
{
    Console.WriteLine("--- [Задание 9] Связь «клиент — посещения» (1:N, Include) ---");
    await using var db = new FitnessContext();

    int clientId = 1;
    var clientExists = await db.Clients.AnyAsync(c => c.ClientId == clientId);
    if (!clientExists)
    {
        Console.WriteLine("Клиента с таким ID нет.");
        return;
    }

    var existingVisit = await db.Visits.FirstOrDefaultAsync(v => v.ClientId == clientId && v.Note == "Первичное Задание");
    if (existingVisit == null)
    {
        db.Visits.Add(new Visit
        {
            ClientId = clientId,
            VisitDate = DateOnly.FromDateTime(DateTime.Today),
            Note = "Первичное Задание"
        });
        await db.SaveChangesAsync();
        Console.WriteLine($"9.4. Добавлено посещение для клиента ID {clientId}.");
    }

    Console.WriteLine("\n9.5. Чтение клиентов вместе со связанными посещениями (.Include(c => c.Visits)):");
    var clientsWithVisits = await db.Clients
        .AsNoTracking()
        .Include(c => c.Visits)
        .OrderBy(c => c.ClientId)
        .ToListAsync();

    foreach (var client in clientsWithVisits)
    {
        Console.WriteLine($"  {client.FirstName} {client.LastName} (Посещений: {client.Visits.Count}):");
        foreach (var visit in client.Visits)
        {
            Console.WriteLine($"    - {visit.VisitDate}: {visit.Note}");
        }
    }

    Console.WriteLine("Задание 9 успешно пройдено.\n");
}

static async Task RunTask10Async()
{
    Console.WriteLine("--- [Задание 10] Миграции и подключение к существующей базе Fitnes (Scaffold) ---");
    await using var db = new FitnessContext();

    Console.WriteLine("10.2. История миграций учебной базы FitnessEfTraining:");
    var migrations = await db.Database.GetAppliedMigrationsAsync();
    foreach (var m in migrations)
    {
        Console.WriteLine($"  - {m}");
    }

    Console.WriteLine("\n10.4. Чтение из существующей базы Fitnes через сгенерированный контекст FitnessExistingContext:");
    var optionsBuilder = new DbContextOptionsBuilder<FitnessExistingContext>();
    optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=Fitnes;Username=heck43");

    await using var existingDb = new FitnessExistingContext(optionsBuilder.Options);
    var existingClients = await existingDb.Clients
        .AsNoTracking()
        .OrderBy(c => c.ClientId)
        .Take(3)
        .ToListAsync();

    foreach (var c in existingClients)
    {
        Console.WriteLine($"  [Fitnes DB] ID {c.ClientId}: {c.FirstName} {c.LastName} | статус: {c.Status}");
    }

    Console.WriteLine("Задание 10 успешно пройдено.\n");
}

static async Task RunTask11Async()
{
    Console.WriteLine("--- [Задание 11] Ошибки, async, tracking и SQL ---");
    
    try
    {
        await using var db = new FitnessContext();

        Console.WriteLine("11.3. Сравнение отслеживаемого и нетрекаемого запроса:");
        var untracked = await db.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.ClientId == 1);
        var tracked = await db.Clients.FirstOrDefaultAsync(c => c.ClientId == 1);
        Console.WriteLine($"  AsNoTracking: State = {(untracked != null ? db.Entry(untracked).State.ToString() : "null")}");
        Console.WriteLine($"  Tracked:      State = {(tracked != null ? db.Entry(tracked).State.ToString() : "null")}");

        Console.WriteLine("\n11.4. Диагностика трансляции SQL (ToQueryString):");
        var query = db.Clients
            .Where(c => c.Status == "active")
            .OrderBy(c => c.LastName);
        Console.WriteLine(query.ToQueryString());

        var clients = await db.Clients.AsNoTracking().ToListAsync();
        Console.WriteLine($"11.1. Базовая обработка: успешно загружено клиентов: {clients.Count}");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"Ошибка конфигурации: {ex.Message}");
    }
    catch (DbUpdateException ex)
    {
        Console.WriteLine($"Ошибка сохранения данных: {ex.GetBaseException().Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Произошла непредвиденная ошибка: {ex.Message}");
    }

    Console.WriteLine("\nЗадание 11 успешно пройдено.\n");
}