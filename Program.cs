using System;
using System.Linq;
using System.Threading.Tasks;
using FitnessEf;
using Microsoft.EntityFrameworkCore;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("==================================================");
Console.WriteLine("       ПРАКТИЧЕСКАЯ РАБОТА: FITNESS EF CORE       ");
Console.WriteLine("==================================================\n");

// Выполняем задания последовательно, сохраняя историю всех шагов
await RunTask2Async();
await RunTask3Async();
await RunTask4Async();

// -----------------------------------------------------------------------------
// Задание 2: Проверка модели Client и конфигурации DbContext
// -----------------------------------------------------------------------------
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

// -----------------------------------------------------------------------------
// Задание 3: Проверка подключения к базе данных и применённых миграций
// -----------------------------------------------------------------------------
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

// -----------------------------------------------------------------------------
// Задание 4: Чтение, сортировка и проекция данных
// -----------------------------------------------------------------------------
static async Task RunTask4Async()
{
    Console.WriteLine("--- [Задание 4] Чтение, сортировка и проекция данных ---");
    await using var db = new FitnessContext();

    // 4.1. Получение всех клиентов (AsNoTracking + OrderBy + ToListAsync)
    Console.WriteLine("4.1. Список всех клиентов (сортировка по ClientId, AsNoTracking):");
    var allClients = await db.Clients
        .AsNoTracking()
        .OrderBy(c => c.ClientId)
        .ToListAsync();

    foreach (var client in allClients)
    {
        Console.WriteLine($"  {client.ClientId}: {client.FirstName} {client.LastName}; статус: {client.Status}");
    }

    // 4.2. Проекция в анонимный тип (FullName, Phone с проверкой null)
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

    // 4.3. Различия методов одиночной выборки: FirstOrDefault, SingleOrDefault, Find
    Console.WriteLine("\n4.3. Демонстрация методов выборки одной записи:");
    
    // FirstOrDefaultAsync
    var firstActive = await db.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Status == "active");
    Console.WriteLine($"  FirstOrDefault (первый активный): {firstActive?.FirstName} {firstActive?.LastName} (ID {firstActive?.ClientId})");

    // FindAsync по первичному ключу
    var clientById = await db.Clients.FindAsync(2);
    Console.WriteLine($"  FindAsync(2) по PK: {clientById?.FirstName} {clientById?.LastName}");

    // SingleOrDefaultAsync (по уникальному предикату)
    var singleClient = await db.Clients.AsNoTracking().SingleOrDefaultAsync(c => c.Email == "ivan@example.com");
    Console.WriteLine($"  SingleOrDefault(email == ivan@example.com): {singleClient?.FirstName} {singleClient?.LastName}");

    Console.WriteLine("Задание 4 успешно пройдено.\n");
}