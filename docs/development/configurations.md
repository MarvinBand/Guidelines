# Конфигурирование приложений

Для упрощения понимания файлов конфигурации всей командой разработки (разработчиками, тестировщиками, сис.админами) наименование параметров и их величины приводятся к единому стилю.

## Временные параметры

### Периоды

При необходимости указания периода работы сервиса, времени жизни объекта (`TTL`) название параметра должно принимать вид `названиеСуффикс`, где название - название параметра, а суффикс - единица измерения на английском (`ms`, `sec`, `min`, `hour`, `day`).

Пример:

```c#
    pingPeriodMs: 500
    actionIdTtlMin: 10
```

### Точное время

Если необходимо указать конкретнное время для выполнения операции (например, запуск фоновой задачи чистки базы), то время указывается в стандартном для .net формате TimeSpan (`d.hh.mm.ss.ms`)
День и милисекунды являются необязателньными.