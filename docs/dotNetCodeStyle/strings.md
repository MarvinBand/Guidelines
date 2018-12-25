# String

## Пустое значение

__Правильно__

```csharp
var text = String.Empty;
```

__Неправильно__

```csharp
var text = "";
```
Первый вариант предпочтительный, потому что не возникает мыслей, не забыл ли автор вставить значение, нет возможности добавить лишний пробел.

## Upper/lower case

__UperCase__

`String` с большой буквы используется, когда идет обращение к статическим полям этого класса.

```csharp
var text = String.Empty;

var formatedText = String.Format("Hello {0}", world);
```

__LowerCase__

`string` с маленькой буквы используется, когда объявляется переменная.

```csharp
string text;
```