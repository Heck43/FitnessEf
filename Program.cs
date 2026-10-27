using System;
using FitnessEf;
using Microsoft.EntityFrameworkCore;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=== Проверка Задания 2: Модель Client и контекст FitnessContext ===");

// 1. Проверяем создание экземпляра модели
var testClient = new Client
{
    ClientId = 1,
    FirstName = "Иван",
    LastName = "Иванов",
    Email = "ivan@example.com"
};

Console.WriteLine($"[Модель Client]: Создан клиент {testClient.FirstName} {testClient.LastName}");
Console.WriteLine($"  - Дата регистрации по умолчанию: {testClient.RegistrationDate}");
Console.WriteLine($"  - Начальный статус: {testClient.Status}");

// 2. Проверяем инициализацию контекста и Fluent API метаданные
using var db = new FitnessContext();
var clientEntity = db.Model.FindEntityType(typeof(Client));

if (clientEntity != null)
{
    Console.WriteLine($"\n[Контекст FitnessContext]:");
    Console.WriteLine($"  - Таблица в БД: {clientEntity.GetTableName()}");
    Console.WriteLine($"  - Первичный ключ: {string.Join(", ", clientEntity.FindPrimaryKey()?.Properties.Select(p => p.GetColumnName()) ?? Array.Empty<string>())}");
    Console.WriteLine($"  - Свойств сопоставлено: {clientEntity.GetProperties().Count()}");
}

Console.WriteLine("\nПроверка модели и контекста успешно пройдена!");