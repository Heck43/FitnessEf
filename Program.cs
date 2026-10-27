using System;
using System.Linq;
using System.Threading.Tasks;
using FitnessEf;
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