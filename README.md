# Практическая работа: Entity Framework Core и PostgreSQL

**Проект:** FitnessEf  
**Студент (ФИО):** Heck43  
**База данных:** PostgreSQL (`FitnessEfTraining`)  
**Платформа:** .NET 8 / C#  

## Описание
Практическая работа по освоению Entity Framework Core (Code First / Database First) и взаимодействию с СУБД PostgreSQL с использованием провайдера Npgsql.

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
3. Применить миграции Code First:
   ```bash
   dotnet ef database update
   ```
4. Запустить проект:
   ```bash
   dotnet run
   ```

## Структура заданий
- [x] **Задание 1:** Установка окружения и создание проекта (NuGet-пакеты EF Core, Npgsql, Design, dotnet-ef)
- [x] **Задание 2:** Модель Client и контекст FitnessContext (Fluent API, сопоставление схемы)
- [x] **Задание 3:** Миграции и первая база (InitialCreate, database update, демо-данные)
- [x] **Задание 4:** Чтение, сортировка и проекция (ToListAsync, OrderBy, Select)
- [x] **Задание 5:** Фильтрация и поиск (Where, Any, Count, Skip/Take)
- [x] **Задание 6:** Добавление записей (Add, SaveChangesAsync, валидация, DbUpdateException)
- [ ] **Задание 7:** Изменение и удаление (UPDATE, DELETE)
- [ ] **Задание 8:** NULL, типы и конфигурация модели (Fluent API)
- [ ] **Задание 9:** Связь «клиент — посещения» (Include, 1-to-many)
- [ ] **Задание 10:** Миграции и существующая база (Scaffold)
- [ ] **Задание 11:** Ошибки, async, tracking и SQL (AsNoTracking, ToQueryString)
- [ ] **Задание 12:** Итоговая работа и проверка навыков (Консольное CRUD-приложение)