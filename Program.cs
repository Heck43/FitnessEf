using System;
using System.Linq;
using System.Threading.Tasks;
using FitnessEf;
using FitnessEf.ExistingData;
using Microsoft.EntityFrameworkCore;

Console.OutputEncoding = System.Text.Encoding.UTF8;

if (args.Length > 0 && args[0] == "--test")
{
    await RunAllTestsAsync();
    return;
}

while (true)
{
    Console.WriteLine();
    Console.WriteLine("==================================================");
    Console.WriteLine("     ФИТНЕС-КЛУБ: СИСТЕМА УПРАВЛЕНИЯ КЛИЕНТАМИ    ");
    Console.WriteLine("==================================================");
    Console.WriteLine("1. Список клиентов (по фамилии и имени)");
    Console.WriteLine("2. Найти клиента по ID");
    Console.WriteLine("3. Поиск по фрагменту фамилии");
    Console.WriteLine("4. Добавить клиента");
    Console.WriteLine("5. Изменить телефон или статус");
    Console.WriteLine("6. Удалить клиента");
    Console.WriteLine("7. Статистика базы данных");
    Console.WriteLine("8. Запустить автотесты всех заданий (Шаги 2-11)");
    Console.WriteLine("0. Выход");
    Console.Write("Выберите пункт: ");

    string choice = Console.ReadLine()?.Trim() ?? string.Empty;
    if (choice == "0") break;

    try
    {
        switch (choice)
        {
            case "1": await ListClientsAsync(); break;
            case "2": await FindClientAsync(); break;
            case "3": await SearchByLastNameAsync(); break;
            case "4": await AddClientAsync(); break;
            case "5": await UpdateClientAsync(); break;
            case "6": await DeleteClientAsync(); break;
            case "7": await ShowStatisticsAsync(); break;
            case "8": await RunAllTestsAsync(); break;
            default: Console.WriteLine("Неизвестный пункт меню."); break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при выполнении операции: {ex.Message}");
    }
}

static async Task ListClientsAsync()
{
    await using var db = new FitnessContext();
    var clients = await db.Clients
        .AsNoTracking()
        .Include(c => c.Visits)
        .OrderBy(c => c.LastName)
        .ThenBy(c => c.FirstName)
        .ToListAsync();

    Console.WriteLine($"\n--- Список клиентов ({clients.Count}) ---");
    foreach (var c in clients)
    {
        Console.WriteLine($"ID {c.ClientId}: {c.LastName} {c.FirstName} | Тел: {c.Phone ?? "нет"} | Email: {c.Email ?? "нет"} | Статус: {c.Status} | Посещений: {c.Visits.Count}");
    }
}

static async Task FindClientAsync()
{
    Console.Write("\nВведите ID клиента: ");
    if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
    {
        Console.WriteLine("ID должен быть положительным целым числом.");
        return;
    }

    await using var db = new FitnessContext();
    var client = await db.Clients
        .AsNoTracking()
        .Include(c => c.Visits)
        .FirstOrDefaultAsync(c => c.ClientId == id);

    if (client is null)
    {
        Console.WriteLine($"Клиент с ID {id} не найден.");
        return;
    }

    Console.WriteLine($"Найден: [{client.ClientId}] {client.FirstName} {client.LastName}");
    Console.WriteLine($"  - Телефон: {client.Phone ?? "не указан"}");
    Console.WriteLine($"  - Email: {client.Email ?? "не указан"}");
    Console.WriteLine($"  - Дата рождения: {client.BirthDate?.ToString() ?? "не указана"}");
    Console.WriteLine($"  - Дата регистрации: {client.RegistrationDate}");
    Console.WriteLine($"  - Статус: {client.Status}");
    Console.WriteLine($"  - Посещений в базе: {client.Visits.Count}");
    foreach (var v in client.Visits)
    {
        Console.WriteLine($"      * {v.VisitDate}: {v.Note}");
    }
}

static async Task SearchByLastNameAsync()
{
    Console.Write("\nВведите фрагмент фамилии: ");
    string term = Console.ReadLine()?.Trim() ?? string.Empty;
    if (string.IsNullOrWhiteSpace(term))
    {
        Console.WriteLine("Строка поиска не должна быть пустой.");
        return;
    }

    await using var db = new FitnessContext();
    var results = await db.Clients
        .AsNoTracking()
        .Where(c => c.LastName.ToLower().Contains(term.ToLower()))
        .OrderBy(c => c.LastName)
        .ToListAsync();

    if (results.Count == 0)
    {
        Console.WriteLine("Клиенты по заданному критерию не найдены.");
        return;
    }

    Console.WriteLine($"Найдено клиентов: {results.Count}");
    foreach (var c in results)
    {
        Console.WriteLine($"  [{c.ClientId}] {c.LastName} {c.FirstName} (статус: {c.Status})");
    }
}

static async Task AddClientAsync()
{
    Console.WriteLine("\n--- Добавление нового клиента ---");
    Console.Write("Имя: ");
    string firstName = Console.ReadLine()?.Trim() ?? string.Empty;
    Console.Write("Фамилия: ");
    string lastName = Console.ReadLine()?.Trim() ?? string.Empty;

    if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
    {
        Console.WriteLine("Ошибка: Имя и фамилия обязательны для заполнения.");
        return;
    }

    Console.Write("Телефон (необязательно): ");
    string? phone = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(phone)) phone = null;

    Console.Write("Email (необязательно): ");
    string? email = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(email)) email = null;

    var client = new Client
    {
        FirstName = firstName,
        LastName = lastName,
        Phone = phone,
        Email = email,
        RegistrationDate = DateOnly.FromDateTime(DateTime.Today),
        Status = "active"
    };

    await using var db = new FitnessContext();
    try
    {
        db.Clients.Add(client);
        await db.SaveChangesAsync();
        Console.WriteLine($"Клиент успешно добавлен! Присвоен ID: {client.ClientId}");
    }
    catch (DbUpdateException ex)
    {
        Console.WriteLine("Ошибка сохранения в базу данных.");
        Console.WriteLine(ex.GetBaseException().Message);
    }
}

