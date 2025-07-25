﻿# X0Game – REST API для крестиков-ноликов
Простой, Web API для игры в крестики-нолики, построенный с учетом архитектурных принципов и актуальных подходов разработки.

## Архитектура
Проект реализован на базе трехслойной архитектуры:
- Controllers – принимают HTTP-запросы и возвращают ответы.
- Services – содержат бизнес-логику игры, включая обработку ходов и проверку победы.
- Repositories – абстрагируют доступ к базе данных и позволяют легко заменить реализацию при необходимости.

Этот подход позволяет строго разделять ответственность между слоями и упрощает поддержку и тестирование.

## Паттерн Репозиторий
Для работы с данными используется паттерн Repository. Это даёт два ключевых преимущества:

- Изоляция бизнес-логики от конкретной реализации базы данных (в моём случае — Entity Framework Core).
- Возможность подмены слоя данных в тестах, что делает юнит-тестирование простым.

## Перехватчик ошибок ErrorHandlerMiddleware
Для централизованной обработки ошибок используется middleware ErrorHandlerMiddleware 
Он перехватывает все необработанные исключения в одном месте.

## Логирование с помощью Serilog
Для логирования используется библиотека Serilog. Все логи пишутся в консоль (для удобства при работе с Docker) и в файлы с ротацией по дням.
Логирование структурировано, что позволяет легко парсить и анализировать события, а также отслеживать полный жизненный цикл запроса.

## преобразование с AutoMapper
Для преобразования между доменными моделями и DTO используется AutoMapper, он помогает избегать длинного и рутинного кода,
Увеличивает читаемость и поддерживаемость кода, позволяя сосредоточиться на бизнес-логике, хоть и может вызвать некоторые сложности при отладке, и выбором типа данных, но в целом это оправдано.

## Обработка параллельных запросов
Для защиты от дублирующих запросов реализован "оптимистичный параллелизм":

- Используется встроенный в PostgreSQL столбец xmin как маркер версии строки.

- В EF Core xmin представлен свойством uint и настроен через .IsRowVersion(). 
Такой подход мне показалался правильным именно для PostgreSQL,в отличие от стандартного атрибута [Timestamp], предназначенного для SQL Server.
- При попытке сделать ход с устаревшей версией, возвращается текущее состояние игры без изменений, так соблюдается идемпотентность.

## Конфигурация через переменные окружения
Все ключевые параметры (размер поля, условия победы, строки подключения к базе и др.) 
задаются через переменные окружения, описанные в docker-compose.yml. это позволяет легко переключаться между средами (dev,prod),

## В проекте реализовано полноценное покрытие тестами:

### Юнит-тесты

- Используются xUnit и Moq.
- Покрывают логику сервисов: победы, ничьи, неправильные ходы, валидации и др.
- Пишутся изолированно, без зависимости от базы данных.

### Интеграционные тесты

- Основаны на WebApplicationFactory (Microsoft.AspNetCore.Mvc.Testing).
- Проверяют всю цепочку: от HTTP-запроса до ответа и взаимодействия с БД.
- Проверяют обработку ошибок, валидации, состояние игры после хода и поддержку параллельного доступа.

## Быстрый старт
docker-compose up --build

API будет доступно по адресу http://localhost:8080.
Интерактивная документация Swagger находится по адресу http://localhost:8080/swagger.

