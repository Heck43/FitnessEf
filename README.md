# Практическая работа: Entity Framework Core и PostgreSQL

**Проект:** FitnessEf  
**Студент (ФИО):** Heck43  
**База данных:** PostgreSQL (`FitnessEfTraining` и `Fitnes`)  
**Платформа:** .NET 8 / C#  

## Описание
Итоговое консольное CRUD-приложение для управления клиентами фитнес-клуба с использованием **Entity Framework Core (Code First & Database First)** и СУБД **PostgreSQL** (`Npgsql`).

## Инструкция по развертыванию
1. Склонировать репозиторий:
   ```bash
   git clone <url>
   cd FitnessEf
   ```
2. Настроить переменную подключения (при необходимости):
   ```bash
   export FITNESS_EF_CONNECTION="Host=localhost;Port=5432;Database=FitnessEfTraining;Username=postgres;Password=ВАШ_ПАРОЛЬ"
   ```
3. Применить миграции Code First (создаст БД и таблицы):
   ```bash
   dotnet ef database update
   ```
4. Запустить интерактивное приложение:
   ```bash
   dotnet run
   ```
   Или запустить автоматические тесты заданий 2-11:
   ```bash
   dotnet run -- --test
   ```

## Структура выполненных заданий
- [x] **Задание 1:** Установка окружения и создание проекта (NuGet-пакеты EF Core, Npgsql, Design, dotnet-ef)
- [x] **Задание 2:** Модель Client и контекст FitnessContext (Fluent API, сопоставление схемы)
- [x] **Задание 3:** Миграции и первая база (InitialCreate, database update, демо-данные)
- [x] **Задание 4:** Чтение, сортировка и проекция (ToListAsync, OrderBy, Select)
- [x] **Задание 5:** Фильтрация и поиск (Where, Any, Count, Skip/Take)
- [x] **Задание 6:** Добавление записей (Add, SaveChangesAsync, валидация, DbUpdateException)
- [x] **Задание 7:** Изменение и удаление (UPDATE, DELETE, SaveChangesAsync)
- [x] **Задание 8:** NULL, типы и конфигурация модели (Fluent API, IsNullable, HasMaxLength)
- [x] **Задание 9:** Связь «клиент — посещения» (Сущность Visit, 1:N, Include, Restrict)
- [x] **Задание 10:** Миграции и существующая база (Scaffold базы Fitnes, ExistingData)
- [x] **Задание 11:** Ошибки, async, tracking и SQL (AsNoTracking, ToQueryString, ChangeTracker)
- [x] **Задание 12:** Итоговая работа — полноценное консольное CRUD-приложение с интерактивным меню