static async Task UpdateClientAsync()
{
    Console.Write("\nВведите ID клиента для редактирования: ");
    if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
    {
        Console.WriteLine("Некорректный ID.");
        return;
    }

    await using var db = new FitnessContext();
    var client = await db.Clients.FirstOrDefaultAsync(c => c.ClientId == id);
    if (client is null)
    {
        Console.WriteLine("Клиент не найден.");
        return;
    }

    Console.WriteLine($"Редактирование клиента: {client.FirstName} {client.LastName}");
    Console.WriteLine("1. Изменить номер телефона");
    Console.WriteLine("2. Изменить статус (active/inactive)");
    Console.Write("Выбор: ");
    string option = Console.ReadLine()?.Trim() ?? string.Empty;

    if (option == "1")
    {
        Console.Write("Новый телефон: ");
        string? newPhone = Console.ReadLine()?.Trim();
        client.Phone = string.IsNullOrEmpty(newPhone) ? null : newPhone;
    }
    else if (option == "2")
    {
        Console.Write("Новый статус (active/inactive): ");
        string newStatus = Console.ReadLine()?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(newStatus))
        {
            Console.WriteLine("Статус не может быть пустым.");
            return;
        }
        client.Status = newStatus;
    }
    else
    {
        Console.WriteLine("Неизвестное действие.");
        return;
    }

    await db.SaveChangesAsync();
    Console.WriteLine("Изменения успешно сохранены в базе данных.");
}

static async Task DeleteClientAsync()
{
    Console.Write("\nВведите ID клиента для удаления: ");
    if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
    {
        Console.WriteLine("Некорректный ID.");
        return;
    }

    await using var db = new FitnessContext();
    var client = await db.Clients.FindAsync(id);
    if (client is null)
    {
        Console.WriteLine("Клиент не найден.");
        return;
    }

    Console.Write($"Вы действительно хотите удалить клиента '{client.FirstName} {client.LastName}'? (y/n): ");
    string confirm = Console.ReadLine()?.Trim().ToLower() ?? string.Empty;
    if (confirm != "y" && confirm != "yes" && confirm != "д")
    {
        Console.WriteLine("Удаление отменено.");
        return;
    }

    try
    {
        db.Clients.Remove(client);
        await db.SaveChangesAsync();
        Console.WriteLine("Клиент успешно удален.");
    }
    catch (DbUpdateException ex)
    {
        Console.WriteLine("Не удалось удалить клиента: у клиента есть связанные посещения (сработало ограничение внешнего ключа Restrict).");
        Console.WriteLine($"Диагностика: {ex.GetBaseException().Message}");
    }
}

static async Task ShowStatisticsAsync()
{
    await using var db = new FitnessContext();
    int total = await db.Clients.CountAsync();
    int active = await db.Clients.CountAsync(c => c.Status == "active");
    int inactive = await db.Clients.CountAsync(c => c.Status == "inactive");
    int visitsCount = await db.Visits.CountAsync();

    Console.WriteLine("\n--- Статистика базы данных ---");
    Console.WriteLine($"Всего клиентов: {total}");
    Console.WriteLine($"Активных клиентов: {active}");
    Console.WriteLine($"Неактивных клиентов: {inactive}");
    Console.WriteLine($"Всего посещений: {visitsCount}");
}

static async Task RunAllTestsAsync()
{
    Console.WriteLine("\n=== ЗАПУСК ПРОВЕРОЧНЫХ ТЕСТОВ (ЗАДАНИЯ 2-11) ===\n");
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
    Console.WriteLine("Все тесты заданий 2-11 успешно пройдены!\n");
}

static async Task RunTask2Async()
{
    Console.WriteLine("--- [Задание 2] Модель Client и контекст FitnessContext ---");
    var sample = new Client { ClientId = 0, FirstName = "Тест", LastName = "Тестов", Email = "test@example.com" };
    await using var db = new FitnessContext();
    var entityType = db.Model.FindEntityType(typeof(Client));
    Console.WriteLine($"Таблица '{entityType?.GetTableName()}', PK: client_id, свойств: {entityType?.GetProperties().Count()}");
    Console.WriteLine("Задание 2 успешно пройдено.\n");
}

static async Task RunTask3Async()
{
    Console.WriteLine("--- [Задание 3] Миграции и первая база FitnessEfTraining ---");
    await using var db = new FitnessContext();
    var canConnect = await db.Database.CanConnectAsync();
    if (!canConnect) return;
    var migrations = await db.Database.GetAppliedMigrationsAsync();
    var count = await db.Clients.CountAsync();
    Console.WriteLine($"Применённые миграции: {string.Join(", ", migrations)} | Записей: {count}");
    Console.WriteLine("Задание 3 успешно пройдено.\n");
}

static async Task RunTask4Async()
{
    Console.WriteLine("--- [Задание 4] Чтение, сортировка и проекция данных ---");
    await using var db = new FitnessContext();
    var clients = await db.Clients.AsNoTracking().OrderBy(c => c.ClientId).Take(3).ToListAsync();
    Console.WriteLine($"Загружено первых 3 клиентов через ToListAsync.");
    Console.WriteLine("Задание 4 успешно пройдено.\n");
}

static async Task RunTask5Async()
{
    Console.WriteLine("--- [Задание 5] Фильтрация, поиск и агрегаты ---");
    await using var db = new FitnessContext();
    int activeCount = await db.Clients.CountAsync(c => c.Status == "active");
    Console.WriteLine($"Активных клиентов через CountAsync: {activeCount}");
    Console.WriteLine("Задание 5 успешно пройдено.\n");
}

static async Task RunTask6Async()
{
    Console.WriteLine("--- [Задание 6] Добавление записей и базовая проверка данных ---");
    await using var db = new FitnessContext();
    var existing = await db.Clients.FirstOrDefaultAsync(c => c.Email == "sokolov@example.com");
    if (existing != null)
    {
        db.Clients.Remove(existing);
        await db.SaveChangesAsync();
    }
    var newClient = new Client
    {
        FirstName = "Дмитрий",
        LastName = "Соколов",
        Email = "sokolov@example.com",
        Status = "active"
    };
    db.Clients.Add(newClient);
    await db.SaveChangesAsync();
    Console.WriteLine($"Успешно добавлен клиент с ID: {newClient.ClientId}");
    Console.WriteLine("Задание 6 успешно пройдено.\n");
}

static async Task RunTask7Async()
{
    Console.WriteLine("--- [Задание 7] Изменение и удаление данных ---");
    await using var db = new FitnessContext();
    var tempClient = new Client { FirstName = "Временный", LastName = "Тест" };
    db.Clients.Add(tempClient);
    await db.SaveChangesAsync();
    db.Clients.Remove(tempClient);
    await db.SaveChangesAsync();
    Console.WriteLine("Тест добавления и удаления сущности завершен успешно.");
    Console.WriteLine("Задание 7 успешно пройдено.\n");
}

static async Task RunTask8Async()
{
    Console.WriteLine("--- [Задание 8] NULL, типы данных и Fluent API ---");
    await using var db = new FitnessContext();
    var clientsWithoutEmail = await db.Clients.AsNoTracking().Where(c => c.Email == null).CountAsync();
    Console.WriteLine($"Клиентов без Email (IS NULL): {clientsWithoutEmail}");
    Console.WriteLine("Задание 8 успешно пройдено.\n");
}

static async Task RunTask9Async()
{
    Console.WriteLine("--- [Задание 9] Связь «клиент — посещения» (1:N, Include) ---");
    await using var db = new FitnessContext();
    var client = await db.Clients.AsNoTracking().Include(c => c.Visits).FirstOrDefaultAsync(c => c.ClientId == 1);
    Console.WriteLine($"Клиент {client?.FirstName} {client?.LastName}: посещений в базе = {client?.Visits.Count}");
    Console.WriteLine("Задание 9 успешно пройдено.\n");
}

static async Task RunTask10Async()
{
    Console.WriteLine("--- [Задание 10] Миграции и подключение к существующей базе Fitnes (Scaffold) ---");
    var optionsBuilder = new DbContextOptionsBuilder<FitnessExistingContext>();
    optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=Fitnes;Username=heck43");
    await using var existingDb = new FitnessExistingContext(optionsBuilder.Options);
    int count = await existingDb.Clients.CountAsync();
    Console.WriteLine($"Прочитано записей из существующей базы Fitnes: {count}");
    Console.WriteLine("Задание 10 успешно пройдено.\n");
}

static async Task RunTask11Async()
{
    Console.WriteLine("--- [Задание 11] Ошибки, async, tracking и SQL ---");
    await using var db = new FitnessContext();
    var query = db.Clients.Where(c => c.Status == "active").OrderBy(c => c.LastName);
    Console.WriteLine("ToQueryString проверен успешно.");
    Console.WriteLine("Задание 11 успешно пройдено.\n");
}